using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;
using Respawning;
using Respawning.Waves;
using Respawning.Waves.Generic;
using RueI.API.Elements;
using UnityEngine;

namespace SCPSL_MicroServer_Tweaks
{
    public sealed class RespawnHintController : MonoBehaviour
    {
        private static readonly Tag RespawnHintTag = new Tag("mst_respawn_hint");

        private SCPSL_MicroServer_TweaksPlugin _plugin;
        private bool _running;
        private float _nextHintAt;
        private const float HintInterval = 1f;

        public void Initialize(SCPSL_MicroServer_TweaksPlugin plugin)
        {
            _plugin = plugin;
        }

        public void StartHints()
        {
            _running = true;
            _nextHintAt = 0f;
        }

        public void StopHints()
        {
            _running = false;
            HintHelper.RemoveFromAll(Player.ReadyList, RespawnHintTag);
        }

        private void Update()
        {
            if (_plugin == null || !_running)
                return;

            if (!_plugin.Config.EnableRespawnHint)
                return;

            float now = Time.realtimeSinceStartup;
            if (now < _nextHintAt)
                return;

            _nextHintAt = now + HintInterval;
            SendRespawnHints();
        }

        private void SendRespawnHints()
        {
            float minTimeLeft = float.MaxValue;
            string nextTeam = "";

            foreach (SpawnableWaveBase waveBase in WaveManager.Waves)
            {
                RespawnWave wave = RespawnWaves.Get(waveBase);
                if (wave == null || wave is MiniRespawnWave)
                    continue;

                float timeLeft = wave.TimeLeft;
                if (timeLeft < minTimeLeft)
                {
                    minTimeLeft = timeLeft;
                    nextTeam = wave.Faction == Faction.FoundationStaff
                        ? _plugin.Config.RespawnHintNtfName
                        : _plugin.Config.RespawnHintChaosName;
                }
            }

            if (minTimeLeft == float.MaxValue)
                return;

            int seconds = Mathf.Max(0, Mathf.CeilToInt(minTimeLeft));
            string hint = string.Format(_plugin.Config.RespawnHintFormat, nextTeam, seconds);

            foreach (Player player in Player.ReadyList)
            {
                if (!player.IsAlive)
                    HintHelper.ShowToPlayer(player, RespawnHintTag, hint, position: 100f);
            }
        }
    }
}
