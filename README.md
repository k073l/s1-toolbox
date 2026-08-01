# Schedule Toolbox

A collection of commands and quality-of-life features for Schedule I mod testing.

![icon](https://raw.githubusercontent.com/k073l/s1-toolbox/master/assets/icon.png)

## Commands

### Player

#### `fly`
Enables an improved freecam. Run the command again to disable it and return to the position where you enabled freecam.

#### `pos`
Displays your current position in both the console and on-screen GUI.

#### `savepos <name>`
Saves your current position under the given name.

**Example**

```text
savepos home
savepos dealer
savepos warehouse
```

#### `tp <destination>`
Teleports you to a saved position, a built-in game location, or a set of coordinates.

**Examples**

```text
tp home
tp docks
tp 100 100
tp 100 0 100
```

- `tp <saved_position>` - Teleport to a saved position.
- `tp <location>` - Teleport to a built-in location (e.g. `docks`).
- `tp <x> <z>` - Teleport to ground level at the specified coordinates.
- `tp <x> <y> <z>` - Teleport to the exact coordinates.

---

### World

#### `timewarp`
Temporarily speeds up game time.

> **Note**
> Timewarp can also be controlled using configurable keybinds (via MelonPreferences).

#### `forcecarteldeal`
Removes the current cartel deal and generates a new one.

#### `setcartelinfluence <region> <value>`
Sets cartel influence for the specified region.

**Example**

```text
setcartelinfluence docks 1
```

#### `forcedeal <customer>`
Forces a new deal with the specified customer.

**Example**

```text
forcedeal kyle_cooley
```

---

### Items

#### `copyhand`
Copies the currently held item, including all of its properties (quality, quantity, packaging, etc.).

#### `pastehand`
Restores the previously copied item.

#### `listitems`
Lists every storable item in the game.

#### `listnpcs`
Lists every NPC in the game.

---

## QoL Features

### Disclaimer skip
Skips the disclaimer screen for faster startup.

### Hold-to-load saves
Hold a number key for **0.5 seconds** while in the main menu to load the corresponding save slot.

**Examples**

- Hold `1` → Load save slot 1
- Hold `2` → Load save slot 2
- Hold `3` → Load save slot 3

> **Note**
> This feature only works in the main menu.

### Command history
Executed commands are saved to:

```text
UserData/ScheduleToolbox/history.log
```

Use the **↑** and **↓** arrow keys in the console to navigate command history.

### Command autocomplete
Press **Tab** to autocomplete commands.

If multiple commands match the current input, pressing **Tab** repeatedly cycles through them.

### Persistent keybinds
Keybinds created with `bind` are automatically saved and restored on startup.

- `unbind` removes a saved keybind.
- `clearbinds` removes all saved keybinds.

---

## Credits

- [HazDS](https://github.com/HazDS) - Timewarp keybinds and GUI contributions.