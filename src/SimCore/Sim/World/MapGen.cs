// =============================================================
// File: /Sim/World/MapGen.cs
// =============================================================
using System;
using System.Collections.Generic;
using Evolvera.Sim.World;

namespace Evolvera.Sim.WorldGen
{
    public static class MapGen
    {
        // quick hash → [0,1)
        private static double Noise(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)seed;
                h ^= (uint)x * 374761393u;
                h = (h << 13) ^ h;
                h *= 1274126177u;
                h ^= (uint)y * 668265263u;
                h = (h << 15) ^ h;
                h *= 2246822519u;
                return (h & 0xFFFFFFu) / 16777216.0; // 2^24
            }
        }


        public static WorldMap Generate(int w, int h, int seed)
        {
            var map = new WorldMap(w, h);
            // Two channels: elevation, moisture
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    double e = (Noise(x, y, seed) + 0.5 * Noise(x / 2, y / 2, seed + 11)) / 1.5;
                    double m = (Noise(x + 999, y - 777, seed + 23) + 0.5 * Noise((x + 3) / 2, (y - 5) / 2, seed + 37)) / 1.5;

                    TerrainType tt = TerrainType.Plains;
                    if (e > 0.75) tt = TerrainType.Mountain;
                    else if (e > 0.60) tt = TerrainType.Hills;
                    else if (m > 0.70) tt = TerrainType.Wetland;
                    else if (m < 0.25) tt = TerrainType.Desert;
                    else if (m > 0.45) tt = TerrainType.Forest;
                    else tt = TerrainType.Plains;

                    var tile = map.Tiles[x, y];
                    tile.Terrain = tt;
                }

            // Rivers: carve a couple of meanders from top toward bottom
            var rng = new Random(seed);
            int rivers = Math.Max(1, w / 16);
            for (int r = 0; r < rivers; r++)
            {
                int rx = rng.Next(0, w);
                int ry = 0;
                for (int steps = 0; steps < h * 2; steps++)
                {
                    if (!map.InBounds(rx, ry)) break;
                    map.Tiles[rx, ry].HasRiver = true;
                    // drift downwards, bias to lower elevation-ish using noise
                    int dir = rng.Next(0, 3); // down-left, down, down-right
                    rx += dir == 0 ? -1 : dir == 2 ? +1 : 0;
                    ry += 1;
                }
            }

            // Resources by terrain
            foreach (var t in map.Tiles)
            {
                var table = ResourceRules.SpawnByTerrain[t.Terrain];
                // sparse
                if (rng.NextDouble() < 0.18)
                {
                    int total = 0; foreach (var p in table) total += p.weight;
                    int pick = rng.Next(total);
                    ResourceType choice = table[0].type;
                    foreach (var p in table) { if (pick < p.weight) { choice = p.type; break; } pick -= p.weight; }
                    t.Resource = new ResourceDeposit { Type = choice, Richness = 1 + rng.Next(5), Difficulty = 1 + rng.Next(5) };
                }
            }

            return map;
        }
    }
}
