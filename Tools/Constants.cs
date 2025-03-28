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
    public static class Constants
    {
        public static ulong DefaultWhitePawns      = 0x000000000000FF00;
        public static ulong DefaultWhiteKnights    = 0x0000000000000042;
        public static ulong DefaultWhiteBishops    = 0x0000000000000024;
        public static ulong DefaultWhiteRooks      = 0x0000000000000081;
        public static ulong DefaultWhiteQueens     = 0x0000000000000008;
        public static ulong DefaultWhiteKing       = 0x0000000000000010;

        public static ulong DefaultBlackPawns      = 0x00FF000000000000;
        public static ulong DefaultBlackKnights    = 0x4200000000000000;
        public static ulong DefaultBlackBishops    = 0x2400000000000000;
        public static ulong DefaultBlackRooks      = 0x8100000000000000;
        public static ulong DefaultBlackQueens     = 0x0800000000000000;
        public static ulong DefaultBlackKing       = 0x1000000000000000;

        public static int[] KingOffsets             = { 8, -8, 1, -1, 9, 7, -7, -9 };
        public static int[] KnightOffsets           = { 17, 15, 10, 6, -17, -15, -10, -6 };
        public static int[] BishopDirections        = { 9, -9, 7, -7 };
        public static int[] RookDirections          = { 1, -1, 8, -8 };
        public static int[] QueenDirections         = {9, -9, 7, -7, 1, -1, 8, -8 };

        public static int SquareSize = 50;
        public static Brush Primary = Brushes.LightBlue;
        public static Brush Secondary = Brushes.CadetBlue;
    }
}
