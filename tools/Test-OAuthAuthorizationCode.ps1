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

function New-CodeVerifier {
    $bytes = New-Object byte[] 64
    [Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    return ConvertTo-Base64Url $bytes
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

$query = [System.Web.HttpUtility]::ParseQueryString([String]::Empty)
$query["client_id"] = $ClientId
$query["redirect_uri"] = $RedirectUri
$query["response_type"] = "code"
$query["scope"] = $Scope
$query["resource"] = $Resource
$query["code_challenge"] = $challenge
$query["code_challenge_method"] = "S256"
$query["state"] = $state

$authorizationUrl = "$authorizeUri?$($query.ToString())"
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

    $returnedState = $callback.Query.TrimStart('?').Split('&') |
        ForEach-Object { $_.Split('=', 2) } |
        Where-Object { $_[0] -eq 'state' } |
        ForEach-Object { [Uri]::UnescapeDataString($_[1]) }

    if ($returnedState -ne $state) {
        throw "OAuth state validation failed."
    }

    $code = $callback.Query.TrimStart('?').Split('&') |
        ForEach-Object { $_.Split('=', 2) } |
        Where-Object { $_[0] -eq 'code' } |
        ForEach-Object { [Uri]::UnescapeDataString($_[1]) }

    if ([String]::IsNullOrWhiteSpace($code)) {
        throw "The callback did not contain an authorization code: $callback"
    }

    $token = Invoke-RestMethod -Method Post -Uri $tokenUri -ContentType "application/x-www-form-urlencoded" -Body @{
        grant_type = "authorization_code"
        client_id = $ClientId
        code = $code
        redirect_uri = $RedirectUri
        code_verifier = $verifier
        resource = $Resource
    }

    Write-Host "Access token received. Calling whoami..."
    $whoAmI = Invoke-RestMethod -Method Get -Uri $whoAmIUri -Headers @{
        Authorization = "Bearer $($token.access_token)"
    }

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
