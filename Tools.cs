using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess
{
    internal class Tools
    {
        public const int BoardSize = 8;
        public const int SquareSize = 80;
        public const int Margin = 10;
        public static Piece.PiecePanelPair[,] ParseFEN(string layout)
        {
            bool moved = false;
            if (layout != Piece.DefaultStartingPosition)
                moved = true;
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
                    pieces[row, col] = AddPiece(currentPiece, row, col, moved);
                    col++;
                }
            }
            return pieces;
        }
        private static Piece.PiecePanelPair AddPiece(int value, int row, int col, bool moved)
        {
            if (row >= BoardSize || col >= BoardSize)
                return new Piece.PiecePanelPair();

            Panel piece = Piece.InitPiece(value, row, col);
            Piece.PiecePanelPair piecePanelPair = new Piece.PiecePanelPair { panel = piece, number = value, hasMoved = moved };
            return piecePanelPair;
        }
        public static string GetMoveNotation(int piece, int startRow, int startCol, int endRow, int endCol, bool capture)
        {
            int pieceType = piece & 7;
            string start = ConvertToNotation(startRow, startCol);
            string end = ConvertToNotation(endRow, endCol);

            string pieceChar = "";

            if (pieceType == Piece.Pawn) pieceChar = "";
            if (pieceType == Piece.Knight) pieceChar = "N";
            if (pieceType == Piece.Bishop) pieceChar = "B";
            if (pieceType == Piece.Rook) pieceChar = "R";
            if (pieceType == Piece.Queen) pieceChar = "Q";
            if (pieceType == Piece.King) pieceChar = "K";
        
            // Roszada
            if ((pieceType == Piece.King) && Math.Abs(startCol - endCol) == 2)
            {
                return (endCol == 6) ? "O-O" : "O-O-O";
            }
            if (pieceType == Piece.Pawn && capture)
            {
                return $"{start[0]}x{end}";
            }
            return capture ? $"{pieceChar}x{end}" : $"{pieceChar}{end}";
        }
        private static string ConvertToNotation(int row, int col)
        {
            char file = (char)('a' + col);
            int rank = 8 - row;
            return $"{file}{rank}";
        }
    }
}
