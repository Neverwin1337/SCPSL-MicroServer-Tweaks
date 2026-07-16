# SmallVanillaFlow

> **SCP:SL 14.x** 微伺服器外掛 — 讓 3–12 人小服也能玩得順  
> 基於 **LabAPI**，零依賴，一個設定檔搞定。

[![Game](https://img.shields.io/badge/SCP%3ASL-14.x-blue.svg)](#)
[![LabAPI](https://img.shields.io/badge/LabAPI-powered-green.svg)](#)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.8-purple.svg)](#)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](#)

---

## 🌐 其他語言

| 語言 | README |
|------|--------|
| 🇬🇧 English | [README.md](./README.md) |
| 🇨🇳 简体中文 | [README.zh-CN.md](./README.zh-CN.md) |
| 🇯🇵 日本語 | [README.ja.md](./README.ja.md) |
| 🇪🇸 Español | [README.es.md](./README.es.md) |

---

## 👋 痛點

小服的核心問題：

> SCP 還沒動就被秒了，援軍永遠不來（或者來得太晚），回合還沒開始就結束了。

**SmallVanillaFlow** 用三個輕量功能解決這個問題。不依賴 EXILED / Harmony / MEC，丟個 DLL 就能用。

---

## ⭐ 快速上手

**30 秒跑起來：**

1. 下載 `SmallVanillaFlow.dll` 從 [Releases](../../releases)
2. 丟進 `LabAPI/plugins/global/` 資料夾
3. 啟動伺服器，開玩 🎉

> 想自己編譯？見下方 [Build](#-編譯)

---

## ✨ 功能

### 🧊 SCP 出生凍結

SCP 出生後原地凍結 X 秒，可以轉頭和語音，**不能移動。**

- 按玩家數量設不同時長（5 人 → 75 秒，8 人 → 40 秒……）
- SCP-079 自動排除
- 可選倒數計時提示

**效果：** 人類有時間跑路，SCP 能真正玩到開局。

---

### 🎟️ 初始援軍令牌

NTF 和 Chaos 開局有幾個波次？**你說了算。**

- `Set` 模式 — 直接覆蓋預設值
- `Add` 模式 — 在預設值上疊加
- 每個陣營獨立控制，`-1` 表示不動

**效果：** 不會再因為援軍波次太少而進入垃圾時間。

---

### 🗳️ 大廳角色投票

大廳等待時，輸入 `.1` `.2` `.3` `.4` 或 `.vote scp/sci/d/guard` 投票選角色。

- **即時螢幕提示**：顯示剩餘時間、各角色票數、投票方法
- 倒數和票數每秒重新整理
- 未投票玩家隨機分配

**效果：** 朋友局想玩什麼自己選，不用搶。

---

### 🪶 輕量無依賴

- **零依賴** — 沒有 EXILED / Harmony / MEC / NWPluginAPI
- 只引用**你當前伺服器**自帶的 DLL
- 一個 `.dll` + 一個 `config.yml`，搞定

---

## ⚙️ 設定

`config.yml` 首次啟動自動產生。5–8 人服預設值已夠用：

```yaml
# SCP 凍結
enable_scp_freeze: true
default_scp_freeze_seconds: 60
scp_freeze_seconds_by_player_count:
  5: 75
  6: 60
  7: 45
  8: 40

# 援軍令牌
starting_token_mode: Set
ntf_starting_tokens: 2
chaos_starting_tokens: 2

# 角色投票
enable_role_voting: true
voting_time_seconds: 45
vote_hint_interval_seconds: 1
```

完整範例見 [`config.example.yml`](./config.example.yml)。

---

## 🔧 編譯

### Windows (PowerShell)

```powershell
.\build.ps1 -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server"
```

### Linux / macOS (bash)

```bash
chmod +x ./build.sh
./build.sh --server-path "/home/steam/SCP Secret Laboratory Dedicated Server"
```

加 `-Deploy` / `--deploy` 自動部署到外掛目錄。

---

## 📦 需求

- SCP:SL 14.x 專用伺服器
- LabAPI 已安裝
- .NET SDK（僅編譯需要，直接下載 DLL 可跳過）

---

## 🤝 貢獻

歡迎 PR。唯一要求：**不要引入 EXILED / Harmony / MEC。**

---

## 📄 授權

[MIT](./LICENSE)

---

**⭐ 覺得好用？點個 Star 支持一下！**