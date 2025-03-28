﻿using Chess.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Chess.Tools
{
    public static class Helpers
    {
        public static Rectangle CreatePiece(int value)
        {
            char pieceChar = Pieces.GetPieceChar(value);
            string resourceName = Pieces.ResourceNames[pieceChar];

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                ImageBrush brush = new ImageBrush(bitmap);
                Rectangle piece = new Rectangle();
                piece.Fill = brush;
                piece.Width = Constants.Square_Size;
                piece.Height = Constants.Square_Size;
                return piece;
            }
        }
        public static bool InBounds(Position position)
        {
            return position.row >= 0 && position.row < 8 && position.column >= 0 && position.column < 8;
        }
        public static int OccupationType(Position position, Piece[,] pieces)
        {
            int row = position.row;
            int column = position.column;
            return pieces[row, column].value & 24;
        }
        public static int GetMoveIndex(List<Move> move, Position startPos, Position endPos)
        {
            for (int i = 0; i < move.Count; i++)
            {
                if (move[i].startPosition == startPos && move[i].targetPosition == endPos)
                {
                    return i;
                }
            }
            return -1;
        }
        public static bool CheckPathClear(Position startPos, Position endPos, Piece[,] pieces)
        {
            int rowDiff = endPos.row - startPos.row;
            int colDiff = endPos.column - startPos.column;
            int rowDir = rowDiff == 0 ? 0 : rowDiff / Math.Abs(rowDiff);
            int colDir = colDiff == 0 ? 0 : colDiff / Math.Abs(colDiff);
            Position currentPos = new Position(startPos.row + rowDir, startPos.column + colDir);
            while (currentPos != endPos)
            {
                if (!InBounds(currentPos))
                    return false;
                if (pieces[currentPos.row, currentPos.column].value != 0)
                    return false;
                currentPos.row += rowDir;
                currentPos.column += colDir;
            }
            return true;
        }
        public static List<Pin> GetAllPiecePins(Position position, List<Pin> pins)
        {
            List<Pin> piecePins = new List<Pin>();
            foreach (Pin pin in pins)
            {
                if (pin.pinned == position)
                {
                    piecePins.Add(pin);
                }
            }
            return piecePins;
        }
        public static bool ColinearPaths(Position startPos, Position endPos, Position pin)
        {
            int moveRow = endPos.row - startPos.row;
            int moveCol = endPos.column - startPos.column;
            int pinRow = pin.row - startPos.row;
            int pinCol = pin.column - startPos.column;

            return (moveRow * pinCol == moveCol * pinRow);
        }
        /// <summary>
        /// Checks if the path between two positions is inline
        /// </summary>
        /// <param name="piecePos">First position</param>
        /// <param name="targetPos">Second Position</param>
        /// <param name="type">Whether to check diagonals, or straight lines, or both. 0 - both, 1 - diagonals, 3 - straight lines</param>
        /// <returns>true if the two positions are in-line</returns>
        public static bool IsInline(Position piecePos, Position targetPos, int type = 0)
        {
            int rowDiff = targetPos.row - piecePos.row;
            int colDiff = targetPos.column - piecePos.column;
            if (type == 0)
            {
                return rowDiff == 0 || colDiff == 0 || Math.Abs(rowDiff) == Math.Abs(colDiff);
            }
            if (type == 1)
            {
                return Math.Abs(rowDiff) == Math.Abs(colDiff);
            }
            if (type == 2)
            {
                return rowDiff == 0 || colDiff == 0;
            }
            return false;
        }
        public static bool IsBetween(Position start, Position checking, Position target)
        {
            int vRow = target.row - start.row;
            int vCol = target.column - start.column;
            int wRow = checking.row - start.row;
            int wCol = checking.column - start.column;

            int dot = vRow * wRow + vCol * wCol;
            int wSquared = wRow * wRow + wCol * wCol;

            return dot >= 0 && dot <= wSquared;
        }
        #region Handling king movement validation
        public static bool ValidForAllChecks(Position startPos, Position endPos, List<Check> checks)
        {
            foreach (Check check in checks)
            {
                if (!IsInline(startPos, check.checkingPiece) || !IsBetween(startPos, check.checkingPiece, endPos))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool CheckValidKingMove(int currentColor, Move move, Piece[,] pieces, List<Position> slidingPieces, bool check = false)
        {
            bool validMove = true;
            Position startPosition = move.startPosition;
            Position checkPosition = move.targetPosition;

            // Unable to move into a knight attack
            foreach (Position direction in Constants.KnightDirections)
            {
                Position position = checkPosition + direction;
                if (Helpers.InBounds(position) && pieces[position.row, position.column].value == (Pieces.Knight | Pieces.GetOppositeColor(currentColor)))
                {
                    validMove = false;
                    break;
                }
            }

            // Unable to move into a sliding attack
            foreach (Position slidingPiece in slidingPieces)
            {
                int attackerType = pieces[slidingPiece.row, slidingPiece.column].value & 7;
                int type = 0;
                if (attackerType == Pieces.Bishop)
                    type = 1;
                else if (attackerType == Pieces.Rook)
                    type = 2;
                if (check)
                {
                    if ((Helpers.IsInline(checkPosition, slidingPiece, type) || Helpers.IsInline(checkPosition, startPosition, type)) && checkPosition != slidingPiece)
                    {
                        validMove = false;
                        break;
                    }
                }
                else if (Helpers.IsInline(checkPosition, slidingPiece, type) && checkPosition != slidingPiece && Helpers.CheckPathClear(checkPosition, slidingPiece, pieces))
                {
                    validMove = false;
                    break;
                }
            }

            // Unable to move into a pawn attack
            int moveDirection = currentColor == Pieces.White ? -1 : 1;
            int[] cols = { -1, 1 };
            foreach (int col in cols)
            {
                Position pawnPos = new Position(checkPosition.row + moveDirection, checkPosition.column + col);
                if (!Helpers.InBounds(pawnPos))
                    continue;
                if (pieces[pawnPos.row, pawnPos.column].value == (Pieces.Pawn | Pieces.GetOppositeColor(currentColor)))
                {
                    validMove = false;
                    break;
                }
            }

            if (validMove)
                return true;
            return false;
        }
        #endregion
        #region Handling edge cases
        public static bool CheckEnPassant(Position startPos, Position endPos, Chessboard board)
        {
            if (board.enPassantTarget == null)
            {
                return false;
            }

            Position targetPos = board.enPassantTarget.Value;
            if (endPos.row != targetPos.row || endPos.column != targetPos.column)
            {
                return false;
            }

            Position capturedPawnPos = new Position(startPos.row, endPos.column);
            Piece capturedPawn = board.pieces[capturedPawnPos.row, capturedPawnPos.column];
            if ((capturedPawn.value & 7) != Pieces.Pawn)
            {
                return false;
            }
            if ((capturedPawn.value & 24) == (board.pieces[startPos.row, startPos.column].value & 24))
            {
                return false;
            }
            return true;
        }
        #endregion
/*        public static Position NotationToMove(string notation, Piece[,] pieces)
        {

        }*/
    }
}
