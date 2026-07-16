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

            error = null;
            return true;
        }
    }
}