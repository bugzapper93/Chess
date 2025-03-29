using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Chess.Objects;
using Chess.Tools;

namespace Chess
{
    public partial class BoardWindow : UserControl
    {
        public bool isBoardFlipped = false;

        // Board variables
        private Rectangle[,] PiecesDisplay = new Rectangle[8, 8];
        private Rectangle[,] Squares = new Rectangle[8, 8];

        private Chessboard board = new Chessboard();

        private Point originalMouseOffset;
        private bool isDragging = false;
        private UIElement? selectedPiece;
        int selectedSquare;
        int playerColor = Pieces.White;

        ChessAI bot;
        
        List<int> possibleMoves = new List<int>();
        public BoardWindow()
        {
            InitializeComponent();

            DrawChessboard();
            PlacePieces();
            board.UpdateMoves();

            bot = new ChessAI(5, playerColor);
        }
        private void DrawChessboard()
        {
            for (int square = 0; square < 64; square++)
            {
                Rectangle backgroundPanel = new Rectangle
                {
                    Width = Constants.SquareSize,
                    Height = Constants.SquareSize
                };

                int row = square / 8;
                int col = square % 8;

                int displayRow = row;
                int displayCol = col;

                Canvas.SetLeft(backgroundPanel, displayCol * Constants.SquareSize);
                Canvas.SetBottom(backgroundPanel, displayRow * Constants.SquareSize);

                Squares[row, col] = backgroundPanel;
                display.Children.Add(backgroundPanel);
            }
            ResetBoardView();
        }

        private void PlacePieces()
        {
            char[] pieces = Helpers.GetPieceArray(board);

            for (int square = 0; square < 64; square++)
            {
                if (pieces[square] == default(char))
                    continue;

                Rectangle piece = Helpers.GeneratePiece(pieces[square]);

                int row = square / 8;
                int col = square % 8;

                int displayRow = row;
                int displayCol = col;

                double pieceLeft = displayCol * Constants.SquareSize + (Constants.SquareSize - piece.Width) / 2;
                double pieceBottom = displayRow * Constants.SquareSize + (Constants.SquareSize - piece.Height) / 2;

                Canvas.SetLeft(piece, pieceLeft);
                Canvas.SetBottom(piece, pieceBottom);

                piece.MouseDown += PieceMouseDown;
                piece.MouseMove += PieceMouseMove;
                piece.MouseUp += PieceMouseUp;

                PiecesDisplay[row, col] = piece;
                display.Children.Add(piece);
            }
        }
        private void RepositionPiece(Move move, bool validMove = true)
        {
            int selectedRow = move.From / 8;
            int selectedColumn = move.From % 8;

            int targetRow = move.To / 8;
            int targetColumn = move.To % 8;

            UIElement selectedPiece = PiecesDisplay[selectedRow, selectedColumn];

            if (selectedPiece == null)
                return;

            if (!validMove)
            {
                double tempPosLeft = selectedColumn * Constants.SquareSize + (Constants.SquareSize - ((Rectangle)selectedPiece).Width) / 2;
                double tempPosBottom = selectedRow * Constants.SquareSize + (Constants.SquareSize - ((Rectangle)selectedPiece).Height) / 2;

                Canvas.SetLeft(selectedPiece, tempPosLeft);
                Canvas.SetBottom(selectedPiece, tempPosBottom);
                return;
            }

            double newLeft = targetColumn * Constants.SquareSize + (Constants.SquareSize - ((Rectangle)selectedPiece).Width) / 2;
            double newBottom = targetRow * Constants.SquareSize + (Constants.SquareSize - ((Rectangle)selectedPiece).Height) / 2;

            Canvas.SetLeft(selectedPiece, newLeft);
            Canvas.SetBottom(selectedPiece, newBottom);

            if (PiecesDisplay[targetRow, targetColumn] != null && PiecesDisplay[targetRow, targetColumn] != selectedPiece)
            {
                RemovePiece(move.To);
            }

            PiecesDisplay[targetRow, targetColumn] = (Rectangle)selectedPiece;
            PiecesDisplay[selectedRow, selectedColumn] = null;
        }
        private void RemovePiece(int square)
        {
            int targetRow = square / 8;
            int targetColumn = square % 8;

            UIElement targetPiece = PiecesDisplay[targetRow, targetColumn];
            var parentCanvas = VisualTreeHelper.GetParent(targetPiece) as Canvas;
            if (parentCanvas == null)
                return;
            parentCanvas.Children.Remove(targetPiece);
            PiecesDisplay[targetRow, targetColumn] = null;
        }
        private bool MovePiece(Move move, bool checkIfValid = true)
        {
            int selectedRow = move.From / 8;
            int selectedColumn = move.From % 8;

            int targetRow = move.To / 8;
            int targetColumn = move.To % 8;

            int startSquare = move.From;
            int endSquare = move.To;

            ulong toMask = 1UL << endSquare;
            ulong fromMask = 1UL << startSquare;            

            if (!possibleMoves.Contains(endSquare) && checkIfValid)
            {
                RepositionPiece(new Move(startSquare, startSquare), false);
                return false;
            }
                
            // Special cases

            if (board.isWhiteTurn)
            {
                // En passant
                if (board.EnPassantSquare != null && (board.WhitePawns & fromMask) != 0 && ((1UL << board.EnPassantSquare) & toMask) != 0)
                {
                    RemovePiece(endSquare - 8);
                }
            }
            else
            {
                if (board.EnPassantSquare != null && (board.BlackPawns & fromMask) != 0 && ((1UL << board.EnPassantSquare) & toMask) != 0)
                {
                    RemovePiece(endSquare + 8);
                }
            }

            // Castling
            ulong kingMask = board.isWhiteTurn ? board.WhiteKing : board.BlackKing;
            if ((kingMask & fromMask) != 0 && Math.Abs(move.From - move.To) == 2)
            {
                int rightRookSquare = board.isWhiteTurn ? 7 : 63;
                int leftRookSquare = board.isWhiteTurn ? 0 : 56;

                if (move.To - move.From == 2)
                {
                    RepositionPiece(new Move(rightRookSquare, move.To - 1));
                }
                else if (move.To - move.From == -2)
                {
                    RepositionPiece(new Move(leftRookSquare, move.To + 1));
                }
            }

            RepositionPiece(move);
            MoveData moveData = board.MakeMove(move);
            string moveNotation = NotationPanelManager.GetAlgebraicNotation(moveData);
            board.CurrentMoves += $"{moveNotation}";
            if (Helpers.GetMoveCount(board) == 0)
            {
                if (Helpers.isKingInCheck(board, board.isWhiteTurn))
                    MessageBox.Show("Checkmate!");
                else
                    MessageBox.Show("Stalemate!");
            }
            return true;
        }
        private void HighlightBoard()
        {
            possibleMoves = Helpers.GetPieceMoves(board, selectedSquare, board.isWhiteTurn);
            foreach (int possibleMove in possibleMoves)
            {
                int row = possibleMove / 8;
                int column = possibleMove % 8;
                Squares[row, column].Fill = Brushes.Red;
            }
        }
        private void PieceMouseDown(object sender, MouseEventArgs e)
        {
            int currentColorTurn = board.isWhiteTurn ? Pieces.White : Pieces.Black;
            if (currentColorTurn != playerColor)
                return;
            if (sender is Rectangle rect)
            {
                Rectangle piece = (Rectangle)sender;
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                        if (PiecesDisplay[i, j] == piece)
                        {
                            selectedPiece = piece;
                            isDragging = true;
                            selectedSquare = i * 8 + j;
                            Point mousePosition = e.GetPosition(piece);
                            originalMouseOffset = new Point(mousePosition.X, mousePosition.Y);
                            piece.CaptureMouse();
                            HighlightBoard();
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
                double newLeft = mousePosition.X - originalMouseOffset.X;

                double canvasHeight = parentCanvas.ActualHeight;
                double newBottom = canvasHeight - mousePosition.Y - (selectedPiece.RenderSize.Height - originalMouseOffset.Y);

                Canvas.SetLeft(selectedPiece, newLeft);
                Canvas.SetBottom(selectedPiece, newBottom);
            }
        }
        private async void PieceMouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging && selectedPiece != null)
            {
                isDragging = false;
                var parentCanvas = VisualTreeHelper.GetParent(selectedPiece) as Canvas;
                selectedPiece.ReleaseMouseCapture();
                Point mousePosition = e.GetPosition(display);

                int row = (int)((display.ActualHeight - mousePosition.Y) / Constants.SquareSize);
                int col = (int)(mousePosition.X / Constants.SquareSize);

                int targetSquare = row * 8 + col;

                selectedPiece = null;
                isDragging = false;
                ResetBoardView();

                if (MovePiece(new Move(selectedSquare, targetSquare)))
                {
                    int botColor = playerColor == Pieces.White ? Pieces.Black : Pieces.White;
                    Move move = await bot.GetBestMove(board.Clone(), botColor);
                    MovePiece(move, false);
                }
            }
        }
        private void ResetBoardView()
        {
            for (int square = 0; square < 64; square++)
            {
                int row = square / 8;
                int column = square % 8;

                Squares[row, column].Fill = (((row + column) % 2) == 0) ? Constants.Primary : Constants.Secondary;
            }
        }
    }
}

