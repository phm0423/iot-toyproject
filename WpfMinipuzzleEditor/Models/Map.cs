using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMinipuzzleEditor.Models
{
    public class Map
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Tile[,] Tiles { get; set; }

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    Tiles[x, y] = new Tile(x, y, TileType.Emtpy);
            }
        }
    }
}
