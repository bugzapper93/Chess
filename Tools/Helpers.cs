using Chess.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            if (!IsPositionValid(startPos) || !IsPositionValid(endPos))
                return false;
            int rowDiff = endPos.row - startPos.row;
            int colDiff = endPos.column - startPos.column;
            int rowDir = rowDiff == 0 ? 0 : rowDiff / Math.Abs(rowDiff);
            int colDir = colDiff == 0 ? 0 : colDiff / Math.Abs(colDiff);
            Position currentPos = new Position(startPos.row + rowDir, startPos.column + colDir);
            while (currentPos != endPos)
            {
                if (!IsPositionValid(currentPos))
                    return false;
                if (pieces[currentPos.row, currentPos.column].value != 0)
                    return false;
                currentPos.row += rowDir;
                currentPos.column += colDir;
            }
            return true;
        }
        private static bool IsPositionValid(Position pos)
        {
            return pos.row >= 0 && pos.row < 8 && pos.column >= 0 && pos.column < 8;
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
        public static bool ValidForAllChecks(Position startPos, Position endPos, List<Check> checks)
        {
            foreach (Check check in checks)
            {
                if (!ColinearPaths(startPos, endPos, check.checkingPiece) || !IsBetween(startPos, check.checkingPiece, endPos))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool CheckPathCheck(Position startPos, Position endPos, Chessboard board)
        {
            int currentColor = board.pieces[startPos.row, startPos.column].value & 24;

            int rowDiff = endPos.row - startPos.row;
            int colDiff = endPos.column - startPos.column;

            int rowDir = rowDiff == 0 ? 0 : rowDiff / Math.Abs(rowDiff);
            int colDir = colDiff == 0 ? 0 : colDiff / Math.Abs(colDiff);

            int step = 1;
            while (true)
            {
                Position currentPos = new Position(startPos.row + step * rowDir, startPos.column + step * colDir);
                if (currentPos == endPos)
                {
                    break;
                }
                if (currentColor == Pieces.White && board.squares[currentPos.row, currentPos.column].dangerBlack)
                    return false;
                if (currentColor == Pieces.Black && board.squares[currentPos.row, currentPos.column].dangerWhite)
                    return false;
                step++;
            }
            return true;
        }
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
    }
}
