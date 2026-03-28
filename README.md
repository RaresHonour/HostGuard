# HostGuard

A BepInEx/Reactor mod for Among Us that gives hosts better control over their lobbies.

## Features

- **Keyword autoban** — kick or ban players who type specific words in chat
- **Contains mode** — trigger on messages that contain a banned word, not just exact matches
- **Name filter** — kick players with offensive names on join
- **Google Sheets ban list** — hard-ban players by friend code using a shared spreadsheet
- **Whitelist** — make specific players immune to all checks
- **Chat commands** — manage the whitelist in real time without leaving the game

## Requirements

- Among Us (Steam) v2026.3.17
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

| Setting | Default | Description |
|---|---|---|
| `BannedWords` | `start,begin,go,play` | Words that trigger a kick. Comma-separated. |
| `ContainsMode` | `false` | If true, kicks if message contains the word. If false, exact match only. |
| `BanInsteadOfKick` | `false` | If true, bans instead of kicks (can't rejoin). |
| `AnnounceKick` | `true` | Logs a message when someone is kicked. |
| `BadNameWords` | (list of slurs) | Players with these words in their name get kicked on join. |
| `BanListUrl` | (empty) | Google Sheets CSV URL for hard bans by friend code. |
| `WhitelistedCodes` | (empty) | Friend codes immune to all checks. Managed via commands. |

## Commands

| Command | Description |
|---|---|
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
