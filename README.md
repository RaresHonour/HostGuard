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

- Among Us v2026.3.17
- [BepInEx 6.0.0-be.755 (IL2CPP x86)](https://builds.bepinex.dev/projects/bepinex_be)
- [Reactor 2.5.0](https://github.com/NuclearPowered/Reactor)

## Installation

1. Install BepInEx and Reactor
2. Drop `HostGuard.dll` into `Among Us/BepInEx/plugins/`
3. Launch the game once to generate the config file
4. Edit `BepInEx/config/com.radoo.hostguard.cfg` to configure

## Commands

| Command | Description |
|---|---|
| `!allow <name>` | Whitelist a player currently in the lobby |
| `!remove <name>` | Remove a player from the whitelist |
| `!allowcode <CODE#XXXX>` | Whitelist a friend code directly |
| `!removecode <CODE#XXXX>` | Remove a friend code from the whitelist |

## Ban List Setup

1. Create a Google Sheet with friend codes in column B
2. Share it as "Anyone with the link can view"
3. Export URL: `File > Share > Publish to web` → CSV format
4. Paste the URL into `BanListUrl` in the config file

## License

MIT
