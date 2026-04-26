# HostGuard

A BepInEx/Reactor mod for Among Us that protects lobbies from bots, cheaters, and griefers. Host-only.

## Features

### Bot Protection
- **Known bot detection** — auto-ban/kick bots by name (TNT, auser, Haunt Bot, etc.)
- **Bot URL detection** — catches bot spam URLs in chat (tntaddict, matchducking, etc.)
- **No friend code detection** — bans guest/bot accounts with no friend code
- **Cosmetic detection** — optional detection of suspicious cosmetic profiles

### Anti-Cheat
- **RPC validation** — blocks unauthorized kills, vents, shapeshifts, and other exploits
- **Chat rate limiting** — prevents chat spam floods
- **Meeting spam protection** — kicks players who spam emergency meetings

### Flood Protection
- **Join flood detection** — detects rapid join attacks and auto-bans
- **Rapid join-leave detection** — catches join-leave spam bots
- **Auto lobby lock** — automatically sets lobby to private during flood attacks, unlocks after configurable duration
- **Session blacklist** — flood-banned players stay banned for the session

### Name Filter
- **Bad name filter** — kicks/bans players with offensive words in their name
- **Default name detection** — kicks randomly generated names (e.g. "Funnybone", "Sillyhawk") with ~4500 word patterns
- **Strict casing** — optionally match only exact default name casing

### Chat Filter
- **Banned words** — kick or ban players who type specific words
- **Contains mode** — trigger on messages containing a banned word, not just exact matches
- **In-game word management** — add/remove words via chat commands

### Minimum Level
- Kick or ban players below a configurable level threshold

### Presets
- **Config presets** — save and load HostGuard settings (filters, thresholds, toggles)
- **Game presets** — save and load Among Us game settings (speed, impostors, kill cooldown, vision, tasks, map, etc.)
- Manage presets from the in-game UI with Config/Game sub-tabs

### In-Game UI
- Sidebar panel accessible from a lobby button
- **Settings tab** — toggle all features, adjust thresholds, view/edit whitelist and blacklist
- **Presets tab** — save/load/delete config and game presets with sub-tabs
- Scrollable with viewport clipping

### Other
- **Whitelist** — make specific players immune to all checks
- **Blacklist** — permanently ban players by friend code (stored locally)
- **Google Sheets ban list** — hard-ban players using a shared spreadsheet
- **Auto-start** — automatically start when the lobby reaches a target player count
- **Verbose join notifications** — see player info (level, friend code, color) on join
- **Rules message** — configurable rules shown in local chat on lobby start
- **Lobby lock** — lock/unlock lobby via commands
- **Crash log** — crash-resistant file logger that survives freezes

## Requirements

- Among Us (Steam) v2026.3.31
- [BepInEx 6.0.0-be.755 (IL2CPP x86)](https://builds.bepinex.dev/projects/bepinex_be)
- [Reactor 2.5.0](https://github.com/NuclearPowered/Reactor/releases)

## Installation

1. Download and extract BepInEx into your Among Us folder (`Steam/steamapps/common/Among Us/`)
2. Launch Among Us once so BepInEx sets itself up, then close it
3. Download `Reactor.dll` from the Reactor releases page and drop it into `BepInEx/plugins/`
4. Download `HostGuard.dll` from the [latest release](https://github.com/RaresHonour/HostGuard/releases/latest) and drop it into `BepInEx/plugins/`
5. Launch Among Us — HostGuard loads automatically. Open the HostGuard panel in the lobby to configure.

## Commands

All commands have short aliases. Type `!help` in chat to see the full list.

### Player Management

| Command | Alias | Description |
|---|---|---|
| `!kick <name\|code>` | `!k` | Kick a player |
| `!ban <name\|code>` | `!b` | Ban a player |
| `!kickall` | `!ka` | Kick all players |
| `!info <name\|code>` | `!i` | Show player info |
| `!whitelist [name\|code]` | `!wl` | View whitelist or add a player |
| `!unwhitelist <name\|code>` | `!uwl` | Remove from whitelist |
| `!blacklist [name\|code]` | `!bl` | View blacklist or add a player |
| `!unblacklist <name\|code>` | `!ubl` | Remove from blacklist |

### Word Management

| Command | Alias | Description |
|---|---|---|
| `!addword <word>` | `!aw` | Add a banned chat word |
| `!removeword <word>` | `!rw` | Remove a banned chat word |
| `!words` | `!wds` | List all banned chat words |
| `!addname <word>` | `!an` | Add a bad name word |
| `!removename <word>` | `!rn` | Remove a bad name word |
| `!namelist` | `!nl` | List all bad name words |

### Toggles

| Command | Alias | Description |
|---|---|---|
| `!defaultnames on/off/ban/kick` | `!dn` | Default name filter |
| `!badnames on/off` | `!bn` | Bad name ban/kick toggle |
| `!badchat on/off` | `!bc` | Banned words ban/kick toggle |
| `!contains on/off` | `!cm` | Contains mode |
| `!botnames on/off` | `!bot` | Known bot ban/kick toggle |
| `!flood on/off` | `!fp` | Flood protection |
| `!anticheat on/off/ban/kick` | `!ac` | Anti-cheat |
| `!cosmetic on/off` | `!cos` | Cosmetic detection |
| `!autolock on/off` | `!al` | Auto-lock on flood |
| `!notify on/off` | `!n` | Verbose join notifications |
| `!autostart on/off/<n>` | `!as` | Auto-start |

### Other

| Command | Alias | Description |
|---|---|---|
| `!status` | `!s` | Show all current settings |
| `!help` | `!h` | Show all commands |
| `!rules` | `!r` | Show rules message |
| `!setrules <msg>` | `!sr` | Set rules message |
| `!lock` | `!lk` | Lock lobby (private) |
| `!unlock` | `!ulk` | Unlock lobby (public) |

**Note:** When using a player name, the player must be in the lobby. Friend codes (containing `#`) work even if the player isn't present.

## Ban List Setup

1. Create a Google Sheet with friend codes in column B (row 1 is the header)
2. Share it as "Anyone with the link can view"
3. Get the CSV export URL: `File > Share > Publish to web > CSV`
4. Paste the URL into the `BanListUrl` config setting

## File Locations

| File | Location |
|---|---|
| Config | `BepInEx/config/com.rareshonour.hostguard.cfg` |
| Blacklist | `BepInEx/config/hostguard_blacklist.txt` |
| Config Presets | `BepInEx/config/HostGuard_Presets/` |
| Game Presets | `BepInEx/config/HostGuard_GamePresets/` |
| Crash Log | `BepInEx/config/hostguard_crash.log` |

## License

MIT
