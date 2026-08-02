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

$authorizationUrl = "$authorizeUri?$query"
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
