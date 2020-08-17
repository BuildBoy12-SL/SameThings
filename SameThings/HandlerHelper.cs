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
            yield return Timing.WaitForSeconds(Plugin.Config.ForceRestart);
            Log.Info("Restarting round.");
            PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
        }

        public static IEnumerator<float> RunAutoWarhead()
        {
            yield return Timing.WaitForSeconds(Plugin.Config.AutoWarheadTime);
            if (Plugin.Config.AutoWarheadLock)
            {
                Warhead.IsLocked = true;
            }
            if (Warhead.IsDetonated || Warhead.IsInProgress)
            {
                Log.Info("Warhead is detonated or is in progress.");
                yield break;
            }
            Log.Info("Activating Warhead.");
            Warhead.Start();
            if (!string.IsNullOrEmpty(Plugin.Config.AutoWarheadStartText))
            {
                Map.Broadcast(10, Plugin.Config.AutoWarheadStartText, Broadcast.BroadcastFlags.Normal);
            }
        }

        public static IEnumerator<float> RunAutoCleanup()
        {
            while (true)
            {
                foreach (Pickup pickup in State.Pickups.Keys)
                {
                    if (pickup == null)
                    {
                        State.Pickups.Remove(pickup);
                    }
                    else if (State.Pickups[pickup] <= Round.ElapsedTime.TotalSeconds)
                    {
                        NetworkServer.Destroy(pickup.gameObject);
                    }
                }
                yield return Timing.WaitForSeconds(Plugin.Config.ItemAutoCleanup);
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
            if (ply.IsHost || !Plugin.Config.SelfHealingAmount.TryGetValue(ply.Role, out int amount) || !Plugin.Config.SelfHealingDuration.TryGetValue(ply.Role, out int duration))
            {
                return;
            }
            State.AfkTime[ply] = (State.PrevPos[ply] == ply.Position) ? (State.AfkTime[ply] + 1) : 0;
            State.PrevPos[ply] = ply.Position;
            if (State.AfkTime[ply] <= duration)
            {
                return;
            }
            ply.Health = ((ply.Health + amount) >= ply.MaxHealth) ? ply.MaxHealth : (ply.Health + amount);
        }

        public static void RunRestoreMaxHp(Exiled.API.Features.Player player, int maxHp)
        {
            player.MaxHealth = maxHp;
            player.Health = maxHp;
        }

    }
}
