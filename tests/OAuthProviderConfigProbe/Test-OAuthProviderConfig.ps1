[CmdletBinding()]
param(
    [ValidateSet('localdev', 'dev', 'live', 'all')]
    [string]$Deployment = 'localdev',

    [ValidateSet('microsoft', 'google', 'all')]
    [string]$Provider = 'all',

    [string]$AppKey = 'web',

    [string]$ConfigServerBaseUrl = 'https://config.nuviot.com'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-Check {
    param(
        [string]$Label,
        [bool]$Passed,
        [string]$Detail = ''
    )

    $status = if ($Passed) { 'PASS' } else { 'FAIL' }
    $suffix = if ([string]::IsNullOrWhiteSpace($Detail)) { '' } else { " - $Detail" }
    Write-Host ('  {0,-5} {1}{2}' -f $status, $Label, $suffix)
}

function Get-ConfigAuthEnvironmentVariableName {
    param([string]$DeploymentKey)

    switch ($DeploymentKey.ToLowerInvariant()) {
        'localdev' { return 'CFG_SRVR_LOCALDEV' }
        'dev'      { return 'CFG_SRVR_DEV' }
        'live'     { return 'CFG_SRVR_LIVE' }
        default    { throw "Unsupported deployment '$DeploymentKey'." }
    }
}

function Get-EffectiveConfiguration {
    param(
        [string]$DeploymentKey,
        [string]$ApplicationKey
    )

    $environmentVariableName = Get-ConfigAuthEnvironmentVariableName $DeploymentKey
    $authToken = [Environment]::GetEnvironmentVariable($environmentVariableName)

    if ([string]::IsNullOrWhiteSpace($authToken)) {
        throw "Environment variable '$environmentVariableName' is not set."
    }

    $escapedAppKey = [Uri]::EscapeDataString($ApplicationKey)
    $escapedDeploymentKey = [Uri]::EscapeDataString($DeploymentKey)
    $uri = "$($ConfigServerBaseUrl.TrimEnd('/'))/api/config/$escapedAppKey/$escapedDeploymentKey"

    $headers = @{ 'x-config-auth' = $authToken }
    $configuration = Invoke-RestMethod -Method Get -Uri $uri -Headers $headers

    return [PSCustomObject]@{
        Uri = $uri
        EnvironmentVariableName = $environmentVariableName
        Configuration = $configuration
    }
}

function Find-ProviderSections {
    param(
        [object]$Value,
        [string]$ProviderName,
        [string]$Path = '$'
    )

    $results = @()
    if ($null -eq $Value) { return $results }

    if ($Value -is [System.Collections.IDictionary]) {
        foreach ($key in $Value.Keys) {
            $child = $Value[$key]
            $childPath = "$Path.$key"
            if ([string]$key -match "(?i)$ProviderName.*oauth|oauth.*$ProviderName|^$ProviderName$") {
                $results += [PSCustomObject]@{ Path = $childPath; Value = $child }
            }

            if ($child -is [System.Collections.IDictionary] -or
                $child -is [PSCustomObject] -or
                ($child -is [System.Collections.IEnumerable] -and -not ($child -is [string]))) {
                $results += Find-ProviderSections -Value $child -ProviderName $ProviderName -Path $childPath
            }
        }
        return $results
    }

    if ($Value -is [System.Collections.IEnumerable] -and -not ($Value -is [string])) {
        $index = 0
        foreach ($item in $Value) {
            if ($item -is [System.Collections.IDictionary] -or
                $item -is [PSCustomObject] -or
                ($item -is [System.Collections.IEnumerable] -and -not ($item -is [string]))) {
                $results += Find-ProviderSections -Value $item -ProviderName $ProviderName -Path "$Path[$index]"
            }
            $index++
        }
        return $results
    }

    if ($Value -is [PSCustomObject]) {
        foreach ($property in $Value.PSObject.Properties | Where-Object { $_.MemberType -eq 'NoteProperty' }) {
            $childPath = "$Path.$($property.Name)"
            if ($property.Name -match "(?i)$ProviderName.*oauth|oauth.*$ProviderName|^$ProviderName$") {
                $results += [PSCustomObject]@{ Path = $childPath; Value = $property.Value }
            }

            $child = $property.Value
            if ($child -is [System.Collections.IDictionary] -or
                $child -is [PSCustomObject] -or
                ($child -is [System.Collections.IEnumerable] -and -not ($child -is [string]))) {
                $results += Find-ProviderSections -Value $child -ProviderName $ProviderName -Path $childPath
            }
        }
    }

    return $results
}

function Get-PropertyValue {
    param(
        [object]$Object,
        [string[]]$Names
    )

    if ($null -eq $Object) { return $null }

    foreach ($name in $Names) {
        $property = $Object.PSObject.Properties | Where-Object { $_.Name -ieq $name } | Select-Object -First 1
        if ($null -ne $property) { return $property.Value }
    }

    return $null
}

function Test-ProviderConfigSection {
    param(
        [string]$ProviderName,
        [object]$Configuration
    )

    $sections = @(Find-ProviderSections -Value $Configuration -ProviderName $ProviderName)
    if ($sections.Count -eq 0) {
        Write-Check "$ProviderName OAuth configuration section" $false 'No matching section found in Config Server JSON.'
        return $false
    }

    $section = $sections[0]
    Write-Check "$ProviderName OAuth configuration section" $true $section.Path

    $clientId = Get-PropertyValue -Object $section.Value -Names @('ClientId', 'ClientID', 'AppId', 'ApplicationId')
    $secret = Get-PropertyValue -Object $section.Value -Names @('Secret', 'ClientSecret', 'ClientSecretValue')

    $clientIdPresent = -not [string]::IsNullOrWhiteSpace([string]$clientId)
    $secretPresent = -not [string]::IsNullOrWhiteSpace([string]$secret)

    Write-Check 'Client ID configured' $clientIdPresent
    Write-Check 'Client secret configured' $secretPresent

    return ($clientIdPresent -and $secretPresent)
}

function Test-JsonEndpoint {
    param(
        [string]$Label,
        [string]$Uri
    )

    try {
        $result = Invoke-RestMethod -Method Get -Uri $Uri
        Write-Check $Label $true $Uri
        return $result
    }
    catch {
        Write-Check $Label $false $_.Exception.Message
        return $null
    }
}

function Test-MicrosoftProvider {
    param([object]$Configuration)

    Write-Host ' Microsoft'
    $healthy = Test-ProviderConfigSection -ProviderName 'microsoft' -Configuration $Configuration

    $metadata = Test-JsonEndpoint -Label 'Discovery document reachable' -Uri 'https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration'
    if ($null -eq $metadata) { return $false }

    $authorizePresent = -not [string]::IsNullOrWhiteSpace([string]$metadata.authorization_endpoint)
    $tokenPresent = -not [string]::IsNullOrWhiteSpace([string]$metadata.token_endpoint)
    $jwksPresent = -not [string]::IsNullOrWhiteSpace([string]$metadata.jwks_uri)

    Write-Check 'Authorization endpoint advertised' $authorizePresent ([string]$metadata.authorization_endpoint)
    Write-Check 'Token endpoint advertised' $tokenPresent ([string]$metadata.token_endpoint)
    Write-Check 'JWKS endpoint advertised' $jwksPresent ([string]$metadata.jwks_uri)

    if ($jwksPresent) {
        $jwks = Test-JsonEndpoint -Label 'JWKS reachable' -Uri ([string]$metadata.jwks_uri)
        $hasKeys = $null -ne $jwks -and $null -ne $jwks.keys -and @($jwks.keys).Count -gt 0
        Write-Check 'JWKS contains signing keys' $hasKeys
        $healthy = $healthy -and $hasKeys
    }
    else {
        $healthy = $false
    }

    return ($healthy -and $authorizePresent -and $tokenPresent)
}

function Test-GoogleProvider {
    param([object]$Configuration)

    Write-Host ' Google'
    $healthy = Test-ProviderConfigSection -ProviderName 'google' -Configuration $Configuration

    $metadata = Test-JsonEndpoint -Label 'Discovery document reachable' -Uri 'https://accounts.google.com/.well-known/openid-configuration'
    if ($null -eq $metadata) { return $false }

    $authorizePresent = -not [string]::IsNullOrWhiteSpace([string]$metadata.authorization_endpoint)
    $tokenPresent = -not [string]::IsNullOrWhiteSpace([string]$metadata.token_endpoint)
    $jwksPresent = -not [string]::IsNullOrWhiteSpace([string]$metadata.jwks_uri)

    Write-Check 'Authorization endpoint advertised' $authorizePresent ([string]$metadata.authorization_endpoint)
    Write-Check 'Token endpoint advertised' $tokenPresent ([string]$metadata.token_endpoint)
    Write-Check 'JWKS endpoint advertised' $jwksPresent ([string]$metadata.jwks_uri)

    if ($jwksPresent) {
        $jwks = Test-JsonEndpoint -Label 'JWKS reachable' -Uri ([string]$metadata.jwks_uri)
        $hasKeys = $null -ne $jwks -and $null -ne $jwks.keys -and @($jwks.keys).Count -gt 0
        Write-Check 'JWKS contains signing keys' $hasKeys
        $healthy = $healthy -and $hasKeys
    }
    else {
        $healthy = $false
    }

    return ($healthy -and $authorizePresent -and $tokenPresent)
}

$deployments = if ($Deployment -eq 'all') { @('localdev', 'dev', 'live') } else { @($Deployment) }
$providers = if ($Provider -eq 'all') { @('microsoft', 'google') } else { @($Provider) }
$overallHealthy = $true

foreach ($deploymentKey in $deployments) {
    Write-Host ''
    Write-Host "OAuth configuration probe: app=$AppKey deployment=$deploymentKey"

    try {
        $configResult = Get-EffectiveConfiguration -DeploymentKey $deploymentKey -ApplicationKey $AppKey
        Write-Check 'Config Server request' $true $configResult.Uri
        Write-Check 'Config auth token available' $true $configResult.EnvironmentVariableName
    }
    catch {
        Write-Check 'Config Server request' $false $_.Exception.Message
        $overallHealthy = $false
        continue
    }

    foreach ($providerName in $providers) {
        $providerHealthy = switch ($providerName) {
            'microsoft' { Test-MicrosoftProvider -Configuration $configResult.Configuration }
            'google'    { Test-GoogleProvider -Configuration $configResult.Configuration }
        }

        $overallHealthy = $overallHealthy -and $providerHealthy
    }
}

Write-Host ''
if ($overallHealthy) {
    Write-Host 'OAuth configuration probe: HEALTHY'
    exit 0
}

Write-Host 'OAuth configuration probe: FAILED'
exit 1
