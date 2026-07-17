using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;
using RueI.API.Elements;
using UnityEngine;

namespace SCPSL_MicroServer_Tweaks
{
    public sealed class VotingController : MonoBehaviour
    {
        private static readonly Tag VoteHintTag = new Tag("mst_vote_hint");

        internal static readonly (RoleTypeId role, string shortName)[] Roles = {
            (RoleTypeId.Scp049, "SCP"),
            (RoleTypeId.Scientist, "Scientists"),
            (RoleTypeId.ClassD, "ClassD"),
            (RoleTypeId.FacilityGuard, "Guard"),
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
            RecalculateCounts();
            _nextHintAt = 0f;   // Keep at 0 so Update sends immediately
            SendVoteHints();        // Important: send first hint right away
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
            HintHelper.RemoveFromAll(Player.ReadyList, VoteHintTag);
        }

        private void Update()
        {
            if (_plugin == null || !_active)
                return;

            float now = Time.realtimeSinceStartup;

            // Force high-frequency refresh (more aggressive than Config)
            if (now >= _nextHintAt)
            {
                SendVoteHints();
                _nextHintAt = now + 0.9f;     // 0.9s interval for stability
            }
        }

        private void SendVoteHints()
        {
            float remainingTime = TimeRemaining;
            if (remainingTime <= 0f) return;

            int remaining = (int)Math.Ceiling(remainingTime);

            string header = string.Format(_plugin.Config.VoteHintHeaderFormat, remaining, TotalVoters, TotalPlayers);

            string body = "\n<size=22>";
            foreach (var (role, _) in Roles)
            {
                _voteCounts.TryGetValue(role, out int count);
                string displayName = GetRoleDisplayName(role);
                body += string.Format(_plugin.Config.VoteHintBodyLineFormat, GetRoleColor(role), displayName, count);
            }
            body += "</size>";

            string instruction = _plugin.Config.VoteHintInstruction;

            string fullHint = header + "\n" + body + instruction;

            foreach (Player player in Player.ReadyList)
            {
                HintHelper.ShowToPlayer(player, VoteHintTag, fullHint, position: 220f);
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

        private string GetRoleDisplayName(RoleTypeId role)
        {
            return role switch
            {
                RoleTypeId.Scp049 => _plugin.Config.VoteRoleScpDisplayName,
                RoleTypeId.Scientist => _plugin.Config.VoteRoleScientistDisplayName,
                RoleTypeId.ClassD => _plugin.Config.VoteRoleClassDDisplayName,
                RoleTypeId.FacilityGuard => _plugin.Config.VoteRoleGuardDisplayName,
                _ => role.ToString()
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
