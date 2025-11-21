# Schedule Toolbox

A few commands and utilities for S1 mod testing.

![icon](https://raw.githubusercontent.com/k073l/s1-toolbox/master/assets/icon.png)

## Features
- `fly` command - better freecam, will teleport you to the position you were at when you disable it by running it again
- `pos` command - prints your current position in console and in GUI
- `savepos` command - allows you to save your position with a name, for later use

`savepos home1`, `savepos home2`, etc. will save your current position as `home1`, `home2`

- `tp` command - teleports you to a saved position, to a base game position (like `docks`) or to a position specified in coordinates (x, z or x, y, z)

`tp home1`, `tp home2` - will teleport you to the saved position `home1`, `home2`

`tp docks` - will teleport you to the docks position (base game position)

`tp 100 0 100` - will teleport you to the position with coordinates (100, 0, 100)

`tp 100 100` - will teleport you to the ground position with coordinates (100, 100)

- `timewarp` command - allows you to temporarily speed up the game time, useful for testing things that take a long time to happen

- `forcecarteldeal` command - Forces a new cartel deal, removing existing one
- `setcartelinfluence` command - Sets the cartel influence in specified region to a specified value.
    `setcartelinfluence docks 1` - sets the cartel influence in the docks region to 1 (100%)

- `forcedeal` command - Forces a new deal with specified customer, e.g. `forcedeal kyle_cooley`

- In-game help for added commands (accessed by commands button, below console switch)
- Disclaimer screen skip - faster game startup
- Hold-to-load - hold the number key button to load the game in that slot

ex. holding `1` will load the game in slot 1, holding `2` will load the game in slot 2, etc. You need to hold the button for 0.5 seconds. This feature only works the in menu.

- Command history - executed commands will be saved in `UserData/ScheduleToolbox/history.log` and can be accessed using up/down arrow keys in console
- Command autocomplete - start typing a command and press tab to autocomplete it. Can also be used to cycle through available commands if multiple match the typed prefix.
