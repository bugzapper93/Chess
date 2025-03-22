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
    private const int SquareSize = 50;
    public MainWindow()
    {
        InitializeComponent();

        //Rectangle rectangle = new Rectangle();
        //rectangle.Width = 100;
        //rectangle.Height = 100;
        //rectangle.Fill = Brushes.Black;
        //rectangle.Margin = new Thickness(0, 0, 0, 0);
        //display.Children.Add(rectangle);
        DrawChessboard();
        PlacePieces();
    }
    private void DrawChessboard()
    {
        // Create an 8x8 chessboard
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Rectangle square = new Rectangle
                {
                    Width = SquareSize,
                    Height = SquareSize,
                    Fill = ((row + col) % 2 == 0) ? Brushes.White : Brushes.Black
                };

                // Position the square on the canvas
                Canvas.SetLeft(square, col * SquareSize);
                Canvas.SetTop(square, row * SquareSize);
                display.Children.Add(square);
            }
        }
    }

    private void PlacePieces()
    {
        // For demonstration, we will place "pieces" using ellipses.
        // You can adjust the placement to mirror a standard chess setup.
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                // Only place a piece on rows that represent a player's side.
                // Here we use row 0-1 for one side and row 6-7 for the other.
                if (row < 2 || row > 5)
                {
                    Ellipse piece = new Ellipse
                    {
                        Width = SquareSize * 0.8,
                        Height = SquareSize * 0.8,
                        // Color the piece based on the row: Blue for one side, Red for the other.
                        Fill = (row < 2) ? Brushes.Blue : Brushes.Red
                    };

                    // Center the piece within the square
                    double pieceLeft = col * SquareSize + (SquareSize - piece.Width) / 2;
                    double pieceTop = row * SquareSize + (SquareSize - piece.Height) / 2;
                    Canvas.SetLeft(piece, pieceLeft);
                    Canvas.SetTop(piece, pieceTop);

                    display.Children.Add(piece);
                }
            }
        }
    }
}