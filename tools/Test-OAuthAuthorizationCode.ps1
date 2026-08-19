param(
    [string]$Authority = "https://localhost:5001",
    [string]$ClientId = "lagovista-oauth-test",
    [string]$Resource = "https://localhost:5001/test-resource",

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

function Assert-True {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (!$Condition) {
        throw $Message
    }
}

$authorityUri = [Uri]$Authority
$authorizeUri = [Uri]::new($authorityUri, "/connect/authorize")
$tokenUri = [Uri]::new($authorityUri, "/connect/token")
$whoAmIUri = [Uri]::new($authorityUri, "/api/oauth/whoami")

$verifier = New-CodeVerifier
$verifierBytes = [Text.Encoding]::ASCII.GetBytes($verifier)
$challengeBytes = [Security.Cryptography.SHA256]::HashData($verifierBytes)
$challenge = ConvertTo-Base64Url $challengeBytes
$state = [Guid]::NewGuid().ToString("N")

$query = ConvertTo-QueryString @{
    client_id = $ClientId
    redirect_uri = $RedirectUri
    response_type = "code"
    scope = $Scope
    resource = $Resource
    code_challenge = $challenge
    code_challenge_method = "S256"
    state = $state
}

<<<<<<< Updated upstream
$authorizationUrl = "$($authorizeUri)?$query"
=======
$authorizationUrl = "${authorizeUri}?$query"
>>>>>>> Stashed changes
$listener = [Net.HttpListener]::new()
$listener.Prefixes.Add($RedirectUri)

try {
    $listener.Start()
    Write-Host "Opening authorization request..."
    Write-Host $authorizationUrl
    Start-Process $authorizationUrl

    $context = $listener.GetContext()
    $callback = $context.Request.Url
    $responseText = "OAuth callback received. You may close this browser window."
    $responseBytes = [Text.Encoding]::UTF8.GetBytes($responseText)
    $context.Response.ContentType = "text/plain; charset=utf-8"
    $context.Response.ContentLength64 = $responseBytes.Length
    $context.Response.OutputStream.Write($responseBytes, 0, $responseBytes.Length)
    $context.Response.Close()

    $returnedState = Get-QueryValue -Uri $callback -Name "state"
    Assert-True ($returnedState -eq $state) "OAuth state validation failed."

    $error = Get-QueryValue -Uri $callback -Name "error"
    if (![String]::IsNullOrWhiteSpace($error)) {
        $description = Get-QueryValue -Uri $callback -Name "error_description"
        throw "OAuth authorization failed: $error $description"
    }

    $code = Get-QueryValue -Uri $callback -Name "code"
    Assert-True (![String]::IsNullOrWhiteSpace($code)) "The callback did not contain an authorization code: $callback"

    $token = Invoke-RestMethod -Method Post -Uri $tokenUri -ContentType "application/x-www-form-urlencoded" -Body @{
        grant_type = "authorization_code"
        client_id = $ClientId
        code = $code
        redirect_uri = $RedirectUri
        code_verifier = $verifier
        resource = $Resource
    }

    Assert-True (![String]::IsNullOrWhiteSpace($token.access_token)) "The token response did not contain an access token."

    Write-Host "Access token received. Calling whoami..."
    $whoAmI = Invoke-RestMethod -Method Get -Uri $whoAmIUri -Headers @{
        Authorization = "Bearer $($token.access_token)"
    }

    $whoAmI

    Assert-True (![String]::IsNullOrWhiteSpace($whoAmI.Subject)) "whoami did not return a subject."
    Assert-True ($whoAmI.ClientId -eq $ClientId) "whoami ClientId [$($whoAmI.ClientId)] did not match expected [$ClientId]."
    Assert-True ($whoAmI.Scopes -contains $Scope) "whoami did not contain expected scope [$Scope]."
    Assert-True ($whoAmI.Resources -contains $Resource) "whoami did not contain expected resource [$Resource]."

    Write-Host "[PASS] Authorization code + S256 PKCE flow completed."
    Write-Host "[PASS] whoami subject present."
    Write-Host "[PASS] client_id, scope, and resource matched the request."=======
    

    [PSCustomObject]@{
        Token = $token
        WhoAmI = $whoAmI
    }
}
finally {
    if ($listener.IsListening) {
        $listener.Stop()
    }

    $listener.Close()
}
