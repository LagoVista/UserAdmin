param(
    [Parameter(Mandatory = $true)]
    [string]$Authority,

    [Parameter(Mandatory = $true)]
    [string]$ClientId,

    [Parameter(Mandatory = $true)]
    [string]$Resource,

    [string]$Scope = "knowledge.read",
    [string]$RedirectUri = "http://127.0.0.1:8765/callback/"
)

$ErrorActionPreference = "Stop"

function ConvertTo-Base64Url {
    param([byte[]]$Bytes)

    return [Convert]::ToBase64String($Bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function ConvertTo-QueryString {
    param([hashtable]$Values)

    return ($Values.GetEnumerator() |
        Sort-Object Key |
        ForEach-Object {
            "{0}={1}" -f [Uri]::EscapeDataString([string]$_.Key), [Uri]::EscapeDataString([string]$_.Value)
        }) -join '&'
}

function Get-QueryValue {
    param(
        [Uri]$Uri,
        [string]$Name
    )

    foreach ($item in $Uri.Query.TrimStart('?').Split('&', [StringSplitOptions]::RemoveEmptyEntries)) {
        $parts = $item.Split('=', 2)
        if ([Uri]::UnescapeDataString($parts[0]) -eq $Name) {
            if ($parts.Length -eq 1) {
                return ""
            }

            return [Uri]::UnescapeDataString($parts[1])
        }
    }

    return $null
}

function New-CodeVerifier {
    $bytes = New-Object byte[] 64
    [Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    return ConvertTo-Base64Url $bytes
}

function New-CodeChallenge {
    param([string]$Verifier)

    $verifierBytes = [Text.Encoding]::ASCII.GetBytes($Verifier)
    $challengeBytes = [Security.Cryptography.SHA256]::HashData($verifierBytes)
    return ConvertTo-Base64Url $challengeBytes
}

function New-AuthorizationUrl {
    param(
        [string]$RequestedScope,
        [string]$RequestedResource,
        [string]$Challenge,
        [string]$State
    )

    $query = ConvertTo-QueryString @{
        client_id = $ClientId
        redirect_uri = $RedirectUri
        response_type = "code"
        scope = $RequestedScope
        resource = $RequestedResource
        code_challenge = $Challenge
        code_challenge_method = "S256"
        state = $State
    }

    return "$($script:authorizeUri)?$query"
}

function Get-AuthorizationCode {
    $verifier = New-CodeVerifier
    $challenge = New-CodeChallenge -Verifier $verifier
    $state = [Guid]::NewGuid().ToString("N")
    $authorizationUrl = New-AuthorizationUrl -RequestedScope $Scope -RequestedResource $Resource -Challenge $challenge -State $state

    $listener = [Net.HttpListener]::new()
    $listener.Prefixes.Add($RedirectUri)

    try {
        $listener.Start()
        Write-Host "Opening browser for authorization..."
        Start-Process $authorizationUrl

        $context = $listener.GetContext()
        $callback = $context.Request.Url
        $responseText = "OAuth security-test callback received. You may close this browser window."
        $responseBytes = [Text.Encoding]::UTF8.GetBytes($responseText)
        $context.Response.ContentType = "text/plain; charset=utf-8"
        $context.Response.ContentLength64 = $responseBytes.Length
        $context.Response.OutputStream.Write($responseBytes, 0, $responseBytes.Length)
        $context.Response.Close()

        $returnedState = Get-QueryValue -Uri $callback -Name "state"
        if ($returnedState -ne $state) {
            throw "OAuth state validation failed."
        }

        $error = Get-QueryValue -Uri $callback -Name "error"
        if (![String]::IsNullOrWhiteSpace($error)) {
            $description = Get-QueryValue -Uri $callback -Name "error_description"
            throw "OAuth authorization failed: $error $description"
        }

        $code = Get-QueryValue -Uri $callback -Name "code"
        if ([String]::IsNullOrWhiteSpace($code)) {
            throw "The callback did not contain an authorization code: $callback"
        }

        return [PSCustomObject]@{
            Code = $code
            Verifier = $verifier
        }
    }
    finally {
        if ($listener.IsListening) {
            $listener.Stop()
        }

        $listener.Close()
    }
}

function Invoke-TokenRequest {
    param(
        [string]$Code,
        [string]$Verifier
    )

    return Invoke-RestMethod -Method Post -Uri $script:tokenUri -ContentType "application/x-www-form-urlencoded" -Body @{
        grant_type = "authorization_code"
        client_id = $ClientId
        code = $Code
        redirect_uri = $RedirectUri
        code_verifier = $Verifier
        resource = $Resource
    }
}

function Test-AuthorizationRejection {
    param(
        [string]$Name,
        [string]$RequestedScope,
        [string]$RequestedResource,
        [string]$ExpectedError
    )

    $verifier = New-CodeVerifier
    $challenge = New-CodeChallenge -Verifier $verifier
    $state = [Guid]::NewGuid().ToString("N")
    $url = New-AuthorizationUrl -RequestedScope $RequestedScope -RequestedResource $RequestedResource -Challenge $challenge -State $state

    try {
        $response = Invoke-WebRequest -Uri $url -MaximumRedirection 0 -SkipHttpErrorCheck
        $location = $response.Headers.Location

        if (![String]::IsNullOrWhiteSpace($location)) {
            $redirect = [Uri]$location
            $error = Get-QueryValue -Uri $redirect -Name "error"
            if ($error -eq $ExpectedError) {
                Write-Host "[PASS] $Name"
                return $true
            }
        }

        if ($response.Content -match [Regex]::Escape($ExpectedError)) {
            Write-Host "[PASS] $Name"
            return $true
        }

        Write-Host "[FAIL] $Name - expected [$ExpectedError], received HTTP $($response.StatusCode)."
        return $false
    }
    catch {
        $details = $_.ErrorDetails.Message
        if ($details -match [Regex]::Escape($ExpectedError)) {
            Write-Host "[PASS] $Name"
            return $true
        }

        Write-Host "[FAIL] $Name - $($_.Exception.Message)"
        return $false
    }
}

function Test-ExpectedTokenFailure {
    param(
        [string]$Name,
        [scriptblock]$Action
    )

    try {
        & $Action | Out-Null
        Write-Host "[FAIL] $Name - request unexpectedly succeeded."
        return $false
    }
    catch {
        Write-Host "[PASS] $Name"
        return $true
    }
}

$authorityUri = [Uri]$Authority
$authorizeUri = [Uri]::new($authorityUri, "/connect/authorize")
$tokenUri = [Uri]::new($authorityUri, "/connect/token")

$passed = 0
$failed = 0

Write-Host "OAuth Authorization Code Security Tests"
Write-Host ""

if (Test-AuthorizationRejection -Name "Invalid scope rejected" -RequestedScope "$Scope.invalid" -RequestedResource $Resource -ExpectedError "invalid_scope") {
    $passed++
}
else {
    $failed++
}

if (Test-AuthorizationRejection -Name "Invalid resource rejected" -RequestedScope $Scope -RequestedResource "$Resource/invalid" -ExpectedError "invalid_target") {
    $passed++
}
else {
    $failed++
}

$badPkce = Get-AuthorizationCode
$wrongVerifier = New-CodeVerifier
if (Test-ExpectedTokenFailure -Name "Incorrect PKCE verifier rejected" -Action {
    Invoke-TokenRequest -Code $badPkce.Code -Verifier $wrongVerifier
}) {
    $passed++
}
else {
    $failed++
}

$replay = Get-AuthorizationCode
$firstToken = Invoke-TokenRequest -Code $replay.Code -Verifier $replay.Verifier
if ([String]::IsNullOrWhiteSpace($firstToken.access_token)) {
    throw "Replay test could not obtain the first access token."
}

if (Test-ExpectedTokenFailure -Name "Authorization code replay rejected" -Action {
    Invoke-TokenRequest -Code $replay.Code -Verifier $replay.Verifier
}) {
    $passed++
}
else {
    $failed++
}

Write-Host ""
Write-Host "$passed passed, $failed failed"

if ($failed -gt 0) {
    exit 1
}
