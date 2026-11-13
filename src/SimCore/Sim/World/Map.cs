// =============================================================
// File: /Sim/World/Map.cs
// =============================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Evolvera.Sim.World
{
    public sealed class Tile
    {
        public int X { get; init; }
        public int Y { get; init; }
        public TerrainType Terrain { get; set; }
        public bool HasRiver { get; set; }
        public ResourceDeposit? Resource { get; set; }

        public (int, int) Pos => (X, Y);
    }

    public sealed class WorldMap
    {
        public int Width { get; }
        public int Height { get; }
        public Tile[,] Tiles { get; }
        public WorldMap(int w, int h)
        {
            Width = w; Height = h; Tiles = new Tile[w, h];
            for (int x = 0; x < w; x++) for (int y = 0; y < h; y++) Tiles[x, y] = new Tile { X = x, Y = y };
        }
        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        public Tile Get(int x, int y) => Tiles[x, y];
        public IEnumerable<Tile> Neighbors8(int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++) for (int dy = -1; dy <= 1; dy++) if (!(dx == 0 && dy == 0))
                    { int nx = x + dx, ny = y + dy; if (InBounds(nx, ny)) yield return Tiles[nx, ny]; }
        }

        public string DrawAscii(HashSet<(int, int)> unitSpots, HashSet<(int, int)> settlementSpots)
        {
            var sb = new StringBuilder();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var t = Tiles[x, y];
                    char c = t.Terrain switch
                    {
                        TerrainType.Plains => '.',
                        TerrainType.Forest => '♣',
                        TerrainType.Hills => '^',
                        TerrainType.Mountain => 'M',
                        TerrainType.Wetland => '~',
                        TerrainType.Desert => '*',
                        _ => '?'
                    };
                    if (t.HasRiver) c = '≈';
                    if (settlementSpots.Contains((x, y))) c = 'S';
                    if (unitSpots.Contains((x, y))) c = 'U';
                    sb.Append(c);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}