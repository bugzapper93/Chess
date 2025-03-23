using Chess.Objects;
using Chess.Tools;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chess;

public partial class MainWindow : Window
{
    private NotationPanelManager notationPanelManager;
    private bool isBoardFlipped = false;

    private int SquareSize = Constants.Square_Size;
    private Rectangle[,] PiecesDisplay = new Rectangle[8, 8];
    private Rectangle[,] Squares = new Rectangle[8, 8];
    private Position selectedPosition;
    private Point originalMouseOffset;
    private bool isDragging = false;
    private UIElement? selectedPiece;
    public bool aiModeON = true;

    private ChessAI AI = new ChessAI(2);
    Chessboard Board = new Chessboard();
    
    public MainWindow()
    {
        InitializeComponent();
        notationPanelManager = new NotationPanelManager(NotationGrid);
        DrawChessboard();
        PlacePieces();
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
                int displayRow = isBoardFlipped ? 7 - row : row;
                int displayCol = isBoardFlipped ? 7 - col : col;
                Canvas.SetLeft(square, displayCol * SquareSize);
                Canvas.SetTop(square, displayRow * SquareSize);
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
                    double pieceTop = displayRow * SquareSize + (SquareSize - piece.Height) / 2;

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
    private void PieceMouseUp(object sender, MouseEventArgs e)
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
            if (row < 0 || row >= 8 || col < 0 || col >= 8 || (row == selectedPosition.row && col == selectedPosition.column))
            {
                int displayStartRow = isBoardFlipped ? 7 - selectedPosition.row : selectedPosition.row;
                int displayStartCol = isBoardFlipped ? 7 - selectedPosition.column : selectedPosition.column;

                Canvas.SetLeft(selectedPiece, displayStartCol * SquareSize);
                Canvas.SetTop(selectedPiece, displayStartRow * SquareSize);
            }
            else
            {
                if (MovePiece(selectedPosition, new Position(row, col)))
                {
                    if (!aiModeON)
                    {
                        if (Board.isWhiteTurn == isBoardFlipped)
                        {
                            FlipBoard();
                        }
                    }
                    else
                    {
                        int color = Board.isWhiteTurn ? Pieces.White : Pieces.Black;
                        Move bestMove = AI.GetBestMove(Board, color);
                        MovePiece(bestMove.startPosition, bestMove.targetPosition);
                    }
                }
            }
        }
    }
    #endregion
    private bool MovePiece(Position positionStart, Position positionEnd)
    {
        int pieceValue = Board.pieces[positionStart.row, positionStart.column].value;
        int startRow = positionStart.row;
        int startCol = positionStart.column;
        int endRow = positionEnd.row;
        int endCol = positionEnd.column;
            
        UIElement selectedPiece = PiecesDisplay[positionStart.row, positionStart.column];
        var parentCanvas = VisualTreeHelper.GetParent(selectedPiece) as Canvas;
        if (parentCanvas == null)
            return false;

        int index = Helpers.GetMoveIndex(Board.moveset.moves, positionStart, positionEnd);
        if (index == -1)
        {
            int displayStartRow = isBoardFlipped ? 7 - positionStart.row : positionStart.row;
            int displayStartCol = isBoardFlipped ? 7 - positionStart.column : positionStart.column;

            Canvas.SetLeft(selectedPiece, displayStartCol * SquareSize);
            Canvas.SetTop(selectedPiece, displayStartRow * SquareSize);
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
        Canvas.SetTop(selectedPiece, displayEndRow * SquareSize);


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
            Canvas.SetTop(rook, startRow * SquareSize);
            PiecesDisplay[startRow, rookTargetCol] = rook;
            PiecesDisplay[startRow, rookCol] = null;
        }
        Board.MakeMove(move);

        string moveNotation = notationPanelManager.GetAlgebraicNotation(move, Board, pieceValue, enPassant);
        notationPanelManager.AddRowToTable(moveNotation, Board.isWhiteTurn);
        ResetBoard();

        return true;
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
    private void FlipBoard()
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
    private void Hide(object sender, RoutedEventArgs e)
    {
        if (display.Visibility == Visibility.Visible)
            display.Visibility = Visibility.Hidden;
        else
            display.Visibility = Visibility.Visible;
    }
}