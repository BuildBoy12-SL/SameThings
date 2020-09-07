using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Mirror;
using NorthwoodLib;
using System;
using System.Linq;
using UnityEngine;

using Object = UnityEngine.Object;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;
using Warhead = Exiled.Events.Handlers.Warhead;

namespace SameThings
{
    internal sealed class EventHandlers
    {
        internal SameThings Plugin => SameThings.Instance;

        #region Subscription

        internal void SubscribeAll()
        {
            Server.RoundStarted += HandleRoundStart;
            Server.RestartingRound += HandleRoundRestaring;

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
            Server.RestartingRound -= HandleRoundRestaring;

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

        #endregion

        #region Handlers

        public void HandleRoundStart()
        {
            if (Plugin.Config.AutoWarheadLock)
                Exiled.API.Features.Warhead.IsLocked = false;

            if (Plugin.Config.ForceRestart > -1)
                State.RunCoroutine(HandlerHelper.RunForceRestart());

            if (Plugin.Config.AutoWarheadTime > -1)
                State.RunCoroutine(HandlerHelper.RunAutoWarhead());

            if (Plugin.Config.ItemAutoCleanup != 0)
                State.RunCoroutine(HandlerHelper.RunAutoCleanup());

            if (Plugin.Config.DecontaminationTime > -1)
                LightContainmentZoneDecontamination.DecontaminationController.Singleton.TimeOffset = (float)((11.7399997711182 - Plugin.Config.DecontaminationTime) * 60.0);

            if (Plugin.Config.GeneratorDuration > -1)
            {
                foreach (Generator079 generator in Generator079.Generators)
                {
                    generator.startDuration = Plugin.Config.GeneratorDuration;
                    generator.SetTime(Plugin.Config.GeneratorDuration);
                }
            }

            if (Plugin.Config.SelfHealingDuration.Count > 0)
                State.RunCoroutine(HandlerHelper.RunSelfHealing());

            if (Plugin.Config.Scp106LureAmount > 0)
                Object.FindObjectOfType<LureSubjectContainer>().SetState(true);
        }

        public void HandleRoundRestaring()
        {
            State.Refresh();
        }

        public void HandlePlayerJoin(JoinedEventArgs ev)
        {
            Timing.CallDelayed(0.25f, () =>
            {
                State.AfkTime[ev.Player] = 0;
                State.PrevPos[ev.Player] = Vector3.zero;

                if (!ev.Player.ReferenceHub.serverRoles.Staff
                && Plugin.Config.NicknameFilter.Any((string s) => ev.Player.Nickname.Contains(s, StringComparison.OrdinalIgnoreCase)))
                {
                    ev.Player.Disconnect($"{Plugin.Config.NicknameFilterReason} [Disconnect by SameThings Exiled Plugin]");
                }
            });
        }

        public void HandleTeslaTrigger(TriggeringTeslaEventArgs ev)
        {
            ev.IsTriggerable = Plugin.Config.TeslaTriggerableTeam.Contains(ev.Player.Team);
        }

        public void HandleWeaponShoot(ShootingEventArgs ev)
        {
            if (Plugin.Config.InfiniteAmmo)
                ev.Shooter.SetWeaponAmmo(ev.Shooter.CurrentItem, (int)ev.Shooter.CurrentItem.durability + 1);
        }

        public void HandleSetClass(ChangingRoleEventArgs ev)
        {
            if (Plugin.Config.MaxHealth.TryGetValue(ev.NewRole, out int maxHp))
                HandlerHelper.RunRestoreMaxHp(ev.Player, maxHp);
        }

        public void HandleDroppedItem(ItemDroppedEventArgs ev)
        {
            if (Plugin.Config.ItemAutoCleanup == 0
                || Plugin.Config.ItemCleanupIgnore.Contains(ev.Pickup.ItemId))
            {
                return;
            }

            State.Pickups.Enqueue(ev.Pickup);
        }

        public void HandleGeneratorEject(EjectingGeneratorTabletEventArgs ev)
        {
            ev.IsAllowed = Plugin.Config.GeneratorEjectTeams.Contains(ev.Player.Team);
        }

        public void HandleGeneratorInsert(InsertingGeneratorTabletEventArgs ev)
        {
            ev.IsAllowed = Plugin.Config.GeneratorInsertTeams.Contains(ev.Player.Team);
        }

        public void HandleGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {
            if (!Plugin.Config.GeneratorUnlockTeams.Contains(ev.Player.Team))
            {
                ev.IsAllowed = false;
                return;
            }
            if (Plugin.Config.GeneratorUnlockItems.Length == 0)
                return;

            if (Plugin.Config.GeneratorUnlockItems.Contains(ev.Player.CurrentItem.id))
                ev.IsAllowed = true;
        }

        public void HandleFemurEnter(EnteringFemurBreakerEventArgs ev)
        {
            if (Plugin.Config.Scp106LureAmount < 0)
            {
                return;
            }
            if (!Plugin.Config.Scp106LureTeam.Contains(ev.Player.Team))
            {
                ev.IsAllowed = false;
                return;
            }
            if (++State.LuresCount < Plugin.Config.Scp106LureAmount)
            {
                State.RunCoroutine(HandlerHelper.RunLureReload());
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
            if (!Plugin.Config.WarheadCleanup)
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

        #endregion
    }
}
