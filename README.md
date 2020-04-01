# Tides
Standard tidal event for the Rust day

## Configuration

```json
{
  "Sunrise": 7.0,
  "Sunset": 18.0,
  "increment": 0.005,
  "speed": 1.0,
  "maxLevel": 3.0,
  "minLevel": 0.0,
  "UseMessageBroadcast": false,
  "UseGUIAnnouncements": false,
  "Version": {
    "Major": 1,
    "Minor": 0,
    "Patch": 0
  }
}
```

Starting at 'Sunrise,' the ocean level will begin to rise to the 'maxLevel.'

Starting at 'Sunset,' the ocean level will begin to recede to the 'minLevel.'

'Sunrise' and 'Sunset' are the Rust day times, and must be either whole numbers or decimal, e.g. 7.5 for 7:30AM.

The level will rise by 'increment' every 'speed' number of seconds until it reaches the target level.

You can selectively use either message broadcast to all players, or use the GUIAnnouncements to post it to their screens.

## Commands

- `ocean check` -- Displays the current level as set by the plugin.
- `ocean X force` -- Force the ocean level to X.  This also disables the timed setting.
- `ocean reset` -- Re-enable the timed setting of ocean level (to undo "force").

## Permissions

- `tides.use` -- Allows player to use the ocean command
