using Chess.Objects;
using Chess.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Chess
{
    /// <summary>
    /// Interaction logic for AIMode.xaml
    /// </summary>
    public partial class AIMode : Window
    {
        private int SquareSize = Constants.Square_Size;
        private Rectangle[,] PiecesDisplay = new Rectangle[8, 8];
        private Rectangle[,] Squares = new Rectangle[8, 8];
        private bool isAIModeActive = true;
        private Point startPos;
        private Position selectedPosition;
        private Point originalMouseOffset;
        private bool isDragging = false;
        private UIElement? selectedPiece;
        private AIPlayer aiPlayer;
        Chessboard Board = new Chessboard();

        public AIMode()
        {
            InitializeComponent();
            DrawChessboard();
            PlacePieces();
            if (isAIModeActive)
            {
                aiPlayer = new AIPlayer(2, Pieces.Black, 100);
            }
        }
        #region Initialization
        private void DrawChessboard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Rectangle square = new Rectangle
                    {
                        Width = SquareSize,
                        Height = SquareSize
                    };
                    Canvas.SetLeft(square, col * SquareSize);
                    Canvas.SetTop(square, row * SquareSize);
                    Squares[row, col] = square;
                    display.Children.Add(square);
                }
            }
            ResetBoard();
        }

        private void PlacePieces()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (Board.pieces[row, col].value != 0)
                    {
                        Rectangle piece = Helpers.CreatePiece(Board.pieces[row, col].value);

                        double pieceLeft = col * SquareSize + (SquareSize - piece.Width) / 2;
                        double pieceTop = row * SquareSize + (SquareSize - piece.Height) / 2;
                        Canvas.SetLeft(piece, pieceLeft);
                        Canvas.SetTop(piece, pieceTop);

                        piece.MouseDown += PieceMouseDown;
                        piece.MouseMove += PieceMouseMove;
                        piece.MouseUp += PieceMouseUp;

                        PiecesDisplay[row, col] = piece;
                        display.Children.Add(piece);
                    }
                }
            }
        }
        #endregion
        #region Mouse handlers
        private void PieceMouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                Rectangle piece = (Rectangle)sender;
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                        if (PiecesDisplay[i, j] == piece)
                        {
                            selectedPiece = piece;
                            selectedPosition = new Position(i, j);
                            isDragging = true;

                            Point mousePosition = e.GetPosition(piece);
                            originalMouseOffset = new Point(mousePosition.X, mousePosition.Y);
                            HighlightBoard();
                            piece.CaptureMouse();
                        }
            }
        }
        private void PieceMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var parentCanvas = VisualTreeHelper.GetParent(selectedPiece) as Canvas;
                if (parentCanvas == null)
                    return;

                Point mousePosition = e.GetPosition(parentCanvas);
                Canvas.SetLeft(selectedPiece, mousePosition.X - originalMouseOffset.X);
                Canvas.SetTop(selectedPiece, mousePosition.Y - originalMouseOffset.Y);
            }
        }

        private void PieceMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging && selectedPiece != null)
            {
                isDragging = false;
                selectedPiece.ReleaseMouseCapture();

                Point mousePosition = e.GetPosition(display);
                int targetRow = (int)(mousePosition.Y / SquareSize);
                int targetCol = (int)(mousePosition.X / SquareSize);

                if (targetRow < 0 || targetRow >= 8 || targetCol < 0 || targetCol >= 8 ||
                    (targetRow == selectedPosition.row && targetCol == selectedPosition.column))
                {
                    // Jeśli ruch jest nieprawidłowy, przywróć pionek na swoje miejsce
                    Canvas.SetLeft(selectedPiece, selectedPosition.column * SquareSize);
                    Canvas.SetTop(selectedPiece, selectedPosition.row * SquareSize);
                }
                else
                {
                    // Sprawdź, czy ruch jest prawidłowy
                    Move move;
                    if (Board.CheckIfValidMove(selectedPosition, new Position(targetRow, targetCol), out move))
                    {
                        MovePiece(selectedPosition, new Position(targetRow, targetCol));

                        // Po ruchu gracza, AI wykonuje swój ruch
                        if (isAIModeActive && !Board.isWhiteTurn)
                        {
                            MakeAIMove();
                        }
                    }
                    else
                    {
                        // Jeśli ruch jest nieprawidłowy, przywróć pionek na swoje miejsce
                        Canvas.SetLeft(selectedPiece, selectedPosition.column * SquareSize);
                        Canvas.SetTop(selectedPiece, selectedPosition.row * SquareSize);
                    }
                }
            }
        }
        private void MovePiece(Position startPos, Position endPos)
        {
            int pieceValue = Board.pieces[startPos.row, startPos.column].value;
            int startRow = startPos.row;
            int startCol = startPos.column;
            int endRow = endPos.row;
            int endCol = endPos.column;

            UIElement selectedPiece = PiecesDisplay[startRow, startCol];
            var parentCanvas = VisualTreeHelper.GetParent(selectedPiece) as Canvas;
            if (parentCanvas == null)
                return;

            int index = Helpers.GetMoveIndex(Board.moveset.moves, startPos, endPos);
            if (index == -1)
            {
                Canvas.SetLeft(selectedPiece, startCol * SquareSize);
                Canvas.SetTop(selectedPiece, startRow * SquareSize);
                return;
            }

            // Usuń zbity pionek (jeśli istnieje)
            if (PiecesDisplay[endRow, endCol] != null && PiecesDisplay[endRow, endCol] != selectedPiece)
            {
                parentCanvas.Children.Remove(PiecesDisplay[endRow, endCol]);
                PiecesDisplay[endRow, endCol] = null;
            }

            // Przesuń pionek
            Canvas.SetLeft(selectedPiece, endCol * SquareSize);
            Canvas.SetTop(selectedPiece, endRow * SquareSize);

            PiecesDisplay[endRow, endCol] = (Rectangle)selectedPiece;
            PiecesDisplay[startRow, startCol] = null;

            Move move = Board.moveset.moves[index];

            // Obsługa specjalnych ruchów
            if ((pieceValue & 7) == Pieces.Pawn && move.capture)
            {
                // En passant
                if (Board.pieces[endRow, endCol].value == 0)
                {
                    parentCanvas.Children.Remove(PiecesDisplay[startRow, endCol]);
                    PiecesDisplay[startRow, endCol] = null;
                }
            }
            if ((pieceValue & 7) == Pieces.King && Math.Abs(move.startPosition.row - move.targetPosition.row) == 2)
            {
                // Roszada
                int rookCol = move.targetPosition.column == 2 ? 0 : 7;
                int rookTargetCol = move.targetPosition.column == 2 ? 3 : 5;
                Rectangle rook = PiecesDisplay[startRow, rookCol];
                Canvas.SetLeft(rook, rookTargetCol * SquareSize);
                Canvas.SetTop(rook, startRow * SquareSize);
                PiecesDisplay[startRow, rookTargetCol] = rook;
                PiecesDisplay[startRow, rookCol] = null;
            }

            // Wykonaj ruch na planszy
            Board.MakeMove(move);
            ResetBoard();
        }

        private void MakeAIMove()
        {
            Move bestMove = aiPlayer.GetBestMove(Board);

            if (bestMove.Equals(default(Move)))
            {
                return;
            }

            if (Board.isWhiteTurn) return;

            int pieceColor = Board.pieces[bestMove.startPosition.row, bestMove.startPosition.column].value & 24;
            if (pieceColor != aiPlayer.GetColor())
            {
                return;
            }

            Rectangle aiPiece = PiecesDisplay[bestMove.startPosition.row, bestMove.startPosition.column];
            if (aiPiece == null)
            {
                return;
            }

            // Usuń zbity pionek (jeśli istnieje)
            if (PiecesDisplay[bestMove.targetPosition.row, bestMove.targetPosition.column] != null)
            {
                var parentCanvas = VisualTreeHelper.GetParent(PiecesDisplay[bestMove.targetPosition.row, bestMove.targetPosition.column]) as Canvas;
                parentCanvas?.Children.Remove(PiecesDisplay[bestMove.targetPosition.row, bestMove.targetPosition.column]);
                PiecesDisplay[bestMove.targetPosition.row, bestMove.targetPosition.column] = null;
            }

            // Przesuń pionek AI
            Canvas.SetLeft(aiPiece, bestMove.targetPosition.column * SquareSize);
            Canvas.SetTop(aiPiece, bestMove.targetPosition.row * SquareSize);

            PiecesDisplay[bestMove.targetPosition.row, bestMove.targetPosition.column] = aiPiece;
            PiecesDisplay[bestMove.startPosition.row, bestMove.startPosition.column] = null;

            // Wykonaj ruch na planszy
            Board.MakeMove(bestMove);
            ResetBoard();
        }

        #endregion      
        private void HighlightBoard()
        {
            ResetBoard();
            int row = selectedPosition.row;
            int col = selectedPosition.column;
            List<Move> moves = Board.moveset.moves;
            foreach (Move move in moves)
            {
                if (move.startPosition == selectedPosition)
                {
                    Rectangle square = Squares[move.targetPosition.row, move.targetPosition.column];
                    square.Fill = Brushes.Red;
                }
            }
        }
        private void ResetBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Squares[row, col].Fill = ((row + col) % 2 == 0) ? Constants.Primary : Constants.Secondary;
                    if (Board.squares[row, col].dangerWhite)
                    {
                        Squares[row, col].Fill = Brushes.LightGoldenrodYellow;
                    }
                    if (Board.squares[row, col].dangerBlack)
                    {
                        Squares[row, col].Fill = Brushes.LightCoral;
                    }
                }
            }
        }
    }
}
