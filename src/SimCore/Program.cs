using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Evolvera.Sim.Models;
using Evolvera.Sim.Systems;
using Evolvera.Sim.Save;
using Evolvera.Sim.World;
using Evolvera.Sim.WorldGen;
using Evolvera.Sim.Units;

public static class Program
{
    public static void Main(string[] args)
    {
        // ===== Seed & world generation =====
        var rng = new Random(12345);
        var map = MapGen.Generate(w: 32, h: 18, seed: 1337);

        // ===== Create civ and pick a good starting tile (avoid Mountain/Desert) =====
        var civ = new Civilization { Name = "Evolvera" };
        int sx = 2, sy = 2;
        for (int y = 0; y < map.Height; y++)
            for (int x = 0; x < map.Width; x++)
            {
                var tt = map.Get(x, y).Terrain;
                if (tt != TerrainType.Mountain && tt != TerrainType.Desert)
                { sx = x; sy = y; y = map.Height; break; }
            }

        // Capital: Riverford on the chosen tile center
        civ.Settlements.Add(new Settlement
        {
            Name = "Riverford",
            Kind = SettlementKind.Town,
            Position = new Vec2(sx + 0.5, sy + 0.5),
            Population = 180,
            Local = Domains.Start
        });

        // Frontier hamlet a few tiles away
        var hx = Math.Min(map.Width - 1, sx + 6);
        var hy = Math.Min(map.Height - 1, sy + 2);
        civ.Settlements.Add(new Settlement
        {
            Name = "Stone Run",
            Kind = SettlementKind.Hamlet,
            Position = new Vec2(hx + 0.5, hy + 0.5),
            Population = 60,
            Local = Domains.Start
        });

        // Units: one Scout & one Worker at capital
        var units = new List<Unit>
        {
            new Unit { Type = UnitType.Scout,  X = sx, Y = sy, MovesPerTurn = 5 },
            new Unit { Type = UnitType.Worker, X = sx, Y = sy, MovesPerTurn = 3 },
        };

        var influence = new InfluenceMap();

        // ===== Turn loop =====
        int turns = 20;
        for (int t = 1; t <= turns; t++)
        {
            // Reset unit movement and roll a terrain-biased event
            foreach (var u in units) Movement.ResetMoves(u);
            var evt = EnvEvents.Tick(rng, map, civ);
            if (evt != null) Console.WriteLine($"      > {evt}");

            // === Per-settlement updates ===
            foreach (var s in civ.Settlements)
            {
                // 1) Two local actions → XP
                var xp = Actions.RandomActionXp(rng);
                // 2) Apply domain growth (diminishing returns rule)
                s.Local = DomainMath.ApplyXp(s.Local, xp);
                // 3) Economy & population step
                var econNote = PopulationSystem.StepEconomyAndPopulation(s);
                if (!string.IsNullOrEmpty(econNote)) Console.WriteLine($"      > {econNote}");
            }

            // === Tiny CLI (player acts) BEFORE roll-up ===
            while (true)
            {
                var unitSpots = new HashSet<(int, int)>(units.Select(u => (u.X, u.Y)));
                var settlementSpots = new HashSet<(int, int)>(civ.Settlements.Select(s => ((int)Math.Round(s.Position.X - 0.5), (int)Math.Round(s.Position.Y - 0.5))));
                Console.WriteLine(map.DrawAscii(unitSpots, settlementSpots));
                Console.Write("cmd [help|end|units|map|mv <id> <dir>|info <x> <y>|harvest|found <name>]: ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts[0].ToLowerInvariant();

                if (cmd == "end") break; // end player phase for this turn
                if (cmd == "help") { Console.WriteLine("mv dirs: N,S,E,W,NE,NW,SE,SW"); continue; }
                if (cmd == "units") { foreach (var u in units) Console.WriteLine($"  {u.Id}: {u.Name} @({u.X},{u.Y}) moves={u.Moves:F1}"); continue; }
                if (cmd == "map") { continue; } // already printed
                if (cmd == "info" && parts.Length >= 3 && int.TryParse(parts[1], out int ix) && int.TryParse(parts[2], out int iy))
                {
                    if (map.InBounds(ix, iy))
                    {
                        var tt = map.Get(ix, iy);
                        Console.WriteLine($"tile ({ix},{iy}) {tt.Terrain} river={(tt.HasRiver ? "yes" : "no")} res={(tt.Resource?.Type.ToString() ?? "none")} r*={(tt.Resource?.Richness.ToString() ?? "-")}");
                    }
                    continue;
                }
                if (cmd == "mv" && parts.Length >= 3 && int.TryParse(parts[1], out int uid))
                {
                    var u = units.FirstOrDefault(z => z.Id == uid); if (u == null) { Console.WriteLine("no such unit"); continue; }
                    var dir = parts[2].ToUpperInvariant();
                    int dx = 0, dy = 0;
                    (dx, dy) = dir switch { "N" => (0, -1), "S" => (0, 1), "E" => (1, 0), "W" => (-1, 0), "NE" => (1, -1), "NW" => (-1, -1), "SE" => (1, 1), "SW" => (-1, 1), _ => (0, 0) };
                    if (dx == 0 && dy == 0) { Console.WriteLine("bad dir"); continue; }
                    var ok = Movement.TryMove(map, u, dx, dy);
                    Console.WriteLine(ok ? $"moved to ({u.X},{u.Y}), moves={u.Moves:F1}" : "blocked");
                    continue;
                }
                if (cmd == "harvest")
                {
                    var worker = units.FirstOrDefault(z => z.Type == UnitType.Worker && z.Moves > 0);
                    if (worker == null) { Console.WriteLine("no ready worker"); continue; }
                    Console.WriteLine(ResourceUse.Harvest(map, worker, civ));
                    worker.Moves = 0; // consume worker's turn
                    continue;
                }
                if (cmd == "found" && parts.Length >= 2)
                {
                    var worker = units.FirstOrDefault(z => z.Type == UnitType.Worker && z.Moves > 0);
                    if (worker == null) { Console.WriteLine("no ready worker"); continue; }
                    var name = string.Join(' ', parts.Skip(1));
                    var pos = new Vec2(worker.X + 0.5, worker.Y + 0.5);
                    bool far = civ.Settlements.All(s => Math.Sqrt(Math.Pow(pos.X - s.Position.X, 2) + Math.Pow(pos.Y - s.Position.Y, 2)) >= 4);
                    if (!far) { Console.WriteLine("too close to existing settlement"); continue; }
                    var parent = civ.Settlements[0]; // capital as sponsor for now
                    var child = Expansion.FoundSettlement(civ, parent, pos, name, settlerPop: Math.Max(15, parent.Population / 10));
                    Console.WriteLine($"founded {child.Name} at ({worker.X},{worker.Y})");
                    worker.Moves = 0;
                    continue;
                }
                Console.WriteLine("unknown command");
            }

            // === AI expansion sweep ===
            int foundedThisTurn = 0; // throttle so AI founds at most one site per turn
            foreach (var s in civ.Settlements.ToList())
            {
                if (Expansion.ShouldExpand(s) && civ.Settlements.Count < 6 && foundedThisTurn < 1)
                {
                    var step = Expansion.ProposeNewSite(s, rng);
                    var target = s.Position + step;
                    string name = civ.Settlements.Count switch { < 3 => s.Name == "Riverford" ? "Reedford" : s.Name == "Stone Run" ? "Ore Run" : "Outpost 1", _ => $"Outpost {civ.Settlements.Count + 1}" };
                    var child = Expansion.FoundSettlement(civ, s, target, name, settlerPop: Math.Max(20, s.Population / 8));
                    Console.WriteLine($"      > Expansion: {s.Name} founded {child.Name} at ({child.Position.X:F1},{child.Position.Y:F1}) with pop {child.Population}");
                    foundedThisTurn++;
                }

                if (s.Kind == SettlementKind.Camp && s.FoodStores < 10)
                {
                    Expansion.MigrateCamp(s, new Vec2(1, 0));
                    Console.WriteLine($"      > {s.Name} migrates to ({s.Position.X:F1},{s.Position.Y:F1})");
                }
            }

            // === Roll-up to civilization & print summary ===
            civ.RecalculateRollup();
            Console.WriteLine($"T{t:00} – Civ Domains S={civ.Domains.S:F1} P={civ.Domains.P:F1} W={civ.Domains.W:F1} E={civ.Domains.E:F1} SO={civ.Domains.SO:F1} X={civ.Domains.X:F1} | NI={civ.NationalIdentity:F1}");
            foreach (var s in civ.Settlements)
            {
                Console.WriteLine($"   · {s.Name} ({s.Kind}) pop={s.Population} food={s.FoodStores:F1} leg={s.Legitimacy:F2} | S={s.Local.S:F1} P={s.Local.P:F1} W={s.Local.W:F1} E={s.Local.E:F1} SO={s.Local.SO:F1} X={s.Local.X:F1}");
            }

            if (t % 5 == 0)
            {
                var probes = new[] { new Vec2(1, 0), new Vec2(4, 1), new Vec2(8, 3) };
                foreach (var p in probes)
                {
                    var owner = influence.ControllerAt(p, civ.Settlements);
                    Console.WriteLine($"      control@({p.X:F1},{p.Y:F1}) → {(owner?.Name ?? "None")} ");
                }
            }
        }

        // ===== Save game =====
        Directory.CreateDirectory("saves");
        SaveGame.Save(Path.Combine("saves", "phase3_demo.json"), new SaveGame { Civ = civ });
        Console.WriteLine("Saved to saves/phase3_demo.json");
    }
}
