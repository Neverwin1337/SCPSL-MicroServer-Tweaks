using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace SCPSL_MicroServer_Tweaks
{
    public sealed class VotingController : MonoBehaviour
    {
        internal static readonly (RoleTypeId role, string shortName, string displayName)[] Roles = {
            (RoleTypeId.Scp049, "SCP", "SCP"),
            (RoleTypeId.Scientist, "Scientists", "科學家"),
            (RoleTypeId.ClassD, "ClassD", "D級人員"),
            (RoleTypeId.FacilityGuard, "Guard", "安保人員"),
        };

        private SCPSL_MicroServer_TweaksPlugin _plugin;
        private readonly Dictionary<Player, RoleTypeId> _votes = new Dictionary<Player, RoleTypeId>();
        private readonly Dictionary<RoleTypeId, int> _voteCounts = new Dictionary<RoleTypeId, int>();
        private readonly Dictionary<Player, RoleTypeId> _assignments = new Dictionary<Player, RoleTypeId>();
        private bool _active;
        private bool _assignmentsReady;
        private float _lobbyTimerSetAt;
        private float _nextHintAt;

        public bool IsActive => _active;
        public IReadOnlyDictionary<RoleTypeId, int> VoteCounts => _voteCounts;
        public int TotalVoters => _votes.Count;
        public int TotalPlayers => Player.ReadyList.Count();
        public float TimeRemaining => _active
            ? Math.Max(0, _plugin.Config.VotingTimeSeconds - (Time.realtimeSinceStartup - _lobbyTimerSetAt))
            : 0;

        public void Initialize(SCPSL_MicroServer_TweaksPlugin plugin)
        {
            _plugin = plugin;
        }

        public void StartVoting()
        {
            _votes.Clear();
            _voteCounts.Clear();
            _assignments.Clear();
            _active = true;
            _assignmentsReady = false;
            _lobbyTimerSetAt = Time.realtimeSinceStartup;
            _nextHintAt = 0f;
            SendVoteHints();
        }

        public bool TryVote(Player player, RoleTypeId role)
        {
            if (!_active || player == null)
                return false;

            _votes[player] = role;
            RecalculateCounts();
            return true;
        }

        public void EnsureAssignmentsReady()
        {
            if (_assignmentsReady)
                return;

            _assignmentsReady = true;
            _active = false;

            List<Player> players = Player.ReadyList.ToList();
            if (players.Count == 0)
                return;

            int scpSlots = GetDefaultScpSlotCount(players.Count);

            List<Player> scpVoters = players
                .Where(p => _votes.TryGetValue(p, out RoleTypeId r) && r == RoleTypeId.Scp049)
                .ToList();

            scpVoters.Shuffle();
            foreach (Player p in scpVoters.Take(scpSlots))
            {
                RoleTypeId actualScp = PickRandomScp();
                _assignments[p] = actualScp;
            }

            List<Player> remaining = players.Where(p => !_assignments.ContainsKey(p)).ToList();
            remaining.Shuffle();

            foreach (Player p in remaining)
            {
                if (_votes.TryGetValue(p, out RoleTypeId voted) && voted != RoleTypeId.Scp049)
                {
                    _assignments[p] = voted;
                }
                else
                {
                    _assignments[p] = RoleTypeId.ClassD;
                }
            }
        }

        public RoleTypeId GetAssignedRole(Player player)
        {
            EnsureAssignmentsReady();
            if (_assignments.TryGetValue(player, out RoleTypeId role))
                return role;
            return RoleTypeId.ClassD;
        }

        public void EndVoting()
        {
            _active = false;
        }

        private void Update()
        {
            if (_plugin == null || !_active)
                return;

            float now = Time.realtimeSinceStartup;
            if (TimeRemaining <= 0f)
                return;

            if (now >= _nextHintAt)
            {
                SendVoteHints();
                _nextHintAt = now + _plugin.Config.VoteHintIntervalSeconds;
            }
        }

        private void SendVoteHints()
        {
            int remaining = (int)Math.Ceiling(TimeRemaining);
            string header = string.Format(
                "<size=28><color=#ffaa00>身份投票 - 剩餘 {0}s ({1}/{2} 已投票)</color></size>",
                remaining, TotalVoters, TotalPlayers);

            string body = "\n<size=22>";
            foreach (var (role, _, displayName) in Roles)
            {
                _voteCounts.TryGetValue(role, out int count);
                body += string.Format("<color={0}>{1}: {2} 票</color>\n",
                    GetRoleColor(role), displayName, count);
            }
            body += "</size>";
            body += "\n<size=16><color=#cccccc>在控制台輸入 .1 .2 .3 .4 投票</color></size>";

            string fullHint = header + body;
            float duration = _plugin.Config.VoteHintIntervalSeconds + 0.25f;

            foreach (Player player in Player.ReadyList)
            {
                player.SendHint(fullHint, duration);
            }
        }

        private static string GetRoleColor(RoleTypeId role)
        {
            return role switch
            {
                RoleTypeId.Scp049 => "#ff5555",
                RoleTypeId.Scientist => "#55aaff",
                RoleTypeId.ClassD => "#ffaa55",
                RoleTypeId.FacilityGuard => "#55ff88",
                _ => "#ffffff"
            };
        }

        private RoleTypeId PickRandomScp()
        {
            RoleTypeId[] scps = {
                RoleTypeId.Scp049, RoleTypeId.Scp096, RoleTypeId.Scp106,
                RoleTypeId.Scp173, RoleTypeId.Scp939, RoleTypeId.Scp3114
            };
            return scps[UnityEngine.Random.Range(0, scps.Length)];
        }

        private int GetDefaultScpSlotCount(int playerCount)
        {
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
                _voteCounts.TryGetValue(role, out int cnt);
                _voteCounts[role] = cnt + 1;
            }
        }
    }
}
