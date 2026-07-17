# SCPSL-MicroServer-Tweaks

> **SCP:SL 14.x** 微服务器插件 — 让 3–12 人小服也能玩得爽  
> 基于 **LabAPI**，零依赖，一个配置文件搞定。

[![Game](https://img.shields.io/badge/SCP%3ASL-14.x-blue.svg)](#)
[![LabAPI](https://img.shields.io/badge/LabAPI-powered-green.svg)](#)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.8-purple.svg)](#)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](#)

---

## 🌐 其他语言

| 语言 | README |
|------|--------|
| 🇬🇧 English | [README.md](./README.md) |
| 🇹🇼 繁體中文 | [README.zh-TW.md](./README.zh-TW.md) |
| 🇯🇵 日本語 | [README.ja.md](./README.ja.md) |
| 🇪🇸 Español | [README.es.md](./README.es.md) |

---

## 👋 痛点

小服的核心问题：

> SCP 还没动就被秒了，援军永远不来（或者来得太晚），回合还没开始就结束了。

**SCPSL-MicroServer-Tweaks** 用四个轻量功能解决这个问题。不依赖 EXILED / Harmony / MEC，丢个 DLL 就能用。

---

## ⭐ 快速上手

**30 秒跑起来：**

1. 下载 `SCPSL_MicroServer_Tweaks.dll` 从 [Releases](../../releases)
2. 丢进 `LabAPI/plugins/global/` 文件夹
3. 启动服务器，开玩 🎉

> 想自己编译？见下方 [Build](#-编译)

---

## ✨ 功能

### 🧊 SCP 出生冻结

SCP 出生后原地冻结 X 秒，可以转头和语音，**不能移动。**

- 按玩家数量设不同时长（5 人 → 75 秒，8 人 → 40 秒……）
- SCP-079 自动排除
- 可选倒计时提示

**效果：** 人类有时间跑路，SCP 能真正玩到开局。

---

### 🎟️ 初始援军令牌

NTF 和 Chaos 开局有几个波次？**你说了算。**

- `Set` 模式 — 直接覆盖默认值
- `Add` 模式 — 在默认值上叠加
- 每个阵营独立控制，`-1` 表示不动

**效果：** 不会再因为援军波次太少而进入垃圾时间。

---

### 🗳️ 大厅角色投票

大厅等待时，输入 `.1` `.2` `.3` `.4` 或 `.vote scp/sci/d/guard` 投票选角色。

- **实时屏幕提示**：显示剩余时间、各角色票数、投票方法
- 倒计时和票数每秒刷新
- 未投票玩家随机分配

**效果：** 朋友局想玩什么自己选，不用抢。

---

### 🎲 随机事件

每 3–5 分钟随机触发一个事件，让回合更有变化。所有通知使用 **C.A.S.S.I.E.** 语音，带自定义音调与故障效果。

- **电梯故障** — 所有电梯锁定 60 秒，带倒计时提示
- **全门打开** — 设施所有门强制开启（永久）
- **隐形协议** — 所有人类获得真隐身（SCP-268 效果）30 秒，带倒计时提示
- **停电闪烁** — 全设施三区域灯光闪烁（暗 5 秒 / 亮 1 秒）持续 3 分钟，带倒计时提示
- **核弹警报** — 50% 虚警 / 50% 真炸，60 秒倒计时（回合开始 10 分钟后才触发）
- **随机传送** — 所有人类随机传送到不同房间；轻收容区已封闭则排除 LCZ

用 `mst_event <elevator|doors|stealth|blackout|nuke|scramble>` 在 RA / 控制台手动测试。

---

### 🪶 轻量无依赖

- **零依赖** — 没有 EXILED / Harmony / MEC / NWPluginAPI
- 只引用**你当前服务器**自带的 DLL
- 一个 `.dll` + 一个 `config.yml`，搞定

---

## ⚙️ 配置

`config.yml` 首次启动自动生成。5–8 人服默认值已够用：

```yaml
# SCP 冻结
enable_scp_freeze: true
default_scp_freeze_seconds: 60
scp_freeze_seconds_by_player_count:
  5: 75
  6: 60
  7: 45
  8: 40

# 援军令牌
starting_token_mode: Set
ntf_starting_tokens: 2
chaos_starting_tokens: 2

# 角色投票
enable_role_voting: true
voting_time_seconds: 45
vote_hint_interval_seconds: 1

# 随机事件
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
```

完整示例见 [`config.example.yml`](./config.example.yml)。

---

## 🔧 编译

### Windows (PowerShell)

```powershell
.\build.ps1 -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server"
```

### Linux / macOS (bash)

```bash
chmod +x ./build.sh
./build.sh --server-path "/home/steam/SCP Secret Laboratory Dedicated Server"
```

加 `-Deploy` / `--deploy` 自动部署到插件目录。

---

## 📦 需求

- SCP:SL 14.x 专用服务器
- LabAPI 已安装
- .NET SDK（仅编译需要，直接下载 DLL 可跳过）

---

## 🤝 贡献

欢迎 PR。唯一要求：**不要引入 EXILED / Harmony / MEC。**

---

## 📄 许可证

[MIT](./LICENSE)

---

**⭐ 觉得好用？点个 Star 支持一下！**