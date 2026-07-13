# Release Notes

## [v8.0.7]

- **Fix Nickless Trades & Showdown Sets**: Implemented ALM integration from FusionBot to support partial commands like `.t pikachu`.
- **ALM Encounter Generation Fix**: Replaced `LanguageHelper.GetTrainerInfoWithLanguage` with `AutoLegalityWrapper.GetTrainerInfo<T>()` to prevent ALM from enforcing language locks on base trainer info during encounter matching.
- **Language Post-Assignment**: Forcefully assigned target language after the initial legal template is resolved.
- **Nickname Legality Fix**: Automatically substitutes default translated species name based on assigned language when nickname is empty to bypass "Nickname does not match species" errors.
- **Asian Language Constraints**: Replicated FusionBot's temporary overwrite of `OriginalTrainerName` to bypass PKHeX's 6-character length limit for Asian languages, preventing false illegality flags.
