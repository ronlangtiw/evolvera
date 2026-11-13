using System;

namespace Evolvera.Sim.Models
{
    public enum SettlementKind { Camp, Hamlet, Town, City, SacredSite, Fort }

    // small hook for future logistics
    public sealed class Stockpiles
    {
        public double Food { get; set; } = 0;
    }

    public sealed class Settlement
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = "New Settlement";
        public SettlementKind Kind { get; set; } = SettlementKind.Hamlet;
        public bool IsMobile { get; set; } = false;   // true for nomadic camp

        public Vec2 Position { get; set; } = Vec2.Zero;

        // Local domains follow same rules as civilization
        public Domains Local { get; set; } = Domains.Start;

        public int Population { get; set; } = 30;
        public double FoodStores { get; set; } = 30;  // abstract units
        public double Legitimacy { get; set; } = 0.5; // 0..1

        public Stockpiles StoresDetail { get; } = new();

        public double BaseInfluence => Math.Max(1, Population * 0.05) * (1.0 + 0.1 * (Local.SO - 5) + 0.05 * (Local.X - 5));
        public double InfluenceScale => Kind switch
        {
            SettlementKind.Camp => 2.5,
            SettlementKind.Hamlet => 3.0,
            SettlementKind.Town => 4.0,
            SettlementKind.City => 5.0,
            SettlementKind.Fort => 3.5,
            SettlementKind.SacredSite => 3.5,
            _ => 3.0
        };

        // Phase 1 estimates (kept)
        public double FoodProductionEstimate()
            => 0.1 * Population * (1.0 + 0.08 * (Local.S - 5) + 0.05 * (Local.P - 5));
        public double FoodConsumption() => 0.09 * Population;

        // Phase 2: carrying capacity & expansion heuristics
        public double CarryCapacity()
        {
            // Base capacity scaled by Survival/Production; towns/cities get a bonus.
            double baseCap = 40 + 18 * (Local.S - 4) + 10 * (Local.P - 4);
            double kindBonus = Kind switch
            {
                SettlementKind.Town => 30,
                SettlementKind.City => 60,
                SettlementKind.Fort => 10,
                SettlementKind.SacredSite => 10,
                _ => 0
            };
            return Math.Max(30, baseCap + kindBonus);
        }

        public double SurplusRatio() =>
            Population <= 0 ? 0 : (FoodStores / Math.Max(1.0, FoodConsumption()));
    }
}
