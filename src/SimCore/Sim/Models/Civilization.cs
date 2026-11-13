// =============================================================
// File: /Sim/Models/Civilization.cs
// =============================================================
using System.Collections.Generic;
using System.Linq;

namespace Evolvera.Sim.Models
{
    public sealed class Civilization
    {
        public string Name { get; set; } = "Evolvera";
        public Domains Domains { get; set; } = Domains.Start;
        public double NationalIdentity { get; set; } = 5.0; // NI 0..10 conceptual
        public List<Settlement> Settlements { get; } = new();

        public void RecalculateRollup()
        {
            if (Settlements.Count == 0) return;
            double totalPop = Settlements.Sum(s => (double)s.Population);
            if (totalPop <= 0) totalPop = 1;

            // Population-weighted mean of local domains
            var weighted = Settlements.Select(s => (s.Local, w: s.Population / totalPop));
            var sum = new Domains(0, 0, 0, 0, 0, 0);
            foreach (var t in weighted)
            {
                sum = sum + t.Local.Scale(t.w);
            }
            Domains = sum;

            // NI formula (from design) with local imbalance penalty
            NationalIdentity = NiFrom(Domains, Settlements);
        }

        public static double NiFrom(Domains d, List<Settlement> settlements)
        {
            // Base NI (design):
            // NI = Clamp(0,10, 5 + 0.25*(SO-10) + 0.20*(X-10) + 0.10*(S-10) - 0.07*max(0, W-SO))
            double baseNi = 5.0
                + 0.25 * (d.SO - 10.0)
                + 0.20 * (d.X - 10.0)
                + 0.10 * (d.S - 10.0)
                - 0.07 * System.Math.Max(0.0, d.W - d.SO);

            // Hotspot penalty: sum of positive (W - SO) weights across settlements
            double hotspot = 0.0;
            foreach (var s in settlements)
            {
                var localPenalty = System.Math.Max(0.0, s.Local.W - s.Local.SO);
                hotspot += localPenalty * (s.Population / System.Math.Max(1.0, settlements.Sum(x => (double)x.Population)));
            }
            baseNi -= 0.05 * hotspot;

            // Clamp 0..10
            if (baseNi < 0) baseNi = 0;
            if (baseNi > 10) baseNi = 10;
            return baseNi;
        }
    }
}
