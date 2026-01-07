$rootPath = "C:\Users\devry\OneDrive\Desktop\PokeBot\SysBot.Pokemon"

# Define replacements map (regex pattern -> replacement string)
# Note: Order matters for overlapping patterns, but these are mostly distinct.
# Using regex to match property access
$replacements = @{
    "\.Config\.TradeAbuse\b" = ".Config.TradeSystem.Abuse"
    "\.Config\.Trade\b" = ".Config.TradeSystem.Settings"
    "\.Config\.Distribution\b" = ".Config.TradeSystem.Distribution"
    "\.Config\.Queues\b" = ".Config.TradeSystem.Queues"
    "\.Config\.Stream\b" = ".Config.Integration.Stream"
    "\.Config\.AntiIdle\b" = ".Config.Global.AntiIdle"
    "\.Config\.Legality\b" = ".Config.Global.Legality"
    "\.Config\.Folder\b" = ".Config.Global.Folder"
    "\.Config\.Recovery\b" = ".Config.Global.Recovery"
    "\.Config\.Timings\b" = ".Config.Global.Timings"
    "\.Config\.Discord\b" = ".Config.Integration.Discord"
    "\.Config\.Twitch\b" = ".Config.Integration.Twitch"
    "\.Config\.YouTube\b" = ".Config.Integration.YouTube"
    "\.Config\.WebServer\b" = ".Config.Integration.WebServer"
    # Also handle Hub.Config which might be accessed directly if Hub is public
    "Hub\.Config\.TradeAbuse\b" = "Hub.Config.TradeSystem.Abuse"
    "Hub\.Config\.Trade\b" = "Hub.Config.TradeSystem.Settings"
    "Hub\.Config\.Distribution\b" = "Hub.Config.TradeSystem.Distribution"
    "Hub\.Config\.Queues\b" = "Hub.Config.TradeSystem.Queues"
    "Hub\.Config\.Stream\b" = "Hub.Config.Integration.Stream"
    "Hub\.Config\.AntiIdle\b" = "Hub.Config.Global.AntiIdle"
    "Hub\.Config\.Legality\b" = "Hub.Config.Global.Legality"
    "Hub\.Config\.Folder\b" = "Hub.Config.Global.Folder"
    "Hub\.Config\.Recovery\b" = "Hub.Config.Global.Recovery"
    "Hub\.Config\.Timings\b" = "Hub.Config.Global.Timings"
    "Hub\.Config\.Discord\b" = "Hub.Config.Integration.Discord"
    "Hub\.Config\.Twitch\b" = "Hub.Config.Integration.Twitch"
    "Hub\.Config\.YouTube\b" = "Hub.Config.Integration.YouTube"
    "Hub\.Config\.WebServer\b" = "Hub.Config.Integration.WebServer"
}

# Get all C# files recursively
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

Write-Host "Global config refactoring complete."
