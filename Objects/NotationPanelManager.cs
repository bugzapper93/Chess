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
            notationGrid = grid;
        }

        public void AddRowToTable(string moveNotation, bool isWhiteTurn)
        {
            notationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            if (!isWhiteTurn)
            {
                TextBlock indexLabel = new TextBlock
                {
                    Text = (currentRow + 1).ToString() + ".",

                    Style = (Style)notationGrid.FindResource("NotationTextBlockStyle"),
                    Width = 52
                };
                Grid.SetRow(indexLabel, currentRow + 1); 
                Grid.SetColumn(indexLabel, 0);
                notationGrid.Children.Add(indexLabel);

                TextBlock whiteMoveLabel = new TextBlock
                {
                    Text = moveNotation,
                    Style = (Style)notationGrid.FindResource("NotationTextBlockStyle"),
                    Width = 78
                };

                Grid.SetRow(whiteMoveLabel, currentRow + 1);
                Grid.SetColumn(whiteMoveLabel, 1);
                notationGrid.Children.Add(whiteMoveLabel);

                TextBlock blackMoveLabel = new TextBlock
                {
                    Text = "", 
                    Style = (Style)notationGrid.FindResource("NotationTextBlockStyle"),
                    Width = 78
                };
                Grid.SetRow(blackMoveLabel, currentRow + 1);
                Grid.SetColumn(blackMoveLabel, 2);
                notationGrid.Children.Add(blackMoveLabel);
            }
            else
            {
                var blackMoveLabel = notationGrid.Children
                  .OfType<TextBlock>()
                  .FirstOrDefault(tb => Grid.GetRow(tb) == currentRow + 1 && Grid.GetColumn(tb) == 2);

                if (blackMoveLabel != null)
                {
                    blackMoveLabel.Text = moveNotation;
                }

                currentRow++; 
            }
        }

        public string GetAlgebraicNotation(Move move, Chessboard board, int pieceType, bool isEnPassant)//Position startPos, Position endPos, bool isWhiteTurn, bool isCapture, bool isPawnMove, bool isEnPassant, int pieceType)
        {
            Position startPos = move.startPosition;
            Position endPos = move.targetPosition;
            bool isWhiteTurn = board.isWhiteTurn;
            bool isCapture = move.capture;
            bool isPawnMove = (pieceType & 7) == Pieces.Pawn;

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
                    notation = $"{columnStart}x{columnEnd}{rowEnd}";
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
                    notation = $"{pieceNotation}x{columnEnd}{rowEnd}";
                }
            }

            return notation;
        }
        public void ClearNotations()
        {
            var elementsToRemove = notationGrid.Children
                .Cast<UIElement>()
                .Where(el => Grid.GetRow(el) > 0) 
                .ToList();

            foreach (var element in elementsToRemove)
            {
                notationGrid.Children.Remove(element);
            }

            while (notationGrid.RowDefinitions.Count > 1) 
            {
                notationGrid.RowDefinitions.RemoveAt(1);
            }

            currentRow = 0;
        }
    }
}
