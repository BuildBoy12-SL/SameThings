using Exiled.API.Features;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace SameThings
{
    internal static class State
    {
        internal static void Refresh()
        {
            foreach (CoroutineHandle coroutine in _coroutines)
                Timing.KillCoroutines(coroutine);

            _coroutines = new List<CoroutineHandle>();
            LuresCount = 0;
            Pickups = new Dictionary<Pickup, int>();
            PrevPos = new Dictionary<Player, Vector3>();
            AfkTime = new Dictionary<Player, int>();
            BreakableWindows = new List<BreakableWindow>();
        }

        internal static void RunCoroutine(IEnumerator<float> coroutine)
        {
            _coroutines.Add(Timing.RunCoroutine(coroutine));
        }

        internal static List<BreakableWindow> BreakableWindows;

        internal static Dictionary<Pickup, int> Pickups;

        internal static Dictionary<Player, Vector3> PrevPos;
        internal static Dictionary<Player, int> AfkTime;

        internal static int LuresCount;

        internal static List<CoroutineHandle> _coroutines;
    }
}
