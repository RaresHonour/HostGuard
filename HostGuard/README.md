# HostGuard

A BepInEx/Reactor mod for Among Us that gives hosts full control over their lobbies. Filters bad names, blocks cheaters, stops bot floods, and more — all configurable in-game or via config file.

## Features

### Name Filter
- Kick/ban players with offensive names (exact or substring match)
- Kick players with randomly generated default names (e.g. Funnybone)
- Strict or loose casing modes for default name detection

### Chat Filter
- Kick/ban players who type banned words (exact or substring match)
- Detect and ban bots that post known malicious URLs

### Bot Protection
- Auto-kick/ban known bot names (TNT, auser, Haunt Bot, etc.)
- Cosmetic crash prevention — blocks players with malformed outfit data

### Flood Protection
- Detect and block join flooding (rapid mass joins)
- Detect rapid join-leave attacks
- Auto-lock lobby during flood attacks with configurable cooldown
- Meeting spam detection and kick

### Anti-Cheat
- RPC validation — blocks kill/vent/shapeshift exploits in lobby and invalid role actions in-game
- Chat rate limiting (silently blocks spam without kicking)

### Minimum Level
- Require a minimum player level to join

### Auto-Start
- Auto-start when lobby reaches a target player count
- Auto-start when lobby countdown timer is about to expire

### Lobby Management
- Auto-return to lobby after game ends (skips post-game screens)
- Lock/unlock lobby via commands
- Rules message shown to host on lobby start
- Toast notifications when HostGuard takes action

### Whitelist & Blacklist
- Whitelist players by friend code (immune to all checks)
- Persistent blacklist (survives game restarts)
- Auto-blacklist option for every violation type
- Google Sheets ban list integration for shared community bans

### Presets
- Save/load HostGuard config presets
- Save/load game option presets (speed, kill cooldown, tasks, etc.)

### In-Game Settings Panel
- Full settings UI accessible from a lobby button
- Toggle every feature, adjust thresholds, manage lists — no config file editing needed

## Requirements

- Among Us (Steam) v2026.9.29+
- [BepInEx 6.0.0-be.791 (IL2CPP x64)](https://builds.bepinex.dev/projects/bepinex_be/791/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.791%2Bb782da4.zip)
- [Reactor 2.5.1+](https://github.com/NuclearPowered/Reactor/releases)

## Installation

1. Uninstall Among Us, delete the Among Us folder, and reinstall through Steam
2. Download and extract BepInEx into your Among Us folder (where `Among Us.exe` is)
3. Launch Among Us once so BepInEx sets itself up, then close it
4. Download `Reactor.dll` from the Reactor releases page and drop it into `BepInEx/plugins/`
5. Download `HostGuard.dll` from the [latest release](https://github.com/RaresHonour/HostGuard/releases/latest) and drop it into `BepInEx/plugins/`
6. Launch Among Us — you should see "HostGuard 3.2.1" on the main menu

Most settings can be changed in-game using the **HostGuard** button in the lobby. You can also edit `BepInEx/config/com.rareshonour.hostguard.cfg` directly.

## Commands

All commands are typed in lobby chat and only visible to the host.

### Player Management
| Command | Alias | Description |
|---|---|---|
| `!kick <name\|code>` | `!k` | Kick a player |
| `!ban <name\|code>` | `!b` | Ban a player (can't rejoin) |
| `!kickall` | `!ka` | Kick all players |
| `!info <name\|code>` | `!i` | Show player info (name, friend code, ID) |

### Whitelist
| Command | Alias | Description |
|---|---|---|
| `!whitelist` | `!wl` | Show all whitelisted players |
| `!whitelist <name\|code>` | `!wl` | Add player to whitelist |
| `!unwhitelist <name\|code>` | `!uwl` | Remove player from whitelist |

### Blacklist
| Command | Alias | Description |
|---|---|---|
| `!blacklist` | `!bl` | Show all blacklisted players |
| `!blacklist <name\|code>` | `!bl` | Add player to blacklist |
| `!unblacklist <name\|code>` | `!ubl` | Remove player from blacklist |

### Lobby
| Command | Alias | Description |
|---|---|---|
| `!lock` | `!lk` | Lock lobby (set to private) |
| `!unlock` | `!ulk` | Unlock lobby (set to public) |
| `!autolock on/off` | `!al` | Toggle auto-lock on flood |
| `!notify on/off` | `!n` | Toggle verbose join notifications |

### Word & Name Lists
| Command | Alias | Description |
|---|---|---|
| `!addword <word>` | `!aw` | Add a banned chat word |
| `!removeword <word>` | `!rw` | Remove a banned chat word |
| `!words` | `!wds` | Show all banned chat words |
| `!addname <word>` | `!an` | Add a banned name |
| `!removename <word>` | `!rn` | Remove a banned name |
| `!namelist` | `!nl` | Show all banned names |

### Other
| Command | Alias | Description |
|---|---|---|
| `!rules` | `!r` | Show rules message |
| `!setrules <msg>` | `!sr` | Update rules message |
| `!status` | `!s` | Show all HostGuard settings |
| `!help` | `!h` | Show all commands |

## Ban List Setup

1. Create a Google Sheet with friend codes in column B (row 1 is the header)
2. Share it as "Anyone with the link can view"
3. Get the CSV export URL: `File → Share → Publish to web → CSV`
4. Paste the URL into the Ban List URL setting (in-game or config file)

The ban list is cached for 5 minutes and refreshed automatically.

## License

MIT
