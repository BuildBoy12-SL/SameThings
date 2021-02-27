using Exiled.API.Features;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace SameThings
{
    internal static class State
    {
        private static readonly List<CoroutineHandle> Coroutines = new List<CoroutineHandle>(10);

        internal static readonly Queue<Pickup> Pickups = new Queue<Pickup>(150);
        internal static readonly Dictionary<Player, Vector3> PrevPos = new Dictionary<Player, Vector3>(20);
        internal static readonly Dictionary<Player, int> AfkTime = new Dictionary<Player, int>(20);

        internal static int LuresCount;

        internal static void Refresh()
        {
            foreach (var coroutine in Coroutines)
                Timing.KillCoroutines(coroutine);

            Coroutines.Clear();
            Pickups.Clear();
            PrevPos.Clear();
            AfkTime.Clear();

            LuresCount = 0;
        }

        internal static void RunCoroutine(IEnumerator<float> coroutine)
        {
            Coroutines.Add(Timing.RunCoroutine(coroutine));
        }
    }
}
