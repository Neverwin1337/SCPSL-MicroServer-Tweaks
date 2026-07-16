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

            if (_controllerObject != null)
                UnityEngine.Object.Destroy(_controllerObject);

            _eventHandlers = null;
            FreezeController = null;
            VotingController = null;
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

                // Keep the base game's global remaining-respawn counter in sync.
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