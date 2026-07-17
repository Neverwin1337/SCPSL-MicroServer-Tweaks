using System;
using CommandSystem;
using LabApi.Loader.Features.Plugins;

namespace SCPSL_MicroServer_Tweaks
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class TestEventCommand : ICommand
    {
        public string Command { get; } = "mst_event";

        public string[] Aliases { get; } = Array.Empty<string>();

        public string Description { get; } = "Triggers a random event manually. Usage: mst_event <elevator|doors|stealth|blackout|nuke|scramble>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            SCPSL_MicroServer_TweaksPlugin plugin = SCPSL_MicroServer_TweaksPlugin.Instance;

            if (plugin == null)
            {
                response = "Plugin is not loaded.";
                return false;
            }

            if (plugin.RandomEventController == null)
            {
                response = "RandomEventController is not initialized.";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage: mst_event <elevator|doors|stealth|blackout|nuke|scramble>";
                return false;
            }

            string eventName = arguments.At(0);

            if (plugin.RandomEventController.TriggerEventByName(eventName))
            {
                response = string.Format("Triggered event: {0}", eventName);
                return true;
            }

            response = string.Format("Unknown event '{0}'. Available: elevator, doors, stealth, blackout, nuke, scramble", eventName);
            return false;
        }
    }
}
