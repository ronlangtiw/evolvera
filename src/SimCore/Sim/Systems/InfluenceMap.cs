// =============================================================
// File: /Sim/Systems/InfluenceMap.cs
// =============================================================
using System;
using System.Collections.Generic;
using Evolvera.Sim.Models;

namespace Evolvera.Sim.Systems
{
    public sealed class InfluenceMap
    {
        public double InfluenceAt(Vec2 p, Settlement s)
        {
            // Radial basis falloff: I = base * exp( -d / scale )
            double d = p.Distance(s.Position);
            double baseI = s.BaseInfluence;
            double scale = Math.Max(1e-3, s.InfluenceScale);
            return baseI * Math.Exp(-d / scale);
        }

        public double CompositeInfluenceAt(Vec2 p, IReadOnlyList<Settlement> settlements)
        {
            double sum = 0;
            foreach (var s in settlements)
                sum += InfluenceAt(p, s);
            return sum;
        }

        public Settlement? ControllerAt(Vec2 p, IReadOnlyList<Settlement> settlements)
        {
            Settlement? best = null;
            double bestI = double.NegativeInfinity;
            foreach (var s in settlements)
            {
                double i = InfluenceAt(p, s);
                if (i > bestI) { bestI = i; best = s; }
            }
            return best;
        }
    }
}
