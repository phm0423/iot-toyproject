using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace WpfMinipuzzleEditor.Models
{
    public class MapDto
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public TileType[][] Types { get; set; } = default!;
    }
}
