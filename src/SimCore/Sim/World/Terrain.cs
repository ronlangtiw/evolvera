// =============================================================
// File: /Sim/World/Terrain.cs
// =============================================================
using System;
using System.Collections.Generic;

namespace Evolvera.Sim.World
{
    public enum TerrainType { Plains, Forest, Hills, Mountain, Wetland, Desert }

    public sealed class TerrainRules
    {
        public TerrainType Type { get; init; }
        public double MoveCost { get; init; } = 1.0;
        public bool Passable { get; init; } = true;
        // Expression modifier is gently positive for beautiful/varied terrain
        public double ExpressionMod { get; init; } = 0.0;

        public static readonly Dictionary<TerrainType, TerrainRules> Table = new()
        {
            [TerrainType.Plains] = new TerrainRules { Type = TerrainType.Plains, MoveCost = 1.0, Passable = true, ExpressionMod = +0.05 },
            [TerrainType.Forest] = new TerrainRules { Type = TerrainType.Forest, MoveCost = 1.5, Passable = true, ExpressionMod = +0.10 },
            [TerrainType.Hills] = new TerrainRules { Type = TerrainType.Hills, MoveCost = 1.8, Passable = true, ExpressionMod = +0.08 },
            [TerrainType.Mountain] = new TerrainRules { Type = TerrainType.Mountain, MoveCost = 3.5, Passable = true, ExpressionMod = +0.03 },
            [TerrainType.Wetland] = new TerrainRules { Type = TerrainType.Wetland, MoveCost = 2.0, Passable = true, ExpressionMod = +0.04 },
            [TerrainType.Desert] = new TerrainRules { Type = TerrainType.Desert, MoveCost = 2.2, Passable = true, ExpressionMod = -0.02 },
        };
    }
}
