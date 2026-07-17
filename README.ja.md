# SCPSL-MicroServer-Tweaks

> **SCP: Secret Laboratory 14.x** 向けの、**LabAPI** ベースの軽量サーバー側プラグイン。**少人数サーバー (約 3〜12 人)** に最適化されています。

[![Game](https://img.shields.io/badge/SCP%3ASL-14.x-blue.svg)](#)
[![LabAPI](https://img.shields.io/badge/LabAPI-required-green.svg)](#)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.8-purple.svg)](#)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)](#)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](#)
[![Downloads](https://img.shields.io/github/downloads/Neverwin1337/SCPSL-MicroServer-Tweaks/total.svg)](https://github.com/Neverwin1337/SCPSL-MicroServer-Tweaks/releases)

プレイヤー数が少ないサーバーでは、バニラ SCP:SL のゲームバランスが崩壊します——ラウンドが数秒で終わり、SCP が動く前に殺され、増援の波が早すぎたり来なかったりします。**SCPSL-MicroServer-Tweaks** は、4 つの焦点を絞った設定可能な機能で、少人数サーバーの序盤を滑らかにします。**無駄は一切ありません**。

## 🌐 言語 / Languages / 言語

| 言語 | README |
|------|--------|
| 🇬🇧 English (default) | [README.md](./README.md) |
| 🇨🇳 简体中文 | [README.zh-CN.md](./README.zh-CN.md) |
| 🇹🇼 繁體中文 | [README.zh-TW.md](./README.zh-TW.md) |
| 🇯🇵 日本語 (現在) | [README.ja.md](./README.ja.md) |
| 🇪🇸 Español | [README.es.md](./README.es.md) |
| 🇧🇷 Português (Brasil) | [README.pt-BR.md](./README.pt-BR.md) |

> ヒント:GitHub は複数の `README.*.md` を検出すると、リポジトリ上部に自動的に言語スイッチャーを表示します。

---

## 📖 クイックスタート (約 5 分)

下記の 5 ステップに従ってください。**Windows と Linux の両方**をカバーしているので、お使いの OS に合わせて読んでください。

### ステップ 1 — 前提条件

| 要件 | Windows | Linux / macOS |
|------|---------|---------------|
| SCP:SL 14.x 専用サーバー | ✅ | ✅ |
| LabAPI がインストール済みで動作している | ✅ | ✅ |
| **.NET Framework 4.8** ターゲットの .NET SDK | ✅ | ✅ (`net48` 参照アセンブリが必要) |
| ビルド用のシェル | **PowerShell** | **bash** (または zsh) |

プラグインを**実行するだけ**(ソースからビルドしない)場合は .NET SDK をスキップして、[Releases](../../releases) からビルド済み DLL をダウンロードできます。

### ステップ 2 — プラグインを取得

- **自分でビルド** — 下記の [Build](#-ビルド) を参照
- **または** [Releases](../../releases) からビルド済み DLL をダウンロード

### ステップ 3 — インストール

- [RueI](https://github.com/pawslee/RueI/releases/latest) — ヒント表示フレームワーク。`RueI.dll` を `LabAPI/plugins/global/` にコピーしてください。
- `SCPSL_MicroServer_Tweaks.dll` を LabAPI **global** プラグインディレクトリにコピーします:

| OS | パス |
|----|------|
| Windows | `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\` |
| Linux   | `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/` |
| macOS   | `~/Library/Application Support/SCP Secret Laboratory/LabAPI/plugins/global/` |

> 💡 ヘッドレス専用サーバーでは、ここでいう「ユーザー」は通常 SCPSL サービスを実行しているユーザー (例: `steam`) です。

### ステップ 4 — 開始ロールキューの設定

サーバーの `config_gameplay.txt` (または `config.txt`) を開いて、次を追加します:

```yaml
team_respawn_queue: 40144443
```

これは下記の「推奨開始ロール構成」テーブルと一致します。

### ステップ 5 — 起動と調整

1. サーバーを起動します。初回起動時に、LabAPI がプラグイン DLL の隣に `config.yml` を自動生成します。
2. その `config.yml` を開いてデフォルト値を確認します(5〜8 人サーバーではデフォルトで十分機能します)。
3. 3 人部屋で凍結時間を長くしたい?NTF の増援を増やしたい?`scp_freeze_seconds_by_player_count` と `ntf_starting_tokens` を調整してください。詳細は [Configuration](#-設定) を参照。
4. 新しい設定を反映させるために**サーバーを再起動**します。

完了。🎉 フレンド数人と参加して、SCP が一時的にスポーン地点で「凍結」するのを見てください。ラウンドが格段に遊びやすくなります。

---

## ✨ 機能

- 🧊 **プレイヤー数に応じたスポーン時 SCP 凍結**
  - 初期スポーンしたすべての移動可能 SCP を、設定可能な期間スポーン位置に保持します。
  - 凍結中も SCP は周囲を見回す、ボイスチャットを使うことができます。
  - SCP-079 は自動的に除外されます(通常のスポーン位置がないため)。
  - プレイヤー数ごとに凍結時間をオーバーライド可能(例: 4 人ラウンドは長く、10+ 人は短く)。
  - オプション: 凍結中の SCP に画面にカウントダウン表示。

- 🎟️ **増援ウェーブトークン**
  - **Nine-Tailed Fox** と **Chaos Insurgency / Foundation Enemy** の**プライマリ増援ウェーブ用 Respawn Token** の初期値を構成。
  - 2 つのモード:
    - `Set` — バニラの初期値を置き換え。
    - `Add` — バニラの値に加算。
  - 陣営ごとに制御可能。`-1` でその陣営を未変更に。

- 🗳️ **ロビー役割投票**
  - ロビー待機中、`.1` `.2` `.3` `.4` または `.vote scp/sci/d/guard` で投票。
  - リアルタイム画面ヒント: 残り時間、票数、投票方法を表示。
  - 未投票者はランダム割り当て。

- 🎲 **ランダムイベント**
  - 3〜5 分ごとにランダムイベントが発生。すべて **C.A.S.S.I.E.** 音声で通知(カスタムピッチ & グリッチ効果)。
  - **エレベーター故障** — 全エレベーター停止 60 秒、カウントダウンヒント付き
  - **全ドア開放** — 施設内全ドアを強制開放(永続)
  - **ステルスプロトコル** — 全人類に真の透明化(SCP-268 効果)30 秒、カウントダウンヒント付き
  - **停電点滅** — 全 3 ゾーンのライトが点滅(5 秒消 / 1 秒点)3 分間、カウントダウンヒント付き
  - **核弾頭警報** — 50% 誤報 / 50% 本物、60 秒カウントダウン(ラウンド開始 10 分後以降のみ)
  - **スクランブル** — 全人類をランダムテレポート。LCZ 汚染済みなら LCZ を除外
  - テストコマンド: `mst_event <elevator|doors|stealth|blackout|nuke|scramble>`

- 🪶 **軽量設計**
  - [RueI](https://github.com/pawslee/RueI) のみ依存（ヒント表示用）。EXILED、MEC、NWPluginAPI には依存しません。
  - 位置ロックは Unity のメインスレッドで動作。
  - **自分の現在のサーバー**に付属する DLL のみを参照——古い DLL パックは使いません。

---

## 📋 推奨開始ロール構成

少人数サーバーに最適な構成:

| プレイヤー数 | SCP | ガード | D 級 | 科学者 |
|--------------|:---:|:------:|:----:|:------:|
| 3            | 1   | 1      | 1    | 0      |
| 4            | 1   | 1      | 2    | 0      |
| 5            | 1   | 1      | 3    | 0      |
| 6            | 1   | 1      | 4    | 0      |
| 7            | 1   | 1      | 5    | 0      |
| 8            | 1   | 1      | 5    | 1      |
| 10           | 1   | 2      | 6    | 1      |
| 12           | 1   | 2      | 7    | 1      |

サーバーで設定:

```yaml
team_respawn_queue: 40144443
```

---

## 📦 要件

- **SCP: Secret Laboratory** 専用サーバー — 現行 14.x
- **LabAPI** インストール済み
- **.NET Framework 4.8** ターゲットの **.NET SDK**
- ビルド用シェル: **PowerShell** (Windows) **または** **bash** (Linux / macOS)

> ⚠️ Linux メモ: Linux 上で `net48` をビルドするには `Microsoft.NETFramework.ReferenceAssemblies.net48` パッケージが必要です。`dotnet` が自動で復元しますが、もし `dotnet build` がエラーになった場合は `dotnet restore` を一度実行してから再試行してください。

---

## 🔧 ビルド

### 🪟 Windows (PowerShell)

リポジトリフォルダで PowerShell を開きます:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build.ps1 `
  -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server"
```

スクリプトは次の 3 つのアセンブリのみをサーバーからコピーします:

- `Assembly-CSharp.dll`
- `LabApi.dll` / `LabAPI.dll`
- `UnityEngine.CoreModule.dll`

`MEC.dll` や `Assembly-CSharp-firstpass.dll` は**意図的に**探しません。

出力先:

```text
SCPSL-MicroServer-Tweaks/bin/Release/net48/SCPSL_MicroServer_Tweaks.dll
```

#### ビルド + デプロイを 1 ステップで(Windows)

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build.ps1 `
  -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server" `
  -Deploy
```

DLL を `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\` に自動配置します。

---

### 🐧 Linux / macOS (bash)

まずスクリプトに実行権限を付与:

```bash
chmod +x ./build.sh
```

ビルド:

```bash
./build.sh \
  --server-path "/home/<user>/SCP Secret Laboratory Dedicated Server"
```

SteamCMD インストール時の一般的なパス: `/home/steam/SCP Secret Laboratory Dedicated Server`。

#### ビルド + デプロイを 1 ステップで(Linux / macOS)

```bash
./build.sh \
  --server-path "/home/steam/SCP Secret Laboratory Dedicated Server" \
  --deploy
```

DLL を `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/` に自動配置します。

#### プラグインディレクトリの指定

```bash
./build.sh \
  --server-path "/opt/scpsl" \
  --deploy \
  --plugin-dir "/opt/scpsl/LabAPI/plugins/global"
```

全オプションは `./build.sh --help` を参照。

---

## 🚀 インストール(手動)

`-Deploy` / `--deploy` を使わない場合:

1. ビルドする(上記参照)**または** リリース DLL をダウンロード。
2. DLL を LabAPI global プラグインディレクトリにコピー:

   | OS | パス |
   |----|------|
   | Windows | `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\` |
   | Linux   | `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/` |
   | macOS   | `~/Library/Application Support/SCP Secret Laboratory/LabAPI/plugins/global/` |
3. サーバーを一度起動 — LabAPI が DLL の隣に `config.yml` を生成します。
4. `config.yml` を編集し、サーバーを再起動。

### Linux 上で SCP:SL サーバー本体を動かす

公式の **SCP:SL 専用サーバー** は Windows 版のみです。Linux では以下のいずれかを使います:

- **Wine** + `winetricks`(カジュアルなセットアップで最も簡単)
- **Proton** (Steam Play 経由)
- コミュニティの **Docker** イメージ(例: [`ghcr.io/serverbp/scp-sl-server`](https://github.com/ServerBp/scp-sl-server))

どれを使う場合も:

1. Windows と同様に **Wine/Proton/Docker の prefix 内に** LabAPI をインストール。
2. プラグイン DLL をその prefix 内の `…/SCP Secret Laboratory/LabAPI/plugins/global/` に配置。
3. プラグインは **`build.sh` で Linux 上ビルド** — 出力はプラットフォームに依存しない純粋なマネージド .NET DLL です。

---

## ⚙️ 設定

サンプルはリポジトリの [`config.example.yml`](./config.example.yml) にあります。

デフォルト:

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
  <size=32><color=#ff5555>収容プロトコル進行中</color>
  <color=white>収容解除まで: {seconds} 秒</color></size>
release_hint_text: |-
  <size=32><color=#ff5555>収容失敗</color>
  <color=white>移動可能です。</color></size>
release_hint_duration_seconds: 4

enable_starting_tokens: true
starting_token_mode: Set    # Set = バニラを置換, Add = バニラに加算
ntf_starting_tokens: 2      # -1 = その陣営を変更しない
chaos_starting_tokens: 2
maximum_token_value: 20

enable_debug_logging: false

enable_role_voting: true
voting_time_seconds: 45
vote_hint_interval_seconds: 1

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

### "Respawn Token" とは?

Respawn Token は**プライマリ増援ウェーブ 1 回分**を表し、プレイヤー 1 人を表すものではありません。ミニウェーブは変更されません。

### 最初のテスト推奨値

```yaml
starting_token_mode: Set
ntf_starting_tokens: 2
chaos_starting_tokens: 2
```

それでもラウンドがまともな増援サイクル前に終わるなら、両方とも `3` にしてみてください。高すぎる値は避けてください——DMS(動的マップ選択)が増援リソースを使い切るまで遅延されます。

---

## 📝 メモ

- トークンは `WaitingForPlayers` 段階で適用され、14.x のウェーブ初期化ライフサイクルに合わせています。
- プラグインは設定/基本値と残り値の両方を更新し、ウェーブトークン更新を接続中のクライアントへ送信します。
- 他のプラグインも同じく初期トークンを上書きする場合、**ロード順**が最終的な値を決定します。
- 本プロジェクトは**あなたの現在のサーバー**の LabAPI アセンブリを直接参照しているため、SCP:SL または LabAPI のメジャーアップデート後に再ビルドが必要です。

---

## 🤝 コントリビュート

PR を歓迎します。以下にご注意ください:

1. 依存関係は最小限に—— EXILED / Harmony / MEC を導入しないでください。
2. サーバーの DLL をリポジトリに含めないでください。
3. PR を作成する前に、ご自身の 14.x サーバーに対して再ビルドしてください。
4. 「何を変更したか」ではなく、**なぜ**その変更が少人数サーバーに役立つかを説明してください。

---

## 📄 ライセンス

[MIT](./LICENSE)
