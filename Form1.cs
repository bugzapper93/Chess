using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess
{
    public partial class Form1 : Form
    {
        private Chessboard Board = new Chessboard(Tools.BoardSize, Tools.SquareSize);

        public Form1()
        {
            InitializeComponent();
            InitializeBoard();
            InitializePieces();
        }
        private void InitializeBoard()
        {
            foreach (Panel square in Board.squares)
            {
                Controls.Add(square);
            }
        }
        private void InitializePieces(string layout = Piece.DefaultStartingPosition)
        {
            Board.FillBoard(layout);
            foreach (Piece.PiecePanelPair piece in Board.pieces)
            {
                if (piece.panel != null)
                {
                    Controls.Add(piece.panel);
                    piece.panel.MouseDown += Piece_MouseDown;
                    piece.panel.MouseMove += Piece_MouseMove;
                    piece.panel.MouseUp += Piece_MouseUp;
                    piece.panel.BringToFront();
                }
            }
        }
        private void Piece_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Panel panel)
            {
                Board.selectedPiece = panel;
                Board.originalLocation = panel.Location;
                for (int row = 0; row < Tools.BoardSize; row++)
                {
                    for (int col = 0; col < Tools.BoardSize; col++)
                    {
                        if (Board.pieces[row, col].panel == Board.selectedPiece)
                        {
                            Board.selectedRow = row;
                            Board.selectedCol = col;
                            return;
                        }
                    }
                }
            }
            Board.selectedPiece = null;
            return;
        }
        private void Piece_MouseMove(object sender, MouseEventArgs e)
        {
            if (Board.selectedPiece != null && e.Button == MouseButtons.Left)
            {
                Board.selectedPiece.Location = new Point(Board.selectedPiece.Location.X + e.X - (Tools.SquareSize / 2), Board.selectedPiece.Location.Y + e.Y - (Tools.SquareSize / 2));
            }
        }
        private void Piece_MouseUp(object sender, MouseEventArgs e)
        {
            if (Board.selectedPiece != null)
            {
                int newCol = (int)Math.Round((float)Board.selectedPiece.Location.X / (float)Tools.SquareSize, 0, MidpointRounding.AwayFromZero);
                int newRow = (int)Math.Round((float)Board.selectedPiece.Location.Y / (float)Tools.SquareSize, 0, MidpointRounding.AwayFromZero);

                if (newRow >= 0 && newRow < Tools.BoardSize && newCol >= 0 && newCol < Tools.BoardSize)
                {
                    Board.selectedPiece.Location = new Point(newCol * Tools.SquareSize + (Tools.Margin / 2), newRow * Tools.SquareSize + (Tools.Margin / 2));
                    if (!(newRow == Board.selectedRow && newCol == Board.selectedCol))
                    {
                        Board.pieces[newRow, newCol] = Board.pieces[Board.selectedRow, Board.selectedCol];
                        Board.pieces[Board.selectedRow, Board.selectedCol] = new Piece.PiecePanelPair();
                    }
                }
            }
        }
    }
}
