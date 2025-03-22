using Chess.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Chess.Tools
{
    class Constants
    {
        public static readonly int Margin = 10;
        public static readonly int Square_Size = 50;
        public static Brush Primary = Brushes.LightBlue;
        public static Brush Secondary = Brushes.DarkBlue;
        public static readonly Position[] Directions =
        {
            new Position { row = -1, column= 0 },
            new Position { row = 1, column =0 },
            new Position { row = 0, column = -1 },
            new Position { row = 0, column = 1 },
            new Position { row = -1, column = -1 },
            new Position { row = -1, column = 1 },
            new Position { row = 1, column = -1 },
            new Position { row = 1, column = 1 }
        };
        public static readonly Position[] KnightDirections =
        {
            new Position {row = 1, column = 2},
            new Position {row = 2, column = 1},
            new Position {row = -1, column = 2},
            new Position {row = -2, column = 1},
            new Position {row = 1, column = -2},
            new Position {row = 2, column = -1},
            new Position {row = -1, column = -2},
            new Position {row = -2, column = -1}
        };
        public static readonly char[] Files = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };
        public static readonly char[] Ranks = { '1', '2', '3', '4', '5', '6', '7', '8' };
    }
}
