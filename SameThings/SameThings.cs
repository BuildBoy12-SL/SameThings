using Exiled.API.Features;
using HarmonyLib;

namespace SameThings
{
    public class SameThings : Plugin<Config>
    {
        internal static SameThings Instance;

        public override string Name => "SameThings";
        public override string Author => "Build";

        public readonly Harmony Harmony = new Harmony(nameof(SameThings).ToLowerInvariant());
        private readonly EventHandlers EventHandlers = new EventHandlers();

        public override void OnEnabled()
        {
            base.OnEnabled();

            Instance = this;

            EventHandlers.SubscribeAll();
            Harmony.PatchAll();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            EventHandlers.UnSubscribeAll();
            Harmony.UnpatchAll();

            Instance = null;
        }
    }
}
