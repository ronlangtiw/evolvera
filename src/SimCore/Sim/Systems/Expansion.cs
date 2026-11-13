// =============================================================
// File: /Sim/Systems/Expansion.cs
// =============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using Evolvera.Sim.Models;

namespace Evolvera.Sim.Systems
{
    public static class Expansion
    {
        public static string UniqueName(Evolvera.Sim.Models.Civilization civ, string baseName)
        {
            var taken = new HashSet<string>(civ.Settlements.Select(x => x.Name));
            if (!taken.Contains(baseName)) return baseName;
            int i = 2;
            string candidate = $"{baseName} {i}";
            while (taken.Contains(candidate)) candidate = $"{baseName} {++i}";
            return candidate;
        }

        public static Settlement FoundSettlement(Civilization civ, Settlement parent, Vec2 pos, string name, int settlerPop, SettlementKind kind = SettlementKind.Hamlet)
        {
            settlerPop = Math.Clamp(settlerPop, 10, Math.Max(10, parent.Population / 2));
            parent.Population -= settlerPop;
            var child = new Settlement
            {
                Name = name,
                Kind = kind,
                Position = pos,
                Population = settlerPop,
                Local = parent.Local, // inherit cultural baseline
                FoodStores = 20,
                Legitimacy = Math.Max(0.45, parent.Legitimacy - 0.05)
            };
            civ.Settlements.Add(child);
            return child;
        }

        public static Settlement FoundOutpost(Civilization civ, Settlement parent, Vec2 offset, string name)
        {
            var pos = parent.Position + offset;
            return FoundSettlement(civ, parent, pos, name, settlerPop: Math.Max(15, (int)(parent.Population * 0.10)), kind: SettlementKind.Hamlet);
        }

        public static void MigrateCamp(Settlement camp, Vec2 step)
        {
            if (!camp.IsMobile) camp.IsMobile = true;
            camp.Position = camp.Position + step; // Phase 2: simple drift; Phase 3 can add trail influence
            camp.Legitimacy = Math.Clamp(camp.Legitimacy * 0.995, 0.0, 1.0); // slight strain while moving
        }

        // Lightweight heuristic: propose expansion when prosperous or overcrowded
        public static bool ShouldExpand(Settlement s)
        {
            bool prosperous = s.SurplusRatio() > 1.8 && s.FoodStores > 80;
            bool crowded = s.Population > s.CarryCapacity() * 0.95;
            return prosperous || crowded;
        }

        public static Vec2 ProposeNewSite(Settlement s, Random rng)
        {
            // Bias away from nearest neighbor to spread
            var angle = rng.NextDouble() * Math.PI * 2;
            double dist = 4 + rng.NextDouble() * 4; // 4..8 units
            return new Vec2(Math.Cos(angle) * dist, Math.Sin(angle) * dist);
        }
    }
}