using Exiled.API.Features;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace SameThings
{
    internal static class State
    {
        internal static readonly List<BreakableWindow> BreakableWindows = new List<BreakableWindow>();
        internal static readonly List<CoroutineHandle> Coroutines = new List<CoroutineHandle>(10);

        internal static readonly Queue<Pickup> Pickups = new Queue<Pickup>(150);
        internal static readonly Dictionary<Player, Vector3> PrevPos = new Dictionary<Player, Vector3>(20);
        internal static readonly Dictionary<Player, int> AfkTime = new Dictionary<Player, int>(20);

        internal static int _luresCount;

        internal static void Refresh()
        {
            Timing.KillCoroutines(Coroutines);

            Coroutines.Clear();
            Pickups.Clear();
            PrevPos.Clear();
            AfkTime.Clear();
            BreakableWindows.Clear();

            _luresCount = 0;
        }

        internal static void RunCoroutine(IEnumerator<float> coroutine)
        {
            Coroutines.Add(Timing.RunCoroutine(coroutine));
        }
    }
}
