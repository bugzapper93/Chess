﻿using Chess.Objects;
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
    public int depth = 2;
    private ChessAI AI = new ChessAI(3);
    Chessboard Board = new Chessboard();
    
    public MainWindow()
    {
        InitializeComponent();
        notationPanelManager = new NotationPanelManager(NotationGrid);
        DrawChessboard();
        PlacePieces();
        CheckVisibility();
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

                    double pieceLeft = displayCol * SquareSize + (SquareSize - piece.Width) / 2 ;
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
                    Move bestMove = await AI.GetBestMove(Board, color);
                    MovePiece(bestMove.startPosition, bestMove.targetPosition);
                }
            }
        }
    }
    #endregion
    #region BoardInteraction
    private bool MovePiece(Position positionStart, Position positionEnd)
    {
        int pieceValue = Board.pieces[positionStart.row, positionStart.column].value;
        int startRow = positionStart.row;
        int startCol = positionStart.column;
        int endRow = positionEnd.row;
        int endCol = positionEnd.column;

        UIElement selectedPiece = PiecesDisplay[positionStart.row, positionStart.column];
        if (selectedPiece == null) 
            return false;

        var parentCanvas = VisualTreeHelper.GetParent(selectedPiece) as Canvas;

        if (parentCanvas == null)
            return false;

        int index = Helpers.GetMoveIndex(Board.moveset.moves, positionStart, positionEnd);
        if (index == -1)
        {
            int displayStartRow = isBoardFlipped ? 7 - positionStart.row : positionStart.row;
            int displayStartCol = isBoardFlipped ? 7 - positionStart.column : positionStart.column;

            double pieceLeft = displayStartCol * SquareSize + (SquareSize - selectedPiece.RenderSize.Width) / 2;
            double pieceTop = (displayStartRow * SquareSize + (SquareSize - selectedPiece.RenderSize.Height) / 2) + 5;

            Canvas.SetLeft(selectedPiece, pieceLeft);
            Canvas.SetTop(selectedPiece, pieceTop);
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
            }
        }
    }
    #endregion
    #region UIManagement
    private void Hide(object sender, RoutedEventArgs e)
    {
        CheckVisibility();
    }
    private void startBTN_Click(object sender, RoutedEventArgs e)
    {
        ChooseModeMenu();
    }
    private void CheckVisibility()
    {
        if (display.Visibility == Visibility.Visible)
        {
            display.Visibility = Visibility.Hidden;
            NotationGridScrollView.Visibility = Visibility.Hidden;
            MainMenu.Visibility = Visibility.Visible;
            HideBtn.Visibility = Visibility.Hidden;
            ModeMenu.Visibility = Visibility.Hidden;
            AuthorsMenu.Visibility = Visibility.Hidden;
            SettingsMenu.Visibility = Visibility.Hidden;
        }
        else
        {
            if(ModeMenu.Visibility == Visibility.Visible 
                || AuthorsMenu.Visibility == Visibility.Visible 
                || SettingsMenu.Visibility == Visibility.Visible)
            {
                MainMenu.Visibility = Visibility.Visible;
                HideBtn.Visibility = Visibility.Hidden;
                ModeMenu.Visibility = Visibility.Hidden;
                AuthorsMenu.Visibility = Visibility.Hidden;
                SettingsMenu.Visibility = Visibility.Hidden;
            }
            else
            {
                display.Visibility = Visibility.Visible;
                NotationGridScrollView.Visibility = Visibility.Visible;
                MainMenu.Visibility = Visibility.Hidden;
                HideBtn.Visibility = Visibility.Visible;
                ModeMenu.Visibility = Visibility.Hidden;
                AuthorsMenu.Visibility = Visibility.Hidden;
                SettingsMenu.Visibility = Visibility.Hidden;
            }
        }
    }
    private void ShowBoard()
    {
        display.Visibility = Visibility.Visible;
        HideBtn.Visibility = Visibility.Visible;
        NotationGridScrollView.Visibility = Visibility.Visible;
        ModeMenu.Visibility = Visibility.Hidden;
        ResetGame();
    }
    private void ShowAuthorsPanel()
    {
        AuthorsMenu.Visibility = Visibility.Visible;
        MainMenu.Visibility = Visibility.Hidden;
        HideBtn.Visibility = Visibility.Visible;
    }
    private void ShowSettingsPanel()
    {
        SettingsMenu.Visibility = Visibility.Visible;
        MainMenu.Visibility = Visibility.Hidden;
        HideBtn.Visibility = Visibility.Visible;
    }
    private void ResetGame()
    {
        isBoardFlipped = false;
        Board = new Chessboard(); 
        display.Children.Clear(); 
        DrawChessboard(); 
        PlacePieces();
        //notationPanelManager.ClearNotations();
    }
    private void ChooseModeMenu()
    {
        ModeMenu.Visibility = Visibility.Visible;
        MainMenu.Visibility = Visibility.Hidden;
        HideBtn.Visibility = Visibility.Visible;
    }

    private void SlowGameCheck_Checked(object sender, RoutedEventArgs e)
    {

    }

    private void SpeedGameCheck_Checked(object sender, RoutedEventArgs e)
    {

    }

    private void PvPBtn_Click(object sender, RoutedEventArgs e)
    {
        aiModeON = false;
        ShowBoard();
    }

    private void PvCBtn_Click(object sender, RoutedEventArgs e)
    {
        aiModeON = true;
        ShowBoard();
    }

    private void PvPLANBtn_Click(object sender, RoutedEventArgs e)
    {
        aiModeON = false;
        ShowBoard();
    }

    private void PvCGMBtn_Click(object sender, RoutedEventArgs e)
    {
        aiModeON = true;
        ShowBoard();
    }

    private void settingsBtn_Click(object sender, RoutedEventArgs e)
    {
        ShowSettingsPanel();
    }

    private void authorsBtn_Click(object sender, RoutedEventArgs e)
    {
        ShowAuthorsPanel();
    }

    private void exitBtn_Click(object sender, RoutedEventArgs e)
    {
        App.Current.Shutdown();
    }

    private void easyAiDiffRB_Checked(object sender, RoutedEventArgs e)
    {
        depth = 1;
        AI = new ChessAI(depth);
    }

    private void mediumAiDiffRB_Checked(object sender, RoutedEventArgs e)
    {
        depth = 3;
        AI = new ChessAI(depth);
    }

    private void hardAiDiffRB_Checked(object sender, RoutedEventArgs e)
    {
        depth = 5;
        AI = new ChessAI(depth);
    }
    #endregion
}