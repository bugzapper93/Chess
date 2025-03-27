using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Objects
{
    public struct Piece
    {
        public int value;
        public bool hasMoved;
        public bool isPinned;
    }
    class Pieces
    {
        public static Dictionary<char, int> PieceNotation = new Dictionary<char, int>
        {
            { 'k', King | Black },
            { 'p', Pawn | Black },
            { 'n', Knight | Black },
            { 'b', Bishop | Black },
            { 'r', Rook | Black },
            { 'q', Queen | Black },
            { 'K', King | White },
            { 'P', Pawn | White },
            { 'N', Knight | White },
            { 'B', Bishop | White },
            { 'R', Rook | White },
            { 'Q', Queen | White }
        };
        public static Dictionary<char, string> ResourceNames = new Dictionary<char, string>
        {
            { 'k', "Chess.Resources.Images.king_black.png"},
            { 'p', "Chess.Resources.Images.pawn_black.png" },
            { 'n', "Chess.Resources.Images.knight_black.png" },
            { 'b', "Chess.Resources.Images.bishop_black.png" },
            { 'r', "Chess.Resources.Images.rook_black.png" },
            { 'q', "Chess.Resources.Images.queen_black.png" },
            { 'K', "Chess.Resources.Images.king_white.png" },
            { 'P', "Chess.Resources.Images.pawn_white.png" },
            { 'N', "Chess.Resources.Images.knight_white.png" },
            { 'B', "Chess.Resources.Images.bishop_white.png" },
            { 'R', "Chess.Resources.Images.rook_white.png" },
            { 'Q', "Chess.Resources.Images.queen_white.png" }
        };
        public const int King = 1;
        public const int Pawn = 2;
        public const int Knight = 3;
        public const int Bishop = 4;
        public const int Rook = 5;
        public const int Queen = 6;

        public const int White = 8;
        public const int Black = 16;

        public const string DefaultPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

    }
}