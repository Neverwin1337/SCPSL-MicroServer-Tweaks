# Role Voting System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add lobby-based role voting to SCPSL-MicroServer-Tweaks

**Architecture:** Three new files (VotingController, VotingCommand, VotingUI) integrate into the existing plugin lifecycle. VotingController manages the state machine and tally. VotingCommand handles `.1` `.2` `.3` `.4` client input. VotingUI renders a DisplayKit canvas with real-time counts. Role assignment is intercepted via `OnPlayerChangingRole` and the lobby timer is controlled via `GameCore.RoundStart.singleton`.

**Tech Stack:** C# 10, .NET Framework 4.8, LabAPI DisplayKit, GameCore.RoundStart

## Global Constraints

- Target: .NET Framework 4.8, C# 10
- No EXILED, Harmony, MEC, or NWPluginAPI dependency
- Only reference DLLs in Binaries/ directory
- Follow existing code style (nullable disable, implicit usings disable)

---

### Task 1: Add Voting Configuration to PluginConfig

**Files:**
- Modify: `SCPSL-MicroServer-Tweaks/PluginConfig.cs`

**Interfaces:**
- Produces: New config properties used by VotingController

- [ ] **Step 1: Add voting config properties**

Add after line 68 (after `EnableDebugLogging`):

```csharp
[Description("Whether players can vote for their initial role in the lobby.")]
public bool EnableRoleVoting { get; set; } = true;

[Description("How many seconds voting lasts in the lobby.")]
public float VotingTimeSeconds { get; set; } = 45f;

[Description("Lobby timer value when voting is active. Gives players time to vote before the round starts.")]
public float LobbyTimerSeconds { get; set; } = 60f;

[Description("Fraction of players (0.0–1.0) that must vote before the lobby timer is shortened to the early-end countdown.")]
public float VotingEarlyEndThreshold { get; set; } = 0.75f;

[Description("Countdown in seconds after the early-end threshold is met.")]
public float VotingEarlyEndCountdown { get; set; } = 10f;
```

- [ ] **Step 2: Add validation in `Validate()`**

Add after the `EnableDebugLogging` block (before `GetScpFreezeSeconds`):

```csharp
if (EnableRoleVoting)
{
    if (VotingTimeSeconds < 10f)
    {
        error = "VotingTimeSeconds must be at least 10.";
        return false;
    }

    if (LobbyTimerSeconds < VotingTimeSeconds + 5f)
    {
        error = "LobbyTimerSeconds must be at least VotingTimeSeconds + 5.";
        return false;
    }

    if (VotingEarlyEndThreshold < 0f || VotingEarlyEndThreshold > 1f)
    {
        error = "VotingEarlyEndThreshold must be between 0 and 1.";
        return false;
    }

    if (VotingEarlyEndCountdown < 3f)
    {
        error = "VotingEarlyEndCountdown must be at least 3.";
        return false;
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add SCPSL-MicroServer-Tweaks/PluginConfig.cs
git commit -m "feat: add voting configuration to PluginConfig"
```

---

### Task 2: Create VotingCommand (Client Command Handler)

**Files:**
- Create: `SCPSL-MicroServer-Tweaks/VotingCommand.cs`

**Interfaces:**
- Consumes: `VotingController.Vote(Player, RoleTypeId)` — defined in Task 3
- Produces: Parses `.1` `.2` `.3` `.4` `.scp` `.sci` `.d` `.guard` and calls VotingController

- [ ] **Step 1: Create VotingCommand.cs**

```csharp
using CommandSystem;
using PlayerRoles;
using RemoteAdmin;
using System;

namespace SCPSL_MicroServer_Tweaks
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class VoteCommand : ICommand
    {
        public string Command => "vote";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "Vote for a role. Usage: .vote scp | .vote sci | .vote d | .vote guard";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Invalid vote. Use .vote scp, .vote sci, .vote d, or .vote guard.";

            if (sender is not CommandSender commandSender)
                return false;

            Player player = Player.Get(commandSender.SenderId);
            if (player == null)
                return false;

            if (arguments.Count < 1)
                return false;

            string vote = arguments.At(0).ToLowerInvariant();
            RoleTypeId? role = vote switch
            {
                "scp" or "1" => RoleTypeId.Scp049,
                "sci" or "scientist" or "2" => RoleTypeId.Scientist,
                "d" or "classd" or "3" => RoleTypeId.ClassD,
                "guard" or "g" or "4" => RoleTypeId.FacilityGuard,
                _ => null
            };

            if (role == null)
                return false;

            bool success = SCPSL_MicroServer_TweaksPlugin.Instance.VotingController.TryVote(player, role.Value);
            response = success ? $"Voted for {vote}!" : "Voting is not active right now.";
            return true;
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add SCPSL-MicroServer-Tweaks/VotingCommand.cs
git commit -m "feat: add VoteCommand client command handler"
```

---

### Task 3: Create VotingController (State Machine + Tally)

**Files:**
- Create: `SCPSL-MicroServer-Tweaks/VotingController.cs`

**Interfaces:**
- Consumes: `PluginConfig` voting properties, `GameCore.RoundStart.singleton`, `Player.Role` setter
- Produces: `TryVote(Player, RoleTypeId) -> bool`, `VoteCounts` property, `OnRoleChanging(Player, ref RoleTypeId)` method

- [ ] **Step 1: Create VotingController.cs**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace SCPSL_MicroServer_Tweaks
{
    public sealed class VotingController
    {
        private readonly SCPSL_MicroServer_TweaksPlugin _plugin;
        private readonly Dictionary<Player, RoleTypeId> _votes = new Dictionary<Player, RoleTypeId>();
        private readonly Dictionary<RoleTypeId, int> _voteCounts = new Dictionary<RoleTypeId, int>();
        private bool _active;
        private float _lobbyTimerSetAt;
        private float _earlyEndTimerStart;

        public bool IsActive => _active;
        public IReadOnlyDictionary<RoleTypeId, int> VoteCounts => _voteCounts;
        public int TotalVoters => _votes.Count;
        public int TotalPlayers => Player.ReadyList.Count();
        public float TimeRemaining => _active ? Math.Max(0, _plugin.Config.VotingTimeSeconds - (Time.realtimeSinceStartup - _lobbyTimerSetAt)) : 0;
        public bool IsEarlyEndTriggered => _earlyEndTimerStart > 0;

        public VotingController(SCPSL_MicroServer_TweaksPlugin plugin)
        {
            _plugin = plugin;
        }

        public void StartVoting()
        {
            _votes.Clear();
            _voteCounts.Clear();
            _active = true;
            _earlyEndTimerStart = 0;
            _lobbyTimerSetAt = Time.realtimeSinceStartup;

            // Extend lobby timer to give voting time
            if (GameCore.RoundStart.singleton != null)
            {
                GameCore.RoundStart.singleton.NetworkRoundStartTime = _lobbyTimerSetAt + _plugin.Config.LobbyTimerSeconds;
            }

            _plugin.Debug("Voting started.");
        }

        public bool TryVote(Player player, RoleTypeId role)
        {
            if (!_active || player == null)
                return false;

            // Map the generic SCP role to the actual SCP slot type
            RoleTypeId mappedRole = MapVoteRole(role);

            _votes[player] = mappedRole;

            // Recalculate counts
            RecalculateCounts();

            // Check early-end threshold
            CheckEarlyEnd();

            return true;
        }

        public RoleTypeId GetAssignedRole(Player player)
        {
            if (!_votes.TryGetValue(player, out RoleTypeId voted))
            {
                // Non-voter: assign randomly
                return AssignRandomRole(new HashSet<Player>());
            }

            return voted;
        }

        public void EndVoting()
        {
            _active = false;
        }

        public void AssignRoles()
        {
            if (!_plugin.Config.EnableRoleVoting)
                return;

            List<Player> players = Player.ReadyList.ToList();
            if (players.Count == 0)
                return;

            // Determine SCP slot count from game default
            int scpSlots = GetDefaultScpSlotCount(players.Count);

            // Separate SCP voters from others
            List<Player> scpVoters = players.Where(p => _votes.TryGetValue(p, out RoleTypeId r) && IsScpRole(r)).ToList();
            List<Player> nonScpVoters = players.Where(p => !scpVoters.Contains(p)).ToList();

            // Shuffle and assign SCP slots
            scpVoters.Shuffle();
            HashSet<Player> assignedScps = new HashSet<Player>(scpVoters.Take(scpSlots));
            List<Player> remaining = players.Where(p => !assignedScps.Contains(p)).ToList();

            // For remaining players, assign based on votes or random
            foreach (Player player in remaining)
            {
                RoleTypeId assignedRole;
                if (_votes.TryGetValue(player, out RoleTypeId voted) && !IsScpRole(voted))
                {
                    assignedRole = voted;
                }
                else
                {
                    assignedRole = AssignRandomRole(assignedScps);
                }

                _plugin.Debug($"Assigned {player.Nickname} -> {assignedRole}");
            }

            EndVoting();
        }

        private bool IsScpRole(RoleTypeId role)
        {
            return role is RoleTypeId.Scp049 or RoleTypeId.Scp0492 or RoleTypeId.Scp096
                or RoleTypeId.Scp106 or RoleTypeId.Scp173 or RoleTypeId.Scp939
                or RoleTypeId.Scp3114 or RoleTypeId.Scp457;
        }

        private RoleTypeId MapVoteRole(RoleTypeId vote)
        {
            // .vote scp / .vote 1 maps to SCP-049 as a generic SCP placeholder
            if (vote == RoleTypeId.Scp049)
                return RoleTypeId.Scp049;
            return vote;
        }

        private RoleTypeId AssignRandomRole(HashSet<Player> exclude)
        {
            // Weighted random: more Class-D than Guard, more Guard than Scientist
            RoleTypeId[] pool = {
                RoleTypeId.ClassD, RoleTypeId.ClassD, RoleTypeId.ClassD,
                RoleTypeId.FacilityGuard, RoleTypeId.FacilityGuard,
                RoleTypeId.Scientist
            };
            return pool[UnityEngine.Random.Range(0, pool.Length)];
        }

        private int GetDefaultScpSlotCount(int playerCount)
        {
            // Match typical SCPSL default for low-pop
            return playerCount switch
            {
                <= 3 => 1,
                <= 6 => 1,
                <= 9 => 2,
                _ => 2
            };
        }

        private void RecalculateCounts()
        {
            _voteCounts.Clear();
            foreach (RoleTypeId role in _votes.Values)
            {
                _voteCounts.TryGetValue(role, out int count);
                _voteCounts[role] = count + 1;
            }
        }

        private void CheckEarlyEnd()
        {
            if (_earlyEndTimerStart > 0)
                return;

            float threshold = _plugin.Config.VotingEarlyEndThreshold;
            if (TotalPlayers == 0)
                return;

            float ratio = (float)TotalVoters / TotalPlayers;
            if (ratio >= threshold)
            {
                _earlyEndTimerStart = Time.realtimeSinceStartup;
                if (GameCore.RoundStart.singleton != null)
                {
                    GameCore.RoundStart.singleton.NetworkRoundStartTime = Time.realtimeSinceStartup + _plugin.Config.VotingEarlyEndCountdown;
                }
                _plugin.Debug("Early-end threshold reached. Shortening lobby timer.");
            }
        }
    }
}
```

- [ ] **Step 2: Add Shuffle extension method**

Add a file `SCPSL-MicroServer-Tweaks/Extensions.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace SCPSL_MicroServer_Tweaks
{
    internal static class Extensions
    {
        private static readonly Random Rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add SCPSL-MicroServer-Tweaks/VotingController.cs SCPSL-MicroServer-Tweaks/Extensions.cs
git commit -m "feat: add VotingController (state machine, tally, role assignment)"
```

---

### Task 4: Create VotingUI (DisplayKit Canvas)

**Files:**
- Create: `SCPSL-MicroServer-Tweaks/VotingUI.cs`

**Interfaces:**
- Consumes: `VotingController.VoteCounts`, `VotingController.TimeRemaining`, `VotingController.TotalVoters`, `VotingController.TotalPlayers`
- Produces: Canvas that updates every 0.5s via MEC coroutine

- [ ] **Step 1: Create VotingUI.cs**

```csharp
using System;
using System.Collections.Generic;
using DisplayKit.Elements;
using DisplayKit.Enums;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using UnityEngine;
using UnityEngine.UIElements;

namespace SCPSL_MicroServer_Tweaks
{
    public sealed class VotingUI
    {
        private readonly VotingController _controller;
        private DisplayCanvas _canvas;
        private DisplayText _titleText;
        private readonly Dictionary<RoleTypeId, DisplayElement> _roleRows = new Dictionary<RoleTypeId, DisplayElement>();
        private readonly Dictionary<RoleTypeId, DisplayText> _roleCountTexts = new Dictionary<RoleTypeId, DisplayText>();
        private readonly Dictionary<RoleTypeId, DisplayElement> _roleBars = new Dictionary<RoleTypeId, DisplayElement>();
        private DisplayText _footerText;
        private CoroutineHandle _updateHandle;

        private static readonly (RoleTypeId role, string name, string item, Color color)[] Roles = {
            (RoleTypeId.Scp049, "SCP", "SCP-018", new Color(1f, 0.2f, 0.2f)),
            (RoleTypeId.Scientist, "科學家", "對講機", new Color(0.2f, 0.6f, 1f)),
            (RoleTypeId.ClassD, "D級人員", "鎖", new Color(1f, 0.8f, 0.2f)),
            (RoleTypeId.FacilityGuard, "安保人員", "鑰匙卡", new Color(0.2f, 1f, 0.4f)),
        };

        public VotingUI(VotingController controller)
        {
            _controller = controller;
        }

        public void Show()
        {
            if (_canvas != null)
                return;

            _canvas = DisplayCanvas.Create();
            _canvas.DefaultVisibility = CanvasVisibility.Visible;
            _canvas.SortOrder = 100;
            _canvas.Flex.Grow = 1f;
            _canvas.Background.Color = new Color(0, 0, 0, 0.7f);
            _canvas.Align.AlignItems = Align.Center;
            _canvas.Align.JustifyContent = Justify.Center;
            _canvas.Text.Color = Color.white;

            // Main container
            DisplayElement container = _canvas.AddElement();
            container.Size.Width = 500f;
            container.Flex.Direction = FlexDirection.Column;
            container.Align.AlignItems = Align.Center;
            container.Background.Color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            container.Border.Color = Color.gray;
            container.Border.Width = 2f;
            container.Border.Radius = 10f;
            container.Spacing.PaddingAll = 20f;

            // Title
            _titleText = container.AddText("🗳️ 身份投票");
            _titleText.Text.FontSize = 28f;
            _titleText.Text.Font = FontType.RobotoBold;
            _titleText.Text.Color = Color.yellow;
            _titleText.Spacing.MarginBottom = 15f;

            // Role rows
            foreach (var (role, name, item, color) in Roles)
            {
                DisplayElement row = container.AddElement();
                row.Flex.Direction = FlexDirection.Row;
                row.Align.AlignItems = Align.Center;
                row.Spacing.MarginBottom = 8f;
                row.Size.Width = Length.Percent(100f);

                // Label
                DisplayText label = row.AddText($"[{GetIndex(role)}] {name} ({item})");
                label.Text.FontSize = 18f;
                label.Text.Color = color;
                label.Text.Font = FontType.RobotoBold;
                label.Size.Width = 180f;

                // Bar background
                DisplayElement barBg = row.AddElement();
                barBg.Size.Width = 200f;
                barBg.Size.Height = 20f;
                barBg.Background.Color = new Color(0.3f, 0.3f, 0.3f);
                barBg.Border.Radius = 3f;

                // Bar fill
                DisplayElement barFill = barBg.AddElement();
                barFill.Size.Width = 0f;
                barFill.Size.Height = Length.Percent(100f);
                barFill.Background.Color = color;
                barFill.Border.Radius = 3f;

                _roleBars[role] = barFill;

                // Count text
                DisplayText countText = row.AddText("0 票");
                countText.Text.FontSize = 16f;
                countText.Text.Font = FontType.RobotoMonoBold;
                countText.Text.Color = Color.white;
                countText.Size.Width = 60f;
                countText.Text.Align = TextAnchor.MiddleRight;

                _roleCountTexts[role] = countText;
                _roleRows[role] = row;
            }

            // Footer
            _footerText = container.AddText("");
            _footerText.Text.FontSize = 16f;
            _footerText.Text.Color = Color.gray;
            _footerText.Spacing.MarginTop = 15f;

            // Instruction text
            DisplayText instruction = container.AddText("輸入 .1 .2 .3 .4 投票");
            instruction.Text.FontSize = 14f;
            instruction.Text.Color = new Color(0.7f, 0.7f, 0.7f);
            instruction.Spacing.MarginTop = 5f;

            _canvas.Spawn();

            // Start update loop
            _updateHandle = Timing.RunCoroutine(UpdateLoop());
        }

        public void Hide()
        {
            if (_updateHandle.IsValid)
                Timing.KillCoroutines(_updateHandle);

            _canvas?.Destroy();
            _canvas = null;
        }

        private IEnumerator<float> UpdateLoop()
        {
            while (true)
            {
                UpdateDisplay();
                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        private void UpdateDisplay()
        {
            if (_canvas == null || !_controller.IsActive)
                return;

            int maxVotes = 1;
            foreach (int count in _controller.VoteCounts.Values)
            {
                if (count > maxVotes)
                    maxVotes = count;
            }

            foreach (var (role, _, _, color) in Roles)
            {
                _controller.VoteCounts.TryGetValue(role, out int count);
                _roleCountTexts[role].Content = $"{count} 票";

                float barWidth = (float)count / maxVotes * 200f;
                _roleBars[role].Size.Width = barWidth;
            }

            int remaining = (int)Math.Ceiling(_controller.TimeRemaining);
            _footerText.Content = $"剩餘時間: {remaining}s  ·  已投票: {_controller.TotalVoters}/{_controller.TotalPlayers}";
        }

        private static string GetIndex(RoleTypeId role)
        {
            return role switch
            {
                RoleTypeId.Scp049 => "1",
                RoleTypeId.Scientist => "2",
                RoleTypeId.ClassD => "3",
                RoleTypeId.FacilityGuard => "4",
                _ => "?"
            };
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add SCPSL-MicroServer-Tweaks/VotingUI.cs
git commit -m "feat: add VotingUI with DisplayKit real-time vote display"
```

---

### Task 5: Integrate into Plugin and EventHandlers

**Files:**
- Modify: `SCPSL-MicroServer-Tweaks/EventHandlers.cs`
- Modify: `SCPSL-MicroServer-Tweaks/SCPSL_MicroServer_TweaksPlugin.cs`

**Interfaces:**
- Consumes: `VotingController`, `VotingUI`, `PlayerChangingRoleEventArgs`

- [ ] **Step 1: Update EventHandlers.cs**

```csharp
using LabApi.Events.CustomHandlers;
using LabApi.Events.Arguments.PlayerEvents;
using PlayerRoles;

namespace SCPSL_MicroServer_Tweaks
{
    internal sealed class EventHandlers : CustomEventsHandler
    {
        private readonly SCPSL_MicroServer_TweaksPlugin _plugin;

        public EventHandlers(SCPSL_MicroServer_TweaksPlugin plugin)
        {
            _plugin = plugin;
        }

        public override void OnServerWaitingForPlayers()
        {
            _plugin.FreezeController.CancelFreeze(false);
            _plugin.ApplyStartingTokens();

            if (_plugin.Config.EnableRoleVoting)
                _plugin.VotingController.StartVoting();
        }

        public override void OnServerRoundStarted()
        {
            _plugin.FreezeController.ScheduleFreeze();
            _plugin.VotingController.EndVoting();
        }

        public override void OnPlayerChangingRole(PlayerChangingRoleEventArgs args)
        {
            if (!_plugin.Config.EnableRoleVoting)
                return;

            // Only intercept initial role assignment at round start
            if (args.ChangeReason != RoleChangeReason.RoundStart)
                return;

            // Get the assigned role from voting results
            RoleTypeId votedRole = _plugin.VotingController.GetAssignedRole(args.Player);
            if (votedRole != args.NewRole)
            {
                args.NewRole = votedRole;
            }
        }
    }
}
```

- [ ] **Step 2: Update Plugin.cs**

Add VotingController property and wire it up:

```csharp
// In the class body, add after FreezeController:
internal VotingController VotingController { get; private set; }
internal VotingUI VotingUI { get; private set; }
```

In `Enable()`, after `FreezeController.Initialize(this)`:

```csharp
VotingController = new VotingController(this);
VotingUI = new VotingUI(VotingController);
```

In `Disable()`, add cleanup:

```csharp
if (VotingUI != null)
    VotingUI.Hide();
VotingUI = null;
VotingController = null;
```

In `OnServerWaitingForPlayers` handler (via EventHandlers), the VotingController.StartVoting() call will show the UI. But we need to trigger the UI from the VotingController. Let me have the VotingController start the UI.

Actually, let me have the VotingController.StartVoting() also call VotingUI.Show(). And VotingController.EndVoting() calls VotingUI.Hide().

Update VotingController.StartVoting() to include:

```csharp
public void StartVoting()
{
    // ... existing code ...
    _plugin.VotingUI.Show();
}
```

Update VotingController.EndVoting() to include:

```csharp
public void EndVoting()
{
    _active = false;
    _plugin.VotingUI.Hide();
}
```

- [ ] **Step 3: Full updated Plugin.cs body**

```csharp
using System;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Console;
using Logger = LabApi.Features.Console.Logger;	
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using PlayerRoles;
using Respawning;
using Respawning.Waves;
using Respawning.Waves.Generic;
using UnityEngine;

namespace SCPSL_MicroServer_Tweaks
{
    public sealed class SCPSL_MicroServer_TweaksPlugin : Plugin<PluginConfig>
    {
        private EventHandlers _eventHandlers;
        private GameObject _controllerObject;

        public static SCPSL_MicroServer_TweaksPlugin Instance { get; private set; }

        public override string Name { get; } = "SCPSL-MicroServer-Tweaks";
        public override string Description { get; } =
            "Freezes initial SCP movement briefly, configures starting NTF/Chaos Respawn Tokens, and enables lobby role voting.";
        public override string Author { get; } = "Custom Server";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredApiVersion { get; } =
            new Version(LabApiProperties.CompiledVersion);
        public override string ConfigFileName { get; set; } = "config.yml";

        public new PluginConfig Config
        {
            get { return base.Config; }
        }

        internal FreezeController FreezeController { get; private set; }
        internal VotingController VotingController { get; private set; }
        internal VotingUI VotingUI { get; private set; }

        public override void Enable()
        {
            string validationError;
            if (!Config.Validate(out validationError))
            {
                Logger.Error("[SCPSL-MicroServer-Tweaks] Invalid configuration: " + validationError);
                return;
            }

            Instance = this;

            _controllerObject = new GameObject("SCPSL_MicroServer_Tweaks.Controller");
            UnityEngine.Object.DontDestroyOnLoad(_controllerObject);

            FreezeController = _controllerObject.AddComponent<FreezeController>();
            FreezeController.Initialize(this);

            VotingController = new VotingController(this);
            VotingUI = new VotingUI(VotingController);

            _eventHandlers = new EventHandlers(this);
            CustomHandlersManager.RegisterEventsHandler(_eventHandlers);

            Logger.Info("[SCPSL-MicroServer-Tweaks] Enabled.");
        }

        public override void Disable()
        {
            if (_eventHandlers != null)
                CustomHandlersManager.UnregisterEventsHandler(_eventHandlers);

            if (FreezeController != null)
                FreezeController.CancelFreeze(false);

            if (VotingUI != null)
                VotingUI.Hide();

            if (_controllerObject != null)
                UnityEngine.Object.Destroy(_controllerObject);

            _eventHandlers = null;
            FreezeController = null;
            VotingController = null;
            VotingUI = null;
            _controllerObject = null;
            Instance = null;

            Logger.Info("[SCPSL-MicroServer-Tweaks] Disabled.");
        }

        internal void ApplyStartingTokens()
        {
            if (!Config.EnableStartingTokens)
                return;

            try
            {
                int modifiedWaves = 0;

                foreach (SpawnableWaveBase spawnableWave in WaveManager.Waves)
                {
                    ILimitedWave limitedWave = spawnableWave as ILimitedWave;
                    if (limitedWave == null)
                        continue;

                    RespawnWave wave = RespawnWaves.Get(spawnableWave);
                    if (wave == null || wave is MiniRespawnWave)
                        continue;

                    int configuredTokens = GetConfiguredTokens(wave.Faction);
                    if (configuredTokens < 0)
                        continue;

                    int nextBaseTokens = ApplyTokenMode(wave.RespawnTokens, configuredTokens);
                    int nextRemainingTokens = ApplyTokenMode(limitedWave.RespawnTokens, configuredTokens);

                    wave.RespawnTokens = nextBaseTokens;
                    limitedWave.RespawnTokens = nextRemainingTokens;

                    WaveUpdateMessage.ServerSendUpdate(spawnableWave, UpdateMessageFlags.Tokens);
                    modifiedWaves++;

                    Debug(
                        string.Format(
                            "{0}: base={1}, remaining={2}, faction={3}.",
                            spawnableWave.GetType().Name,
                            nextBaseTokens,
                            nextRemainingTokens,
                            wave.Faction));
                }

                int totalRemainingTokens = 0;

                foreach (SpawnableWaveBase spawnableWave in WaveManager.Waves)
                {
                    ILimitedWave limitedWave = spawnableWave as ILimitedWave;
                    if (limitedWave == null)
                        continue;

                    RespawnWave wave = RespawnWaves.Get(spawnableWave);
                    if (wave == null || wave is MiniRespawnWave)
                        continue;

                    if (limitedWave.RespawnTokens > 0)
                        totalRemainingTokens += limitedWave.RespawnTokens;
                }

                RespawnTokensManager.AvailableRespawnsLeft = totalRemainingTokens;

                Debug(
                    string.Format(
                        "Starting tokens applied to {0} primary wave(s); global remaining={1}.",
                        modifiedWaves,
                        totalRemainingTokens));
            }
            catch (Exception exception)
            {
                Logger.Error("[SCPSL-MicroServer-Tweaks] Failed to apply starting Respawn Tokens:\n" + exception);
            }
        }

        internal void Debug(string message)
        {
            if (Config.EnableDebugLogging)
                Logger.Debug("[SCPSL-MicroServer-Tweaks] " + message);
        }

        private int GetConfiguredTokens(Faction faction)
        {
            switch (faction)
            {
                case Faction.FoundationStaff:
                    return Config.NtfStartingTokens;

                case Faction.FoundationEnemy:
                    return Config.ChaosStartingTokens;

                default:
                    return -1;
            }
        }

        private int ApplyTokenMode(int currentValue, int configuredValue)
        {
            int result;

            if (Config.StartingTokenMode == TokenApplyMode.Add)
                result = Math.Max(0, currentValue) + configuredValue;
            else
                result = configuredValue;

            if (result < 0)
                result = 0;

            if (result > Config.MaximumTokenValue)
                result = Config.MaximumTokenValue;

            return result;
        }
    }
}
```

- [ ] **Step 4: Commit**

```bash
git add SCPSL-MicroServer-Tweaks/EventHandlers.cs SCPSL-MicroServer-Tweaks/SCPSL_MicroServer_TweaksPlugin.cs
git commit -m "feat: integrate VotingController and VotingUI into plugin lifecycle"
```

---

### Task 6: Add UIElementModule references and update config.example.yml

**Files:**
- Modify: `SCPSL-MicroServer-Tweaks/SCPSL-MicroServer-Tweaks.csproj`
- Modify: `config.example.yml`

- [ ] **Step 1: Update csproj with DisplayKit dependencies**

```xml
<Reference Include="UnityEngine.UIElementsModule">
  <HintPath>..\Binaries\UnityEngine.UIElementsModule.dll</HintPath>
  <Private>false</Private>
</Reference>
<Reference Include="UnityEngine.TextRenderingModule">
  <HintPath>..\Binaries\UnityEngine.TextRenderingModule.dll</HintPath>
  <Private>false</Private>
</Reference>
```

Add after the UnityEngine.CoreModule reference.

- [ ] **Step 2: Update config.example.yml**

Add at the end of the file:

```yaml
# Role Voting
enable_role_voting: true
voting_time_seconds: 45
lobby_timer_seconds: 60
voting_early_end_threshold: 0.75
voting_early_end_countdown: 10
```

- [ ] **Step 3: Commit**

```bash
git add SCPSL-MicroServer-Tweaks/SCPSL-MicroServer-Tweaks.csproj config.example.yml
git commit -m "feat: add DisplayKit refs and voting config example"
```

---

### Task 7: Build and verify

**Files:** None (build process)

- [ ] **Step 1: Build the project**

```bash
cd /path/to/project
./build.sh --server-path "/path/to/scpsl/server"
```

Expected: build succeeds, DLL output at `SCPSL-MicroServer-Tweaks/bin/Release/net48/SCPSL_MicroServer_Tweaks.dll`

- [ ] **Step 2: Deploy and test**

```bash
./build.sh --server-path "/path/to/scpsl/server" --deploy
```

Restart server, verify:
- Voting UI appears in lobby
- `.1` `.2` `.3` `.4` commands register votes
- Vote counts update in real-time
- Round starts with correct role assignments