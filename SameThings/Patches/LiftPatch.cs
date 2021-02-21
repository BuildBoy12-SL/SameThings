using HarmonyLib;

namespace SameThings.Patches
{
    [HarmonyPatch(typeof(Lift), nameof(Lift.UseLift))]
    internal static class LiftPatch
    {
        private static void Prefix(Lift __instance)
        {
            if (SameThings.Instance.Config.LiftMoveDuration > -1)
            {
                __instance.movingSpeed = SameThings.Instance.Config.LiftMoveDuration;
            }
        }
    }
}