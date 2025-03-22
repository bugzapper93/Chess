﻿using Chess.Objects;
using Chess.Tools;
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
    private int SquareSize = Constants.Square_Size;
    private Rectangle[,] Pieces = new Rectangle[8, 8];
    private Rectangle[,] Squares = new Rectangle[8, 8];

    private Point startPos;
    private Position selectedPosition;
    private Point originalMouseOffset;
    private bool isDragging = false;
    private UIElement? selectedPiece;

    Chessboard Board = new Chessboard();

    public MainWindow()
    {
        InitializeComponent();
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

                    Pieces[row, col] = piece;
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
                    if (Pieces[i, j] == piece)
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
            selectedPiece.ReleaseMouseCapture();
            Point mousePosition = e.GetPosition(display);
            int row = (int)(mousePosition.Y / SquareSize);
            int col = (int)(mousePosition.X / SquareSize);
            if (row < 0 || row >= 8 || col < 0 || col >= 8 || (row == selectedPosition.row && col == selectedPosition.column))
            {
                Canvas.SetLeft(selectedPiece, selectedPosition.column * SquareSize);
                Canvas.SetTop(selectedPiece, selectedPosition.row * SquareSize);
            }
            else
            {
                MovePiece(selectedPosition, new Position(row, col));
            }
        }
    }
    #endregion
    private void MovePiece(Position positionStart, Position positionEnd)
    {
        

        UIElement selectedPiece = Pieces[positionStart.row, positionStart.column];
        var parentCanvas = VisualTreeHelper.GetParent(selectedPiece) as Canvas;
        if (parentCanvas == null)
            return;

        int index = Helpers.GetMoveIndex(Board.possibleMoves, positionStart, positionEnd);
        if (index == -1)
        {
            Canvas.SetLeft(selectedPiece, positionStart.column * SquareSize);
            Canvas.SetTop(selectedPiece, positionStart.row * SquareSize);
            return;
        }

        if (Pieces[positionEnd.row, positionEnd.column] != null && Pieces[positionEnd.row, positionEnd.column] != selectedPiece)
        {
            parentCanvas.Children.Remove(Pieces[positionEnd.row, positionEnd.column]);
            Pieces[positionEnd.row, positionEnd.column] = null;
        }

        Canvas.SetLeft(selectedPiece, positionEnd.column * SquareSize);
        Canvas.SetTop(selectedPiece, positionEnd.row * SquareSize);

        Pieces[positionEnd.row, positionEnd.column] = (Rectangle)selectedPiece;
        Pieces[positionStart.row, positionStart.column] = null;  // Clear the starting square

        // Continue with any additional game logic
        ResetBoard();
        Move move = Board.possibleMoves[index];
        Board.MakeMove(move);
    }
    private void HighlightBoard()
    {
        ResetBoard();
        int row = selectedPosition.row;
        int col = selectedPosition.column;
        List<Move> moves = Board.possibleMoves;
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
            }
        }
    }
}