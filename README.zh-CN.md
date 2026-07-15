# SCPSL-MicroServer-Tweaks

> 一个轻量、基于 **LabAPI** 的 **SCP: Secret Laboratory 14.x** 服务器插件,专为 **人少的微服** (≈ 3–12 名玩家) 调优。

[![Game](https://img.shields.io/badge/SCP%3ASL-14.x-blue.svg)](#)
[![LabAPI](https://img.shields.io/badge/LabAPI-required-green.svg)](#)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.8-purple.svg)](#)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)](#)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](#)

在玩家数量很少的服务器上,原版 SCP:SL 的节奏会崩——回合几秒就结束、SCP 还没动起来就要被围殴、增援来得不是太早就是太晚。**SCPSL-MicroServer-Tweaks** 用两个小而精的可调功能,把小服务器的早期游戏体验拉回来,而且**没有任何花里胡哨的依赖**。

## 🌐 语言 / Languages / 言語

| 语言 | README |
|------|--------|
| 🇬🇧 English (default) | [README.md](./README.md) |
| 🇨🇳 简体中文 (当前) | [README.zh-CN.md](./README.zh-CN.md) |
| 🇹🇼 繁體中文 | [README.zh-TW.md](./README.zh-TW.md) |
| 🇯🇵 日本語 | [README.ja.md](./README.ja.md) |
| 🇪🇸 Español | [README.es.md](./README.es.md) |
| 🇧🇷 Português (Brasil) | [README.pt-BR.md](./README.pt-BR.md) |

> 小提示:GitHub 在检测到多个 `README.*.md` 时,会在仓库顶部自动显示语言切换器。

---

## 📖 快速上手教程 (5 分钟)

跟着下面五步走,**Windows 和 Linux 都有**,挑对应你系统的部分看。

### 第一步 — 准备环境

| 需求 | Windows | Linux / macOS |
|------|---------|---------------|
| SCP:SL 14.x 专用服务器 | ✅ | ✅ |
| 已安装并可用的 LabAPI | ✅ | ✅ |
| 带 **.NET Framework 4.8** 目标的 .NET SDK | ✅ | ✅ (需要 `net48` reference assemblies) |
| 编译用的 Shell | **PowerShell** | **bash** (或 zsh) |

如果只想**用现成的插件 DLL**(不自己编译),可以跳过 .NET SDK,直接去 [Releases](../../releases) 下载即可。

### 第二步 — 拿到插件

- **自己编译** — 见下方 [Build](#-build)
- **或** 从 [Releases](../../releases) 下载已经编译好的 DLL

### 第三步 — 安装

把 `SmallVanillaFlow.dll` 复制到 LabAPI **global** 插件目录:

| 系统 | 路径 |
|------|------|
| Windows | `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\` |
| Linux   | `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/` |
| macOS   | `~/Library/Application Support/SCP Secret Laboratory/LabAPI/plugins/global/` |

> 💡 在无头专用服务器上,这里的"用户"一般是跑 SCPSL 服务进程的那个用户(例如 `steam`)。

### 第四步 — 设定开场角色队列

打开服务器的 `config_gameplay.txt`(或 `config.txt`),加入:

```yaml
team_respawn_queue: 40144443
```

这正好对应下面的"推荐角色配置"表格。

### 第五步 — 启动服务器并微调

1. 启动服务器。第一次启动时,LabAPI 会自动在插件 DLL 旁边生成 `config.yml`。
2. 打开 `config.yml` 看一下默认值(5–8 人服务器上默认值已经够用)。
3. 想要在 3 人小房间里冻结更久?想要给 NTF 更多增援?改 `scp_freeze_seconds_by_player_count` 和 `ntf_starting_tokens`,详见 [Configuration](#-配置说明)。
4. **重启服务器**,新配置才会生效。

搞定。🎉 拉几个朋友进来,你会看到 SCP 在出生点短暂"冻结"几秒,整局体验顺滑不少。

---

## ✨ 功能特性

- 🧊 **按玩家数量冻结 SCP 出生点**
  - 回合开始时,所有可移动 SCP 在出生位置保持静止一段可配置的时间。
  - 冻结期间,SCP 仍可以转头观察、用语音聊天。
  - SCP-079 自动排除(没有正常出生位置)。
  - 按玩家数量设置不同的冻结时长(例如 4 人局冻结久一些,10+ 人局冻结短一些)。
  - 可选:在屏幕上给被冻结的 SCP 显示倒计时提示。

- 🎟️ **增援波次 Token**
  - 配置 **Nine-Tailed Fox** 与 **Chaos Insurgency / 基金会敌对势力** 起始的**主增援波次 Token**。
  - 两种模式:
    - `Set` — 直接覆盖原版初始值。
    - `Add` — 在原版基础上加。
  - 支持按阵营控制,`-1` 表示不动该阵营。

- 🪶 **极简设计**
  - 不依赖 EXILED、Harmony、MEC、NWPluginAPI。
  - 位置锁定走 Unity 主线程。
  - 只引用**你自己当前服务器**的 DLL,没有老版本 DLL 包。

---

## 📋 推荐开场角色配置

适用于人少服务器的开局组合:

| 玩家数 | SCP | 守卫 | D 级人员 | 科学家 |
|--------|:---:|:----:|:--------:|:------:|
| 3      | 1   | 1    | 1        | 0      |
| 4      | 1   | 1    | 2        | 0      |
| 5      | 1   | 1    | 3        | 0      |
| 6      | 1   | 1    | 4        | 0      |
| 7      | 1   | 1    | 5        | 0      |
| 8      | 1   | 1    | 5        | 1      |
| 10     | 1   | 2    | 6        | 1      |
| 12     | 1   | 2    | 7        | 1      |

在服务器上设置:

```yaml
team_respawn_queue: 40144443
```

---

## 📦 依赖

- **SCP: Secret Laboratory** 专用服务器 — 当前 14.x
- **LabAPI** 已安装
- 带 **.NET Framework 4.8** 目标的 **.NET SDK**
- 编译用 Shell:**PowerShell**(Windows)**或** **bash**(Linux / macOS)

> ⚠️ Linux 小贴士:在 Linux 上编译 `net48` 需要 `Microsoft.NETFramework.ReferenceAssemblies.net48` 包,`dotnet` 一般会自动还原。如果 `dotnet build` 报错,先跑一次 `dotnet restore` 再试。

---

## 🔧 编译

### 🪟 Windows (PowerShell)

在仓库目录打开 PowerShell:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build.ps1 `
  -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server"
```

脚本只会拷贝这三个服务器 DLL:

- `Assembly-CSharp.dll`
- `LabApi.dll` / `LabAPI.dll`
- `UnityEngine.CoreModule.dll`

它**不会**去找 `MEC.dll` 或 `Assembly-CSharp-firstpass.dll`。

输出位置:

```text
SmallVanillaFlow\bin\Release\net48\SmallVanillaFlow.dll
```

#### 一步编译并部署(Windows)

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build.ps1 `
  -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server" `
  -Deploy
```

会自动复制到 `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\`。

---

### 🐧 Linux / macOS (bash)

先把脚本设为可执行:

```bash
chmod +x ./build.sh
```

然后编译:

```bash
./build.sh \
  --server-path "/home/<user>/SCP Secret Laboratory Dedicated Server"
```

SteamCMD 安装的常见路径是 `/home/steam/SCP Secret Laboratory Dedicated Server`。

#### 一步编译并部署(Linux / macOS)

```bash
./build.sh \
  --server-path "/home/steam/SCP Secret Laboratory Dedicated Server" \
  --deploy
```

会自动复制到 `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/`。

#### 自定义插件目录

```bash
./build.sh \
  --server-path "/opt/scpsl" \
  --deploy \
  --plugin-dir "/opt/scpsl/LabAPI/plugins/global"
```

完整参数可以跑 `./build.sh --help`。

---

## 🚀 安装(手动)

如果你没用 `-Deploy` / `--deploy`:

1. 自己编译(见上)**或**下载 Release DLL。
2. 把 DLL 复制到 LabAPI global 插件目录:

   | 系统 | 路径 |
   |------|------|
   | Windows | `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\` |
   | Linux   | `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/` |
   | macOS   | `~/Library/Application Support/SCP Secret Laboratory/LabAPI/plugins/global/` |
3. 先启动一次服务器——LabAPI 会自动在 DLL 旁边生成 `config.yml`。
4. 改 `config.yml`,然后重启服务器。

### 在 Linux 上跑 SCP:SL 服务器本身

官方的 **SCP:SL 专用服务器**只发 Windows 版。Linux 上一般通过以下方式:

- **Wine** + `winetricks`(临时环境最推荐)
- **Proton**(通过 Steam Play)
- 社区 **Docker** 镜像,例如 [`ghcr.io/serverbp/scp-sl-server`](https://github.com/ServerBp/scp-sl-server)

无论用哪种:

1. 把 LabAPI **装到 Wine/Proton/Docker 的 prefix 里**,方法跟 Windows 一样。
2. 插件 DLL 放到该 prefix 下的 `…/SCP Secret Laboratory/LabAPI/plugins/global/`。
3. 插件**在 Linux 上用 `build.sh` 编译**——产物是平台无关的纯托管 .NET DLL。

---

## ⚙️ 配置说明

完整示例见仓库里的 [`config.example.yml`](./config.example.yml)。

默认配置:

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
  <size=32><color=#ff5555>收容程序进行中</color>
  <color=white>收容失效倒计时:{seconds} 秒</color></size>
release_hint_text: |-
  <size=32><color=#ff5555>收容失效</color>
  <color=white>你现在可以移动了。</color></size>
release_hint_duration_seconds: 4

enable_starting_tokens: true
starting_token_mode: Set    # Set = 覆盖原版,Add = 在原版基础上加
ntf_starting_tokens: 2      # -1 = 不动该阵营
chaos_starting_tokens: 2
maximum_token_value: 20

enable_debug_logging: false
```

### 什么是 "Respawn Token"?

一个 Respawn Token 代表**一次主增援波次**,不是一个人。Mini-wave 不受此设置影响。

### 建议先用这个试

```yaml
starting_token_mode: Set
ntf_starting_tokens: 2
chaos_starting_tokens: 2
```

如果一回合还是没等到有意义的增援循环,就把两边都改成 `3`。不建议设太大——DMS(动态地图轮换)会被推迟到增援资源耗尽之后。

---

## 📝 备注

- Token 在 `WaitingForPlayers` 阶段写入,符合 14.x 的增援初始化生命周期。
- 插件会同时更新"配置/基础值"和"剩余值",然后把增援波次更新推送给已连接的客户端。
- 如果有别的插件也覆盖初始 Token,**加载顺序**决定最终值。
- 本项目直接引用**你当前服务器**的 LabAPI 程序集——SCP:SL 或 LabAPI 大版本更新后,需要重新编译。

---

## 🤝 贡献

欢迎 PR。请注意:

1. 保持依赖极简——不要引入 EXILED / Harmony / MEC。
2. 不要把服务器 DLL 提交进仓库。
3. 提交前,先在自己当前 14.x 服务器上重新编译一遍。
4. 说明**为什么**这个改动对小服务器有帮助,而不只是做了什么。

---

## 📄 许可证

[MIT](./LICENSE)
