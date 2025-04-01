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
        public int capturedPiece;
        public bool isWhite;
        public bool capture;
        public bool enPassant;
        public MoveData Clone()
        {
            MoveData copy = new MoveData();
            copy.move = new Move(this.move.From, this.move.To);
            copy.piece = this.piece;
            copy.capturedPiece = this.capturedPiece;
            copy.isWhite = this.isWhite;
            copy.capture = this.capture;
            copy.enPassant = this.enPassant;
            return copy;
        }
    }
    public class NotationPanelManager
    {
        private int currentRow = 0;
        private Grid notationGrid;
        private bool useLongNotation = false;
        private List<MoveData> moveHistory = new List<MoveData>();
        public NotationPanelManager(Grid grid, List<MoveData> movesMade)
        {
            notationGrid = grid;
            moveHistory = movesMade;
        }

        public void AddRowToTable(MoveData moveData, bool isWhiteTurn)
        {
            string sanNotation = GetAlgebraicNotation(moveData);
            string longNotation = GetLongNotation(moveData);

            if (!isWhiteTurn)
            {
                notationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                int rowIndex = currentRow + 1;

                TextBlock indexLabel = new TextBlock
                {
                    Text = (currentRow + 1).ToString() + ".",
                    Style = (Style)notationGrid.FindResource("NotationTextBlockStyle"),
                    Width = 52
                };
                Grid.SetRow(indexLabel, rowIndex);
                Grid.SetColumn(indexLabel, 0);
                notationGrid.Children.Add(indexLabel);

                TextBlock whiteMoveLabel = new TextBlock
                {
                    Text = useLongNotation ? longNotation : sanNotation,
                    Style = (Style)notationGrid.FindResource("NotationTextBlockStyle"),
                    Width = 78
                };
                Grid.SetRow(whiteMoveLabel, rowIndex);
                Grid.SetColumn(whiteMoveLabel, 1);
                notationGrid.Children.Add(whiteMoveLabel);

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
            else
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
        private void UpdateTextBlock(int row, int column, string text)
        {
            var textBlock = notationGrid.Children
                .OfType<TextBlock>()
                .FirstOrDefault(tb => Grid.GetRow(tb) == row && Grid.GetColumn(tb) == column);

            if (textBlock != null)
            {
                textBlock.Text = text;
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
            bool isWhiteTurn = true;
            int moveIndex = 0;
            var moveHistoryCopy = moveHistory.ToList(); 

            foreach (var moveEntry in moveHistoryCopy)
            {
                string notation = useLongNotation ? GetLongNotation(moveEntry) : GetAlgebraicNotation(moveEntry);
                int rowIndex = moveIndex / 2 + 1;

                if (isWhiteTurn && moveIndex % 2 == 0)
                {
                    if (notationGrid.RowDefinitions.Count <= rowIndex)
                    {
                        notationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        AddRowToTable(moveEntry, isWhiteTurn);
                    }
                    else
                    {
                        UpdateTextBlock(rowIndex, 1, notation);
                    }
                }
                else
                {
                    UpdateTextBlock(rowIndex, 2, notation);
                    currentRow = rowIndex - 1;
                }

                isWhiteTurn = !isWhiteTurn;
                moveIndex++;
            }

            notationGrid.UpdateLayout();
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

            if (pieceType == Pieces.King && Math.Abs(end - start) == 2)
            {
                return end > start ? "O-O" : "O-O-O"; 
            }

            string notation = pieceNotation + startSquare;
            if (move.capture || move.enPassant)
            {
                notation += "x";
            }
            notation += endSquare;

            if (move.enPassant)
            {
                notation += "(e.p.)";
            }

            return notation;
        }
    }
}

