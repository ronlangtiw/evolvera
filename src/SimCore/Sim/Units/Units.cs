// =============================================================
// File: /Sim/Units/Units.cs
// =============================================================
using System;
using Evolvera.Sim.Models;

namespace Evolvera.Sim.Units
{
    public enum UnitType { Scout, Worker, Militia, Caravan }

    public sealed class Unit
    {
        private static int _seq = 1;
        public int Id { get; } = _seq++;
        public UnitType Type { get; init; }
        public int X { get; set; }
        public int Y { get; set; }
        public int MovesPerTurn { get; init; } = 4;
        public double Moves { get; set; } = 0;
        public string Name => $"{Type}#{Id}";
    }
}
