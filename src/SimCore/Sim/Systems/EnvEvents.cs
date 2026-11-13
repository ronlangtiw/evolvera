// =============================================================
// File: /Sim/Systems/EnvEvents.cs
// =============================================================
using System;
using Evolvera.Sim.World;
using Evolvera.Sim.Models;

namespace Evolvera.Sim.Systems
{
    public static class EnvEvents
    {
        public static string? Tick(Random rng, WorldMap map, Civilization civ)
        {
            if (rng.NextDouble() > 0.22) return null; // ~22% per turn
            var s = civ.Settlements.Count > 0 ? civ.Settlements[rng.Next(civ.Settlements.Count)] : null;
            if (s is null) return null;
            // Terrain-biased simple events
            var tileX = (int)Math.Clamp(Math.Round(s.Position.X - 0.5), 0, map.Width - 1);
            var tileY = (int)Math.Clamp(Math.Round(s.Position.Y - 0.5), 0, map.Height - 1);
            var t = map.Get(tileX, tileY).Terrain;
            string name;
            switch (t)
            {
                case TerrainType.Desert:
                    name = "Drought"; s.Local = DomainMath.ApplyXp(s.Local, new Domains(-0.45, 0, 0, 0, -0.15, 0)); break;
                case TerrainType.Wetland:
                    name = "Flood"; s.Local = DomainMath.ApplyXp(s.Local, new Domains(+0.30, -0.15, 0, 0, 0, 0)); break;
                default:
                    name = "Plague"; s.Local = DomainMath.ApplyXp(s.Local, new Domains(0, 0, 0, -0.10, -0.45, -0.15)); break;
            }
            return $"Event at {s.Name}: {name}";
        }
    }
}