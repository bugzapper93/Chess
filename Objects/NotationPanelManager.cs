using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace Chess.Objects
{
    class NotationPanelManager
    {
        private int currentRow = 0;
        private Grid notationGrid;

        public NotationPanelManager(Grid grid)
        {
            this.notationGrid = grid;
        }

        public void AddRowToTable(string moveNotation, bool isWhiteTurn)
        {
            notationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            if (!isWhiteTurn)
            {
                TextBlock indexLabel = new TextBlock
                {
                    Text = (currentRow + 1).ToString(),

                    Style = (Style)notationGrid.FindResource("NotationTextBlockStyle")
                };
                Grid.SetRow(indexLabel, currentRow + 1); 
                Grid.SetColumn(indexLabel, 0);
                notationGrid.Children.Add(indexLabel);

                TextBlock whiteMoveLabel = new TextBlock
                {
                    Text = moveNotation,
                    Style = (Style)notationGrid.FindResource("NotationTextBlockStyle")
                };

                Grid.SetRow(whiteMoveLabel, currentRow + 1);
                Grid.SetColumn(whiteMoveLabel, 1);
                notationGrid.Children.Add(whiteMoveLabel);
            }
            else
            {
                TextBlock blackMoveLabel = new TextBlock
                {
                    Text = moveNotation,
                    Style = (Style)notationGrid.FindResource("NotationTextBlockStyle")
                };
                Grid.SetRow(blackMoveLabel, currentRow + 1);
                Grid.SetColumn(blackMoveLabel, 2);
                notationGrid.Children.Add(blackMoveLabel);

                currentRow++; 
            }
            ScrollToLastRow();


        }
        public void ScrollToLastRow()
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(notationGrid);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToEnd();
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                {
                    return result;
                }
                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                {
                    return descendant;
                }
            }
            return null;
        }
        public string GetAlgebraicNotation(Position startPos, Position endPos, bool isWhiteTurn, bool isCapture, bool isPawnMove, bool isEnPassant, int pieceType)
        {
            string pieceNotation = Pieces.PieceValueToString(pieceType);
            char columnStart = (char)('a' + startPos.column);
            char rowStart = (char)('8' - startPos.row);

            char columnEnd = (char)('a' + endPos.column);
            char rowEnd = (char)('8' - endPos.row);

            string notation = "";
            if (isPawnMove)
            {
                if (isCapture)
                {
                    notation = $"{columnStart} x {columnEnd}{rowEnd}";
                }
                else
                {
                    notation = $"{columnEnd}{rowEnd}";
                }

                if (isEnPassant)
                {
                    notation += "(e.p.)";
                }
            }
            else
            {
                notation = $"{pieceNotation}{columnEnd}{rowEnd}";

                if (isCapture)
                {
                    notation = $"{pieceNotation} x {columnEnd}{rowEnd}";
                }
            }

            return notation;
        }   
    }
}
