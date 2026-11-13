// =============================================================
// File: /Sim/Systems/ResourceUse.cs
// =============================================================
using System;
using System.Linq;
using Evolvera.Sim.World;
using Evolvera.Sim.Units;
using Evolvera.Sim.Models;

namespace Evolvera.Sim.Systems
{
    public static class ResourceUse
    {
        // Very first pass: Worker harvest converts resource richness→food or domain XP to nearest settlement
        public static string Harvest(WorldMap map, Unit worker, Civilization civ)
        {
            var tile = map.Get(worker.X, worker.Y);
            if (worker.Type != UnitType.Worker) return "Only Workers can harvest.";
            if (tile.Resource is null) return "No resource here.";

            // nearest settlement by Euclidean distance
            var nearest = civ.Settlements.OrderBy(s => Math.Sqrt(Math.Pow((worker.X + 0.5) - s.Position.X, 2) + Math.Pow((worker.Y + 0.5) - s.Position.Y, 2))).FirstOrDefault();
            if (nearest is null) return "No settlement to deliver to.";

            if (!ResourceRules.MeetsPrereq(tile.Resource, nearest.Local))
                return $"{tile.Resource.Type} requires stronger domains to exploit.";

            // Payout: Food for organics, +Production XP for materials
            double gain = tile.Resource.Richness * 2.0;
            string note;
            switch (tile.Resource.Type)
            {
                case ResourceType.Game:
                    nearest.FoodStores += gain; note = $"Delivered {gain:F0} food"; break;
                case ResourceType.Timber:
                case ResourceType.Stone:
                case ResourceType.Clay:
                case ResourceType.Ore:
                case ResourceType.Salt:
                    nearest.Local = DomainMath.ApplyXp(nearest.Local, new Domains(0, gain * 0.4, 0, 0, 0, 0));
                    note = $"Boosted Production via {tile.Resource.Type}"; break;
                case ResourceType.Herbs:
                    nearest.Local = DomainMath.ApplyXp(nearest.Local, new Domains(0.2 * gain, 0, 0, 0, 0.1 * gain, 0.1 * gain));
                    note = "Herbal knowledge increased"; break;
                case ResourceType.Horses:
                    nearest.Local = DomainMath.ApplyXp(nearest.Local, new Domains(0, 0.1 * gain, 0.2 * gain, 0.3 * gain, 0, 0));
                    note = "Mounted mobility improved"; break;
                default:
                    note = "Minimal effect"; break;
            }

            // one-time harvest for now (Phase 4 could add depletion/regrowth)
            tile.Resource = null;
            return $"{nearest.Name}: {note}";
        }
    }
}