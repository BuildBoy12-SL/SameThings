using Exiled.API.Features;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;

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

        public static IEnumerator<float> RunAutoWarhead()
        {
            yield return Timing.WaitForSeconds(Plugin.Config.AutoWarheadTime);

            if (Plugin.Config.AutoWarheadLock)
                Warhead.IsLocked = true;

            if (Warhead.IsDetonated || Warhead.IsInProgress)
            {
                Log.Info("Warhead is detonated or is in progress.");
                yield break;
            }

            Log.Info("Activating Warhead.");
            Warhead.Start();

            if (!string.IsNullOrEmpty(Plugin.Config.AutoWarheadStartText))
                Map.Broadcast(10, Plugin.Config.AutoWarheadStartText, Broadcast.BroadcastFlags.Normal);
        }

        public static IEnumerator<float> RunAutoCleanup()
        {
            while (true)
            {
                foreach (Pickup pickup in State._pickups.Keys)
                {
                    if (pickup == null)
                        State._pickups.Remove(pickup);
                    else if (State._pickups[pickup] <= Round.ElapsedTime.TotalSeconds)
                        NetworkServer.Destroy(pickup.gameObject);
                }

                for (var z = 0; z < Plugin.Config.ItemAutoCleanup * 50; z++)
                    yield return 0f;
            }
        }

        public static IEnumerator<float> RunLureReload()
        {
            yield return Timing.WaitForSeconds(Plugin.Config.Scp106LureReload > 0 ? Plugin.Config.Scp106LureReload : 0);
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
                    yield return Timing.WaitForSeconds(1f);
                }
            }
        }

        private static void DoSelfHealing(Exiled.API.Features.Player ply)
        {
            if (ply.IsHost
                || !Plugin.Config.SelfHealingAmount.TryGetValue(ply.Role, out int amount)
                || !Plugin.Config.SelfHealingDuration.TryGetValue(ply.Role, out int duration))
            {
                return;
            }

            State._afkTime[ply] = (State._prevPos[ply] == ply.Position) ? (State._afkTime[ply] + 1) : 0;
            State._prevPos[ply] = ply.Position;

            if (State._afkTime[ply] <= duration)
                return;

            ply.Health = ((ply.Health + amount) >= ply.MaxHealth) ? ply.MaxHealth : (ply.Health + amount);
        }

        public static void RunRestoreMaxHp(Exiled.API.Features.Player player, int maxHp)
        {
            player.MaxHealth = maxHp;
            player.Health = maxHp;
        }
    }
}
