using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Annotations;
using System.Windows.Documents;

namespace Chess.Objects
{
    public struct MoveData
    {
        public Move move;
        public int piece;
        public bool isWhite;
        public bool capture;
        public bool enPassant;
    }
    public class NotationPanelManager
    {
        private int currentRow = 0;
        private Grid notationGrid;
        private bool useLongNotation = false;
        private List<MoveData> moveHistory = new List<MoveData>();
        public NotationPanelManager(Grid grid)
        {
            notationGrid = grid;
        }

        public void AddRowToTable(MoveData moveData, bool isWhiteTurn)
        {
            string sanNotation = GetAlgebraicNotation(moveData);
            string longNotation = GetLongNotation(moveData);
            moveHistory.Add(moveData);

            if (!isWhiteTurn) // White's move - new row
            {
                notationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                int rowIndex = currentRow + 1;

                // Add move number
                TextBlock indexLabel = new TextBlock
                {
                    Text = (currentRow + 1).ToString() + ".",
                    Style = (Style)notationGrid.FindResource("NotationTextBlockStyle"),
                    Width = 52
                };
                Grid.SetRow(indexLabel, rowIndex);
                Grid.SetColumn(indexLabel, 0);
                notationGrid.Children.Add(indexLabel);

                // Add white's move
                TextBlock whiteMoveLabel = new TextBlock
                {
                    Text = useLongNotation ? longNotation : sanNotation,
                    Style = (Style)notationGrid.FindResource("NotationTextBlockStyle"),
                    Width = 78
                };
                Grid.SetRow(whiteMoveLabel, rowIndex);
                Grid.SetColumn(whiteMoveLabel, 1);
                notationGrid.Children.Add(whiteMoveLabel);

                // Add empty black's move (will be filled later)
                TextBlock blackMoveLabel = new TextBlock
                {
                    Text = "",
                    Style = (Style)notationGrid.FindResource("NotationTextBlockStyle"),
                    Width = 78
                };
                Grid.SetRow(blackMoveLabel, rowIndex);
                Grid.SetColumn(blackMoveLabel, 2);
                notationGrid.Children.Add(blackMoveLabel);
            }
            else // Black's move - update existing row
            {
                int rowIndex = currentRow + 1;
                var blackMoveLabel = notationGrid.Children
                    .OfType<TextBlock>()
                    .FirstOrDefault(tb => Grid.GetRow(tb) == rowIndex && Grid.GetColumn(tb) == 2);

                if (blackMoveLabel != null)
                {
                    blackMoveLabel.Text = useLongNotation ? longNotation : sanNotation;
                }

                currentRow++;
            }
        }

        public static string GetAlgebraicNotation(MoveData move)
        {
            bool isPawnMove = move.piece == Pieces.Pawn;
            bool isCapture = move.capture;
            bool isEnpassant = move.enPassant;

            int start = move.move.From;
            int end = move.move.To;

            char pieceNotation = Pieces.GetPieceNotation(move.piece, move.isWhite);
            string startSquare = Move.SquareToString(start);
            string endSquare = Move.SquareToString(end);
            char columnStart = startSquare[0];
            char columnEnd = endSquare[0];
            char rowEnd = endSquare[1];

            string notation = "";

            if (move.piece == Pieces.King && end - start == 2)
                return "O-O";
            else if (move.piece == Pieces.King && end - start == -2)
                return "O-O-O";

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

                if (isEnpassant)
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
            moveHistory.Clear();
        }
        public void SetNotationType(bool useLong)
        {
            if (useLongNotation != useLong)
            {
                useLongNotation = useLong;
                RefreshNotationDisplay();
            }
        }

        public void RefreshNotationDisplay()
        {
            MessageBox.Show($"Refresh 1! {notationGrid.RowDefinitions.Count}");

            // Clear existing children and row definitions.
            notationGrid.Children.Clear();
            notationGrid.RowDefinitions.Clear();

            // Optionally, add a header row if needed. For this example, we start with no header.
            currentRow = 0;
            MessageBox.Show($"Refresh 2! {notationGrid.RowDefinitions.Count}");
            // Rebuild the UI using the current move history.
            bool isWhiteTurn = true;
            foreach (var moveEntry in moveHistory)
            {
                AddRowToTable(moveEntry, isWhiteTurn);
                isWhiteTurn = !isWhiteTurn;
            }

            // Force layout update.
            notationGrid.UpdateLayout();
            MessageBox.Show($"Refresh 3! {notationGrid.RowDefinitions.Count}");
        }

        public string GetLongNotation(MoveData move)
        {
            int start = move.move.From;
            int end = move.move.To;
            int pieceValue = move.piece;
            int pieceType = pieceValue & 7;
            string pieceNotation = pieceType == Pieces.Pawn ? "" : Pieces.PieceValueToString(pieceValue);

            string startSquare = Move.SquareToString(start);
            string endSquare = Move.SquareToString(end);

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

