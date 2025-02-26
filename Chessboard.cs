using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess
{
    internal class Chessboard
    {
        private int BoardSize;
        private int SquareSize;
        public Panel[,] squares;
        public Piece.PiecePanelPair[,] pieces;
        public Panel selectedPiece;
        public Point originalLocation;
        public int selectedRow;
        public int selectedCol;
        public Chessboard(int boardSize, int squareSize)
        {
            BoardSize = boardSize;
            SquareSize = squareSize;
            squares = new Panel[BoardSize, BoardSize];
            pieces = new Piece.PiecePanelPair[BoardSize, BoardSize];

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    Panel square = new Panel
                    {
                        Size = new Size(SquareSize, SquareSize),
                        Location = new Point(col * SquareSize, row * SquareSize),
                        BackColor = (row + col) % 2 == 0 ? Color.LightYellow : Color.Brown
                    };
                    squares[row, col] = square;
                }
            }
        }
        public void FillBoard(string layout)
        {
            pieces = Tools.ParseFEN(layout);
        }
    }
}
