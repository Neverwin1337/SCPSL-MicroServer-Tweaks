using System.Collections.Generic;
using System.ComponentModel;

namespace SCPSL_MicroServer_Tweaks
{
    public sealed class PluginConfig
    {
        [Description("Whether initial movable SCPs are kept at their spawn position after the round starts.")]
        public bool EnableScpFreeze { get; set; } = true;

        [Description("Fallback freeze duration in seconds. Used when the current player count has no override.")]
        public float DefaultScpFreezeSeconds { get; set; } = 60f;

        [Description("Use ScpFreezeSecondsByPlayerCount before falling back to DefaultScpFreezeSeconds.")]
        public bool UsePlayerCountOverrides { get; set; } = true;

        [Description("Freeze duration by READY player count. Recommended for a 5-8 player server.")]
        public Dictionary<int, float> ScpFreezeSecondsByPlayerCount { get; set; } =
            new Dictionary<int, float>
            {
                { 5, 75f },
                { 6, 60f },
                { 7, 45f },
                { 8, 40f },
            };

        [Description("Delay before recording SCP spawn positions. Gives the game time to finish assigning and spawning roles.")]
        public float FreezeCaptureDelaySeconds { get; set; } = 0.5f;

        [Description("How far an SCP may drift from its recorded position before being moved back.")]
        public float PositionTolerance { get; set; } = 0.03f;

        [Description("Show a countdown hint to frozen SCP players.")]
        public bool ShowCountdownHint { get; set; } = true;

        [Description("How often the countdown hint is refreshed, in seconds.")]
        public float CountdownHintIntervalSeconds { get; set; } = 1f;

        [Description("Hint shown while frozen. Available placeholders: {seconds}, {player_count}.")]
        public string CountdownHintText { get; set; } =
            "<size=32><color=#ff5555>收容程序仍在運作</color>\n" +
            "<color=white>解除收容：{seconds} 秒</color></size>";

        [Description("Hint shown when SCP movement is released.")]
        public string ReleaseHintText { get; set; } =
            "<size=32><color=#ff5555>收容失效</color>\n" +
            "<color=white>你現在可以移動。</color></size>";

        [Description("How long the release hint remains visible.")]
        public float ReleaseHintDurationSeconds { get; set; } = 4f;

        [Description("Whether to alter NTF and Chaos starting primary-wave Respawn Tokens every round.")]
        public bool EnableStartingTokens { get; set; } = true;

        [Description("Set replaces vanilla tokens. Add adds the configured amount to vanilla tokens.")]
        public TokenApplyMode StartingTokenMode { get; set; } = TokenApplyMode.Set;

        [Description("NTF primary-wave starting tokens. -1 leaves this faction unchanged.")]
        public int NtfStartingTokens { get; set; } = 2;

        [Description("Chaos primary-wave starting tokens. -1 leaves this faction unchanged.")]
        public int ChaosStartingTokens { get; set; } = 2;

        [Description("Safety cap used by both Set and Add modes.")]
        public int MaximumTokenValue { get; set; } = 20;

        [Description("Print additional diagnostic messages to the server console.")]
        public bool EnableDebugLogging { get; set; } = false;

        [Description("Whether players can vote for their initial role in the lobby.")]
        public bool EnableRoleVoting { get; set; } = true;

        [Description("How many seconds voting lasts in the lobby.")]
        public float VotingTimeSeconds { get; set; } = 45f;

        [Description("How often the vote hint is refreshed, in seconds.")]
        public float VoteHintIntervalSeconds { get; set; } = 1f;

        [Description("Display name for the SCP role in vote hints.")]
        public string VoteRoleScpDisplayName { get; set; } = "SCP";

        [Description("Display name for the Scientist role in vote hints.")]
        public string VoteRoleScientistDisplayName { get; set; } = "科學家 Scientists";

        [Description("Display name for the Class-D role in vote hints.")]
        public string VoteRoleClassDDisplayName { get; set; } = "D級人員 ClassD";

        [Description("Display name for the Facility Guard role in vote hints.")]
        public string VoteRoleGuardDisplayName { get; set; } = "安保人員 Guard";

        [Description("Vote hint header format. Placeholders: {0}=remaining seconds, {1}=voters, {2}=total players.")]
        public string VoteHintHeaderFormat { get; set; } =
            "<size=28><color=#ffaa00>身份投票 - 剩餘 {0}s ({1}/{2} 已投票)</color></size>";

        [Description("Vote hint body line format. Placeholders: {0}=color hex, {1}=role display name, {2}=vote count.")]
        public string VoteHintBodyLineFormat { get; set; } =
            "<color={0}>{1}: {2} 票</color>\n";

        [Description("Vote hint instruction text shown at the bottom.")]
        public string VoteHintInstruction { get; set; } =
            "\n<size=16><color=#cccccc>點鍵盤左上角~開控制臺 輸入 .1 .2 .3 .4 投票</color></size>";

        [Description("Whether timed random events are enabled during rounds.")]
        public bool EnableRandomEvents { get; set; } = true;

        [Description("Minimum interval between random events, in seconds (180 = 3 minutes).")]
        public float RandomEventMinIntervalSeconds { get; set; } = 180f;

        [Description("Maximum interval between random events, in seconds (300 = 5 minutes).")]
        public float RandomEventMaxIntervalSeconds { get; set; } = 300f;

        [Description("Minimum alive players required for random events to trigger.")]
        public int RandomEventMinPlayers { get; set; } = 1;

        [Description("How long elevators are disabled during the Elevator Malfunction event, in seconds.")]
        public float RandomEventElevatorLockDuration { get; set; } = 60f;

        [Description("How long humans are invisible and silent during the Stealth event, in seconds.")]
        public float RandomEventStealthDuration { get; set; } = 30f;

        [Description("How long lights are off during the Blackout event, in seconds (180 = 3 minutes).")]
        public float RandomEventBlackoutDuration { get; set; } = 180f;

        [Description("Minimum minutes after round start before the Nuke Alert event can trigger.")]
        public float RandomEventNukeMinMinutes { get; set; } = 10f;

        [Description("Countdown duration for the Nuke Alert event, in seconds.")]
        public float RandomEventNukeCountdownSeconds { get; set; } = 60f;

        [Description("Chance (0-1) that the nuke alert is a false alarm. 0.5 = 50%.")]
        public float RandomEventNukeFalseAlarmChance { get; set; } = 0.5f;

        [Description("Broadcast message for the Elevator Malfunction event.")]
        public string EventElevatorBroadcast { get; set; } = "電梯故障：所有電梯已停用";

        [Description("Countdown hint format for the Elevator Malfunction event. {0}=seconds remaining.")]
        public string EventElevatorHintFormat { get; set; } =
            "<size=24><color=#ff9900>電梯故障中</color> <color=white>剩餘 {0} 秒</color></size>";

        [Description("Broadcast message for the All Doors Open event.")]
        public string EventDoorsBroadcast { get; set; } = "設施門禁解除：所有門已開啟";

        [Description("Broadcast message for the Stealth event.")]
        public string EventStealthBroadcast { get; set; } = "隱形協議啟動：所有人類隱形且靜音";

        [Description("Countdown hint format for the Stealth event. {0}=seconds remaining.")]
        public string EventStealthHintFormat { get; set; } =
            "<size=24><color=#66ffcc>隱形協議中</color> <color=white>剩餘 {0} 秒</color></size>";

        [Description("Broadcast message for the Blackout event.")]
        public string EventBlackoutBroadcast { get; set; } = "設施停電：全設施停電中";

        [Description("Countdown hint format for the Blackout event. {0}=seconds remaining.")]
        public string EventBlackoutHintFormat { get; set; } =
            "<size=24><color=#ff5555>設施停電中</color> <color=white>剩餘 {0} 秒</color></size>";

        [Description("Broadcast message for the Nuke Alert event.")]
        public string EventNukeBroadcast { get; set; } = "核彈警報：偵測到核彈啟動序列";

        [Description("Broadcast message when the nuke alert is a false alarm.")]
        public string EventNukeFalseAlarmBroadcast { get; set; } = "虛警：核彈警報已取消";

        [Description("Broadcast message for the Scramble event.")]
        public string EventScrambleBroadcast { get; set; } = "空間異常：所有人員隨機傳送";

        public float GetScpFreezeSeconds(int readyPlayerCount)
        {
            float configured;

            if (UsePlayerCountOverrides &&
                ScpFreezeSecondsByPlayerCount != null &&
                ScpFreezeSecondsByPlayerCount.TryGetValue(readyPlayerCount, out configured))
            {
                return configured;
            }

            return DefaultScpFreezeSeconds;
        }

        public bool Validate(out string error)
        {
            if (DefaultScpFreezeSeconds < 0f)
            {
                error = "DefaultScpFreezeSeconds cannot be negative.";
                return false;
            }

            if (FreezeCaptureDelaySeconds < 0f)
            {
                error = "FreezeCaptureDelaySeconds cannot be negative.";
                return false;
            }

            if (PositionTolerance < 0f)
            {
                error = "PositionTolerance cannot be negative.";
                return false;
            }

            if (CountdownHintIntervalSeconds < 0.1f)
            {
                error = "CountdownHintIntervalSeconds must be at least 0.1.";
                return false;
            }

            if (ReleaseHintDurationSeconds < 0f)
            {
                error = "ReleaseHintDurationSeconds cannot be negative.";
                return false;
            }

            if (NtfStartingTokens < -1 || ChaosStartingTokens < -1)
            {
                error = "Starting token values must be -1 or greater.";
                return false;
            }

            if (MaximumTokenValue < 0)
            {
                error = "MaximumTokenValue cannot be negative.";
                return false;
            }

            if (ScpFreezeSecondsByPlayerCount != null)
            {
                foreach (KeyValuePair<int, float> pair in ScpFreezeSecondsByPlayerCount)
                {
                    if (pair.Key < 1 || pair.Value < 0f)
                    {
                        error = "ScpFreezeSecondsByPlayerCount keys must be >= 1 and values cannot be negative.";
                        return false;
                    }
                }
            }

            if (EnableRoleVoting)
            {
                if (VotingTimeSeconds < 10f)
                {
                    error = "VotingTimeSeconds must be at least 10.";
                    return false;
                }

                if (VoteHintIntervalSeconds < 0.1f)
                {
                    error = "VoteHintIntervalSeconds must be at least 0.1.";
                    return false;
                }
            }

            if (EnableRandomEvents)
            {
                if (RandomEventMinIntervalSeconds < 10f)
                {
                    error = "RandomEventMinIntervalSeconds must be at least 10.";
                    return false;
                }

                if (RandomEventMaxIntervalSeconds < RandomEventMinIntervalSeconds)
                {
                    error = "RandomEventMaxIntervalSeconds cannot be less than RandomEventMinIntervalSeconds.";
                    return false;
                }

                if (RandomEventMinPlayers < 1)
                {
                    error = "RandomEventMinPlayers must be at least 1.";
                    return false;
                }

                if (RandomEventElevatorLockDuration < 1f)
                {
                    error = "RandomEventElevatorLockDuration must be at least 1.";
                    return false;
                }

                if (RandomEventStealthDuration < 1f)
                {
                    error = "RandomEventStealthDuration must be at least 1.";
                    return false;
                }

                if (RandomEventBlackoutDuration < 1f)
                {
                    error = "RandomEventBlackoutDuration must be at least 1.";
                    return false;
                }

                if (RandomEventNukeMinMinutes < 0f)
                {
                    error = "RandomEventNukeMinMinutes cannot be negative.";
                    return false;
                }

                if (RandomEventNukeCountdownSeconds < 10f)
                {
                    error = "RandomEventNukeCountdownSeconds must be at least 10.";
                    return false;
                }

                if (RandomEventNukeFalseAlarmChance < 0f || RandomEventNukeFalseAlarmChance > 1f)
                {
                    error = "RandomEventNukeFalseAlarmChance must be between 0 and 1.";
                    return false;
                }
            }

            error = null;
            return true;
        }
    }
}