using Exiled.API.Features;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace SameThings
{
    internal static class State
    {
        internal static readonly List<BreakableWindow> _breakableWindows = new List<BreakableWindow>();
        internal static readonly List<CoroutineHandle> _coroutines = new List<CoroutineHandle>(10);

        internal static readonly Dictionary<Pickup, int> _pickups = new Dictionary<Pickup, int>(150);
        internal static readonly Dictionary<Player, Vector3> _prevPos = new Dictionary<Player, Vector3>(20);
        internal static readonly Dictionary<Player, int> _afkTime = new Dictionary<Player, int>(20);

        internal static int _luresCount;

        internal static void Refresh()
        {
            Timing.KillCoroutines(_coroutines);

            _coroutines.Clear();
            _pickups.Clear();
            _prevPos.Clear();
            _afkTime.Clear();
            _breakableWindows.Clear();

            _luresCount = 0;
        }

        internal static void RunCoroutine(IEnumerator<float> coroutine)
        {
            _coroutines.Add(Timing.RunCoroutine(coroutine));
        }
    }
}
