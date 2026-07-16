using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
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
                _plugin.Debug($"Intercepted role: {args.Player.Nickname} {args.NewRole} -> {assignedRole}");
                args.NewRole = assignedRole;
            }
        }
    }
}