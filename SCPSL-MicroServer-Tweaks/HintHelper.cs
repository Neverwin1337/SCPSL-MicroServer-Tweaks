using System.Collections.Generic;
using LabApi.Features.Wrappers;
using RueI.API;
using RueI.API.Elements;

namespace SCPSL_MicroServer_Tweaks
{
    internal static class HintHelper
    {
        public const float DefaultPosition = 500f;

        public static void ShowToAll(IEnumerable<Player> players, Tag tag, string content, float position = DefaultPosition)
        {
            foreach (Player player in players)
            {
                RueDisplay display = RueDisplay.Get(player);
                display.Show(tag, new BasicElement(position, content));
            }
        }

        public static void ShowToAll(IEnumerable<Player> players, Tag tag, string content, float duration, float position = DefaultPosition)
        {
            foreach (Player player in players)
            {
                RueDisplay display = RueDisplay.Get(player);
                display.Show(tag, new BasicElement(position, content), duration);
            }
        }

        public static void ShowToPlayer(Player player, Tag tag, string content, float position = DefaultPosition)
        {
            RueDisplay display = RueDisplay.Get(player);
            display.Show(tag, new BasicElement(position, content));
        }

        public static void ShowToPlayer(Player player, Tag tag, string content, float duration, float position = DefaultPosition)
        {
            RueDisplay display = RueDisplay.Get(player);
            display.Show(tag, new BasicElement(position, content), duration);
        }

        public static void RemoveFromAll(IEnumerable<Player> players, Tag tag)
        {
            foreach (Player player in players)
            {
                RueDisplay display = RueDisplay.Get(player);
                display.Remove(tag);
            }
        }

        public static void RemoveFromPlayer(Player player, Tag tag)
        {
            RueDisplay display = RueDisplay.Get(player);
            display.Remove(tag);
        }
    }
}
