$content = Get-Content .env
$tokenLine = $content | Select-String "GITHUB_TOKEN="
if ($tokenLine) {
    $token = $tokenLine.Line.Split("=")[1].Trim()
    $env:GITHUB_TOKEN = $token
    git add .
    git commit -m "chore: release v6.3.9"
    git push
    gh release create v6.3.9 --title "DudeBot.NET v6.3.9" --notes-file RELEASE_NOTES.md
} else {
    Write-Error "GITHUB_TOKEN not found in .env"
}
