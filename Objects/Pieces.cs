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
            { 'Q', Queen | White },
            { '1', 1 },
            { '2', 2 },
            { '3', 3 },
            { '4', 4 },
            { '5', 5 },
            { '6', 6 },
            { '7', 7 },
            { '8', 8 },
            { '/', 0 }
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
        public static int GetOppositeColor(int color)
        {
            return color == White ? Black : White;
        }
        public static char GetPieceChar(int piece)
        {
            foreach (KeyValuePair<char, int> pair in PieceNotation)
            {
                if (pair.Value == piece)
                    return pair.Key;
            }
            return ' ';
        }
        public static Piece[,] Parse_FEN(string FEN_string)
        {
            Piece[,] pieces = new Piece[8, 8];
            bool has_moved = false;

            int row = 0;
            int col = 0;

            foreach (char piece in FEN_string)
            {
                if (!PieceNotation.ContainsKey(piece))
                    continue;

                if (FEN_string != DefaultPosition)
                    has_moved = true;

                int current_piece = PieceNotation[piece];

                if (current_piece == 0)
                {
                    row++;
                    col = 0;
                    continue;
                }

                if (current_piece >= 1 && current_piece <= 8)
                {
                    col += current_piece;
                }

                if (col < 8 && row < 8)
                {
                    pieces[row, col] = new Piece
                    {
                        value = current_piece,
                        hasMoved = has_moved,
                        isPinned = false
                    };
                    col++;
                }
            }
            return pieces;
        }
    }
}
