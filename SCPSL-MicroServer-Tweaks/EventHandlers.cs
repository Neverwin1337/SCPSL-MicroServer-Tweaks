using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using PlayerRoles;

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
    }
}