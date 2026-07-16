# SCPSL-MicroServer-Tweaks

> 一個輕量、基於 **LabAPI** 的 **SCP: Secret Laboratory 14.x** 伺服器外掛,專為**人數少的微服** (≈ 3–12 位玩家) 調校。

[![Game](https://img.shields.io/badge/SCP%3ASL-14.x-blue.svg)](#)
[![LabAPI](https://img.shields.io/badge/LabAPI-required-green.svg)](#)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.8-purple.svg)](#)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)](#)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](#)

在玩家不多的伺服器上,原版 SCP:SL 的節奏會崩——回合幾秒就結束、SCP 還沒走兩步就要被圍毆、增援不是太早就是太晚。**SCPSL-MicroServer-Tweaks** 用兩個小而精的可調功能,把小服的早期遊戲體驗救回來,而且**沒有任何多餘的依賴**。

## 🌐 語言 / Languages / 言語

| 語言 | README |
|------|--------|
| 🇬🇧 English (default) | [README.md](./README.md) |
| 🇨🇳 简体中文 | [README.zh-CN.md](./README.zh-CN.md) |
| 🇹🇼 繁體中文 (目前) | [README.zh-TW.md](./README.zh-TW.md) |
| 🇯🇵 日本語 | [README.ja.md](./README.ja.md) |
| 🇪🇸 Español | [README.es.md](./README.es.md) |
| 🇧🇷 Português (Brasil) | [README.pt-BR.md](./README.pt-BR.md) |

> 小提示:GitHub 在偵測到多個 `README.*.md` 時,會在倉庫頂部自動顯示語言切換器。

---

## 📖 快速上手教學 (5 分鐘)

照著下面五步走,**Windows 與 Linux 都有**,挑對應你系統的部分看。

### 第一步 — 準備環境

| 需求 | Windows | Linux / macOS |
|------|---------|---------------|
| SCP:SL 14.x 專用伺服器 | ✅ | ✅ |
| 已安裝且可運作的 LabAPI | ✅ | ✅ |
| 含 **.NET Framework 4.8** 目標的 .NET SDK | ✅ | ✅ (需要 `net48` reference assemblies) |
| 編譯用的 Shell | **PowerShell** | **bash**(或 zsh) |

如果只想**直接用現成的外掛 DLL**(不自己編譯),可以跳過 .NET SDK,直接去 [Releases](../../releases) 下載即可。

### 第二步 — 取得外掛

- **自己編譯** — 見下方 [Build](#-編譯)
- **或** 從 [Releases](../../releases) 下載已編譯好的 DLL

### 第三步 — 安裝

把 `SmallVanillaFlow.dll` 複製到 LabAPI **global** 外掛目錄:

| 系統 | 路徑 |
|------|------|
| Windows | `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\` |
| Linux   | `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/` |
| macOS   | `~/Library/Application Support/SCP Secret Laboratory/LabAPI/plugins/global/` |

> 💡 在無頭專用伺服器上,這裡的「使用者」通常是跑 SCPSL 服務的那個帳號(例如 `steam`)。

### 第四步 — 設定開場角色佇列

打開伺服器的 `config_gameplay.txt`(或 `config.txt`),加入:

```yaml
team_respawn_queue: 40144443
```

這正好對應下面的「推薦角色配置」表格。

### 第五步 — 啟動伺服器並微調

1. 啟動伺服器。第一次啟動時,LabAPI 會自動在外掛 DLL 旁邊產生 `config.yml`。
2. 打開 `config.yml` 看一下預設值(5–8 人伺服器上預設值已經夠用)。
3. 想要在 3 人小房裡凍結更久?想要給 NTF 更多增援?改 `scp_freeze_seconds_by_player_count` 與 `ntf_starting_tokens`,詳見 [Configuration](#-配置說明)。
4. **重啟伺服器**,新組態才會生效。

搞定。🎉 拉幾個朋友進來,你會看到 SCP 在出生點短暫「凍結」幾秒,整局體驗順暢不少。

---

## ✨ 功能特色

- 🧊 **依玩家數凍結 SCP 出生點**
  - 回合開始時,所有可移動 SCP 在出生位置保持靜止一段可設定的時間。
  - 凍結期間,SCP 仍可轉頭觀察、用語音聊天。
  - SCP-079 自動排除(沒有正常出生位置)。
  - 依玩家數設定不同的凍結時長(例如 4 人局凍結久一些,10+ 人局凍結短一些)。
  - 可選:在被凍結的 SCP 螢幕上顯示倒數計時提示。

- 🎟️ **增援波次 Token**
  - 設定 **Nine-Tailed Fox** 與 **Chaos Insurgency / 基金會敵對勢力** 起始的**主增援波次 Token**。
  - 兩種模式:
    - `Set` — 直接覆寫原版初始值。
    - `Add` — 在原版基礎上再加。
  - 支援依陣營控制,`-1` 表示不動該陣營。

- 🪶 **極簡設計**
  - 不依賴 EXILED、Harmony、MEC、NWPluginAPI。
  - 位置鎖定走 Unity 主執行緒。
  - 只引用**你自己當前伺服器**的 DLL,沒有老版本 DLL 包。

- 🗳️ **大廳身份投票**
  - 在大廳倒數期間玩家可以在控制台輸入 `.1` `.2` `.3` `.4` 或 `.vote scp/sci/d/guard` 投票選擇開局身份。
  - 透過螢幕提示即時顯示各角色票數。
  - 可設定投票時長與提前截止門檻（如 75% 投完即加速）。
  - 角色分配在回合開始時攔截原始分配來套用投票結果，未投票者隨機分配。

---

## 📋 推薦開場角色配置

適用於人少伺服器的開局組合:

| 玩家數 | SCP | 守衛 | D 級人員 | 科學家 |
|--------|:---:|:----:|:--------:|:------:|
| 3      | 1   | 1    | 1        | 0      |
| 4      | 1   | 1    | 2        | 0      |
| 5      | 1   | 1    | 3        | 0      |
| 6      | 1   | 1    | 4        | 0      |
| 7      | 1   | 1    | 5        | 0      |
| 8      | 1   | 1    | 5        | 1      |
| 10     | 1   | 2    | 6        | 1      |
| 12     | 1   | 2    | 7        | 1      |

在伺服器上設定:

```yaml
team_respawn_queue: 40144443
```

---

## 📦 依賴

- **SCP: Secret Laboratory** 專用伺服器 — 當前 14.x
- **LabAPI** 已安裝
- 含 **.NET Framework 4.8** 目標的 **.NET SDK**
- 編譯用 Shell:**PowerShell**(Windows)**或** **bash**(Linux / macOS)

> ⚠️ Linux 小提醒:在 Linux 上編譯 `net48` 需要 `Microsoft.NETFramework.ReferenceAssemblies.net48` 套件,`dotnet` 一般會自動還原。如果 `dotnet build` 報錯,先跑一次 `dotnet restore` 再試。

---

## 🔧 編譯

### 🪟 Windows (PowerShell)

在倉庫目錄開啟 PowerShell:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build.ps1 `
  -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server"
```

腳本只會複製這幾個伺服器 DLL:

- `Assembly-CSharp.dll`
- `Mirror.dll`
- `CommandSystem.Core.dll`
- `LabApi.dll` / `LabAPI.dll`
- `UnityEngine.CoreModule.dll`

它**不會**去找 `MEC.dll` 或 `Assembly-CSharp-firstpass.dll`。

輸出位置:

```text
SmallVanillaFlow\bin\Release\net48\SmallVanillaFlow.dll
```

#### 一步編譯並部署(Windows)

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build.ps1 `
  -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server" `
  -Deploy
```

會自動複製到 `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\`。

---

### 🐧 Linux / macOS (bash)

先把腳本設為可執行:

```bash
chmod +x ./build.sh
```

然後編譯:

```bash
./build.sh \
  --server-path "/home/<user>/SCP Secret Laboratory Dedicated Server"
```

SteamCMD 安裝的常見路徑是 `/home/steam/SCP Secret Laboratory Dedicated Server`。

#### 一步編譯並部署(Linux / macOS)

```bash
./build.sh \
  --server-path "/home/steam/SCP Secret Laboratory Dedicated Server" \
  --deploy
```

會自動複製到 `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/`。

#### 自訂外掛目錄

```bash
./build.sh \
  --server-path "/opt/scpsl" \
  --deploy \
  --plugin-dir "/opt/scpsl/LabAPI/plugins/global"
```

完整參數可跑 `./build.sh --help`。

---

## 🚀 安裝(手動)

如果你沒用 `-Deploy` / `--deploy`:

1. 自己編譯(見上)**或**下載 Release DLL。
2. 把 DLL 複製到 LabAPI global 外掛目錄:

   | 系統 | 路徑 |
   |------|------|
   | Windows | `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\` |
   | Linux   | `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/` |
   | macOS   | `~/Library/Application Support/SCP Secret Laboratory/LabAPI/plugins/global/` |
3. 先啟動一次伺服器——LabAPI 會自動在 DLL 旁邊產生 `config.yml`。
4. 改 `config.yml`,然後重啟伺服器。

### 在 Linux 上跑 SCP:SL 伺服器本身

官方的 **SCP:SL 專用伺服器**只有 Windows 版。Linux 上通常透過以下方式:

- **Wine** + `winetricks`(臨時環境最推薦)
- **Proton**(透過 Steam Play)
- 社群 **Docker** 映像檔,例如 [`ghcr.io/serverbp/scp-sl-server`](https://github.com/ServerBp/scp-sl-server)

不管用哪一種:

1. 把 LabAPI **裝進 Wine/Proton/Docker 的 prefix 裡**,方法跟 Windows 一樣。
2. 外掛 DLL 放到該 prefix 下的 `…/SCP Secret Laboratory/LabAPI/plugins/global/`。
3. 外掛**在 Linux 上用 `build.sh` 編譯**——產出物是與平台無關的純託管 .NET DLL。

---

## ⚙️ 配置說明

完整範例見倉庫裡的 [`config.example.yml`](./config.example.yml)。

預設組態:

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
  <size=32><color=#ff5555>收容程序進行中</color>
  <color=white>收容失效倒數:{seconds} 秒</color></size>
release_hint_text: |-
  <size=32><color=#ff5555>收容失效</color>
  <color=white>你現在可以移動了。</color></size>
release_hint_duration_seconds: 4

enable_starting_tokens: true
starting_token_mode: Set    # Set = 覆寫原版,Add = 在原版基礎上再加
ntf_starting_tokens: 2      # -1 = 不動該陣營
chaos_starting_tokens: 2
maximum_token_value: 20

enable_debug_logging: false

# 身份投票
enable_role_voting: true
voting_time_seconds: 45
```

### 什麼是 "Respawn Token"?

一個 Respawn Token 代表**一次主增援波次**,不是一個人。Mini-wave 不受此設定影響。

### 建議先用這個試

```yaml
starting_token_mode: Set
ntf_starting_tokens: 2
chaos_starting_tokens: 2
```

如果一回合還是等不到有意義的增援循環,就把兩邊都改成 `3`。不建議設太大——DMS(動態地圖輪換)會被延到增援資源耗盡之後。

---

## 📝 備註

- Token 在 `WaitingForPlayers` 階段寫入,符合 14.x 的增援初始化生命週期。
- 外掛會同時更新「配置/基礎值」與「剩餘值」,然後把增援波次更新推送給已連線的客戶端。
- 如果有其他外掛也覆寫初始 Token,**載入順序**決定最終值。
- 本專案直接引用**你當前伺服器**的 LabAPI 組件——SCP:SL 或 LabAPI 大版本更新後,需要重新編譯。

---

## 🤝 貢獻

歡迎 PR。請注意:

1. 保持依賴極簡——不要引入 EXILED / Harmony / MEC。
2. 不要把伺服器 DLL 提交進倉庫。
3. 提交前,先在自己當前 14.x 伺服器上重新編譯一遍。
4. 說明**為什麼**這個改動對小伺服器有幫助,而不只是做了什麼。

---

## 📄 授權

[MIT](./LICENSE)
