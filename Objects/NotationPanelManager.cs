using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Documents;

namespace Chess.Objects
{
    class NotationPanelManager
    {
        private int currentRow = 0;
        private Grid notationGrid;
        private bool useLongNotation = false;
        private List<(Move move, string sanNotation)> moveHistory = new List<(Move, string)>();

        public NotationPanelManager(Grid grid)
        {
            notationGrid = grid;
        }

        public void AddRowToTable(Move move, string moveNotation, bool isWhiteTurn, Chessboard board)
        {
            moveHistory.Add((move, moveNotation));
            string notation = useLongNotation ? GetLongNotation(move, board) : moveNotation;

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
                    Text = notation, // Use computed notation
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
                    blackMoveLabel.Text = notation; 
                }
                currentRow++;
            }
        }
        public void SetNotationType(bool useLong, Chessboard board)
        {
            useLongNotation = useLong; 
            UpdateNotations(board);        
        }
        private void UpdateNotations(Chessboard Board)
        {
            int row = 1; 
            for (int i = 0; i < moveHistory.Count; i += 2)
            {
                string whiteNotation = useLongNotation ? GetLongNotation(moveHistory[i].move, Board) : moveHistory[i].sanNotation;
                var whiteMoveLabel = notationGrid.Children
                    .OfType<TextBlock>()
                    .FirstOrDefault(tb => Grid.GetRow(tb) == row && Grid.GetColumn(tb) == 1);
                if (whiteMoveLabel != null)
                {
                    whiteMoveLabel.Text = whiteNotation;
                }

                string blackNotation = (i + 1 < moveHistory.Count) ?
                    (useLongNotation ? GetLongNotation(moveHistory[i + 1].move, Board) : moveHistory[i + 1].sanNotation) : "";
                var blackMoveLabel = notationGrid.Children
                    .OfType<TextBlock>()
                    .FirstOrDefault(tb => Grid.GetRow(tb) == row && Grid.GetColumn(tb) == 2);
                if (blackMoveLabel != null)
                {
                    blackMoveLabel.Text = blackNotation;
                }

                row++;
            }
        }
        public string GetAlgebraicNotation(Move move, Chessboard board, int pieceType, bool isEnPassant)
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

        public string GetLongNotation(Move move, Chessboard board)
        {
            // Get piece value from the board at the start position
            int pieceValue = board.pieces[move.startPosition.row, move.startPosition.column].value;
            int pieceType = pieceValue & 7; // Extract piece type (e.g., Pawn = 1)
            string pieceNotation = pieceType == Pieces.Pawn ? "" : Pieces.PieceValueToString(pieceValue);

            char startCol = (char)('a' + move.startPosition.column);
            char startRow = (char)('8' - move.startPosition.row);
            char endCol = (char)('a' + move.targetPosition.column);
            char endRow = (char)('8' - move.targetPosition.row);

            string startSquare = $"{startCol}{startRow}";
            string endSquare = $"{endCol}{endRow}";
            string notation = pieceNotation + startSquare;

            if (move.capture)
            {
                notation += "x";
            }
            notation += endSquare;

            return notation;
        }
    }
}