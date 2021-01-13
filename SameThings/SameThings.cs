using Exiled.API.Features;
using HarmonyLib;

namespace SameThings
{
    public sealed class SameThings : Plugin<SameThingsConfig>
    {
        internal static SameThings Instance;

        public override string Name => "SameThings";
        public override string Author => "Build";

        private readonly Harmony _harmony = new Harmony(nameof(SameThings).ToLowerInvariant());
        private readonly EventHandlers _eventHandlers = new EventHandlers();

        public override void OnEnabled()
        {
            base.OnEnabled();

            Instance = this;

            _eventHandlers.SubscribeAll();
            _harmony.PatchAll();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            _eventHandlers.UnSubscribeAll();
            _harmony.UnpatchAll();

            Instance = null;
        }
    }
}