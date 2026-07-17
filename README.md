# SCPSL-MicroServer-Tweaks

> **SCP:SL 14.x** micro-server plugin — makes 3–12 player rounds actually fun  
> Built on **LabAPI**, zero bloat, one config file.

[![Game](https://img.shields.io/badge/SCP%3ASL-14.x-blue.svg)](#)
[![LabAPI](https://img.shields.io/badge/LabAPI-powered-green.svg)](#)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.8-purple.svg)](#)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](#)
[![Downloads](https://img.shields.io/github/downloads/Neverwin1337/SCPSL-MicroServer-Tweaks/total.svg)](https://github.com/Neverwin1337/SCPSL-MicroServer-Tweaks/releases)

---

## 🌐 Localized READMEs

| Language | Link |
|----------|------|
| 🇨🇳 简体中文 | [README.zh-CN.md](./README.zh-CN.md) |
| 🇹🇼 繁體中文 | [README.zh-TW.md](./README.zh-TW.md) |
| 🇯🇵 日本語 | [README.ja.md](./README.ja.md) |
| 🇪🇸 Español | [README.es.md](./README.es.md) |


## ⭐ Quick Start

**30 seconds to try it:**

1. Install [RueI](https://github.com/pawslee/RueI/releases/latest) (hint framework) — drop `RueI.dll` into `LabAPI/plugins/global/`
2. Grab `SCPSL_MicroServer_Tweaks.dll` from [Releases](../../releases)
3. Drop it into `LabAPI/plugins/global/`
4. Start your server — you're done 🎉

> Want to build from source? See [Build](#-build) below.

---

## ✨ Features

### 🧊 SCP Spawn Freeze

SCPs are held at their spawn position for a configurable duration after round start. They can still look around and use voice chat — **they just can't move.**

- Per-player-count duration table (5 players → 75s, 8 players → 40s, etc.)
- SCP-079 automatically excluded
- Optional on-screen countdown hint

**Result:** Humans have time to scatter. SCPs get to actually *play* the early round.

---

### 🎟️ Starting Respawn Tokens

Control how many reinforcement waves NTF and Chaos start with.

- `Set` mode — replace the vanilla value
- `Add` mode — add on top of the vanilla value
- Per-faction control; `-1` leaves a faction untouched

**Result:** No more rounds that stall out because nobody can respawn.

---

### 🗳️ Lobby Role Voting

During the lobby countdown, players type `.1` `.2` `.3` `.4` or `.vote scp/sci/d/guard` in the game console to vote for their preferred role.

- **Real-time on-screen hint** showing remaining time, vote counts, and instructions
- Countdown and vote counts update every second — no stale info
- Non-voters get assigned randomly

**Result:** Friend groups decide who plays what. No more race-to-claim-SCP.

---

### 🎲 Random Events

Every 3–5 minutes, a random event fires to keep rounds dynamic. All announcements use **C.A.S.S.I.E.** voice with custom pitch & glitch effects.

- **Elevator Malfunction** — All elevators locked for 60s, with countdown hint
- **All Doors Open** — Every facility door forced open (permanent)
- **Stealth Protocol** — All humans get true invisibility (SCP-268 effect) for 30s, with countdown hint
- **Blackout** — All three zones lights flicker (5s off / 1s on) for 3 min, with countdown hint
- **Nuke Alert** — 50% false alarm / 50% real detonation, 60s countdown (min 10 min into round)
- **Scramble** — All humans randomly teleported; LCZ rooms excluded if decontaminated

Use `mst_event <elevator|doors|stealth|blackout|nuke|scramble>` in RA / console to test manually.

---

### 🔑 Remote Keycard

Players can open doors, generators, SCP lockers, and the warhead panel **without equipping a keycard** — as long as they have the right keycard somewhere in their inventory.

- Works on doors, generators, warhead panel, and SCP lockers (each toggleable)
- Amnesia effect blocks usage (configurable)
- No need to hold the keycard in hand — just have it in your inventory

**Result:** Less fumbling in your inventory during intense moments.

---

### ⏱️ Respawn Countdown Hint

Spectators see a live countdown to the next respawn wave.

- Shows the next spawning team (NTF or Chaos) and seconds remaining
- Updates every second
- Only visible to spectators (dead players)
- Displayed at the bottom of the screen

**Result:** Spectators know exactly when they'll respawn — no more guessing.

---

### 🪶 Lightweight & Clean

- **Minimal dependencies** — only [RueI](https://github.com/pawslee/RueI) for hint display; no EXILED, MEC, or NWPluginAPI
- References only the DLLs shipped with **your own server**
- One `.dll`, one `config.yml`, nothing else

---

## ⚙️ Configuration

`config.yml` is auto-generated on first run. Sensible defaults for 5–8 players:

```yaml
# SCP freeze
enable_scp_freeze: true
default_scp_freeze_seconds: 60
scp_freeze_seconds_by_player_count:
  5: 75
  6: 60
  7: 45
  8: 40

# Respawn tokens
starting_token_mode: Set
ntf_starting_tokens: 2
chaos_starting_tokens: 2

# Role voting
enable_role_voting: true
voting_time_seconds: 45
vote_hint_interval_seconds: 1

# Random events
enable_random_events: true
random_event_min_interval_seconds: 180
random_event_max_interval_seconds: 300
random_event_min_players: 1
random_event_elevator_lock_duration: 60
random_event_stealth_duration: 30
random_event_blackout_duration: 180
random_event_nuke_countdown_seconds: 60
random_event_nuke_false_alarm_chance: 0.5
random_event_nuke_min_minutes: 10

# Remote keycard
enable_remote_keycard: true
remote_keycard_affect_doors: true
remote_keycard_affect_generators: true
remote_keycard_affect_warhead_panel: true
remote_keycard_affect_scp_lockers: true

# Respawn hint
enable_respawn_hint: true
respawn_hint_format: "<size=20><color=#aaaaaa>距離 {0} 重生還有 {1} 秒</color></size>"
respawn_hint_ntf_name: "NTF"
respawn_hint_chaos_name: "混沌分裂者"
```

Full example at [`config.example.yml`](./config.example.yml).

---

## 🔧 Build

### Windows (PowerShell)

```powershell
.\build.ps1 -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server"
```

### Linux / macOS (bash)

```bash
chmod +x ./build.sh
./build.sh --server-path "/home/steam/SCP Secret Laboratory Dedicated Server"
```

Add `-Deploy` / `--deploy` to auto-install to the plugins folder.

---

## 📦 Requirements

- SCP:SL 14.x dedicated server
- LabAPI installed
- .NET SDK (build only — just grab the DLL from Releases if you don't want to build)

---

## 🤝 Contributing

PRs welcome. One rule: **no EXILED / Harmony / MEC dependencies.**

---

## 📄 License

[MIT](./LICENSE)

---

**⭐ If this plugin makes your server better, give it a star!**