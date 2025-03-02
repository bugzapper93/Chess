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
        private bool isWhiteTurn = true;
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
                    piece.panel.Click += MoveClick;
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
                        if (Board.pieces[row, col].panel == square)
                        {
                            if (Board.selectedRow != row && Board.selectedCol != col)
                            {
                                targetRow = row;
                                targetCol = col;
                                break;
                            }
                            break;
                        }
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
            if (sender is Panel panel)
            {
                Panel tempPiece = panel;
                Point tempLocation = panel.Location;
                
                for (int row = 0; row < Tools.BoardSize; row++)
                {
                    for (int col = 0; col < Tools.BoardSize; col++)
                    {
                        if (Board.pieces[row, col].panel == tempPiece)
                        {
                            if (((Board.pieces[row, col].number & Piece.White) != 0 && !isWhiteTurn) || (Board.pieces[row, col].number & Piece.Black) != 0 && isWhiteTurn)
                            {
                                //Board.selectedPiece = null;
                                return;
                            }
                            Board.selectedPiece = panel;
                            Board.originalLocation = panel.Location;

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

                int startRow = Board.selectedRow;
                int startCol = Board.selectedCol;
                int rowDiff = Math.Abs(newRow - startRow);
                int colDiff = Math.Abs(newCol - startCol);
                bool capture = false;

                if ((piece & 7) == Piece.King && colDiff == 2)
                {
                    if (newCol == 6) 
                    {
                        var rookPair = Board.pieces[startRow, 7];
                        if (rookPair.panel != null)
                        {
                            rookPair.panel.Location = new Point(5 * Tools.SquareSize + (Tools.Margin / 2), startRow * Tools.SquareSize + (Tools.Margin / 2));
                            Board.pieces[startRow, 5] = rookPair;
                            Board.pieces[startRow, 7] = new Piece.PiecePanelPair();
                            Board.pieces[startRow, 5].hasMoved = true;
                        }
                    }
                    else if (newCol == 2) 
                    {
                        var rookPair = Board.pieces[startRow, 0];
                        if (rookPair.panel != null)
                        {
                            rookPair.panel.Location = new Point(3 * Tools.SquareSize + (Tools.Margin / 2), startRow * Tools.SquareSize + (Tools.Margin / 2));
                            Board.pieces[startRow, 3] = rookPair;
                            Board.pieces[startRow, 0] = new Piece.PiecePanelPair();
                            Board.pieces[startRow, 3].hasMoved = true;
                        }
                    }
                }

                if ((piece & 7) == Piece.Pawn && colDiff == 1 && rowDiff == 1 && Board.pieces[newRow, newCol].number == 0)
                {
                    int capturedPawnRow = ((piece & Piece.White) != 0) ? newRow + 1 : newRow - 1;
                    var capturedPair = Board.pieces[capturedPawnRow, newCol];
                    if (capturedPair.panel != null)
                    {
                        Controls.Remove(capturedPair.panel);
                        capturedPair.panel.Dispose();
                    }
                    Board.pieces[capturedPawnRow, newCol] = new Piece.PiecePanelPair();
                    capture = true;
                }
                else if (Board.pieces[newRow, newCol].number != 0)
                {
                    capture = true;
                    Controls.Remove(Board.pieces[newRow, newCol].panel);
                    Board.pieces[newRow, newCol] = new Piece.PiecePanelPair();
                    //Panel capturedPanel = Board.pieces[newRow, newCol].panel;
                    //if (capturedPanel != null)
                    //{
                    //    Controls.Remove(capturedPanel);
                    //    capturedPanel.Dispose();
                    //}                    
                }

                Board.enPassantTargetRow = null;
                Board.enPassantTargetCol = null;
                if ((piece & 7) == Piece.Pawn)
                {

                    if ((piece & Piece.White) != 0 && startRow == 6 && newRow == 4)
                    {
                        Board.enPassantTargetRow = 5;
                        Board.enPassantTargetCol = startCol;
                    }

                    if ((piece & Piece.Black) != 0 && startRow == 1 && newRow == 3)
                    {
                        Board.enPassantTargetRow = 2;
                        Board.enPassantTargetCol = startCol;
                    }
                }

                Board.selectedPiece.Location = new Point(newCol * Tools.SquareSize + (Tools.Margin / 2), newRow * Tools.SquareSize + (Tools.Margin / 2));
                Board.pieces[newRow, newCol] = Board.pieces[Board.selectedRow, Board.selectedCol];
                Board.pieces[Board.selectedRow, Board.selectedCol] = new Piece.PiecePanelPair();
                Board.pieces[newRow, newCol].hasMoved = true;

                Board.SetBackColor();
                Board.currentMoves += Tools.GetMoveNotation(piece, Board.selectedRow, Board.selectedCol, newRow, newCol, capture);
                isWhiteTurn = !isWhiteTurn;
                MessageBox.Show(Board.currentMoves);
                Board.selectedPiece = null;
                return;
            }
            else
            {
                Board.selectedPiece.Location = Board.originalLocation;
            }
        }
    }
}
