# HostGuard

A BepInEx/Reactor mod for Among Us that gives hosts better control over their lobbies.

## Features

- **Chat filter** — kick or ban players who type specific words in chat, with optional contains mode
- **Name filter** — kick players with offensive names on join
- **Default name detection** — automatically kick players using randomly generated default names (e.g. "Funnybone", "Sillyhawk")
- **Google Sheets ban list** — hard-ban players by friend code using a shared spreadsheet
- **Whitelist** — make specific players immune to all checks
- **Chat commands** — manage settings, kick/ban players, and control filters in real time without leaving the game

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

### Other

| Setting | Default | Description |
|---|---|---|
| `BanListUrl` | (empty) | Google Sheets CSV URL for hard bans by friend code. |
| `WhitelistedCodes` | (empty) | Friend codes immune to all checks. Managed via commands. |
| `SendRulesOnLobbyStart` | `true` | Shows a rules reminder in local chat when the lobby opens. |
| `RulesMessage` | (default message) | The rules message shown when the lobby opens. |

## Commands

| Command | Description |
|---|---|
| `!help` | Show all commands |
| `!status` | Show current settings |
| `!kick <name\|code>` | Kick a player by name or friend code |
| `!ban <name\|code>` | Ban a player by name or friend code |
| `!defaultnames on/off` | Toggle default name filter |
| `!defaultnames ban/kick` | Set default name action (ban or kick) |
| `!badnames on/off` | Toggle ban (vs kick) for bad names |
| `!badchat on/off` | Toggle ban (vs kick) for banned words in chat |
| `!contains on/off` | Toggle contains mode for chat filter |
| `!rules` | Show current rules message |
| `!setrules <msg>` | Set a new rules message |
| `!whitelist` | Show all whitelisted friend codes |
| `!allow <name>` | Whitelist a player currently in the lobby by name |
| `!remove <name>` | Remove a player from the whitelist by name |
| `!allowcode <CODE#XXXX>` | Whitelist a friend code directly |
| `!removecode <CODE#XXXX>` | Remove a friend code from the whitelist |

## Ban List Setup

1. Create a Google Sheet with friend codes in column B (row 1 is the header)
2. Share it as "Anyone with the link can view"
3. Get the CSV export URL: `File → Share → Publish to web → CSV`
4. Paste the URL into `BanListUrl` in the config file

## License

MIT
