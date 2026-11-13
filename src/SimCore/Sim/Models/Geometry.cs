// =============================================================
// File: /Sim/Models/Geometry.cs
// =============================================================
using System;

namespace Evolvera.Sim.Models
{
    public readonly record struct Vec2(double X, double Y)
    {
        public static Vec2 Zero => new(0, 0);
        public double Distance(Vec2 other)
        {
            var dx = X - other.X; var dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator *(Vec2 a, double k) => new(a.X * k, a.Y * k);
    }
}
