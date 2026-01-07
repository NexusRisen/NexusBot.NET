$rootPath = "C:\Users\devry\OneDrive\Desktop\PokeBot\SysBot.Pokemon"

$replacements = @{
    "\.Config\.Favoritism\b" = ".Config.Integration.Favoritism"
    "\.Config\.EncounterSWSH\b" = ".Config.EncounterSystem.EncounterSWSH"
    "\.Config\.RaidSWSH\b" = ".Config.EncounterSystem.RaidSWSH"
    "\.Config\.SkipConsoleBotCreation\b" = ".Config.Global.SkipConsoleBotCreation"
    "Hub\.Config\.Favoritism\b" = "Hub.Config.Integration.Favoritism"
    "Hub\.Config\.EncounterSWSH\b" = "Hub.Config.EncounterSystem.EncounterSWSH"
    "Hub\.Config\.RaidSWSH\b" = "Hub.Config.EncounterSystem.RaidSWSH"
    "Hub\.Config\.SkipConsoleBotCreation\b" = "Hub.Config.Global.SkipConsoleBotCreation"
}

$files = Get-ChildItem -Path $rootPath -Recurse -Filter "*.cs"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    foreach ($key in $replacements.Keys) {
        $content = [regex]::Replace($content, $key, $replacements[$key])
    }

    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content
        Write-Host "Updated: $($file.Name)"
    }
}
