using HarmonyLib;

namespace SameThings.Patches
{
    [HarmonyPatch(typeof(BreakableWindow), nameof(BreakableWindow.ServerDamageWindow))]
    internal static class WindowHealthPatch
    {
        private static void Prefix(BreakableWindow __instance)
        {
            if (SameThings.Instance.Config.WindowHealth > 1 || State._breakableWindows.Contains(__instance))
            {
                return;
            }
            __instance.health = SameThings.Instance.Config.WindowHealth;
            State._breakableWindows.Add(__instance);
        }
    }
}
