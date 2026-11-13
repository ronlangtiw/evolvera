// =============================================================
// File: /Sim/Systems/Actions.cs
// =============================================================
using System;
using Evolvera.Sim.Models;

namespace Evolvera.Sim.Systems
{
    public static class Actions
    {
        // Local action XP grants (mirrors global actions — can be tuned per kind)
        public static Domains Hunt => new(1.0, 0.0, 0.5, 0.0, 0.0, 0.0);
        public static Domains Farm => new(2.0, 1.0, 0.0, 0.0, 0.0, 0.0);
        public static Domains Raid => new(0.0, 1.0, 2.0, 0.0, -0.5, 0.0);
        public static Domains Trade => new(0.0, 0.5, 0.0, 1.0, 1.0, 0.0);
        public static Domains Ritual => new(0.0, 0.0, 0.0, 0.0, 0.5, 1.5);
        public static Domains Explore => new(0.5, 0.0, 0.0, 1.5, 0.0, 0.0);
        public static Domains Infrastructure => new(0.0, 1.5, 0.0, 0.0, 0.5, 0.0);

        private static readonly Domains[] ActionBag = new[] { Hunt, Farm, Raid, Trade, Ritual, Explore, Infrastructure };

        public static Domains RandomActionXp(Random rng)
        {
            var a1 = ActionBag[rng.Next(ActionBag.Length)];
            var a2 = ActionBag[rng.Next(ActionBag.Length)];
            return a1 + a2; // two actions per turn
        }
    }
}