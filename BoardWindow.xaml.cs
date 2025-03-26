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
        private int SquareSize = Constants.Square_Size;
        private Rectangle[,] PiecesDisplay = new Rectangle[8, 8];
        private Rectangle[,] Squares = new Rectangle[8, 8];
        public Chessboard Board = new Chessboard();

        private Position selectedPosition;
        private Point originalMouseOffset;
        private bool isDragging = false;
        private UIElement? selectedPiece;

        private int playerColor = Pieces.White;
        public BoardWindow()
        {
            InitializeComponent();

            DrawChessboard();
            PlacePieces();
        }
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
                    int displayRow = isBoardFlipped ? 7 - row : row;
                    int displayCol = isBoardFlipped ? 7 - col : col;
                    Canvas.SetLeft(square, displayCol * SquareSize);
                    Canvas.SetTop(square, displayRow * SquareSize + 5);
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

                        int displayRow = isBoardFlipped ? 7 - row : row;
                        int displayCol = isBoardFlipped ? 7 - col : col;

                        double pieceLeft = displayCol * SquareSize + (SquareSize - piece.Width) / 2;
                        double pieceTop = (displayRow * SquareSize + (SquareSize - piece.Height) / 2) + 5;

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
        public bool MovePiece(Position positionStart, Position positionEnd, int playerColor)
        {
            int pieceValue = Board.pieces[positionStart.row, positionStart.column].value;
            int startRow = positionStart.row;
            int startCol = positionStart.column;
            int endRow = positionEnd.row;
            int endCol = positionEnd.column;

            int currentColorTurn = Board.isWhiteTurn ? Pieces.White : Pieces.Black;

            UIElement selectedPiece = PiecesDisplay[positionStart.row, positionStart.column];
            if (selectedPiece == null)
                return false;

            var parentCanvas = VisualTreeHelper.GetParent(selectedPiece) as Canvas;

            if (parentCanvas == null)
                return false;

            if (playerColor != currentColorTurn)
            {
                ResetPiecePosition(positionStart, selectedPiece);
                return false;
            }

            int index = Helpers.GetMoveIndex(Board.moveset.moves, positionStart, positionEnd);
            if (index == -1)
            {
                ResetPiecePosition(positionStart, selectedPiece);

                //int displayStartRow = isBoardFlipped ? 7 - positionStart.row : positionStart.row;
                //int displayStartCol = isBoardFlipped ? 7 - positionStart.column : positionStart.column;

                //double pieceLeft = displayStartCol * SquareSize + (SquareSize - selectedPiece.RenderSize.Width) / 2;
                //double pieceTop = (displayStartRow * SquareSize + (SquareSize - selectedPiece.RenderSize.Height) / 2) + 5;

                //Canvas.SetLeft(selectedPiece, pieceLeft);
                //Canvas.SetTop(selectedPiece, pieceTop);
                return false;
            }

            if (PiecesDisplay[positionEnd.row, positionEnd.column] != null && PiecesDisplay[positionEnd.row, positionEnd.column] != selectedPiece)
            {
                parentCanvas.Children.Remove(PiecesDisplay[positionEnd.row, positionEnd.column]);
                PiecesDisplay[positionEnd.row, positionEnd.column] = null;
            }

            int displayEndRow = isBoardFlipped ? 7 - positionEnd.row : positionEnd.row;
            int displayEndCol = isBoardFlipped ? 7 - positionEnd.column : positionEnd.column;

            Canvas.SetLeft(selectedPiece, displayEndCol * SquareSize);
            Canvas.SetTop(selectedPiece, displayEndRow * SquareSize + 5);


            PiecesDisplay[positionEnd.row, positionEnd.column] = (Rectangle)selectedPiece;
            PiecesDisplay[positionStart.row, positionStart.column] = null;

            Move move = Board.moveset.moves[index];
            bool enPassant = false;
            if ((pieceValue & 7) == Pieces.Pawn && move.capture)
            {
                // En passant
                if (Board.pieces[endRow, endCol].value == 0)
                {
                    parentCanvas.Children.Remove(PiecesDisplay[positionStart.row, positionEnd.column]);
                    PiecesDisplay[positionStart.row, positionEnd.column] = null;
                    enPassant = true;
                }
            }
            if ((pieceValue & 7) == Pieces.King && Math.Abs(move.startPosition.row - move.targetPosition.row) == 2)
            {
                int rookCol = move.targetPosition.column == 2 ? 0 : 7;
                int rookTargetCol = move.targetPosition.column == 2 ? 3 : 5;
                Rectangle rook = PiecesDisplay[startRow, rookCol];
                Canvas.SetLeft(rook, rookTargetCol * SquareSize);
                Canvas.SetTop(rook, startRow * SquareSize + 5);
                PiecesDisplay[startRow, rookTargetCol] = rook;
                PiecesDisplay[startRow, rookCol] = null;
            }
            Board.MakeMove(move);

            //string moveNotation = notationPanelManager.GetAlgebraicNotation(move, Board, pieceValue, enPassant);
            //notationPanelManager.AddRowToTable(moveNotation, Board.isWhiteTurn);
            ResetBoard();

            return true;
        }
        private void ResetPiecePosition(Position position, UIElement piece)
        {
            int displayStartRow = isBoardFlipped ? 7 - position.row : position.row;
            int displayStartCol = isBoardFlipped ? 7 - position.column : position.column;

            double pieceLeft = displayStartCol * SquareSize + (SquareSize - piece.RenderSize.Width) / 2;
            double pieceTop = (displayStartRow * SquareSize + (SquareSize - piece.RenderSize.Height) / 2) + 5;

            Canvas.SetLeft(piece, pieceLeft);
            Canvas.SetTop(piece, pieceTop);
        }
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
        private async void PieceMouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging && selectedPiece != null)
            {
                isDragging = false;
                var parentCanvas = VisualTreeHelper.GetParent(selectedPiece) as Canvas;
                selectedPiece.ReleaseMouseCapture();
                Point mousePosition = e.GetPosition(display);
                int row = (int)(mousePosition.Y / SquareSize);
                int col = (int)(mousePosition.X / SquareSize);
                if (isBoardFlipped)
                {
                    row = 7 - row;
                    col = 7 - col;
                }
                Position targetPosition = new Position(row, col); // Define targetPosition here
                if (MovePiece(selectedPosition, targetPosition, playerColor))
                {
                    //if (!aiModeON && !onlineModeON)
                    //{
                    if (Board.isWhiteTurn == isBoardFlipped)
                    {
                        FlipBoard();
                    }
                    //}
                    //else if (aiModeON)
                    //{
                    //    int color = Board.isWhiteTurn ? Pieces.White : Pieces.Black;
                    //    Move bestMove = await AI.GetBestMove(Board, color);
                    //    MovePiece(bestMove.startPosition, bestMove.targetPosition, Pieces.GetOppositeColor(playerColor));
                    //}
                    //else if (onlineModeON)
                    //{
                    //    await _chessOnline.SendMoveAsync(selectedPosition, targetPosition); // Use targetPosition here
                    //    Trace.WriteLine($"[PieceMouseUp] Sending move from {selectedPosition.row},{selectedPosition.column} " +
                    //$"to {targetPosition.row},{targetPosition.column}");
                    //    if (Board.isWhiteTurn == isBoardFlipped)
                    //    {
                    //        FlipBoard();
                    //    }
                    //}
                }
            }
        }
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
        public void FlipBoard()
        {
            isBoardFlipped = !isBoardFlipped;
            display.Children.Clear();
            DrawChessboard();
            PlacePieces();
        }
        private void ResetBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Squares[row, col].Fill = ((row + col) % 2 == 0) ? Constants.Primary : Constants.Secondary;
                }
            }
        }
    }
}

