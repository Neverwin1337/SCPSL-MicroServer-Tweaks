using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using LabApi.Features.Wrappers;
using MapGeneration;
using UnityEngine;

namespace SCPSL_MicroServer_Tweaks
{
    public sealed class RandomEventController : MonoBehaviour
    {
        private SCPSL_MicroServer_TweaksPlugin _plugin;
        private bool _running;
        private float _nextEventAt;
        private float _roundStartTime;

        private struct PendingAction
        {
            public float ExecuteAt;
            public Action Action;
        }

        private readonly List<PendingAction> _pendingActions = new List<PendingAction>();

        private bool _elevatorsLocked;
        private readonly List<Player> _stealthedPlayers = new List<Player>();
        private bool _blackoutActive;

        private string _activeHintText;
        private float _activeHintEndAt;
        private float _lastHintTime;
        private const float HintInterval = 1f;

        public void Initialize(SCPSL_MicroServer_TweaksPlugin plugin)
        {
            _plugin = plugin;
        }

        public void StartEvents()
        {
            StopEvents();

            if (!_plugin.Config.EnableRandomEvents)
                return;

            _running = true;
            _roundStartTime = Time.realtimeSinceStartup;
            ScheduleNextEvent();
            _plugin.Debug("Random event loop started.");
        }

        public void StopEvents()
        {
            _running = false;
            _pendingActions.Clear();
            StopActiveHint();
            CleanupActiveEffects();
        }

        private void ScheduleNextEvent()
        {
            PluginConfig config = _plugin.Config;
            float interval = UnityEngine.Random.Range(
                config.RandomEventMinIntervalSeconds,
                config.RandomEventMaxIntervalSeconds);
            _nextEventAt = Time.realtimeSinceStartup + interval;
        }

        private void ScheduleAction(float delaySeconds, Action action)
        {
            _pendingActions.Add(new PendingAction
            {
                ExecuteAt = Time.realtimeSinceStartup + delaySeconds,
                Action = action
            });
        }

        private void Update()
        {
            if (_plugin == null || !_running)
                return;

            float now = Time.realtimeSinceStartup;

            for (int i = _pendingActions.Count - 1; i >= 0; i--)
            {
                if (now >= _pendingActions[i].ExecuteAt)
                {
                    _pendingActions[i].Action();
                    _pendingActions.RemoveAt(i);
                }
            }

            UpdateActiveHint(now);

            if (now < _nextEventAt)
                return;

            int aliveCount = 0;
            foreach (Player player in Player.ReadyList)
            {
                if (player.IsAlive)
                    aliveCount++;
            }

            if (aliveCount < _plugin.Config.RandomEventMinPlayers)
            {
                _plugin.Debug(string.Format(
                    "Random event skipped: only {0} alive player(s), need {1}.",
                    aliveCount, _plugin.Config.RandomEventMinPlayers));
                ScheduleNextEvent();
                return;
            }

            TriggerRandomEvent();
            ScheduleNextEvent();
        }

        private void UpdateActiveHint(float now)
        {
            if (string.IsNullOrEmpty(_activeHintText) || now >= _activeHintEndAt)
            {
                _activeHintText = null;
                return;
            }

            if (now - _lastHintTime < HintInterval)
                return;

            _lastHintTime = now;
            int secondsLeft = Mathf.Max(0, Mathf.CeilToInt(_activeHintEndAt - now));
            string hint = string.Format(_activeHintText, secondsLeft);
            foreach (Player player in Player.ReadyList)
            {
                if (player.IsAlive)
                    player.SendHint(hint, 2f);
            }
        }

        private void StartActiveHint(string format, float duration)
        {
            _activeHintText = format;
            _activeHintEndAt = Time.realtimeSinceStartup + duration;
            _lastHintTime = 0f;
        }

        private void StopActiveHint()
        {
            _activeHintText = null;
        }

        private void TriggerRandomEvent()
        {
            List<int> available = new List<int> { 0, 1, 2, 3, 5 };

            float elapsedMinutes = (Time.realtimeSinceStartup - _roundStartTime) / 60f;
            if (elapsedMinutes >= _plugin.Config.RandomEventNukeMinMinutes)
                available.Add(4);

            int eventType = available[UnityEngine.Random.Range(0, available.Count)];
            string eventName;

            switch (eventType)
            {
                case 0:
                    eventName = "ElevatorMalfunction";
                    ElevatorMalfunctionEvent();
                    break;
                case 1:
                    eventName = "AllDoorsOpen";
                    AllDoorsOpenEvent();
                    break;
                case 2:
                    eventName = "Stealth";
                    StealthEvent();
                    break;
                case 3:
                    eventName = "Blackout";
                    BlackoutEvent();
                    break;
                case 4:
                    eventName = "NukeAlert";
                    NukeAlertEvent();
                    break;
                default:
                    eventName = "Scramble";
                    ScrambleEvent();
                    break;
            }

            _plugin.Debug(string.Format("Triggered random event: {0}", eventName));
        }

        private void ElevatorMalfunctionEvent()
        {
            float duration = _plugin.Config.RandomEventElevatorLockDuration;

            CassieMessage("pitch_85 . glitch_0.3 jam_0.2 . attention . elevator systems malfunction . all elevators have been disabled .", 0.3f);
            BroadcastMessage("電梯故障：所有電梯已停用", 6);

            foreach (Elevator elevator in Map.Elevators)
            {
                elevator.DynamicAdminLock = true;
            }
            _elevatorsLocked = true;

            StartActiveHint("<size=24><color=#ff9900>電梯故障中</color> <color=white>剩餘 {0} 秒</color></size>", duration);

            ScheduleAction(duration, () =>
            {
                foreach (Elevator elevator in Map.Elevators)
                {
                    elevator.DynamicAdminLock = false;
                }
                _elevatorsLocked = false;
                StopActiveHint();
                CassieMessage("pitch_100 . attention . elevator systems restored .", 0f);
            });
        }

        private void AllDoorsOpenEvent()
        {
            CassieMessage("pitch_105 . attention . all facility doors have been opened . containment barriers released .", 0f);
            BroadcastMessage("設施門禁解除：所有門已開啟", 6);

            foreach (Door door in Map.Doors)
            {
                door.IsOpened = true;
            }
        }

        private void StealthEvent()
        {
            float duration = _plugin.Config.RandomEventStealthDuration;

            CassieMessage("pitch_90 . glitch_0.1 jam_0.1 . attention . stealth protocol activated . all personnel rendered invisible and silent .", 0.1f);
            BroadcastMessage("隱形協議啟動：所有人類隱形且靜音", 6);

            _stealthedPlayers.Clear();
            foreach (Player player in Player.ReadyList)
            {
                if (!player.IsAlive || !player.IsHuman)
                    continue;

                player.EnableEffect<Invisible>(1, duration, false);
                player.IsSpectatable = false;
                _stealthedPlayers.Add(player);
            }

            StartActiveHint("<size=24><color=#66ffcc>隱形協議中</color> <color=white>剩餘 {0} 秒</color></size>", duration);

            ScheduleAction(duration, () =>
            {
                foreach (Player player in _stealthedPlayers)
                {
                    if (player != null && player.IsAlive)
                    {
                        player.DisableEffect<Invisible>();
                        player.IsSpectatable = true;
                    }
                }
                _stealthedPlayers.Clear();
                StopActiveHint();
                CassieMessage("pitch_100 . attention . stealth protocol deactivated .", 0f);
            });
        }

        private void BlackoutEvent()
        {
            float duration = _plugin.Config.RandomEventBlackoutDuration;

            CassieMessage("pitch_75 . glitch_0.4 jam_0.3 . attention . facility power systems failure . maintaining backup power .", 0.4f);
            BroadcastMessage("設施停電：全設施停電中", 6);

            _blackoutActive = true;
            StartActiveHint("<size=24><color=#ff5555>設施停電中</color> <color=white>剩餘 {0} 秒</color></size>", duration);
            ScheduleBlackoutCycle(duration, 0f);
        }

        private void ScheduleBlackoutCycle(float remaining, float elapsed)
        {
            if (remaining <= 0f)
            {
                Map.TurnOnLights();
                _blackoutActive = false;
                StopActiveHint();
                CassieMessage("pitch_100 . attention . facility power systems restored .", 0f);
                return;
            }

            Map.TurnOffLights(float.MaxValue);
            float offTime = Mathf.Min(5f, remaining);
            ScheduleAction(offTime, () =>
            {
                if (!_blackoutActive)
                    return;

                float left = remaining - offTime;
                if (left <= 0f)
                {
                    Map.TurnOnLights();
                    _blackoutActive = false;
                    StopActiveHint();
                    CassieMessage("pitch_100 . attention . facility power systems restored .", 0f);
                    return;
                }

                Map.TurnOnLights();
                float onTime = Mathf.Min(1f, left);
                ScheduleAction(onTime, () =>
                {
                    if (_blackoutActive)
                        ScheduleBlackoutCycle(left - onTime, elapsed + offTime + onTime);
                });
            });
        }

        private void NukeAlertEvent()
        {
            float countdown = _plugin.Config.RandomEventNukeCountdownSeconds;
            float falseAlarmChance = _plugin.Config.RandomEventNukeFalseAlarmChance;

            CassieMessage("pitch_110 . glitch_0.5 jam_0.4 . alpha warhead emergency detonation sequence initiated . all personnel evacuate immediately .", 0.5f);
            BroadcastMessage("核彈警報：偵測到核彈啟動序列", 10);

            Warhead.Start(true, false);
            Warhead.DetonationTime = countdown;

            ScheduleAction(countdown, () =>
            {
                if (Warhead.IsDetonationInProgress)
                {
                    if (UnityEngine.Random.Range(0f, 1f) < falseAlarmChance)
                    {
                        Warhead.Stop();
                        CassieMessage("pitch_95 . false alarm . alpha warhead detonation cancelled .", 0.2f);
                        BroadcastMessage("虛警：核彈警報已取消", 6);
                    }
                }
            });
        }

        private void CleanupActiveEffects()
        {
            if (_elevatorsLocked)
            {
                foreach (Elevator elevator in Map.Elevators)
                {
                    elevator.DynamicAdminLock = false;
                }
                _elevatorsLocked = false;
            }

            if (_stealthedPlayers.Count > 0)
            {
                foreach (Player player in _stealthedPlayers)
                {
                    if (player != null && player.IsAlive)
                    {
                        player.DisableEffect<Invisible>();
                        player.IsSpectatable = true;
                    }
                }
                _stealthedPlayers.Clear();
            }

            if (_blackoutActive)
            {
                Map.TurnOnLights();
                _blackoutActive = false;
            }

            if (Warhead.IsDetonationInProgress)
            {
                Warhead.Stop();
            }
        }

        private void ScrambleEvent()
        {
            bool lczClosed = Decontamination.IsDecontaminating;

            CassieMessage("pitch_80 . glitch_0.2 jam_0.15 . attention . spatial anomaly detected . relocating all personnel .", 0.2f);
            BroadcastMessage("空間異常：所有人員隨機傳送", 6);

            List<Room> validRooms = new List<Room>();
            foreach (Room room in Map.Rooms)
            {
                if (lczClosed && room.Zone == FacilityZone.LightContainment)
                    continue;

                validRooms.Add(room);
            }

            if (validRooms.Count == 0)
                return;

            int teleported = 0;
            foreach (Player player in Player.ReadyList)
            {
                if (!player.IsAlive || !player.IsHuman)
                    continue;

                Room target = validRooms[UnityEngine.Random.Range(0, validRooms.Count)];
                player.Position = target.Position + Vector3.up * 1.5f;
                teleported++;
            }

            _plugin.Debug(string.Format("Scramble event: teleported {0} human(s), LCZ closed={1}.", teleported, lczClosed));
        }

        private void CassieMessage(string message, float glitchScale)
        {
            Announcer.Message(message, "", true, 0f, glitchScale);
        }

        public bool TriggerEventByName(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "elevator":
                    ElevatorMalfunctionEvent();
                    return true;
                case "doors":
                    AllDoorsOpenEvent();
                    return true;
                case "stealth":
                    StealthEvent();
                    return true;
                case "blackout":
                    BlackoutEvent();
                    return true;
                case "nuke":
                    NukeAlertEvent();
                    return true;
                case "scramble":
                    ScrambleEvent();
                    return true;
                default:
                    return false;
            }
        }

        private void BroadcastMessage(string message, int duration)
        {
            Server.SendBroadcast(message, (ushort)duration);
        }
    }
}
