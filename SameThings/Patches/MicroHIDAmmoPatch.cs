using Exiled.API.Features;
using HarmonyLib;

namespace SameThings.Patches
{
    [HarmonyPatch(typeof(MicroHID), nameof(MicroHID.UpdateServerside))]
    internal static class MicroHIDAmmoPatch
    {
        private static void Prefix(MicroHID __instance)
        {
            var ply = Player.Get(__instance.gameObject);
            if (SameThings.Instance.Config.InfiniteMicroAmmo && ply.CurrentItem.id == ItemType.MicroHID)
            {
                __instance.ChangeEnergy(1);
                __instance.NetworkEnergy = 1;
            }
        }
    }
}
