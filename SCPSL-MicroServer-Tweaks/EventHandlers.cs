using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using PlayerRoles;
using Respawning;

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

            if (_plugin.Config.EnableRoleVoting)
                _plugin.VotingController.EndVoting();

            _plugin.RandomEventController.StartEvents();

            if (_plugin.Config.EnableRespawnHint)
                _plugin.RespawnHintController.StartHints();
        }

        public override void OnServerRoundEnded(RoundEndedEventArgs args)
        {
            _plugin.RandomEventController.StopEvents();
            _plugin.RespawnHintController.StopHints();
        }

        public override void OnPlayerChangingRole(PlayerChangingRoleEventArgs args)
        {
            if (!_plugin.Config.EnableRoleVoting)
                return;

            if (args.ChangeReason != RoleChangeReason.RoundStart)
                return;

            RoleTypeId assignedRole = _plugin.VotingController.GetAssignedRole(args.Player);
            if (assignedRole != args.NewRole)
            {
                _plugin.Debug(string.Format(
                    "Intercepted role: {0} {1} -> {2}",
                    args.Player.Nickname, args.NewRole, assignedRole));
                args.NewRole = assignedRole;
            }
        }

        public override void OnServerCommandExecuting(CommandExecutingEventArgs args)
        {
            if (!_plugin.Config.EnableRoleVoting)
                return;

            if (args.CommandType != CommandType.Client)
                return;

            if (args.CommandName == null)
                return;

            string cmd = args.CommandName.ToLowerInvariant();

            RoleTypeId? role = cmd switch
            {
                "1" or "scp" => RoleTypeId.Scp049,
                "2" or "sci" or "scientist" => RoleTypeId.Scientist,
                "3" or "d" or "classd" => RoleTypeId.ClassD,
                "4" or "guard" or "g" => RoleTypeId.FacilityGuard,
                _ => null
            };

            if (role == null && cmd == "vote" && args.Arguments.Count > 0)
            {
                string arg = args.Arguments.At(0).ToLowerInvariant();
                role = arg switch
                {
                    "1" or "scp" => RoleTypeId.Scp049,
                    "2" or "sci" or "scientist" => RoleTypeId.Scientist,
                    "3" or "d" or "classd" => RoleTypeId.ClassD,
                    "4" or "guard" or "g" => RoleTypeId.FacilityGuard,
                    _ => null
                };
            }

            if (role == null)
                return;

            Player player = Player.Get(args.Sender.SenderId);
            if (player == null)
            {
                _plugin.Debug("Failed to resolve CommandSender to Player");
                return;
            }

            bool voted = _plugin.VotingController.TryVote(player, role.Value);
            if (voted)
            {
                args.IsAllowed = false;
                player.SendConsoleMessage(string.Format("Voted for {0}!", role.Value), "green");
            }
        }

        public override void OnPlayerInteractingDoor(PlayerInteractingDoorEventArgs args)
        {
            if (!_plugin.Config.EnableRemoteKeycard || !_plugin.Config.RemoteKeycardAffectDoors)
                return;

            if (args.Player == null || args.CanOpen || args.IsAllowed)
                return;

            if (args.Door == null)
                return;

            try
            {
                if (args.Player.HasKeycardPermission(args.Door.Base) && !args.Door.IsLocked)
                    args.CanOpen = true;
            }
            catch (System.Exception e)
            {
                _plugin.Debug($"RemoteKeycard door error: {e.Message}");
            }
        }

        public override void OnPlayerUnlockingGenerator(PlayerUnlockingGeneratorEventArgs args)
        {
            if (!_plugin.Config.EnableRemoteKeycard || !_plugin.Config.RemoteKeycardAffectGenerators)
                return;

            if (args.Player == null || args.CanOpen || args.IsAllowed)
                return;

            if (args.Generator == null)
                return;

            try
            {
                if (args.Player.HasKeycardPermission(args.Generator.Base))
                    args.CanOpen = true;
            }
            catch (System.Exception e)
            {
                _plugin.Debug($"RemoteKeycard generator error: {e.Message}");
            }
        }

        public override void OnPlayerInteractingLocker(PlayerInteractingLockerEventArgs args)
        {
            if (!_plugin.Config.EnableRemoteKeycard || !_plugin.Config.RemoteKeycardAffectScpLockers)
                return;

            if (args.Player == null || args.CanOpen || args.IsAllowed)
                return;

            try
            {
                MapGeneration.Distributors.LockerChamber chamber = args.Chamber?.Base;
                if (chamber != null && args.Player.HasKeycardPermission(chamber))
                    args.CanOpen = true;
            }
            catch (System.Exception e)
            {
                _plugin.Debug($"RemoteKeycard locker error: {e.Message}");
            }
        }

        public override void OnPlayerUnlockingWarheadButton(PlayerUnlockingWarheadButtonEventArgs args)
        {
            if (!_plugin.Config.EnableRemoteKeycard || !_plugin.Config.RemoteKeycardAffectWarheadPanel)
                return;

            if (args.Player == null || args.IsAllowed)
                return;

            try
            {
                if (args.Player.HasKeycardPermission(AlphaWarheadActivationPanel.Instance))
                    args.IsAllowed = true;
            }
            catch (System.Exception e)
            {
                _plugin.Debug($"RemoteKeycard warhead error: {e.Message}");
            }
        }
    }
}