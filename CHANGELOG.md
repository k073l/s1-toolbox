# Changelog

## 2.1.3
- Added max buffer size option to limit the size of command history log file. Default is 0 lines (unlimited).
- Removed base game command history to avoid conflicts with toolbox command history.
## 2.1.2
- Added a patch to fix `Settings.GetDefaultUnitTypeForPlayer` crashing if CultureInfo is `CultureInvariant`. This issue caused settings to reset to default on every game start for some players.
## 2.1.1
- Added `forcedeal` command - forces a new deal with specified customer
## 2.1.0
- Added command history, browse previous commands with Up/Down arrow keys
- Rudimentary autocomplete - press Tab to autocomplete command names
## 2.0.0
- Added `forcecarteldeal` command - forces a new cartel deal, removing existing one
- Added `setcartelinfluence` command - sets the cartel influence in specified region to a specified value
- Fixed `timewarp` command to default to 5x timescale
## 1.1.1
- `timewarp` now accepts 2 arguments - duration and timescale
## 1.1.0
- added `timewarp` command - allows you to temporarily speed up the game time
## 1.0.0
- initial
