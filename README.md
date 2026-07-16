# SCPSL-MicroServer-Tweaks

> A lightweight **SCP: Secret Laboratory 14.x** server plugin built on **LabAPI**, tuned for **micro / low-pop servers** (≈ 3–12 players).

[![Game](https://img.shields.io/badge/SCP%3ASL-14.x-blue.svg)](#)
[![LabAPI](https://img.shields.io/badge/LabAPI-required-green.svg)](#)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.8-purple.svg)](#)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)](#)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](#)

When a server only has a handful of players, vanilla SCP:SL pacing falls apart — rounds end in seconds, the SCP never gets a chance to move, and reinforcement waves arrive either too early or not at all. **SCPSL-MicroServer-Tweaks** smooths out the early game on small servers with two focused, configurable features and **zero fluff**.

## 🌐 Languages / 语言 / 言語

| Language | README |
|----------|--------|
| 🇬🇧 English (default) | [README.md](./README.md) |
| 🇨🇳 简体中文 | [README.zh-CN.md](./README.zh-CN.md) |
| 🇹🇼 繁體中文 | [README.zh-TW.md](./README.zh-TW.md) |


> Tip: GitHub also shows a language picker in the repo header once multiple `README.*.md` files are present.

---

## 📖 Quick start tutorial

Follow these five steps to get the plugin running in about 5 minutes. The tutorial covers **both Windows and Linux** — pick the block that matches your OS.

### Step 1 — Prerequisites

| Requirement | Windows | Linux / macOS |
|-------------|---------|---------------|
| SCP:SL 14.x dedicated server | ✅ | ✅ |
| LabAPI installed and working | ✅ | ✅ |
| .NET SDK with **.NET Framework 4.8** targeting | ✅ | ✅ (use `net48` reference assemblies) |
| Shell for the build script | **PowerShell** | **bash** (or zsh) |

If you only want to **run** the plugin (not build from source), skip the .NET SDK and just grab a prebuilt DLL from [Releases](../../releases).

### Step 2 — Get the plugin

- **Build it yourself** — see [Build](#-build) below
- **Or** download the prebuilt DLL from the [Releases](../../releases) page

### Step 3 — Install

Copy `SmallVanillaFlow.dll` into your LabAPI **global** plugins folder:

| OS | Plugin path |
|----|-------------|
| Windows | `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\` |
| Linux   | `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/` |
| macOS   | `~/Library/Application Support/SCP Secret Laboratory/LabAPI/plugins/global/` |

> 💡 On a headless dedicated server box, the user is usually the one that runs the SCPSL service (e.g. `steam` on a SteamCMD install).


### Step 5 — Launch and tune

1. Start the server. On first launch, LabAPI auto-generates a `config.yml` next to the plugin DLL.
2. Open that `config.yml` and review the defaults (they already work well for 5–8 player servers).
3. Want a longer freeze for tiny 3-player rounds, or more NTF tokens? Tweak `scp_freeze_seconds_by_player_count` and `ntf_starting_tokens` — see [Configuration](#-configuration).
4. **Restart the server** so the new config takes effect.

Done. 🎉 Join with a few friends, watch the SCPs spawn-frozen for a moment, and the round should feel much more playable.

---

## ✨ Features

- 🧊 **Per-player-count SCP freeze at spawn**
  - All initial movable SCPs are held at their spawn position for a configurable duration.
  - SCPs can still look around and use voice chat while frozen.
  - SCP-079 is automatically excluded (no spawn position).
  - Per-player-count override table (e.g. longer freeze for 4-player rounds, shorter for 10+).
  - Optional on-screen countdown hint for the frozen SCPs.

- 🎟️ **Reinforcement wave tokens**
  - Configure starting **primary-wave Respawn Tokens** for **Nine-Tailed Fox** and **Chaos Insurgency** / Foundation Enemy.
  - Two modes:
    - `Set` — replace the vanilla starting value.
    - `Add` — add on top of the vanilla value.
  - Per-faction control, with `-1` to leave a faction untouched.

- 🪶 **Lightweight by design**
  - No EXILED, Harmony, MEC, or NWPluginAPI dependency.
  - Position locking runs on Unity's main thread.
  - Only references the DLLs shipped with **your own current server** — no frozen DLL packs.

- 🗳️ **Lobby role voting**
  - Players can vote for their desired starting role during the lobby countdown by typing `.1` `.2` `.3` `.4` or `.vote scp/sci/d/guard` in the game console.
  - Real-time vote counts shown via on-screen hints.
  - Configurable voting duration and early-end threshold (e.g. 75% voted → shorten lobby timer).
  - Role assignment intercepts the initial spawn to apply votes; non-voters get a random role.

---

## 📋 Recommended opening role queue

A sane starting composition for low-pop servers:

| Players | SCPs | Guards | Class-D | Scientists |
|---------|:----:|:------:|:-------:|:----------:|
| 3       | 1    | 1      | 1       | 0          |
| 4       | 1    | 1      | 2       | 0          |
| 5       | 1    | 1      | 3       | 0          |
| 6       | 1    | 1      | 4       | 0          |
| 7       | 1    | 1      | 5       | 0          |
| 8       | 1    | 1      | 5       | 1          |
| 10      | 1    | 2      | 6       | 1          |
| 12      | 1    | 2      | 7       | 1          |

Set this on your server:

```yaml
team_respawn_queue: 40144443
```

---

## 📦 Requirements

- **SCP: Secret Laboratory** dedicated server — current 14.x
- **LabAPI** installed
- **.NET SDK** with **.NET Framework 4.8** targeting support
- **PowerShell** (Windows) **or** **bash** (Linux / macOS) to run the build script

> ⚠️ Linux note: building against `net48` from Linux requires the **Microsoft.NETFramework.ReferenceAssemblies.net48** package, which `dotnet` can restore automatically on Linux. If `dotnet build` complains, run `dotnet restore` once and try again.

---

## 🔧 Build

### 🪟 Windows (PowerShell)

Open PowerShell in the repository folder:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build.ps1 `
  -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server"
```

The script copies only these current server assemblies:

- `Assembly-CSharp.dll`
- `Mirror.dll`
- `CommandSystem.Core.dll`
- `LabApi.dll` / `LabAPI.dll`
- `UnityEngine.CoreModule.dll`

It deliberately does **not** look for `MEC.dll` or `Assembly-CSharp-firstpass.dll`.

The compiled plugin is written to:

```text
SmallVanillaFlow\bin\Release\net48\SmallVanillaFlow.dll
```

#### Build + deploy in one step (Windows)

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build.ps1 `
  -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server" `
  -Deploy
```

This drops the DLL into `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\`.

---

### 🐧 Linux / macOS (bash)

Make the script executable once:

```bash
chmod +x ./build.sh
```

Then build:

```bash
./build.sh \
  --server-path "/home/<user>/SCP Secret Laboratory Dedicated Server"
```

Typical path on a SteamCMD install: `/home/steam/SCP Secret Laboratory Dedicated Server`.

#### Build + deploy in one step (Linux / macOS)

```bash
./build.sh \
  --server-path "/home/steam/SCP Secret Laboratory Dedicated Server" \
  --deploy
```

This drops the DLL into `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/`.

#### Override the plugin folder

```bash
./build.sh \
  --server-path "/opt/scpsl" \
  --deploy \
  --plugin-dir "/opt/scpsl/LabAPI/plugins/global"
```

Run `./build.sh --help` for the full flag list.

---

## 🚀 Installation (manual)

If you don't use `-Deploy` / `--deploy`:

1. Build the plugin (see above) **or** grab a release DLL.
2. Copy the DLL into the LabAPI global plugins folder:

   | OS | Path |
   |----|------|
   | Windows | `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\` |
   | Linux   | `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/` |
   | macOS   | `~/Library/Application Support/SCP Secret Laboratory/LabAPI/plugins/global/` |
3. Start the server once — LabAPI will generate `config.yml` next to the plugin DLL.
4. Edit `config.yml` to taste, then restart the server.

### Running the SCP:SL server itself on Linux

The official **SCP:SL dedicated server** ships as Windows-only. Most Linux hosts run it through:

- **Wine** + `winetricks` (recommended for casual setups)
- **Proton** (via Steam Play)
- A community **Docker** image such as [`ghcr.io/serverbp/scp-sl-server`](https://github.com/ServerBp/scp-sl-server)

Whichever path you use:

1. Install LabAPI **inside the Wine/Proton/Docker prefix** the same way you would on Windows.
2. The plugin DLL goes into that prefix's `…/SCP Secret Laboratory/LabAPI/plugins/global/` folder.
3. Build the plugin **on Linux** with `build.sh` — its output is platform-independent (managed .NET DLL).

---

## ⚙️ Configuration

A ready-to-copy example is included as [`config.example.yml`](./config.example.yml).

Default behavior:

```yaml
enable_scp_freeze: true
default_scp_freeze_seconds: 60
use_player_count_overrides: true

scp_freeze_seconds_by_player_count:
  5: 75
  6: 60
  7: 45
  8: 40

freeze_capture_delay_seconds: 0.5
position_tolerance: 0.03

show_countdown_hint: true
countdown_hint_interval_seconds: 1
countdown_hint_text: |-
  <size=32><color=#ff5555>Containment procedure in progress</color>
  <color=white>Containment breach in: {seconds} s</color></size>
release_hint_text: |-
  <size=32><color=#ff5555>Containment failed</color>
  <color=white>You may now move.</color></size>
release_hint_duration_seconds: 4

enable_starting_tokens: true
starting_token_mode: Set    # Set = replace vanilla, Add = add on top
ntf_starting_tokens: 2      # -1 = leave this faction unchanged
chaos_starting_tokens: 2
maximum_token_value: 20

enable_debug_logging: false

# Role Voting
enable_role_voting: true
voting_time_seconds: 45
```

### What is a "Respawn Token"?

A Respawn Token represents **one primary reinforcement wave**, not one individual player. Mini-waves are not modified.

### Suggested first test

```yaml
starting_token_mode: Set
ntf_starting_tokens: 2
chaos_starting_tokens: 2
```

If rounds still end before a useful reinforcement cycle, bump both values to `3`. Avoid very high values — DMS (Dynamic Map Selection) will be delayed until all reinforcement resources are spent.

---

## 📝 Notes

- Tokens are applied during `WaitingForPlayers`, matching the 14.x wave initialization lifecycle.
- The plugin updates both the configured/base token count and the remaining-token count, then sends the wave token update to connected clients.
- If another plugin also overwrites starting tokens, **load order** determines the final value.
- This source project targets the LabAPI API exposed by **your** current server assemblies — rebuild after a major SCP:SL or LabAPI update.

---

## 🤝 Contributing

PRs welcome. Please:

1. Keep dependencies minimal — no EXILED / Harmony / MEC.
2. Don't bundle server DLLs in the repo.
3. Rebuild against your own 14.x server before opening a PR.
4. Describe **why** the change helps low-pop servers, not just *what* it does.

---

## 📄 License

[MIT](./LICENSE)
