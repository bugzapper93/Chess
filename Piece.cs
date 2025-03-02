using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess
{
    internal class Piece
    {
        public static Dictionary<char, int> PiecesNotation = new Dictionary<char, int>
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

        public const int King = 1;
        public const int Pawn = 2;
        public const int Knight = 3;
        public const int Bishop = 4;
        public const int Rook = 5;
        public const int Queen = 6;

        public const int White = 8;
        public const int Black = 16;

        public const string DefaultStartingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

        public static Panel InitPiece(int value, int row, int col, int squareSize = Tools.SquareSize, int margin = Tools.Margin)
        {
            Panel piece = new Panel
            {
                Size = new Size(squareSize - margin, squareSize - margin),
                Location = new Point(col * squareSize + margin / 2, row * squareSize + margin / 2),
                BackColor = (value & Piece.White) != 0 ? Color.White : Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };
            return piece;
        }
        public struct PiecePanelPair
        {
            public int number;
            public Panel panel;
            public bool hasMoved;
        }
    }
}
