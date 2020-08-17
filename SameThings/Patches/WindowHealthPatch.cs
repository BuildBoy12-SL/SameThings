using HarmonyLib;

namespace SameThings.Patches
{
    [HarmonyPatch(typeof(BreakableWindow), nameof(BreakableWindow.ServerDamageWindow))]
    internal class WindowHealthPatch
    {
        private static void Prefix(BreakableWindow __instance)
        {
            if (SameThings.Instance.Config.WindowHealth > 1 || State.BreakableWindows.Contains(__instance))
            {
                return;
            }
            __instance.health = SameThings.Instance.Config.WindowHealth;
            State.BreakableWindows.Add(__instance);
        }
    }
}
