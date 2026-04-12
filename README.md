# HostGuard

A BepInEx/Reactor mod for Among Us that gives hosts better control over their lobbies.

## Features

- **Chat filter** — kick or ban players who type specific words in chat, with optional contains mode
- **Name filter** — kick players with offensive names on join
- **Default name detection** — automatically kick players using randomly generated default names (e.g. "Funnybone", "Sillyhawk")
- **Local blacklist** — permanently ban players by friend code, stored locally
- **Google Sheets ban list** — hard-ban players by friend code using a shared spreadsheet
- **Whitelist** — make specific players immune to all checks
- **Auto-start** — automatically start the game when the lobby reaches a target player count
- **Chat commands** — manage everything in real time without leaving the game (all commands have short aliases)

## Requirements

- Among Us (Steam, I don't know if works for Epic Games) v2026.3.17
- [BepInEx 6.0.0-be.755 (IL2CPP x86)](https://builds.bepinex.dev/projects/bepinex_be)
- [Reactor 2.5.0](https://github.com/NuclearPowered/Reactor/releases)

## Installation

1. Download and extract BepInEx into your Among Us folder (`Steam/steamapps/common/Among Us/`)
2. Launch Among Us once so BepInEx sets itself up, then close it
3. Download `Reactor.dll` from the Reactor releases page and drop it into `BepInEx/plugins/`
4. Download `HostGuard.dll` from the [latest release](https://github.com/RaresHonour/HostGuard/releases/latest) and drop it into `BepInEx/plugins/`
5. Launch Among Us — the config file will be generated automatically
6. Close the game and edit `BepInEx/config/com.rareshonour.hostguard.cfg` to configure

## Configuration

### Chat Filter

| Setting | Default | Description |
|---|---|---|
| `BannedWords` | `start` | Words that trigger a kick/ban. Comma-separated, case-insensitive. |
| `ContainsMode` | `false` | If true, triggers if message contains a banned word. If false, exact match only. |
| `BanInsteadOfKick` | `false` | If true, bans instead of kicks for banned words. |

### Name Filter

| Setting | Default | Description |
|---|---|---|
| `BadNameWords` | (list of slurs) | Players with these words in their name get kicked on join. |
| `BanForBadName` | `false` | If true, bans instead of kicks for bad names. |
| `KickDefaultNames` | `true` | If true, kicks players with randomly generated default names. |
| `BanForDefaultName` | `false` | If true, bans instead of kicks for default names. |
| `StrictDefaultNameCasing` | `true` | If true, only matches exact default casing (e.g. Funnybone). If false, matches any casing. |

### Auto-Start

| Setting | Default | Description |
|---|---|---|
| `Enabled` | `false` | If true, auto-starts when the lobby reaches the target player count. |
| `PlayerCount` | `0` | Target player count. 0 = use the lobby's max player setting. |

### Other

| Setting | Default | Description |
|---|---|---|
| `BanListUrl` | (empty) | Google Sheets CSV URL for hard bans by friend code. |
| `WhitelistedCodes` | (empty) | Friend codes immune to all checks. Managed via commands. |
| `SendRulesOnLobbyStart` | `true` | Shows a rules reminder in local chat when the lobby opens. |
| `RulesMessage` | (default message) | The rules message shown when the lobby opens. |

## Commands

All commands support both the full name and a short alias. Use `!help` or `!h` to see the full list in-game.

| Command | Alias | Description |
|---|---|---|
| `!kick <name\|code>` | `!k` | Kick a player by name or friend code |
| `!ban <name\|code>` | `!b` | Ban a player by name or friend code |
| `!kickall` | `!ka` | Kick all players from the lobby |
| `!info <name\|code>` | `!i` | Show player info (name, friend code, ID) |
| `!status` | `!s` | Show all current settings |
| `!help` | `!h` | Show all commands |
| `!rules` | `!r` | Show current rules message |
| `!setrules <msg>` | `!sr` | Set a new rules message |
| `!autostart on/off/<n>` | `!as` | Toggle auto-start or set player count |
| `!defaultnames on/off` | `!dn` | Toggle default name filter |
| `!defaultnames ban/kick` | `!dn` | Set default name action |
| `!badnames on/off` | `!bn` | Toggle ban (vs kick) for bad names |
| `!badchat on/off` | `!bc` | Toggle ban (vs kick) for banned words |
| `!contains on/off` | `!cm` | Toggle contains mode for chat filter |
| `!whitelist [name\|code]` | `!wl` | View whitelist or add a player |
| `!unwhitelist <name\|code>` | `!uwl` | Remove from whitelist |
| `!blacklist [name\|code]` | `!bl` | View blacklist or add a player |
| `!unblacklist <name\|code>` | `!ubl` | Remove from blacklist |

**Note:** When using a name, the player must be in the lobby. When using a friend code (contains `#`), the player doesn't need to be present.

## Ban List Setup

1. Create a Google Sheet with friend codes in column B (row 1 is the header)
2. Share it as "Anyone with the link can view"
3. Get the CSV export URL: `File → Share → Publish to web → CSV`
4. Paste the URL into `BanListUrl` in the config file

## Blacklist

The local blacklist is stored in `BepInEx/config/hostguard_blacklist.txt` (one friend code per line). Use `!blacklist` / `!bl` commands to manage it in-game, or edit the file directly.

## License

MIT
