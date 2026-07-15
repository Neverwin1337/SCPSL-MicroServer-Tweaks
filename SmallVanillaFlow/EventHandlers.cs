using LabApi.Events.CustomHandlers;

namespace SmallVanillaFlow
{
    internal sealed class EventHandlers : CustomEventsHandler
    {
        private readonly SmallVanillaFlowPlugin _plugin;

        public EventHandlers(SmallVanillaFlowPlugin plugin)
        {
            _plugin = plugin;
        }

        public override void OnServerWaitingForPlayers()
        {
            _plugin.FreezeController.CancelFreeze(false);
            _plugin.ApplyStartingTokens();
        }

        public override void OnServerRoundStarted()
        {
            _plugin.FreezeController.ScheduleFreeze();
        }
    }
}
