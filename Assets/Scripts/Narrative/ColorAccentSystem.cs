using System;
using System.Collections.Generic;
using UnityEngine;

namespace aerisOS.Narrative
{
    public static class ColorAccentSystem
    {
        private static readonly List<Action<Color>> _listeners = new List<Action<Color>>();

        public static void Register(Action<Color> onApply)   => _listeners.Add(onApply);
        public static void Unregister(Action<Color> onApply) => _listeners.Remove(onApply);

        public static void Apply(Color color)
        {
            PlayerState.AccentColor = color;
            foreach (var l in _listeners)
                l?.Invoke(color);
        }
    }
}
