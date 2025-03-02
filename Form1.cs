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
                square.Click += MoveClick;
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
        private void MoveClick(object sender, EventArgs e)
        {
            if (Board.selectedPiece == null) return;

            if (sender is Panel square)
            {
                int targetRow = -1;
                int targetCol = -1;

                for (int row = 0; row < Tools.BoardSize; row++)
                {
                    for (int col = 0; col < Tools.BoardSize; col++)
                    {
                        if (Board.squares[row, col] == square)
                        {
                            targetRow = row;
                            targetCol = col;
                            break;
                        }
                    }
                }
                if (targetRow == -1 || targetCol == -1) return;

                int pieceValue = Board.pieces[Board.selectedRow, Board.selectedCol].number;

                if (Board.CheckValidity(pieceValue, Board.selectedRow, Board.selectedCol, targetRow, targetCol))
                {
                    MovePiece(pieceValue, targetRow, targetCol);
                }
            }
        }
        private void Piece_MouseDown(object sender, MouseEventArgs e)
        {
            Board.SetBackColor();
            if (sender is Panel panel && Board.selectedPiece == null)
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

                            Board.HighlightLegalMoves(Board.pieces[row, col].number, row, col);
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

                int piece = Board.pieces[Board.selectedRow, Board.selectedCol].number;

                if (newRow >= 0 && newRow < Tools.BoardSize && newCol >= 0 && newCol < Tools.BoardSize)
                {
                    MovePiece(piece, newRow, newCol);
                }
            }
        }
        public void MovePiece(int piece, int newRow, int newCol)
        {
            if (Board.CheckValidity(piece, Board.selectedRow, Board.selectedCol, newRow, newCol))
            {
                if (Board.pieces[newRow, newCol].number != 0)
                {
                    Controls.Remove(Board.pieces[newRow, newCol].panel);
                    //handle capture
                }
                Board.selectedPiece.Location = new Point(newCol * Tools.SquareSize + (Tools.Margin / 2), newRow * Tools.SquareSize + (Tools.Margin / 2));
                Board.pieces[newRow, newCol] = Board.pieces[Board.selectedRow, Board.selectedCol];
                Board.pieces[Board.selectedRow, Board.selectedCol] = new Piece.PiecePanelPair();

                Board.SetBackColor();
            }
            else
            {
                Board.selectedPiece.Location = Board.originalLocation;
            }
        }
    }
}
