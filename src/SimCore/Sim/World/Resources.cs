// =============================================================
// File: /Sim/World/Resources.cs
// =============================================================
using System;
using System.Collections.Generic;
using Evolvera.Sim.Models;

namespace Evolvera.Sim.World
{
    public enum ResourceType { Game, Timber, Stone, Ore, Clay, Herbs, Horses, Salt }

    public sealed class ResourceDeposit
    {
        public ResourceType Type { get; init; }
        public int Richness { get; init; } // 1..5
        public int Difficulty { get; init; } // 1..5
    }

    public static class ResourceRules
    {
        // Terrain → possible resources (+weights)
        public static readonly Dictionary<TerrainType, (ResourceType type, int weight)[]> SpawnByTerrain = new()
        {
            [TerrainType.Plains] = new[] { (ResourceType.Game, 4), (ResourceType.Horses, 2), (ResourceType.Clay, 2) },
            [TerrainType.Forest] = new[] { (ResourceType.Timber, 5), (ResourceType.Herbs, 3), (ResourceType.Game, 3) },
            [TerrainType.Hills] = new[] { (ResourceType.Stone, 4), (ResourceType.Ore, 3), (ResourceType.Herbs, 2) },
            [TerrainType.Mountain] = new[] { (ResourceType.Stone, 5), (ResourceType.Ore, 4) },
            [TerrainType.Wetland] = new[] { (ResourceType.Herbs, 4), (ResourceType.Clay, 4), (ResourceType.Game, 2) },
            [TerrainType.Desert] = new[] { (ResourceType.Salt, 4), (ResourceType.Stone, 2) },
        };

        // Very first pass: domain thresholds to effectively exploit a resource
        public static bool MeetsPrereq(ResourceDeposit dep, Domains d)
        {
            return dep.Type switch
            {
                ResourceType.Game => d.S >= 6,
                ResourceType.Timber => d.P >= 6,
                ResourceType.Stone => d.P >= 6,
                ResourceType.Ore => d.P >= 8 && d.S >= 6,
                ResourceType.Clay => d.P >= 6,
                ResourceType.Herbs => d.S >= 7 || d.X >= 6,
                ResourceType.Horses => d.E >= 7 || d.W >= 6,
                ResourceType.Salt => d.P >= 7,
                _ => false
            };
        }
    }
}