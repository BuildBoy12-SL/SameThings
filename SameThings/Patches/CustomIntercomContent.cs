using HarmonyLib;
using UnityEngine;

namespace SameThings.Patches
{
    [HarmonyPatch(typeof(Intercom), nameof(Intercom.Start))]
    internal static class CustomIntercomContent_Intercom_Start
    {
        private static void Postfix(Intercom __instance)
        {
            __instance.UpdateText();
        }
    }

    [HarmonyPatch(typeof(Intercom), nameof(Intercom.UpdateText))]
    internal static class CustomIntercomContent_Intercom_UpdateText
    {
        private static bool Prefix(Intercom __instance)
        {
            if (!string.IsNullOrEmpty(__instance.CustomContent))
            {
                __instance.IntercomState = Intercom.State.Custom;
                __instance.Network_intercomText = __instance.CustomContent;
            }
            else if (__instance.Muted)
            {
                //IntercomState = State.Muted;
                SetContent(__instance, Intercom.State.Muted);
            }
            else if (Intercom.AdminSpeaking)
            {
                //IntercomState = State.AdminSpeaking;
                SetContent(__instance, Intercom.State.AdminSpeaking);
            }
            else if (__instance.remainingCooldown > 0f)
            {
                int num = Mathf.CeilToInt(__instance.remainingCooldown);
                //IntercomState = State.Restarting;
                __instance.NetworkIntercomTime = (ushort) ((num >= 0) ? ((ushort) num) : 0);
                SetContent(__instance, Intercom.State.Restarting);
            }
            else if (__instance.Networkspeaker != null)
            {
                if (__instance.bypassSpeaking)
                {
                    //IntercomState = State.TransmittingBypass;
                    SetContent(__instance, Intercom.State.TransmittingBypass);
                }
                else
                {
                    int num2 = Mathf.CeilToInt(__instance.speechRemainingTime);
                    //IntercomState = State.Transmitting;
                    __instance.NetworkIntercomTime = (ushort) ((num2 >= 0) ? ((ushort) num2) : 0);
                    SetContent(__instance, Intercom.State.Transmitting);
                }
            }
            else
            {
                //IntercomState = State.Ready;
                SetContent(__instance, Intercom.State.Ready);
            }

            if (Intercom.AdminSpeaking != Intercom.LastState)
            {
                Intercom.LastState = Intercom.AdminSpeaking;
                __instance.RpcUpdateAdminStatus(Intercom.AdminSpeaking);
            }

            return false;
        }

        private static void SetContent(Intercom intercom, Intercom.State state)
        {
            var customContents = SameThings.Instance.Config.CustomIntercomContent;
            if (customContents.TryGetValue(state, out var content))
            {
                intercom.Network_intercomText = string.Format(content, intercom.NetworkIntercomTime);
                intercom.Network_state = Intercom.State.Custom;
            }
            else
            {
                intercom.Network_state = state;
            }
        }
    }
}