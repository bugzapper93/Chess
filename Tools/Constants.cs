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
        public static readonly Dictionary<string, string> Grandmasters = new Dictionary<string, string>
        {
            { "Magnus Carlsen",  "magnus-carlsen.json"}
        };

        public static readonly ulong DefaultWhitePawns      = 0x000000000000FF00;
        public static readonly ulong DefaultWhiteKnights    = 0x0000000000000042;
        public static readonly ulong DefaultWhiteBishops    = 0x0000000000000024;
        public static readonly ulong DefaultWhiteRooks      = 0x0000000000000081;
        public static readonly ulong DefaultWhiteQueens     = 0x0000000000000008;
        public static readonly ulong DefaultWhiteKing       = 0x0000000000000010;

        public static readonly ulong DefaultBlackPawns      = 0x00FF000000000000;
        public static readonly ulong DefaultBlackKnights    = 0x4200000000000000;
        public static readonly ulong DefaultBlackBishops    = 0x2400000000000000;
        public static readonly ulong DefaultBlackRooks      = 0x8100000000000000;
        public static readonly ulong DefaultBlackQueens     = 0x0800000000000000;
        public static readonly ulong DefaultBlackKing       = 0x1000000000000000;

        public static readonly int[] KingOffsets             = { 8, -8, 1, -1, 9, 7, -7, -9 };
        public static readonly int[] KnightOffsets           = { 17, 15, 10, 6, -17, -15, -10, -6 };
        public static readonly int[] BishopDirections        = { 9, -9, 7, -7 };
        public static readonly int[] RookDirections          = { 1, -1, 8, -8 };
        public static readonly int[] QueenDirections         = {9, -9, 7, -7, 1, -1, 8, -8 };

        public static readonly int[] WhitePawnTable = new int[64] {
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5, 10, 25, 25, 10,  5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5, -5,-10,  0,  0,-10, -5,  5,
            5, 10, 10,-20,-20, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0
        };

        public static readonly int[] WhiteKnightTable = new int[64] {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50,
        };

        public static readonly int[] WhiteBishopTable = new int[64] {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20,
        };

        public static readonly int[] WhiteRookTable = new int[64] {
             0,  0,  0,  0,  0,  0,  0,  0,
             5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
             0,  0,  0,  5,  5,  0,  0,  0
        };

        public static readonly int[] WhiteQueenTable = new int[64] {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
             -5,  0,  5,  5,  5,  5,  0, -5,
              0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
        };

        public static readonly int[] WhiteKingTable = new int[64] {
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
             20, 20,  0,  0,  0,  0, 20, 20,
             20, 30, 10,  0,  0, 10, 30, 20
        };

        public static readonly int[] BlackPawnTable = WhitePawnTable.Reverse().ToArray();
        public static readonly int[] BlackKnightTable = WhiteKnightTable.Reverse().ToArray();
        public static readonly int[] BlackBishopTable = WhiteBishopTable.Reverse().ToArray();
        public static readonly int[] BlackRookTable = WhiteRookTable.Reverse().ToArray();
        public static readonly int[] BlackQueenTable = WhiteQueenTable.Reverse().ToArray();
        public static readonly int[] BlackKingTable = WhiteKingTable.Reverse().ToArray();

        public static readonly int SquareSize = 50;
        public static readonly Brush Primary = Brushes.LightBlue;
        public static readonly Brush Secondary = Brushes.CadetBlue;

        public static readonly ulong[,] ZobristTable;

        static Constants()
        {
            ulong[,] table = new ulong[12, 64];
            Random random = new Random(42);
            for (int piece = 0; piece < 12; piece++)
            {
                for (int square = 0; square < 64; square++)
                {
                    table[piece, square] = (ulong)random.Next() << 32 | (ulong)random.Next();
                }
            }
            ZobristTable = table;
        }
    }
}
