using Exiled.API.Features;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SameThings
{
    internal static class HandlerHelper
    {
        internal static SameThings Plugin => SameThings.Instance;

        public static IEnumerator<float> RunForceRestart()
        {
            for (var z = 0; z < 50 * Plugin.Config.ForceRestart; z++)
                yield return 0f;

            Log.Info($"Force restarting round after timeout in {Plugin.Config.ForceRestart}.");

            var pStats = Server.Host.ReferenceHub.GetComponent<RoundSummary>();
            pStats._roundEnded = true;
            RoundSummary.RoundLock = false;
            pStats._keepRoundOnOne = false;
        }

        /*
        public static IEnumerator<float> RunAutoWarhead()
        {
            for (var z = 0; z < 50 * Plugin.Config.AutoWarheadTime; z++)
                yield return 0f;

            if (Plugin.Config.AutoWarheadLock)
                Warhead.IsLocked = true;

            if (Warhead.IsDetonated || Warhead.IsInProgress)
            {
                Log.Info("Warhead is detonated or is in progress.");
                yield break;
            }

            Log.Info("Activating Warhead.");
            Warhead.Start();

            if (!string.IsNullOrEmpty(Plugin.Config.AutoWarheadStartText) && Plugin.Config.AutoWarheadStartTextTime != 0)
                Map.Broadcast(Plugin.Config.AutoWarheadStartTextTime, Plugin.Config.AutoWarheadStartText, Broadcast.BroadcastFlags.Normal);
        }
        */

        public static IEnumerator<float> RunAutoCleanup()
        {
            while (true)
            {
                var skipWaiting = false;
                if (State.Pickups.TryDequeue(out var pickup))
                {
                    if (!(skipWaiting = pickup == null))
                        NetworkServer.Destroy(pickup.gameObject);
                }

                if (skipWaiting)
                    continue;

                for (var z = 0; z < Plugin.Config.ItemAutoCleanup * 50; z++)
                    yield return 0f;
            }
        }

        public static IEnumerator<float> RunLureReload()
        {
            yield return Timing.WaitForSeconds(Mathf.Min(Plugin.Config.Scp106LureReload, 0));
            Object.FindObjectOfType<LureSubjectContainer>().NetworkallowContain = false;
        }

        public static IEnumerator<float> RunSelfHealing()
        {
            while (true)
            {
                foreach (Player ply in Player.List)
                {
                    try
                    {
                        DoSelfHealing(ply);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error during SelfHealing in SameThings: {e}");
                    }
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }

        private static void DoSelfHealing(Player ply)
        {
            if (ply.IsHost
                || !Plugin.Config.SelfHealingAmount.TryGetValue(ply.Role, out int amount)
                || !Plugin.Config.SelfHealingDuration.TryGetValue(ply.Role, out int duration))
            {
                return;
            }

            State.AfkTime[ply] = (State.PrevPos[ply] == ply.Position) ? (State.AfkTime[ply] + 1) : 0;
            State.PrevPos[ply] = ply.Position;

            if (State.AfkTime[ply] <= duration)
                return;

            ply.Health = ((ply.Health + amount) >= ply.MaxHealth) ? ply.MaxHealth : (ply.Health + amount);
        }

        public static void RunRestoreMaxHp(Player player, int maxHp)
        {
            player.MaxHealth = maxHp;
            player.Health = maxHp;
        }

        public static void SetupWindowsHealth()
        {
            var cfg = SameThings.Instance.Config;
            if (cfg.WindowHealth == 0)
                return;

            var windows = Object.FindObjectsOfType<BreakableWindow>();
            foreach (var window in windows)
            {
                window.health = cfg.WindowHealth;
            }
        }
    }
}
