using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;
using Warhead = Exiled.Events.Handlers.Warhead;

namespace SameThings
{
    internal class EventHandlers
    {
        internal SameThings plugin;
        internal EventHandlers(SameThings plugin) => this.plugin = plugin;

        internal void SubscribeAll()
        {
            Server.RoundStarted += HandleRoundStart;
            Server.RoundEnded += HandleRoundEnd;
            Player.Joined += HandlePlayerJoin;
            Player.TriggeringTesla += HandleTeslaTrigger;
            Player.Shooting += HandleWeaponShoot;
            Player.ChangingRole += HandleSetClass;
            Player.ItemDropped += HandleDroppedItem;
            Player.EjectingGeneratorTablet += HandleGeneratorEject;
            Player.InsertingGeneratorTablet += HandleGeneratorInsert;
            Player.UnlockingGenerator += HandleGeneratorUnlock;
            Player.EnteringFemurBreaker += HandleFemurEnter;
            Player.Left += HandlePlayerLeave;
            Warhead.Detonated += HandleWarheadDetonation;
        }

        internal void UnSubscribeAll()
        {
            Server.RoundStarted -= HandleRoundStart;
            Server.RoundEnded -= HandleRoundEnd;
            Player.Joined -= HandlePlayerJoin;
            Player.TriggeringTesla -= HandleTeslaTrigger;
            Player.Shooting -= HandleWeaponShoot;
            Player.ChangingRole -= HandleSetClass;
            Player.ItemDropped -= HandleDroppedItem;
            Player.EjectingGeneratorTablet -= HandleGeneratorEject;
            Player.InsertingGeneratorTablet -= HandleGeneratorInsert;
            Player.UnlockingGenerator -= HandleGeneratorUnlock;
            Player.EnteringFemurBreaker -= HandleFemurEnter;
            Player.Left -= HandlePlayerLeave;
            Warhead.Detonated -= HandleWarheadDetonation;
        }

        public void HandleRoundStart()
        {
            if (plugin.Config.AutoWarheadLock) 
                Exiled.API.Features.Warhead.IsLocked = false;
            if (plugin.Config.ForceRestart > -1) 
                State.RunCoroutine(RunForceRestart());
            if (plugin.Config.AutoWarheadTime > -1) 
                State.RunCoroutine(RunAutoWarhead());
            if (plugin.Config.ItemAutoCleanup > -1) 
                State.RunCoroutine(RunAutoCleanup());
            if (plugin.Config.DecontaminationTime > -1) 
                LightContainmentZoneDecontamination.DecontaminationController.Singleton.TimeOffset = (float)((11.7399997711182 - plugin.Config.DecontaminationTime) * 60.0);
            if (plugin.Config.GeneratorDuration > -1)
            {
                foreach (Generator079 generator in Generator079.Generators)
                {
                    generator.startDuration = plugin.Config.GeneratorDuration;
                    generator.SetTime(plugin.Config.GeneratorDuration);
                }
            }
            if (plugin.Config.SelfHealingDuration.Count > 0)
                State.RunCoroutine(RunSelfHealing());
            if (plugin.Config.Scp106LureAmount > 0)
                Object.FindObjectOfType<LureSubjectContainer>().SetState(true);
        }

        public void HandleRoundEnd(RoundEndedEventArgs _)
        {
            State.Refresh();
        }

        public void HandlePlayerJoin(JoinedEventArgs ev)
        {
            Timing.CallDelayed(0.25f, () =>
            {
                State.AfkTime[ev.Player] = 0;
                State.PrevPos[ev.Player] = Vector3.zero;
                if (!ev.Player.ReferenceHub.serverRoles.Staff && plugin.Config.NicknameFilter.Any((string s) => ev.Player.Nickname.Contains(s)))
                {
                    ev.Player.Disconnect(plugin.Config.NicknameFilterReason);
                }
            });
        }

        public void HandleTeslaTrigger(TriggeringTeslaEventArgs ev)
        {
            ev.IsTriggerable = plugin.Config.TeslaTriggerableTeam.Contains(ev.Player.Team);
        }

        public void HandleWeaponShoot(ShootingEventArgs ev)
        {
            if (plugin.Config.InfiniteAmmo)
            {
                ev.Shooter.SetWeaponAmmo(ev.Shooter.CurrentItem, (int)ev.Shooter.CurrentItem.durability + 1);
            }
        }

        public void HandleSetClass(ChangingRoleEventArgs ev)
        {
            if (plugin.Config.MaxHealth.TryGetValue(ev.NewRole, out int maxHp))
                RunRestoreMaxHp(ev.Player, maxHp);
        }

        public void HandleDroppedItem(ItemDroppedEventArgs ev)
        {
            if (plugin.Config.ItemAutoCleanup <= 0 || plugin.Config.ItemCleanupIgnore.Contains(ev.Pickup.ItemId))
            {
                return;
            }
            State.Pickups.Add(ev.Pickup, (int)Round.ElapsedTime.TotalSeconds + plugin.Config.ItemAutoCleanup);
        }

        public void HandleGeneratorEject(EjectingGeneratorTabletEventArgs ev)
        {
            ev.IsAllowed = plugin.Config.GeneratorEjectTeams.Contains(ev.Player.Team);
        }

        public void HandleGeneratorInsert(InsertingGeneratorTabletEventArgs ev)
        {
            ev.IsAllowed = plugin.Config.GeneratorInsertTeams.Contains(ev.Player.Team);
        }

        public void HandleGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {
            if (!plugin.Config.GeneratorUnlockTeams.Contains(ev.Player.Team))
            {
                ev.IsAllowed = false;
                return;
            }
            if (plugin.Config.GeneratorUnlockItems.Count == 0)
            {
                return;
            }
            if (plugin.Config.GeneratorUnlockItems.Contains(ev.Player.Inventory.NetworkcurItem))
            {
                ev.IsAllowed = true;
            }
        }

        public void HandleFemurEnter(EnteringFemurBreakerEventArgs ev)
        {
            if (plugin.Config.Scp106LureAmount < 0)
            {
                return;
            }
            if (!plugin.Config.Scp106LureTeam.Contains(ev.Player.Team))
            {
                ev.IsAllowed = false;
                return;
            }
            if (++State.LuresCount < plugin.Config.Scp106LureAmount)
            {
                State.RunCoroutine(RunLureReload());
            }
        }

        public void HandlePlayerLeave(LeftEventArgs ev)
        {
            if (State.PrevPos.ContainsKey(ev.Player))
            {
                State.PrevPos.Remove(ev.Player);
            }
            if (State.AfkTime.ContainsKey(ev.Player))
            {
                State.AfkTime.Remove(ev.Player);
            }
        }

        public void HandleWarheadDetonation()
        {
            if (!plugin.Config.WarheadCleanup)
            {
                return;
            }
            foreach (Pickup pickup in Object.FindObjectsOfType<Pickup>())
            {
                if (pickup.transform.position.y < 5f)
                {
                    NetworkServer.Destroy(pickup.gameObject);
                }
            }
            foreach (Ragdoll ragdoll in Object.FindObjectsOfType<Ragdoll>())
            {
                if (ragdoll.transform.position.y < 5f)
                {
                    NetworkServer.Destroy(ragdoll.gameObject);
                }
            }
        }

        private IEnumerator<float> RunForceRestart()
        {
            yield return Timing.WaitForSeconds(plugin.Config.ForceRestart);
            Log.Info("Restarting round.");
            PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
            yield break;
        }

        private IEnumerator<float> RunAutoWarhead()
        {
            yield return Timing.WaitForSeconds(plugin.Config.AutoWarheadTime);
            if (plugin.Config.AutoWarheadLock)
            {
                Exiled.API.Features.Warhead.IsLocked = true;
            }
            if (Exiled.API.Features.Warhead.IsDetonated || Exiled.API.Features.Warhead.IsInProgress)
            {
                Log.Info("Warhead is detonated or is in progress.");
                yield break;
            }
            Log.Info("Activating Warhead.");
            Exiled.API.Features.Warhead.Start();
            if (!string.IsNullOrEmpty(plugin.Config.AutoWarheadStartText))
            {
                Map.Broadcast(10, plugin.Config.AutoWarheadStartText, Broadcast.BroadcastFlags.Normal);
            }
        }

        private IEnumerator<float> RunAutoCleanup()
        {
            for (; ; )
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
                yield return Timing.WaitForSeconds(plugin.Config.ItemAutoCleanup);
            }
        }

        private IEnumerator<float> RunLureReload()
        {
            yield return Timing.WaitForSeconds(plugin.Config.Scp106LureReload > 0 ? plugin.Config.Scp106LureReload : 0);
            Object.FindObjectOfType<LureSubjectContainer>().NetworkallowContain = false;
            yield break;
        }

        private IEnumerator<float> RunSelfHealing()
        {
            for(; ; )
            {
                foreach (Exiled.API.Features.Player ply in Exiled.API.Features.Player.List)
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

        private void DoSelfHealing(Exiled.API.Features.Player ply)
        {
            if (ply.IsHost || !plugin.Config.SelfHealingAmount.TryGetValue(ply.Role, out int amount) || !plugin.Config.SelfHealingDuration.TryGetValue(ply.Role, out int duration))
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

        private void RunRestoreMaxHp(Exiled.API.Features.Player player, int maxHp)
        {
            player.MaxHealth = maxHp;
            player.Health = maxHp;
        }
    }
}
