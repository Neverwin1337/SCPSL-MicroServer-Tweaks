using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace SmallVanillaFlow
{
    /// <summary>
    /// Unity main-thread controller. This intentionally avoids MEC and EXILED dependencies.
    /// </summary>
    public sealed class FreezeController : MonoBehaviour
    {
        private readonly Dictionary<Player, Vector3> _anchors = new Dictionary<Player, Vector3>();

        private SmallVanillaFlowPlugin _plugin;
        private bool _pendingCapture;
        private bool _active;
        private float _captureAt;
        private float _releaseAt;
        private float _nextHintAt;
        private float _plannedDuration;
        private int _roundPlayerCount;

        public void Initialize(SmallVanillaFlowPlugin plugin)
        {
            _plugin = plugin;
        }

        public void ScheduleFreeze()
        {
            CancelFreeze(false);

            PluginConfig config = _plugin.Config;
            if (!config.EnableScpFreeze)
                return;

            _roundPlayerCount = Player.ReadyList.Count();
            _plannedDuration = config.GetScpFreezeSeconds(_roundPlayerCount);

            if (_plannedDuration <= 0f)
            {
                _plugin.Debug("SCP freeze skipped because the configured duration is 0.");
                return;
            }

            _captureAt = Time.realtimeSinceStartup + config.FreezeCaptureDelaySeconds;
            _pendingCapture = true;

            _plugin.Debug(
                string.Format(
                    "Scheduled SCP position capture in {0:0.00}s; freeze duration {1:0.00}s for {2} ready players.",
                    config.FreezeCaptureDelaySeconds,
                    _plannedDuration,
                    _roundPlayerCount));
        }

        public void CancelFreeze(bool showReleaseHint)
        {
            _pendingCapture = false;

            if (_active && showReleaseHint)
                SendReleaseHint();

            _active = false;
            _anchors.Clear();
        }

        private void Update()
        {
            if (_plugin == null)
                return;

            float now = Time.realtimeSinceStartup;

            if (_pendingCapture && now >= _captureAt)
                CaptureInitialScps(now);

            if (!_active)
                return;

            float remaining = _releaseAt - now;
            if (remaining <= 0f)
            {
                _plugin.Debug("SCP movement restriction ended.");
                CancelFreeze(true);
                return;
            }

            KeepScpsAtAnchors();

            if (_plugin.Config.ShowCountdownHint && now >= _nextHintAt)
            {
                SendCountdownHint(remaining);
                _nextHintAt = now + _plugin.Config.CountdownHintIntervalSeconds;
            }
        }

        private void CaptureInitialScps(float now)
        {
            _pendingCapture = false;
            _anchors.Clear();

            foreach (Player player in Player.ReadyList)
            {
                if (!IsMovableScp(player))
                    continue;

                _anchors[player] = player.Position;
            }

            if (_anchors.Count == 0)
            {
                _plugin.Debug("No movable SCP player was found when the freeze began.");
                return;
            }

            _releaseAt = now + _plannedDuration;
            _nextHintAt = 0f;
            _active = true;

            _plugin.Debug(string.Format("Frozen {0} initial SCP player(s).", _anchors.Count));
            KeepScpsAtAnchors();

            if (_plugin.Config.ShowCountdownHint)
                SendCountdownHint(_plannedDuration);
        }

        private void KeepScpsAtAnchors()
        {
            float tolerance = _plugin.Config.PositionTolerance;
            float toleranceSquared = tolerance * tolerance;

            // Copy the keys because players may disconnect or change roles during this loop.
            List<Player> players = new List<Player>(_anchors.Keys);

            foreach (Player player in players)
            {
                if (!IsMovableScp(player) || !Player.ReadyList.Contains(player))
                {
                    _anchors.Remove(player);
                    continue;
                }

                Vector3 anchor = _anchors[player];
                if ((player.Position - anchor).sqrMagnitude > toleranceSquared)
                    player.Position = anchor;
            }

            if (_anchors.Count == 0)
                CancelFreeze(false);
        }

        private void SendCountdownHint(float remaining)
        {
            int seconds = Math.Max(0, (int)Math.Ceiling(remaining));
            string text = FormatHint(_plugin.Config.CountdownHintText, seconds);

            foreach (Player player in _anchors.Keys.ToArray())
            {
                if (!IsMovableScp(player) || !Player.ReadyList.Contains(player))
                    continue;

                player.SendHint(text, _plugin.Config.CountdownHintIntervalSeconds + 0.25f);
            }
        }

        private void SendReleaseHint()
        {
            string text = FormatHint(_plugin.Config.ReleaseHintText, 0);

            foreach (Player player in _anchors.Keys.ToArray())
            {
                if (!IsMovableScp(player) || !Player.ReadyList.Contains(player))
                    continue;

                player.SendHint(text, _plugin.Config.ReleaseHintDurationSeconds);
            }
        }

        private string FormatHint(string template, int seconds)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            return template
                .Replace("{seconds}", seconds.ToString())
                .Replace("{player_count}", _roundPlayerCount.ToString());
        }

        private static bool IsMovableScp(Player player)
        {
            return player != null &&
                   player.IsSCP &&
                   player.Role != RoleTypeId.Scp079;
        }
    }
}
