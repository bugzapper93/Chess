using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess
{
    internal class Tools
    {
        public const int BoardSize = 8;
        public const int SquareSize = 50;
        public const int Margin = 10;
        public static Piece.PiecePanelPair[,] ParseFEN(string layout)
        {
            int row = 0, col = 0;
            Piece.PiecePanelPair[,] pieces = new Piece.PiecePanelPair[BoardSize, BoardSize];

            foreach (char piece in layout)
            {
                if (!Piece.PiecesNotation.ContainsKey(piece))
                    continue;

                int currentPiece = Piece.PiecesNotation[piece];

                if (currentPiece == 0)
                {
                    row++;
                    col = 0;
                    continue;
                }

                if (currentPiece >= 1 && currentPiece <= 8)
                {
                    col += currentPiece;
                    continue;
                }

                if (col < BoardSize && row < BoardSize)
                {
                    pieces[row, col] = AddPiece(currentPiece, row, col);
                    col++;
                }
            }
            return pieces;
        }
        private static Piece.PiecePanelPair AddPiece(int value, int row, int col)
        {
            if (row >= BoardSize || col >= BoardSize)
                return new Piece.PiecePanelPair();

            Panel piece = Piece.InitPiece(value, row, col);
            Piece.PiecePanelPair piecePanelPair = new Piece.PiecePanelPair { panel = piece, number = value };
            return piecePanelPair;
        }
    }
}
