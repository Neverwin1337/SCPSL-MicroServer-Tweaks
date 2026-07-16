using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace SCPSL_MicroServer_Tweaks
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class VoteCommand : ICommand
    {
        public string Command => "vote";

        public string[] Aliases => new[] { "1", "2", "3", "4" };

        public string Description => "Vote for a role. Use .1 .2 .3 .4 or .vote scp/sci/d/guard";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Invalid vote. Use .1 SCP | .2 Scientist | .3 ClassD | .4 Guard";

            Player player = Player.Get(sender);
            if (player == null)
                return false;

            string vote = ResolveVote(arguments);
            if (string.IsNullOrEmpty(vote))
                return false;

            RoleTypeId? role = vote switch
            {
                "scp" => RoleTypeId.Scp049,
                "sci" => RoleTypeId.Scientist,
                "d" => RoleTypeId.ClassD,
                "guard" => RoleTypeId.FacilityGuard,
                _ => null
            };

            if (role == null)
                return false;

            bool success = SCPSL_MicroServer_TweaksPlugin.Instance.VotingController.TryVote(player, role.Value);
            response = success ? $"Voted for {vote}!" : "Voting is not active right now.";
            return true;
        }

        private string ResolveVote(ArraySegment<string> arguments)
        {
            if (arguments.Count >= 1)
            {
                string first = arguments.At(0).ToLowerInvariant();
                if (first is "scp" or "1")
                    return "scp";
                if (first is "sci" or "scientist" or "2")
                    return "sci";
                if (first is "d" or "classd" or "3")
                    return "d";
                if (first is "guard" or "g" or "4")
                    return "guard";
            }

            string cmd = Command.ToLowerInvariant();
            if (cmd is "1" or "scp")
                return "scp";
            if (cmd is "2" or "sci" or "scientist")
                return "sci";
            if (cmd is "3" or "d" or "classd")
                return "d";
            if (cmd is "4" or "guard" or "g")
                return "guard";

            return string.Empty;
        }
    }
}