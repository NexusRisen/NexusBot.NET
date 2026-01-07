$swshFile = "C:\Users\devry\OneDrive\Desktop\PokeBot\SysBot.Pokemon\SWSH\BotTrade\PokeTradeBotSWSH.cs"
$plzaFile = "C:\Users\devry\OneDrive\Desktop\PokeBot\SysBot.Pokemon\PLZA\BotTrade\PokeTradeBotPLZA.cs"
$bsFile = "C:\Users\devry\OneDrive\Desktop\PokeBot\SysBot.Pokemon\BDSP\BotTrade\PokeTradeBotBS.cs"

# Fix SWSH Hub casing
$swshContent = Get-Content $swshFile -Raw
$swshContent = $swshContent -replace "Hub\.Config", "hub.Config"
Set-Content -Path $swshFile -Value $swshContent

# Define replacements for config paths
$replacements = @{
    "\.Config\.TradeAbuse" = ".Config.TradeSystem.Abuse"
    "\.Config\.Trade\b" = ".Config.TradeSystem.Settings"
    "\.Config\.Distribution" = ".Config.TradeSystem.Distribution"
    "\.Config\.Queues" = ".Config.TradeSystem.Queues"
    "\.Config\.Stream" = ".Config.Integration.Stream"
    "\.Config\.AntiIdle" = ".Config.Global.AntiIdle"
    "\.Config\.Legality" = ".Config.Global.Legality"
    "\.Config\.Folder" = ".Config.Global.Folder"
    "\.Config\.Recovery" = ".Config.Global.Recovery"
    "\.Config\.Timings" = ".Config.Global.Timings"
}

# Apply to PLZA
$plzaContent = Get-Content $plzaFile -Raw
foreach ($key in $replacements.Keys) {
    $plzaContent = $plzaContent -replace $key, $replacements[$key]
}
Set-Content -Path $plzaFile -Value $plzaContent

# Apply to BS
$bsContent = Get-Content $bsFile -Raw
foreach ($key in $replacements.Keys) {
    $bsContent = $bsContent -replace $key, $replacements[$key]
}
Set-Content -Path $bsFile -Value $bsContent

Write-Host "Replacements complete."
