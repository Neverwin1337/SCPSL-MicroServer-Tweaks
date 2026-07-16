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
        private readonly Dictionary<Player, RoleTypeId> _assignments = new Dictionary<Player, RoleTypeId>();
        private bool _active;
        private bool _assignmentsReady;
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
            _assignments.Clear();
            _active = true;
            _assignmentsReady = false;
            _earlyEndTimerStart = 0;
            _lobbyTimerSetAt = Time.realtimeSinceStartup;

            if (GameCore.RoundStart.singleton != null)
            {
                GameCore.RoundStart.singleton.NetworkRoundStartTime = _lobbyTimerSetAt + _plugin.Config.LobbyTimerSeconds;
            }

            _plugin.VotingUI.Show();
            _plugin.Debug("Voting started.");
        }

        public bool TryVote(Player player, RoleTypeId role)
        {
            if (!_active || player == null)
                return false;

            _votes[player] = role;
            RecalculateCounts();
            _plugin.VotingUI.UpdateDisplay();
            CheckEarlyEnd();
            return true;
        }

        public void EnsureAssignmentsReady()
        {
            if (_assignmentsReady)
                return;

            _assignmentsReady = true;
            _active = false;
            _plugin.VotingUI.Hide();

            List<Player> players = Player.ReadyList.ToList();
            if (players.Count == 0)
                return;

            int scpSlots = GetDefaultScpSlotCount(players.Count);

            List<Player> scpVoters = players.Where(p => _votes.TryGetValue(p, out RoleTypeId r) && IsScpVote(r)).ToList();

            scpVoters.Shuffle();
            HashSet<Player> assignedScps = new HashSet<Player>(scpVoters.Take(scpSlots));
            foreach (Player p in assignedScps)
            {
                RoleTypeId actualScp = PickRandomScp();
                _assignments[p] = actualScp;
                _plugin.Debug($"Assigned {p.Nickname} -> {actualScp} (SCP vote)");
            }

            List<Player> remaining = players.Where(p => !_assignments.ContainsKey(p)).ToList();
            remaining.Shuffle();

            List<Player> votersForClassD = remaining.Where(p => _votes.TryGetValue(p, out RoleTypeId r) && r == RoleTypeId.ClassD).ToList();
            List<Player> votersForGuard = remaining.Where(p => _votes.TryGetValue(p, out RoleTypeId r) && r == RoleTypeId.FacilityGuard).ToList();
            List<Player> votersForScientist = remaining.Where(p => _votes.TryGetValue(p, out RoleTypeId r) && r == RoleTypeId.Scientist).ToList();
            List<Player> noVote = remaining.Where(p => !_votes.ContainsKey(p)).ToList();

            AssignRemaining(votersForClassD, RoleTypeId.ClassD);
            AssignRemaining(votersForGuard, RoleTypeId.FacilityGuard);
            AssignRemaining(votersForScientist, RoleTypeId.Scientist);
            AssignRemaining(noVote, RoleTypeId.ClassD);

            void AssignRemaining(List<Player> pool, RoleTypeId preferred)
            {
                foreach (Player p in pool)
                {
                    if (_assignments.ContainsKey(p))
                        continue;
                    _assignments[p] = preferred;
                    _plugin.Debug($"Assigned {p.Nickname} -> {preferred}");
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

        private bool IsScpVote(RoleTypeId role)
        {
            return role == RoleTypeId.Scp049;
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