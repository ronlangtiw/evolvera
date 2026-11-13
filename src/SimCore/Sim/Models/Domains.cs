// =============================================================
// File: /Sim/Models/Domains.cs
// =============================================================
using System;
using System.Text.Json.Serialization;

namespace Evolvera.Sim.Models
{
    public readonly record struct Domains(
        double S, // Survival
        double P, // Production
        double W, // Warfare
        double E, // Exploration
        double SO, // Social
        double X  // Expression
    )
    {
        public static Domains Start => new(5, 5, 5, 5, 5, 5);

        public (double S, double P, double W, double E, double SO, double X) Deconstruct()
            => (S, P, W, E, SO, X);

        public Domains Add(Domains b) => new(S + b.S, P + b.P, W + b.W, E + b.E, SO + b.SO, X + b.X);
        public Domains Scale(double k) => new(S * k, P * k, W * k, E * k, SO * k, X * k);

        public static Domains operator +(Domains a, Domains b) => a.Add(b);
        public static Domains operator *(Domains a, double k) => a.Scale(k);
    }

    public static class DomainMath
    {
        // Growth rule (from design): delta = xp / (2.0 * (1.0 + current * 0.05))
        public static double Delta(double current, double xp)
            => xp / (2.0 * (1.0 + current * 0.05));

        public static Domains ApplyXp(Domains current, Domains xp)
        {
            return new Domains(
                current.S + Delta(current.S, xp.S),
                current.P + Delta(current.P, xp.P),
                current.W + Delta(current.W, xp.W),
                current.E + Delta(current.E, xp.E),
                current.SO + Delta(current.SO, xp.SO),
                current.X + Delta(current.X, xp.X)
            );
        }

        public static Domains Zero => new(0, 0, 0, 0, 0, 0);
    }
}