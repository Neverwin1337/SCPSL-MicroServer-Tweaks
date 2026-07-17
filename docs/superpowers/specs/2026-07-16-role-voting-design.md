# Role Voting System — Design Spec

## Overview

Add a lobby-based role voting system to SCPSL-MicroServer-Tweaks. During the WaitingForPlayers phase, players vote for their desired initial role (SCP, Scientist, Class-D, Guard) by typing console commands. A DisplayKit UI shows real-time vote counts. The lobby timer is extended to give time for voting, and when the round starts, `OnPlayerChangingRole` intercepts the default role assignment and applies the voting results.

## Motivation

Low-pop servers have fixed role distributions that don't account for player preference. This system lets players express their role preference without leaving the server, improving the experience on micro-servers (3–12 players).

## Architecture

### New Files

| File | Purpose |
|------|---------|
| `VotingController.cs` | Vote state machine, tally, timer, role assignment logic |
| `VotingCommand.cs` | Client command handler (`.1` `.2` `.3` `.4`) |
| `VotingUI.cs` | DisplayKit canvas for real-time vote display |

### Modified Files

| File | Changes |
|------|---------|
| `EventHandlers.cs` | Wire up VotingController lifecycle |
| `PluginConfig.cs` | Add voting configuration properties |
| `SCPSL_MicroServer_TweaksPlugin.cs` | Instantiate/manage VotingController |

## Configuration (PluginConfig.cs)

```yaml
enable_role_voting: true
voting_time_seconds: 45
lobby_timer_seconds: 60
voting_early_end_threshold: 0.75
voting_early_end_countdown: 10
```

## Voting Flow

```
WaitingForPlayers
  ├── Set lobby timer to voting_time_seconds (45s)
  ├── Create DisplayKit canvas with vote UI
  ├── Show to all players
  │
  ├── Player types ".1" / ".scp"
  │   └── Record vote, update UI
  │
  ├── [When 75% voted] OR [timer expires]
  │   ├── Set lobby timer to 10s
  │   └── Show "Voting ends soon!" message
  │
  └── Round starts
      └── OnPlayerChangingRole fires for each player
          └── args.NewRole = voting result (or random fallback)
```

## Role Assignment Algorithm

1. Determine SCP slot count from game default (based on player count)
2. Among players who voted SCP, randomly select N to be SCP
3. Remaining players (including non-voters) are assigned to Class-D, Scientist, or Guard:
   - Priority order: Class-D > Guard > Scientist (matching typical low-pop distribution)
   - Players who voted for a specific role get priority if slots remain
   - Excess votes for a role are resolved by random reassignment
   - Non-voters fill any remaining slots randomly

## Command Interface

| Command | Aliases | Votes For |
|---------|---------|-----------|
| `.1`    | `.scp`  | SCP       |
| `.2`    | `.sci`  | Scientist |
| `.3`    | `.d`    | Class-D   |
| `.4`    | `.g` / `.guard` | Guard |

## DisplayKit UI Layout

```
┌─────────────────────────────────────────────┐
│           🗳️ 身份投票                        │
│                                              │
│  [1] SCP-018      ████████░░  3 票    ───   │
│  [2] 對講機        ██░░░░░░░░  1 票    ───   │
│  [3] 鎖           ██████░░░░  2 票    ───   │
│  [4] 鑰匙卡        ████████░░  3 票    ───   │
│                                              │
│  剩餘時間: 35s  ·  已投票: 9/12              │
│  輸入 .1 .2 .3 .4 投票                       │
└─────────────────────────────────────────────┘
```

- Full-screen semi-transparent background
- 4 rows, each with: index number, item icon, progress bar, vote count, "───" indicator
- Timer and vote percentage at bottom
- Per-player canvas (each player sees their own vote highlighted)

## Lobby Timer Control

- `GameCore.RoundStart.singleton.NetworkRoundStartTime` = configurable value
- Set to `voting_time_seconds + 15` at WaitingForPlayers
- When early-end threshold met, set to `early_end_countdown`
- This gives enough time for voting without requiring a fixed timer

## Role Interception

In `OnPlayerChangingRole`:
- Check `ChangeReason` — only intercept `RoundStart` or `Spawn` reasons
- Set `args.NewRole` to the pre-computed voting result
- Let `RoleSpawnFlags` pass through normally
- Players who disconnected mid-vote get random role (no entry in vote dict)

## Edge Cases

| Case | Handling |
|------|----------|
| Player joins mid-vote | Show UI, allow voting, random fallback if timer expires |
| Player disconnects | Remove from vote dict, random assignment |
| All votes for one role | Distribute excess randomly, no minimum guarantee |
| Voting disabled | Pass through unchanged, no UI shown |
| SCP limit = 0 | All SCP voters go to random human roles |
| Single player | Tutorial role, no voting |