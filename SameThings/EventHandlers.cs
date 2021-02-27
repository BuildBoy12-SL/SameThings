using Exiled.API.Extensions;
using Exiled.Events.EventArgs;
using MEC;
using Mirror;
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
        private static SameThings Plugin => SameThings.Instance;

        #region Subscription

        internal static void SubscribeAll()
        {
            Server.RoundStarted += HandleRoundStart;
            Server.RestartingRound += HandleRoundRestarting;

            Player.Verified += HandlePlayerVerified;
            Player.TriggeringTesla += HandleTeslaTrigger;
            Player.Shooting += HandleWeaponShoot;
            Player.ChangingRole += HandleSetClass;
            Player.ItemDropped += HandleDroppedItem;

            Player.EjectingGeneratorTablet += HandleGeneratorEject;
            Player.InsertingGeneratorTablet += HandleGeneratorInsert;
            Player.UnlockingGenerator += HandleGeneratorUnlock;
            Player.EnteringFemurBreaker += HandleFemurEnter;

            Player.Destroying += HandlePlayerDestroying;
            Warhead.Detonated += HandleWarheadDetonation;
        }

        internal static void UnSubscribeAll()
        {
            Server.RoundStarted -= HandleRoundStart;
            Server.RestartingRound -= HandleRoundRestarting;

            Player.Verified -= HandlePlayerVerified;
            Player.TriggeringTesla -= HandleTeslaTrigger;
            Player.Shooting -= HandleWeaponShoot;
            Player.ChangingRole -= HandleSetClass;
            Player.ItemDropped -= HandleDroppedItem;

            Player.EjectingGeneratorTablet -= HandleGeneratorEject;
            Player.InsertingGeneratorTablet -= HandleGeneratorInsert;
            Player.UnlockingGenerator -= HandleGeneratorUnlock;
            Player.EnteringFemurBreaker -= HandleFemurEnter;

            Player.Destroying -= HandlePlayerDestroying;
            Warhead.Detonated -= HandleWarheadDetonation;
        }

        #endregion

        #region Handlers

        private static void HandleRoundStart()
        {
            if (Plugin.Config.ForceRestart > -1)
                State.RunCoroutine(HandlerHelper.RunForceRestart());

            if (Plugin.Config.ItemAutoCleanup != 0)
                State.RunCoroutine(HandlerHelper.RunAutoCleanup());

            if (Plugin.Config.DecontaminationTime > -1)
                LightContainmentZoneDecontamination.DecontaminationController.Singleton.TimeOffset =
                    (float) ((11.7399997711182 - Plugin.Config.DecontaminationTime) * 60.0);

            if (Plugin.Config.GeneratorDuration > -1)
            {
                foreach (Generator079 generator in Generator079.Generators)
                {
                    generator.startDuration = Plugin.Config.GeneratorDuration;
                    generator.SetTime(Plugin.Config.GeneratorDuration);
                }
            }

            if (!Plugin.Config.SelfHealingDuration.IsEmpty() && !Plugin.Config.SelfHealingAmount.IsEmpty())
                State.RunCoroutine(HandlerHelper.RunSelfHealing());

            if (Plugin.Config.Scp106LureAmount < 1)
                Object.FindObjectOfType<LureSubjectContainer>().SetState(true);

            HandlerHelper.SetupWindowsHealth();

            if (Plugin.Config.InsertTablets)
            {
                foreach (WorkStation workstation in Object.FindObjectsOfType<WorkStation>())
                {
                    workstation.NetworkisTabletConnected = true;
                }
            }
        }

        private static void HandleRoundRestarting()
        {
            State.Refresh();
        }

        private static void HandlePlayerVerified(VerifiedEventArgs ev)
        {
            Timing.CallDelayed(0.25f, () =>
            {
                State.AfkTime[ev.Player] = 0;
                State.PrevPos[ev.Player] = Vector3.zero;

                if (Plugin.Config.NicknameFilter.Length == 0)
                    return;

                if (!ev.Player.ReferenceHub.serverRoles.Staff
                    && Plugin.Config.NicknameFilter.Any(s =>
                        ev.Player.Nickname.ToLower().Contains(s.ToLower())))
                {
                    ev.Player.Disconnect(
                        $"{Plugin.Config.NicknameFilterReason} [Disconnect by SameThings Exiled Plugin]");
                }
            });
        }

        private static void HandleTeslaTrigger(TriggeringTeslaEventArgs ev)
        {
            ev.IsTriggerable = Plugin.Config.TeslaTriggerableTeam.Contains(ev.Player.Team);
        }

        private static void HandleWeaponShoot(ShootingEventArgs ev)
        {
            if (Plugin.Config.InfiniteAmmo)
                ev.Shooter.SetWeaponAmmo(ev.Shooter.CurrentItem, (int) ev.Shooter.CurrentItem.durability + 1);
        }

        private static void HandleSetClass(ChangingRoleEventArgs ev)
        {
            if (Plugin.Config.MaxHealth.TryGetValue(ev.NewRole, out int maxHp))
                HandlerHelper.RunRestoreMaxHp(ev.Player, maxHp);
        }

        private static void HandleDroppedItem(ItemDroppedEventArgs ev)
        {
            if (Plugin.Config.ItemAutoCleanup == 0
                || Plugin.Config.ItemCleanupIgnore.Contains(ev.Pickup.ItemId))
            {
                return;
            }

            State.Pickups.Enqueue(ev.Pickup);
        }

        private static void HandleGeneratorEject(EjectingGeneratorTabletEventArgs ev)
        {
            ev.IsAllowed = Plugin.Config.GeneratorEjectTeams.Contains(ev.Player.Team);
        }

        private static void HandleGeneratorInsert(InsertingGeneratorTabletEventArgs ev)
        {
            ev.IsAllowed = Plugin.Config.GeneratorInsertTeams.Contains(ev.Player.Team);
        }

        private static void HandleGeneratorUnlock(UnlockingGeneratorEventArgs ev)
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

        #region SCP-106

        private static void HandleFemurEnter(EnteringFemurBreakerEventArgs ev)
        {
            // That means the femur breaker is always open
            if (Plugin.Config.Scp106LureAmount < 1)
                return;

            // Allowed team check
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

        /*public void HandleContain106(ContainingEventArgs ev)
        {
            // That means it's disabled
            if (Plugin.Config.Scp106LureAmount < 1)
                return;

            ev.IsAllowed = State.LuresCount > Plugin.Config.Scp106LureAmount;
        }*/

        #endregion

        private static void HandlePlayerDestroying(DestroyingEventArgs ev)
        {
            State.PrevPos.Remove(ev.Player);
            State.AfkTime.Remove(ev.Player);
        }

        private static void HandleWarheadDetonation()
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
