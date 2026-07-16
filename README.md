# SmallVanillaFlow

> **SCP:SL 14.x** micro-server plugin — makes 3–12 player rounds actually fun  
> Built on **LabAPI**, zero bloat, one config file.

[![Game](https://img.shields.io/badge/SCP%3ASL-14.x-blue.svg)](#)
[![LabAPI](https://img.shields.io/badge/LabAPI-powered-green.svg)](#)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.8-purple.svg)](#)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](#)

---

## 🌐 Localized READMEs

| Language | Link |
|----------|------|
| 🇨🇳 简体中文 | [README.zh-CN.md](./README.zh-CN.md) |
| 🇹🇼 繁體中文 | [README.zh-TW.md](./README.zh-TW.md) |
| 🇯🇵 日本語 | [README.ja.md](./README.ja.md) |
| 🇪🇸 Español | [README.es.md](./README.es.md) |

---

## 👋 The Problem

Small servers have a pacing problem:

> SCPs get insta-killed before they can move. Reinforcements never arrive (or arrive too late). Rounds end before they begin.

**SmallVanillaFlow** fixes the early game with three focused features. No EXILED, no Harmony, no MEC — just drop the DLL and go.

---

## ⭐ Quick Start

**30 seconds to try it:**

1. Grab `SmallVanillaFlow.dll` from [Releases](../../releases)
2. Drop it into `LabAPI/plugins/global/`
3. Start your server — you're done 🎉

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

### 🪶 Lightweight & Clean

- **Zero dependencies** — no EXILED, Harmony, MEC, or NWPluginAPI
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