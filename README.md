# SmallVanillaFlow

A small **SCP:SL 14.x LabAPI** plugin for a 5–8 player Vanilla+ server.

## Features

1. Keeps every initial movable SCP at its spawn position for a configurable period.
   - SCP players may still look around and use voice chat.
   - SCP-079 is excluded because it has no normal movement position.
   - Position locking runs on Unity's main thread.
   - No MEC, EXILED, Harmony, or NwPluginAPI dependency.

2. Configures starting **primary-wave Respawn Tokens** for:
   - Nine-Tailed Fox / Foundation Staff
   - Chaos Insurgency / Foundation Enemy

3. Supports two token modes:
   - `Set`: replace vanilla starting tokens.
   - `Add`: add tokens on top of vanilla.

4. Supports a different SCP freeze duration for each player count.

## Recommended server queue

For your requested opening roles:

- 5 players: 1 SCP, 1 Guard, 3 Class-D
- 6 players: 1 SCP, 1 Guard, 4 Class-D
- 7 players: 1 SCP, 1 Guard, 5 Class-D
- 8 players: 1 SCP, 1 Guard, 5 Class-D, 1 Scientist

Use:

```yaml
team_respawn_queue: 40144443
```

## Build requirements

- Current SCP:SL 14.x Dedicated Server files
- .NET SDK with .NET Framework 4.8 targeting support
- PowerShell

The project references DLLs from **your own current server**, so it does not bundle or rely on an old DLL pack.

## Build and optionally deploy

Open PowerShell in this folder:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build.ps1 `
  -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server" `
  -Deploy
```

The script copies only these current server assemblies:

- `Assembly-CSharp.dll`
- `LabApi.dll` / `LabAPI.dll`
- `UnityEngine.CoreModule.dll`

It deliberately does **not** look for `MEC.dll` or `Assembly-CSharp-firstpass.dll`.

Without `-Deploy`, the DLL is written to:

```text
SmallVanillaFlow\bin\Release\net48\SmallVanillaFlow.dll
```

Place it in the LabAPI plugin directory, normally:

```text
%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\
```

Then restart the server.

## Configuration

LabAPI generates `config.yml` after the plugin loads for the first time. A ready-to-copy example is included as `config.example.yml`.

Default behavior:

```yaml
enable_scp_freeze: true

scp_freeze_seconds_by_player_count:
  5: 75
  6: 60
  7: 45
  8: 40

enable_starting_tokens: true
starting_token_mode: Set
ntf_starting_tokens: 2
chaos_starting_tokens: 2
```

### Token meaning

A Respawn Token is one available **primary reinforcement wave**, not one individual player. Mini-waves are not modified.

### Suggested first test

Start with:

```yaml
starting_token_mode: Set
ntf_starting_tokens: 2
chaos_starting_tokens: 2
```

If the round still ends before a useful reinforcement cycle, try `3` for each side. Avoid very high values because DMS will be delayed until reinforcement resources are exhausted.

## Notes

- Tokens are applied during `WaitingForPlayers`, matching the 14.x wave initialization lifecycle.
- The plugin updates both the configured/base token count and the remaining-token count, then sends the wave token update to connected clients.
- If another plugin also overwrites starting tokens, load order determines the final value.
- This source project targets the current LabAPI API exposed by your server assemblies. Rebuild it after a major SCP:SL or LabAPI update.
