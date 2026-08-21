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

    $environmentVariableName =
        Get-ConfigAuthEnvironmentVariableName $DeploymentKey

    $authToken =
        [Environment]::GetEnvironmentVariable($environmentVariableName)

    if ([string]::IsNullOrWhiteSpace($authToken)) {
        throw "Environment variable '$environmentVariableName' is not set."
    }

    $escapedAppKey = [Uri]::EscapeDataString($ApplicationKey)
    $escapedDeploymentKey = [Uri]::EscapeDataString($DeploymentKey)

    $uri =
        "$($ConfigServerBaseUrl.TrimEnd('/'))/api/config/$escapedAppKey/$escapedDeploymentKey"

    $headers = @{
        'x-config-auth' = $authToken
    }

    $response = Invoke-RestMethod `
        -Method Get `
        -Uri $uri `
        -Headers $headers

    if ($null -eq $response) {
        throw 'Config Server returned an empty response.'
    }

    #
    # Config Server may deserialize as either:
    #
    # {
    #     AppKey: ...
    #     DeploymentKey: ...
    #     Values: { ... }
    # }
    #
    # or directly as the Values property bag.
    #
    # Normalize both into ConfigValues.
    #

    $valuesProperty =
        $response.PSObject.Properties |
        Where-Object { $_.Name -ieq 'Values' } |
        Select-Object -First 1

    if ($null -ne $valuesProperty -and $null -ne $valuesProperty.Value) {
        $configValues = $valuesProperty.Value

        $resolvedAppKey = $response.AppKey
        $resolvedDeploymentKey = $response.DeploymentKey
        $responseShape = 'ResolvedConfiguration.Values'
    }
    else {
        $configValues = $response

        $resolvedAppKey = $ApplicationKey
        $resolvedDeploymentKey = $DeploymentKey
        $responseShape = 'root configuration dictionary'
    }

    if ($null -eq $configValues) {
        throw 'Config Server response did not contain configuration values.'
    }

    #
    # Unary comma prevents PowerShell from accidentally enumerating
    # anything stored in ConfigValues as function output.
    #
    return ,([PSCustomObject]@{
        Uri                     = $uri
        EnvironmentVariableName = $environmentVariableName
        AppKey                  = $resolvedAppKey
        DeploymentKey           = $resolvedDeploymentKey
        ResponseShape           = $responseShape
        ConfigValues            = $configValues
    })
}


function Get-ExactConfigurationValue {
    param(
        [object]$Values,
        [string]$Key
    )

    if ($null -eq $Values) {
        throw "No configuration values were available while looking for '$Key'."
    }

    #
    # Hashtable / IDictionary
    #
    if ($Values -is [System.Collections.IDictionary]) {
        foreach ($candidateKey in $Values.Keys) {
            if ([string]::Equals(
                [string]$candidateKey,
                $Key,
                [System.StringComparison]::OrdinalIgnoreCase)) {

                $value = $Values[$candidateKey]

                if ([string]::IsNullOrWhiteSpace([string]$value)) {
                    throw "Required configuration key '$Key' is present but blank."
                }

                return $value
            }
        }
    }
    else {
        #
        # Invoke-RestMethod usually gives us a PSCustomObject.
        # Colon-containing JSON keys such as OAuth:Microsoft:ClientId
        # are available through PSObject.Properties.
        #
        $property =
            $Values.PSObject.Properties |
            Where-Object {
                [string]::Equals(
                    $_.Name,
                    $Key,
                    [System.StringComparison]::OrdinalIgnoreCase)
            } |
            Select-Object -First 1

        if ($null -ne $property) {
            if ([string]::IsNullOrWhiteSpace([string]$property.Value)) {
                throw "Required configuration key '$Key' is present but blank."
            }

            return $property.Value
        }
    }

    throw "Required configuration key '$Key' was not returned by Config Server."
}


function Assert-ProviderConfiguration {
    param(
        [string]$ProviderName,
        [object]$Values
    )

    $requiredKeys =
        switch ($ProviderName.ToLowerInvariant()) {
            'microsoft' {
                @(
                    'OAuth:Microsoft:ClientId',
                    'OAuth:Microsoft:Secret',
                    'OAuth:Microsoft:SecretId'
                )
            }

            'google' {
                @(
                    'OAuth:Google:ClientId',
                    'OAuth:Google:Secret'
                )
            }

            default {
                throw "Unsupported provider '$ProviderName'."
            }
        }

    $resolvedValues = @{}

    foreach ($key in $requiredKeys) {
        $resolvedValues[$key] =
            Get-ExactConfigurationValue `
                -Values $Values `
                -Key $key

        Write-Check $key $true 'configured'
    }

    return $resolvedValues
}


function Test-JsonEndpoint {
    param(
        [string]$Label,
        [string]$Uri
    )

    try {
        $result =
            Invoke-RestMethod `
                -Method Get `
                -Uri $Uri

        Write-Check $Label $true $Uri

        return $result
    }
    catch {
        Write-Check $Label $false $_.Exception.Message
        return $null
    }
}


function Test-MicrosoftProvider {
    param([object]$Values)

    Write-Host ''
    Write-Host ' Microsoft'

    $null =
        Assert-ProviderConfiguration `
            -ProviderName 'microsoft' `
            -Values $Values

    $metadata =
        Test-JsonEndpoint `
            -Label 'Discovery document reachable' `
            -Uri 'https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration'

    if ($null -eq $metadata) {
        return $false
    }

    $authorizePresent =
        -not [string]::IsNullOrWhiteSpace(
            [string]$metadata.authorization_endpoint)

    $tokenPresent =
        -not [string]::IsNullOrWhiteSpace(
            [string]$metadata.token_endpoint)

    $jwksPresent =
        -not [string]::IsNullOrWhiteSpace(
            [string]$metadata.jwks_uri)

    Write-Check `
        'Authorization endpoint advertised' `
        $authorizePresent `
        ([string]$metadata.authorization_endpoint)

    Write-Check `
        'Token endpoint advertised' `
        $tokenPresent `
        ([string]$metadata.token_endpoint)

    Write-Check `
        'JWKS endpoint advertised' `
        $jwksPresent `
        ([string]$metadata.jwks_uri)

    $healthy =
        $authorizePresent -and
        $tokenPresent -and
        $jwksPresent

    if ($jwksPresent) {
        $jwks =
            Test-JsonEndpoint `
                -Label 'JWKS reachable' `
                -Uri ([string]$metadata.jwks_uri)

        $hasKeys =
            $null -ne $jwks -and
            $null -ne $jwks.keys -and
            @($jwks.keys).Count -gt 0

        Write-Check 'JWKS contains signing keys' $hasKeys

        $healthy =
            $healthy -and $hasKeys
    }

    return $healthy
}


function Test-GoogleProvider {
    param([object]$Values)

    Write-Host ''
    Write-Host ' Google'

    $null =
        Assert-ProviderConfiguration `
            -ProviderName 'google' `
            -Values $Values

    $metadata =
        Test-JsonEndpoint `
            -Label 'Discovery document reachable' `
            -Uri 'https://accounts.google.com/.well-known/openid-configuration'

    if ($null -eq $metadata) {
        return $false
    }

    $authorizePresent =
        -not [string]::IsNullOrWhiteSpace(
            [string]$metadata.authorization_endpoint)

    $tokenPresent =
        -not [string]::IsNullOrWhiteSpace(
            [string]$metadata.token_endpoint)

    $jwksPresent =
        -not [string]::IsNullOrWhiteSpace(
            [string]$metadata.jwks_uri)

    Write-Check `
        'Authorization endpoint advertised' `
        $authorizePresent `
        ([string]$metadata.authorization_endpoint)

    Write-Check `
        'Token endpoint advertised' `
        $tokenPresent `
        ([string]$metadata.token_endpoint)

    Write-Check `
        'JWKS endpoint advertised' `
        $jwksPresent `
        ([string]$metadata.jwks_uri)

    $healthy =
        $authorizePresent -and
        $tokenPresent -and
        $jwksPresent

    if ($jwksPresent) {
        $jwks =
            Test-JsonEndpoint `
                -Label 'JWKS reachable' `
                -Uri ([string]$metadata.jwks_uri)

        $hasKeys =
            $null -ne $jwks -and
            $null -ne $jwks.keys -and
            @($jwks.keys).Count -gt 0

        Write-Check 'JWKS contains signing keys' $hasKeys

        $healthy =
            $healthy -and $hasKeys
    }

    return $healthy
}


$deployments =
    if ($Deployment -eq 'all') {
        @('localdev', 'dev', 'live')
    }
    else {
        @($Deployment)
    }

$providers =
    if ($Provider -eq 'all') {
        @('microsoft', 'google')
    }
    else {
        @($Provider)
    }


$overallHealthy = $true


foreach ($deploymentKey in $deployments) {

    Write-Host ''
    Write-Host "OAuth configuration probe: app=$AppKey deployment=$deploymentKey"

    $configResult =
        Get-EffectiveConfiguration `
            -DeploymentKey $deploymentKey `
            -ApplicationKey $AppKey

    Write-Check `
        'Config Server request' `
        $true `
        $configResult.Uri

    Write-Check `
        'Config auth token available' `
        $true `
        $configResult.EnvironmentVariableName

    Write-Check `
        'Configuration response' `
        $true `
        $configResult.ResponseShape

    foreach ($providerName in $providers) {

        $providerHealthy =
            switch ($providerName) {
                'microsoft' {
                    Test-MicrosoftProvider `
                        -Values $configResult.ConfigValues
                }

                'google' {
                    Test-GoogleProvider `
                        -Values $configResult.ConfigValues
                }
            }

        $overallHealthy =
            $overallHealthy -and $providerHealthy
    }
}


Write-Host ''

if ($overallHealthy) {
    Write-Host 'OAuth configuration probe: HEALTHY'
    exit 0
}

Write-Host 'OAuth configuration probe: FAILED'
exit 1