using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Evolvera.SimCore
{
    public enum Domain { Survival, Production, Warfare, Exploration, Social, Expression }

    public class Civ
    {
        public string Name { get; set; } = "Demo Tribe";
        public Dictionary<Domain, double> Domains { get; } = new()
        {
            {Domain.Survival, 5}, {Domain.Production, 5}, {Domain.Warfare, 5},
            {Domain.Exploration, 5}, {Domain.Social, 5}, {Domain.Expression, 5}
        };
        public HashSet<string> Capabilities { get; } = new(); // milestone flags
        public double NI { get; set; } = 5.0;                 // national identity 0..10
        public List<AgeTag> AgeHistory { get; } = new();
    }

    public class EventDef
    {
        public string Name = "";
        // flat deltas (applied after XP -> domain increment)
        public Dictionary<Domain, double> DomainDeltas = new();
        // multipliers to this turn’s XP (e.g., drought reduces survival gain)
        public Dictionary<Domain, double> XPMultipliers = new(); // 1.0 = no change
    }

    // ---- Procedural Age system (data + generators) ----
    public record AgeTag(
        string Id,
        string Name,
        Dictionary<string, double> Causes,
        Dictionary<string, double> Effects,
        int StartedTurn,
        int? EndsTurn
    );

    static class AgeNamer
    {
        static readonly List<string> Templates = new()
    {
        "The {Adj} {Noun}",
        "{Noun} of {Concept}",
        "Era of {Concept}",
        "The {Adj} {Concept}",
        "{Biome} {Noun}"
    };

        // Use List<string>, NOT string[]
        static readonly Dictionary<string, List<string>> Adjectives = new()
        {
            ["survival"] = new() { "Fertile", "Resilient", "Provisioned", "Green" },
            ["production"] = new() { "Forged", "Masoned", "Industrious", "Artisan" },
            ["warfare"] = new() { "Martial", "Bronzeclad", "Hardened", "Bannered" },
            ["exploration"] = new() { "Wandering", "Seafaring", "Expansive", "Astral" },
            ["social"] = new() { "Ordered", "Civic", "Codified", "Cohesive" },
            ["expression"] = new() { "Radiant", "Golden", "Sacred", "Harmonic" },
            ["crisis"] = new() { "Fractured", "Tempest", "Ashen", "Trial" },
            ["unity"] = new() { "United", "Concord", "Commonweal", "Harmonic" }
        };

        static readonly Dictionary<string, List<string>> Nouns = new()
        {
            ["survival"] = new() { "Harvest", "Granaries", "Rivers", "Plenty" },
            ["production"] = new() { "Stoneworks", "Forges", "Workshops", "Engines" },
            ["warfare"] = new() { "Standards", "Spears", "Legions", "Strongholds" },
            ["exploration"] = new() { "Ways", "Currents", "Horizons", "Maps" },
            ["social"] = new() { "Edicts", "Assemblies", "Laws", "Provinces" },
            ["expression"] = new() { "Rites", "Muse", "Theater", "Constellations" },
            ["crisis"] = new() { "Trials", "Storms", "Woe", "Upheaval" },
            ["unity"] = new() { "Unity", "Concord", "Accord", "Commons" }
        };

        public static string BuildName(Random rng, List<(string signal, double weight)> signals, string? biomeHint = null)
        {
            string t = Templates[rng.Next(Templates.Count)];
            signals.Sort((a, b) => b.weight.CompareTo(a.weight));
            string head = signals.Count > 0 ? signals[0].signal : "social";
            string tail = signals.Count > 1 ? signals[1].signal : head;

            string adj = Pick(Adjectives, head, tail, rng);
            string noun = Pick(Nouns, head, tail, rng);
            string concept = Pick(Nouns, head, tail, rng);

            if (!string.IsNullOrEmpty(biomeHint) && rng.NextDouble() < 0.5)
                noun = biomeHint + " " + noun;

            return t.Replace("{Adj}", adj).Replace("{Noun}", noun)
                    .Replace("{Concept}", concept).Replace("{Biome}", biomeHint ?? "Wilds");
        }

        static string Pick(Dictionary<string, List<string>> dict, string a, string b, Random rng)
        {
            // Build a candidate bag from up to 2 signals
            var bag = new List<string>();
            if (dict.TryGetValue(a, out var aa)) bag.AddRange(aa);
            if (dict.TryGetValue(b, out var bb)) bag.AddRange(bb);
            if (bag.Count == 0 && dict.TryGetValue("social", out var ss)) bag.AddRange(ss);
            return bag[rng.Next(bag.Count)];
        }
    }

    static class AgeEffects
    {
        public static Dictionary<string, double> Build(List<(string signal, double weight)> sig)
        {
            var e = new Dictionary<string, double>();
            double Get(string k) => e.TryGetValue(k, out var v) ? v : 0;
            foreach (var sw in sig)
            {
                var s = sw.signal; var w = sw.weight;
                if (w <= 0) continue;
                switch (s)
                {
                    case "survival": e["food_yield"] = Get("food_yield") + 0.03 * w; break;
                    case "production": e["build_speed"] = Get("build_speed") + 0.03 * w; break;
                    case "warfare": e["combat"] = Get("combat") + 0.05 * w; break;
                    case "exploration": e["trade"] = Get("trade") + 0.05 * w; break;
                    case "social": e["stability"] = Get("stability") + 0.05 * w; break;
                    case "expression": e["diplomacy"] = Get("diplomacy") + 0.05 * w; break;
                    case "crisis": e["resilience"] = Get("resilience") + 0.05 * w; break;
                    case "unity": e["ni_cap"] = Math.Max(Get("ni_cap"), 10); break;
                }
            }
            return e;
        }
    }



    class Program
    {
        static readonly EventDef[] Events = new[]
        {
            new EventDef{ Name="Drought",
                DomainDeltas = new() {{Domain.Survival,-0.45},{Domain.Social,-0.15}},
                XPMultipliers = new() {{Domain.Survival,0.9}}
            },
            new EventDef{ Name="Flood",
                DomainDeltas = new() {{Domain.Survival,+0.3},{Domain.Production,-0.15}},
                XPMultipliers = new() {{Domain.Production,0.93}}
            },
            new EventDef{ Name="Plague",
                DomainDeltas = new() {{Domain.Social,-0.45},{Domain.Expression,-0.15}},
                XPMultipliers = new() {{Domain.Exploration,0.9}}
            },
        };

        static StreamWriter StartCsv(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var w = new StreamWriter(path, false);
            w.WriteLine("turn,action1,action2,event,survival,production,warfare,exploration,social,expression,ni");
            return w;
        }

        static void WriteCsv(StreamWriter w, int turn, List<string> actions, string? evName, Civ c)
        {
            string a1 = actions.Count > 0 ? actions[0] : "";
            string a2 = actions.Count > 1 ? actions[1] : "";
            w.WriteLine($"{turn},{a1},{a2},{evName ?? ""},{c.Domains[Domain.Survival]:0.000},{c.Domains[Domain.Production]:0.000},{c.Domains[Domain.Warfare]:0.000},{c.Domains[Domain.Exploration]:0.000},{c.Domains[Domain.Social]:0.000},{c.Domains[Domain.Expression]:0.000},{c.NI:0.000}");
        }

        static void CheckAgeTriggers(Civ civ, int turn, Random rng)
        {
            foreach (Domain d in Enum.GetValues(typeof(Domain)))
            {
                int majorBuckets = (int)Math.Floor(civ.Domains[d] / 15.0);
                string gateKey = $"age_gate_{d}_{majorBuckets}";
                if (majorBuckets >= 1 && !civ.Capabilities.Contains(gateKey))
                {
                    civ.Capabilities.Add(gateKey);

                    // signals: domain strength over 10 normalized 0..1
                    var signals = new List<(string signal, double weight)>();
                    foreach (Domain dx in Enum.GetValues(typeof(Domain)))
                    {
                        double w = Math.Max(0, (civ.Domains[dx] - 10) / 10.0);
                        if (w > 0) signals.Add((dx.ToString().ToLower(), w));
                    }
                    if (civ.NI >= 9.0) signals.Add(("unity", 1.0));

                    string biomeHint = "River"; // TODO: wire to real map later
                    string name = AgeNamer.BuildName(rng, signals, biomeHint);
                    var eff = AgeEffects.Build(signals);

                    var causes = new Dictionary<string, double>();
                    foreach (var s in signals) causes[s.signal] = s.weight;

                    var tag = new AgeTag(
                        Id: $"age_{civ.AgeHistory.Count + 1:D3}",
                        Name: name,
                        Causes: causes,
                        Effects: eff,
                        StartedTurn: turn,
                        EndsTurn: null
                    );
                    civ.AgeHistory.Add(tag);

                    // tiny immediate flavor (example): nudge SO/EXP a hair from effects
                    if (eff.TryGetValue("stability", out var stb)) civ.Domains[Domain.Social] += 0.1 * stb;
                    if (eff.TryGetValue("diplomacy", out var dip)) civ.Domains[Domain.Expression] += 0.1 * dip;

                    Console.WriteLine($"        [AGE] {tag.Name}  (from {d} ≥ {majorBuckets * 15})");
                }
            }
        }



        static void Main(string[] args)
        {
            int turns = 20;
            int seed = 12345;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--turns" && i + 1 < args.Length && int.TryParse(args[i + 1], out var t)) { turns = Math.Max(1, t); i++; }
                else if (args[i] == "--seed" && i + 1 < args.Length && int.TryParse(args[i + 1], out var s)) { seed = s; i++; }
            }

            var rng = new Random(seed);
            var civ = new Civ();
            var prev = new Dictionary<Domain, double>();
            foreach (Domain d in Enum.GetValues(typeof(Domain))) prev[d] = civ.Domains[d];
            using var csv = StartCsv(Path.Combine("saves", "run.csv"));

            Console.WriteLine($"== Evolvera Sim (turns={turns}, seed={seed}) ==");
            // ... keep rest of code, but replace `for (int turn = 1; turn <= 20; turn++)`
            // with:
            for (int turn = 1; turn <= turns; turn++)
            {
                // 1) choose two actions
                var actions = PickActions(rng);
                var actionsStr = string.Join(", ", actions);
                var xp = ApplyActions(actions);

                // 2) maybe draw an event (30% chance)
                EventDef? ev = null;
                if (rng.NextDouble() < 0.30)
                {
                    ev = Events[rng.Next(Events.Length)];
                    // scale XP by event multipliers
                    foreach (var kv in ev.XPMultipliers)
                        xp[kv.Key] *= kv.Value;
                }

                // 3) convert XP -> domain increments (diminishing returns)
                foreach (var kv in xp)
                {
                    var d = kv.Key; var val = kv.Value;
                    var current = civ.Domains[d];
                    var delta = val / (2.0 * (1.0 + current * 0.05)); // base_cost=2, alpha=0.05
                    civ.Domains[d] = current + delta;
                }

                // 4) apply flat event deltas after growth
                if (ev != null)
                {
                    foreach (var kv in ev.DomainDeltas)
                        civ.Domains[kv.Key] += kv.Value;
                }

                // 5) milestone checks (only when crossing)
                ApplyMilestones(civ, prev);
                CheckAgeTriggers(civ, turn, rng);
                foreach (Domain d in Enum.GetValues(typeof(Domain))) prev[d] = civ.Domains[d];
                // simple trigger: when any domain crosses a 15 boundary
                foreach (Domain d in Enum.GetValues(typeof(Domain)))
                {
                    if (civ.Domains[d] >= 15 && !civ.Capabilities.Contains($"age_{d}_15"))
                    {
                        civ.Capabilities.Add($"age_{d}_15");

                        // signals based on domains
                        var signals = new List<(string, double)> { (d.ToString().ToLower(), 1.0) };
                        if (civ.NI >= 9.0) signals.Add(("unity", 1.0));

                        var name = AgeNamer.BuildName(rng, signals, "River");
                        var eff = AgeEffects.Build(signals);

                        var tag = new AgeTag($"age_{civ.AgeHistory.Count + 1:D3}", name,
                            signals.ToDictionary(s => s.Item1, s => s.Item2), eff, turn, null);
                        civ.AgeHistory.Add(tag);

                        Console.WriteLine($"        [AGE] {tag.Name} (triggered by {d})");
                    }
                }


                // 6) NI preview (simple)
                civ.NI = Clamp(0, 10,
                    5.0
                    + 0.25 * (civ.Domains[Domain.Social] - 10)      // institutions
                    + 0.20 * (civ.Domains[Domain.Expression] - 10)  // culture
                    + 0.10 * (civ.Domains[Domain.Survival] - 10)    // prosperity
                    - 0.07 * Math.Max(0, civ.Domains[Domain.Warfare] - civ.Domains[Domain.Social]) // mil vs social
                );

                // 7) print
                var line = $"T{turn:00} [{actionsStr}]: " +
                           $"S={civ.Domains[Domain.Survival]:0.0} " +
                           $"P={civ.Domains[Domain.Production]:0.0} " +
                           $"W={civ.Domains[Domain.Warfare]:0.0} " +
                           $"E={civ.Domains[Domain.Exploration]:0.0} " +
                           $"SO={civ.Domains[Domain.Social]:0.0} " +
                           $"X={civ.Domains[Domain.Expression]:0.0} " +
                           $"| NI={civ.NI:0.0}";
                if (ev != null) line += $"  [Event: {ev.Name}]";
                WriteCsv(csv, turn, actions, ev?.Name, civ);
                Console.WriteLine(line);
            }

            // 8) save snapshot
            csv.Flush();
            Directory.CreateDirectory("saves");
            var json = JsonSerializer.Serialize(civ, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine("saves", "demo.json"), json);
            Console.WriteLine("Saved -> saves/demo.json");
        }

        // --- actions & XP weights ---
        static List<string> PickActions(Random rng)
        {
            string[] bag = { "hunt", "farm", "raid", "trade", "ritual", "explore", "infrastructure" };
            return new List<string> { bag[rng.Next(bag.Length)], bag[rng.Next(bag.Length)] };
        }

        static Dictionary<Domain, double> ApplyActions(List<string> actions)
        {
            var xp = new Dictionary<Domain, double>
            {
                {Domain.Survival,0},{Domain.Production,0},{Domain.Warfare,0},
                {Domain.Exploration,0},{Domain.Social,0},{Domain.Expression,0}
            };
            foreach (var a in actions)
            {
                switch (a)
                {
                    case "hunt": xp[Domain.Survival] += 1; xp[Domain.Warfare] += 0.5; break;
                    case "farm": xp[Domain.Survival] += 2; xp[Domain.Production] += 1; break;
                    case "raid": xp[Domain.Warfare] += 2; xp[Domain.Production] += 1; xp[Domain.Social] -= 0.5; break;
                    case "trade": xp[Domain.Exploration] += 1; xp[Domain.Social] += 1; xp[Domain.Production] += 0.5; break;
                    case "ritual": xp[Domain.Expression] += 1.5; xp[Domain.Social] += 0.5; break;
                    case "explore": xp[Domain.Exploration] += 1.5; xp[Domain.Survival] += 0.5; break;
                    case "infrastructure": xp[Domain.Production] += 1.5; xp[Domain.Social] += 0.5; break;
                }
            }
            return xp;
        }

        // --- milestones (crossing checks only) ---
        static void ApplyMilestones(Civ civ, Dictionary<Domain, double> prev)
        {
            foreach (Domain d in Enum.GetValues(typeof(Domain)))
            {
                var vNow = civ.Domains[d];
                var vPrev = prev[d];

                // major: crossing 15, 30, ...
                int majorNow = (int)Math.Floor(vNow / 15.0);
                int majorPrev = (int)Math.Floor(vPrev / 15.0);
                if (majorNow > majorPrev)
                {
                    var key = $"major_{d}_{majorNow}";
                    if (!civ.Capabilities.Contains(key))
                    {
                        civ.Capabilities.Add(key);
                        civ.Domains[d] += 0.5; // placeholder; swap for real capability later
                        Console.WriteLine($"    >> MAJOR breakthrough in {d} (>= {majorNow * 15})");
                        Console.WriteLine($"        [AGE] {d} age triggered"); // print only when major crossed
                    }
                }

                // minor: crossing 5,10,15 ...
                int minorNow = (int)Math.Floor(vNow / 5.0);
                int minorPrev = (int)Math.Floor(vPrev / 5.0);
                if (minorNow > minorPrev)
                {
                    var key = $"minor_{d}_{minorNow}";
                    if (!civ.Capabilities.Contains(key))
                    {
                        civ.Capabilities.Add(key);
                        civ.Domains[d] += 0.1; // tiny buff
                        Console.WriteLine($"    > minor perk unlocked in {d} (+5 x{minorNow})");
                    }
                }
            }
        }

        static double Clamp(double lo, double hi, double v) => Math.Max(lo, Math.Min(hi, v));
    }
}
