$rootPath = "C:\Users\devry\OneDrive\Desktop\PokeBot\SysBot.Pokemon"

$replacements = @{
    # Fix direct config object references (e.g. config.Timings)
    "config\.Timings\b" = "config.Global.Timings"
    "config\.StopConditions\b" = "config.EncounterSystem.StopConditions"
    "config\.Distribution\b" = "config.TradeSystem.Distribution"
    "config\.Trade\b" = "config.TradeSystem.Settings"
    "config\.TradeAbuse\b" = "config.TradeSystem.Abuse"
    
    # Fix property access I might have missed
    "\.Config\.StopConditions\b" = ".Config.EncounterSystem.StopConditions"
    "Hub\.Config\.StopConditions\b" = "Hub.Config.EncounterSystem.StopConditions"
    
    # Fix remaining Distribution/Timings/etc if any
    "\.Config\.Distribution\b" = ".Config.TradeSystem.Distribution"
    "\.Config\.Timings\b" = ".Config.Global.Timings"
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
