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

        public string[] Aliases => Array.Empty<string>();

        public string Description => "Vote for a role. Usage: .vote scp | .vote sci | .vote d | .vote guard";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Invalid vote. Use .vote scp, .vote sci, .vote d, or .vote guard.";

            Player player = Player.Get(sender);
            if (player == null)
                return false;

            if (arguments.Count < 1)
                return false;

            string vote = arguments.At(0).ToLowerInvariant();
            RoleTypeId? role = vote switch
            {
                "scp" or "1" => RoleTypeId.Scp049,
                "sci" or "scientist" or "2" => RoleTypeId.Scientist,
                "d" or "classd" or "3" => RoleTypeId.ClassD,
                "guard" or "g" or "4" => RoleTypeId.FacilityGuard,
                _ => null
            };

            if (role == null)
                return false;

            bool success = SCPSL_MicroServer_TweaksPlugin.Instance.VotingController.TryVote(player, role.Value);
            response = success ? $"Voted for {vote}!" : "Voting is not active right now.";
            return true;
        }
    }
}