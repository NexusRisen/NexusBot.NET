param(
  [string]$Owner,
  [string]$Repo = "PokeBot",
  [string]$Tag = "v1.0.0",
  [switch]$CreateRelease = $true,
  [string]$ReleaseBody = "",
  [string]$AssetPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Info($msg) { Write-Host "[INFO] $msg" }
function Write-Err($msg) { Write-Host "[ERROR] $msg" -ForegroundColor Red }

$token = $env:GITHUB_TOKEN
if ([string]::IsNullOrWhiteSpace($token)) {
  Write-Err "GITHUB_TOKEN env var not set. Set a GitHub Personal Access Token with repo scope."
  exit 1
}

$headers = @{
  "Authorization" = "Bearer $token"
  "Accept"        = "application/vnd.github+json"
  "User-Agent"    = "PokeBot-Publisher"
}

$userUrl = "https://api.github.com/user"
try {
  $userResp = Invoke-RestMethod -Method GET -Uri $userUrl -Headers $headers
  if ([string]::IsNullOrWhiteSpace($Owner)) {
    $Owner = $userResp.login
  }
  Write-Info "Authenticated GitHub user: $Owner"
} catch {
  Write-Err "Failed to get authenticated user info: $($_.Exception.Message)"
}

$repoUrl = "https://api.github.com/user/repos"
$body = @{ name = $Repo; private = $false } | ConvertTo-Json

Write-Info "Creating repository '$Repo' under authenticated user..."
try {
  $createResp = Invoke-RestMethod -Method POST -Uri $repoUrl -Headers $headers -Body $body
  Write-Info "Repository created at $($createResp.html_url)"
} catch {
  Write-Info "Repo may already exist or creation failed: $($_.Exception.Message)"
}

Write-Info "Configuring git remote and pushing main with tags..."
git remote remove origin 2>$null | Out-Null
git remote add origin "https://github.com/$Owner/$Repo.git"
git -c http.extraheader="Authorization: Bearer $token" push -u origin main --tags

if ($CreateRelease) {
  $relUrl = "https://api.github.com/repos/$Owner/$Repo/releases"
  $relBody = @{
    tag_name = $Tag
    name     = $Tag
    prerelease = $false
    body     = $ReleaseBody
  } | ConvertTo-Json
  Write-Info "Creating GitHub release $Tag..."
  try {
    $relResp = Invoke-RestMethod -Method POST -Uri $relUrl -Headers $headers -Body $relBody
    Write-Info "Release created: $($relResp.html_url)"
    if (-not [string]::IsNullOrWhiteSpace($AssetPath) -and (Test-Path $AssetPath)) {
      $fileName = Split-Path -Path $AssetPath -Leaf
      $uploadUrl = "https://uploads.github.com/repos/$Owner/$Repo/releases/$($relResp.id)/assets?name=$fileName"
      Write-Info "Uploading release asset $fileName..."
      Invoke-WebRequest -Method POST -Uri $uploadUrl -Headers @{
        "Authorization" = "Bearer $token"
        "Accept"        = "application/vnd.github+json"
        "User-Agent"    = "PokeBot-Publisher"
        "Content-Type"  = "application/octet-stream"
      } -InFile $AssetPath | Out-Null
      Write-Info "Asset upload completed."
    } else {
      Write-Info "No asset to upload or asset path missing."
    }
  } catch {
    Write-Err "Failed to create release: $($_.Exception.Message)"
    try {
      $existingRel = Invoke-RestMethod -Method GET -Uri "$relUrl/tags/$Tag" -Headers $headers
      Write-Info "Found existing release: $($existingRel.html_url)"
      if (-not [string]::IsNullOrWhiteSpace($AssetPath) -and (Test-Path $AssetPath)) {
        $fileName = Split-Path -Path $AssetPath -Leaf
        
        # Check for existing asset and delete it
        $existingAsset = $existingRel.assets | Where-Object { $_.name -eq $fileName }
        if ($existingAsset) {
            Write-Info "Deleting existing asset $($existingAsset.name)..."
            Invoke-RestMethod -Method DELETE -Uri $existingAsset.url -Headers $headers
        }

        $uploadUrl = "https://uploads.github.com/repos/$Owner/$Repo/releases/$($existingRel.id)/assets?name=$fileName"
        Write-Info "Uploading release asset $fileName to existing release..."
        Invoke-WebRequest -Method POST -Uri $uploadUrl -Headers @{
          "Authorization" = "Bearer $token"
          "Accept"        = "application/vnd.github+json"
          "User-Agent"    = "PokeBot-Publisher"
          "Content-Type"  = "application/octet-stream"
        } -InFile $AssetPath | Out-Null
        Write-Info "Asset upload completed."
      }
    } catch {
      Write-Err "Failed to update existing release: $($_.Exception.Message)"
    }
  }
}

Write-Info "Done."
