using System;
using Exiled.API.Features;
using HarmonyLib;

namespace SameThings
{
    public sealed class SameThings : Plugin<SameThingsConfig>
    {
        internal static SameThings Instance;

        public override string Name => "SameThings";
        public override string Author => "Build";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(2, 3, 4);

        private readonly Harmony _harmony = new Harmony(nameof(SameThings).ToLowerInvariant());
        private readonly EventHandlers _eventHandlers = new EventHandlers();

        public override void OnEnabled()
        {
            base.OnEnabled();

            Instance = this;

            EventHandlers.SubscribeAll();
            _harmony.PatchAll();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            EventHandlers.UnSubscribeAll();
            _harmony.UnpatchAll();

            Instance = null;
        }
    }
}
