using CustomPlayerEffects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using LabApi.Features.Wrappers;

namespace SCPSL_MicroServer_Tweaks
{
    internal static class KeycardExtensions
    {
        public static bool HasKeycardPermission(this Player player, IDoorPermissionRequester requester)
        {
            if (player == null || requester == null)
                return false;

            if (player.HasEffect<AmnesiaVision>())
                return false;

            foreach (Item item in player.Items)
            {
                if (item.Base is not IDoorPermissionProvider provider)
                    continue;

                if (requester.CheckPermissions(provider, out _))
                    return true;
            }

            return false;
        }
    }
}
