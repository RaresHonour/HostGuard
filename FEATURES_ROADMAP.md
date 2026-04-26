# HostGuard Feature Roadmap

## Priority Features (Poll Winners)

### 1. Anti-Cheat Detection System (4 votes) — HARDEST
- Monitor player behavior in real-time during gameplay
- Speed hacks: track position updates and calculate velocity
- Task completion: monitor task progress timing
- Kill range/cooldown: validate kill events against game settings
- Vision hacks: harder to detect server-side (client-rendered)
- False positives are a concern — laggy players might look like cheaters
- Some cheats fundamentally undetectable from host side (P2P architecture)
- Needs extensive testing and threshold tuning

### 2. Game Settings Presets (3 votes) — EASIEST
- Save/load entire game settings (roles, cooldowns, vision, tasks, etc.)
- Among Us already has a presets tab — extend that existing UI
- Make presets more useful by integrating HostGuard config into it

### 3. In-Game Settings UI (2 votes) — MEDIUM
- Hook into the existing Among Us presets/settings tab
- Add HostGuard configuration options directly in-game
- Toggles, dropdowns, text fields for each config option
- Replace the need to manually edit the config file
- Unity UI (IL2CPP) — need to figure out how the game builds its tabs

**Suggested implementation order:** Presets + UI together (since they share the same tab), then Anti-Cheat last

---

## All Brainstormed Features

1. Game settings presets (save/load)
2. Anti-cheat detection system
3. Player reputation/trust tracking
4. Custom game modes framework
5. Lobby announcements to all players
6. Player queue & VIP slots
7. Cross-game statistics & leaderboards
8. In-game polls & vote-kick
9. Discord webhook integration
10. Scheduled auto-lobby creation
11. In-game settings UI tab
12. Map-specific auto-configurations
13. Temporary bans with expiry timers
14. Player notes/tags for hosts
15. Auto-assign colors/hats on join
16. Custom chat commands for players (like !rules anyone can type)
17. Anti-spam/flood chat limiter
18. Lobby password protection system
19. Game event log/replay export
20. Multi-host permission sharing

---

## Notes
- Poll was run in an Among Us Discord server
- The presets tab already exists in Among Us — plan is to extend it rather than create a new tab
- Anti-cheat is most requested but hardest to implement
- Settings UI is "super necessary" even with fewer votes
