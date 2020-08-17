using HarmonyLib;

namespace SameThings.Patches
{
    [HarmonyPatch(typeof(Radio), nameof(Radio.UseBattery))]
    internal class RadioPatch
    {
        private static bool Prefix()
        {
            return !SameThings.Instance.Config.UnlimitedRadioBattery;
        }
    }
}
