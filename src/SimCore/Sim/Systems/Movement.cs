// =============================================================
// File: /Sim/Systems/Movement.cs
// =============================================================
using System;
using Evolvera.Sim.World;
using Evolvera.Sim.Units;

namespace Evolvera.Sim.Systems
{
    public static class Movement
    {
        public static void ResetMoves(Unit u) => u.Moves = u.MovesPerTurn;

        public static bool Passable(Unit u, TerrainType t)
        {
            if (t == TerrainType.Mountain && u.Type != UnitType.Scout) return false; // only scouts climb
            return true;
        }

        public static bool TryMove(WorldMap map, Unit u, int dx, int dy)
        {
            int nx = u.X + dx; int ny = u.Y + dy;
            if (!map.InBounds(nx, ny)) return false;
            var tile = map.Get(nx, ny);
            if (!Passable(u, tile.Terrain)) return false;
            double cost = TerrainRules.Table[tile.Terrain].MoveCost;
            if (u.Moves < cost) return false;
            u.X = nx; u.Y = ny; u.Moves -= cost; return true;
        }
    }
}