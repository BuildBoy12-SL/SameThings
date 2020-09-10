using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace SameThings
{
    public sealed class SameThingsConfig : IConfig
    {
        #region Default

        public bool IsEnabled { get; set; } = true;

        #endregion

        #region Environment

        // Backward compatibility to 'window_health'
        [Description("Sets the health of breakable windows. Values below 1 disable this.")]
        public int WindowHealth { get; set; } = 0;

        // Backward compatibility to 'unlimited_radio_battery'
        [Description("If radios should have unlimited charge.")]
        public bool UnlimitedRadioBattery { get; set; } = false;

        // Backward compatibility to 'lift_move_duration'
        [Description("Amount of time an elevator takes to transition levels. Values below 0 disable this.")]
        public float LiftMoveDuration { get; set; } = 5f;

        [Description("If all regular guns should have infinite ammo.")]
        public bool InfiniteAmmo { get; set; } = false;

        #endregion

        #region Warhead

        // Not yet implemented, pending reorganized or something else.
        /*
        // No backward compatibility
        [Description("Text to be broadcasted when the AutoWarhead starts.")]
        public string AutoWarheadStartText { get; set; }

        // No backward compatibility
        [Description("Duration of the broadcast in seconds.")]
        public ushort AutoWarheadStartTextTime { get; set; } = 10;
        */

        // Backward compatibility to 'nuke_cleanup'
        [Description("If all items and ragdolls in the facility should be removed after detonation.")]
        public bool WarheadCleanup { get; set; } = true;

        #endregion

        #region Item cleanup

        [Description("Cleans up items dropped by players after this amount of time. A zero value will disable this.")]
        public uint ItemAutoCleanup { get; set; } = 0;

        [Description("Ignores these items during cleanup.")]
        public ItemType[] ItemCleanupIgnore { get; set; } =
        {
            ItemType.MicroHID,
            ItemType.KeycardO5
        };

        #endregion

        #region Misc

        [Description("Restarts the round after this many seconds. Values below 0 disable this.")]
        public int ForceRestart { get; set; } = -1;

        [Description("How many minutes before decontamination should start. Values below 0 disable this.")]
        public int DecontaminationTime { get; set; } = -1;

        [Description("If the micro ammo is infinite.")]
        public bool InfiniteMicroAmmo { get; set; } = false;

        [Description("Amount of time it takes for a generator to activate. Values below 0 disable this.")]
        public int GeneratorDuration { get; set; } = -1;

        [Description("How many sacrifices it takes to lure 106. Values below 1 set the recontainer to always active.")]
        public int Scp106LureAmount { get; set; } = 0;

        [Description("Amount of time before another sacrifice can be made.")]
        public int Scp106LureReload { get; set; } = 0;

        [Description("Teams that can enter the femur breaker.")]
        public Team[] Scp106LureTeam { get; set; } = { Team.MTF, Team.CHI, Team.RSC, Team.CDP };

        [Description("Cancels a player connection if they have any of these in their name.")]
        public string[] NicknameFilter { get; set; } = { ".com", ".org" };

        [Description("Displays this message when they get disconnected by the nickname filter.")]
        public string NicknameFilterReason { get; set; } = "Disconnected by this servers name filter.";

        [Description("Teams that can unlock generators.")]
        public Team[] GeneratorUnlockTeams { get; set; } = { Team.SCP, Team.MTF, Team.CHI, Team.RSC, Team.CDP };

        [Description("Items that can unlock generators. Ignored if the list is empty.")]
        public ItemType[] GeneratorUnlockItems { get; set; } =
        {
            ItemType.KeycardSeniorGuard,
            ItemType.KeycardNTFLieutenant,
            ItemType.KeycardNTFCommander,
            ItemType.KeycardO5
        };

        [Description("Teams that can insert generators.")]
        public Team[] GeneratorInsertTeams { get; set; } = new Team[]
        {
            Team.SCP,
            Team.MTF,
            Team.CHI,
            Team.RSC,
            Team.CDP
        };

        [Description("Teams that can eject generators.")]
        public Team[] GeneratorEjectTeams { get; set; } = new Team[]
        {
            Team.SCP,
            Team.MTF,
            Team.CHI,
            Team.RSC,
            Team.CDP
        };

        [Description("Teams that can trigger the tesla gates.")]
        public Team[] TeslaTriggerableTeam { get; set; } = new Team[]
        {
            Team.SCP,
            Team.MTF,
            Team.CHI,
            Team.RSC,
            Team.CDP,
        };

        [Description("To ignore a role, remove its entry. Sets the roles maximum health.")]
        public Dictionary<RoleType, int> MaxHealth { get; set; } = new Dictionary<RoleType, int>
        {
            [RoleType.Scp173] = 3200,
            [RoleType.ClassD] = 100,
            [RoleType.Spectator] = 100,
            [RoleType.Scp106] = 650,
            [RoleType.NtfScientist] = 120,
            [RoleType.Scp049] = 1700,
            [RoleType.Scientist] = 100,
            [RoleType.Scp079] = 100_000,
            [RoleType.ChaosInsurgency] = 120,
            [RoleType.Scp096] = 500,
            [RoleType.Scp0492] = 300,
            [RoleType.NtfLieutenant] = 120,
            [RoleType.NtfCommander] = 150,
            [RoleType.NtfCadet] = 100,
            [RoleType.Tutorial] = 100,
            [RoleType.FacilityGuard] = 100,
            [RoleType.Scp93953] = 2200,
            [RoleType.Scp93989] = 2200
        };

        [Description("Amount of time a role can stand still before their natural regen cancels.")]
        public Dictionary<RoleType, int> SelfHealingDuration { get; set; } = new Dictionary<RoleType, int>
        {
            { RoleType.Scp096, 5 }
        };

        [Description("Amount of health a role will heal every second.")]
        public Dictionary<RoleType, int> SelfHealingAmount { get; set; } = new Dictionary<RoleType, int>
        {
            { RoleType.Scp096, 1 }
        };

        #endregion
    }
}
