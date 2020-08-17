using Exiled.API.Features;
using HarmonyLib;
using System;

namespace SameThings
{
    public class SameThings : Plugin<Config>
    {
        public Harmony Harmony { get; private set; }
        private int patchCounter;
        private EventHandlers EventHandlers;

        public override void OnEnabled()
        {
            base.OnEnabled();
            Instance = this;
            EventHandlers = new EventHandlers(this);
            EventHandlers.SubscribeAll();
            Harmony = new Harmony($"SameThings.{++patchCounter}");
            Harmony.PatchAll();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            EventHandlers.UnSubscribeAll();
            EventHandlers = null;
            Harmony.UnpatchAll();
            Instance = null;
        }

        public override string Name => "SameThings";
        public override Version Version => new Version(2, 0, 0);
        public override string Author => "Build";

        internal static SameThings Instance;
    }
}
