// =============================================================
// File: /Sim/Save/SaveGame.cs
// =============================================================
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Evolvera.Sim.Models;

namespace Evolvera.Sim.Save
{
    public sealed class SaveGame
    {
        public string Version { get; set; } = "0.1-phase1";
        public Civilization Civ { get; set; } = new();

        public static void Save(string path, SaveGame g)
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(path, JsonSerializer.Serialize(g, opts));
        }

        public static SaveGame Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<SaveGame>(json) ?? new SaveGame();
        }
    }
}