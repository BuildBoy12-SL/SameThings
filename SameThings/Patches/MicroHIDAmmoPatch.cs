using HarmonyLib;

namespace SameThings.Patches
{
    [HarmonyPatch(typeof(MicroHID), nameof(MicroHID.UpdateServerside))]
    internal class MicroHIDAmmoPatch
    {
        private static void Prefix(MicroHID __instance)
        {
            if (SameThings.Instance.Config.InfiniteMicroAmmo && __instance.refHub.inventory.NetworkcurItem == ItemType.MicroHID)
            {
                __instance.ChangeEnergy(1);
                __instance.NetworkEnergy = 1;
            }
        }
    }
}
