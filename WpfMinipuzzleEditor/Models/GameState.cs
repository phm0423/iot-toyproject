using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMinipuzzleEditor.Models
{
    public class GameState
    {
        public int PlayerX { get; set; }
        public int PlayerY { get; set; }
        public bool isClear { get; set; }
    }
}
