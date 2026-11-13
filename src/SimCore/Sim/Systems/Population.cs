// =============================================================
// File: /Sim/Systems/Population.cs
// =============================================================
using System;
using Evolvera.Sim.Models;

namespace Evolvera.Sim.Systems
{
    public static class PopulationSystem
    {
        // Returns a textual note if something notable happens (starvation, boom)
        public static string StepEconomyAndPopulation(Settlement s)
        {
            // Recompute production/consumption (slightly stronger consumption vs Phase 1)
            double prod = 0.09 * s.Population * (1.0 + 0.09 * (s.Local.S - 5) + 0.06 * (s.Local.P - 5));
            double cons = 0.10 * s.Population;
            s.FoodStores += prod - cons;

            string? note = null;
            double K = s.CarryCapacity();

            if (s.FoodStores <= 0)
            {
                // Starvation attrition
                int loss = Math.Max(1, (int)Math.Round(Math.Min(s.Population * 0.01, 8)));
                s.Population = Math.Max(10, s.Population - loss);
                s.FoodStores = 0;
                note = $"{s.Name}: starvation (-{loss} pop)";
            }
            else
            {
                // Growth limited by carrying capacity and surplus strength
                double surplus = Math.Max(0, s.FoodStores);
                double surplusPerCap = surplus / Math.Max(1.0, s.Population);
                double logisticLimiter = Math.Max(0.0, (K - s.Population) / Math.Max(K, 1.0));
                int gain = (int)Math.Floor(Math.Min(
                    surplusPerCap * 0.25 * s.Population,     // feed-driven growth
                    logisticLimiter * 0.05 * s.Population     // capacity-limited growth
                ));
                if (gain > 0)
                {
                    s.Population += gain;
                    // consume part of stores when growing
                    s.FoodStores = Math.Max(0, s.FoodStores - gain * 0.5);
                    note = $"{s.Name}: growth (+{gain} pop)";
                }
            }

            // Legitimacy nudge from prosperity hardship
            s.Legitimacy = Math.Clamp(s.Legitimacy + 0.0015 * (s.Local.SO - 5) + 0.0007 * (s.Local.X - 5) + (s.FoodStores > 0 ? 0.0005 : -0.0010), 0.0, 1.0);
            return note ?? string.Empty;
        }
    }
}