using Chess.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Chess.Tools
{
    public static class Helpers
    {
        public static int BitScan(ulong bitboard)
        {
            for (int i = 0; i < 64; i++)
            {
                if ((bitboard & (1UL << i)) != 0)
                    return i;
            }
            return -1;
        }
        public static bool isPawnCaptureValid(int fromSquare, int toSquare, bool enemyIsWhite = false)
        {
            if (!isOnBoard(toSquare))
                return false;
            int fromFile = fromSquare % 8;
            int toFile = toSquare % 8;
            return Math.Abs(fromFile - toFile) == 1;
        }
        public static bool IsKingMoveInBounds(int from, int to)
        {
            if (!isOnBoard(to))
                return false;
            int fromRank = from / 8;
            int fromFile = from % 8;
            int toRank = to / 8;
            int toFile = to % 8;

            return Math.Abs(fromRank - toRank) <= 1 && Math.Abs(fromFile - toFile) <= 1;
        }
        public static bool isKnightMoveInBounds(int from, int to)
        {
            if (!isOnBoard(to))
                return false;
            int fromRank = from / 8;
            int fromFile = from % 8;
            int toRank = to / 8;
            int toFile = to % 8;

            int rankDiff = Math.Abs(fromRank - toRank);
            int fileDiff = Math.Abs(fromFile - toFile);

            return (rankDiff == 2 && fileDiff == 1) || (rankDiff == 1 && fileDiff == 2);
        }
        public static bool isSlidingMoveInBounds(int from, int target, int dir)
        {
            if (!isOnBoard(target))
                return false;
            int fromRank = from / 8;
            int fromFile = from % 8;
            int targetRank = target / 8;
            int targetFile = target % 8;

            if (dir == 1 || dir == -1)
                return fromRank == targetRank;
            if (dir == 9 || dir == -9 || dir == 7 || dir == -7)
                return Math.Abs(fromRank - targetRank) == Math.Abs(fromFile - targetFile);
            if (dir == 8 || dir == 4)
                return true;
            return true;
        }
        public static bool isOnBoard(int square)
        {
            return square >= 0 && square < 64;
        }
        /// <summary>
        /// Checks whether castling is valid.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="isWhite"></param>
        /// <returns>A boolean pair, with index 0 being queenside castling, and 1 being kingside castling.</returns>
        public static bool[] isCastlingValid(Chessboard board)
        {
            bool isWhite = board.isWhiteTurn;
            bool[] valid = { true, true };
            int[] castlingDirections = { -1, 1 };
            int kingSquare = isWhite ? BitScan(board.WhiteKing) : BitScan(board.BlackKing);

            if (isWhite && !board.WhiteKingMoved)
            {
                if (board.WhiteKingMoved)
                    return [false, false];
                    
                int[] rookPositions = { 0, 7 };
                for (int i = 0; i < 2; i++)
                {
                    if (board.RooksMoved[i])
                    { 
                        valid[i] = false;
                        continue;
                    }
                    int dir = castlingDirections[i];
                    int targetSquare = kingSquare;
                    while (true)
                    {
                        targetSquare += dir;
                        if (targetSquare == rookPositions[i])
                            break;
                        ulong mask = 1UL << targetSquare;
                        if ((board.AllPieces & mask) != 0)
                        {
                            valid[i] = false;
                            break;
                        }
                        if (!Moves.ValidateMove(board, new Move(kingSquare, targetSquare), isWhite))
                        {
                            valid[i] = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (board.BlackKingMoved)
                    return [false, false];

                int[] rookPositions = { 56, 63 };
                for (int i = 0; i < 2; i++)
                {
                    if (board.RooksMoved[i + 2])
                    {
                        valid[i] = false;
                        continue;
                    }
                    int dir = castlingDirections[i];
                    int targetSquare = kingSquare;
                    while (true)
                    {
                        targetSquare += dir;
                        if (targetSquare == rookPositions[i])
                            break;
                        ulong mask = 1UL << targetSquare;
                        if ((board.AllPieces & mask) != 0 && kingSquare != rookPositions[i])
                        {
                            valid[i] = false;
                            break;
                        }
                        if (!Moves.ValidateMove(board, new Move(kingSquare, targetSquare), isWhite))
                        {
                            valid[i] = false;
                            break;
                        }
                    }
                }
            }
            return valid;
        }
        public static bool isKingInCheck(Chessboard board, bool isWhite)
        {
            int kingSquare = BitScan(isWhite ? board.WhiteKing : board.BlackKing);
            if (kingSquare == -1)
                return false;

            if (isWhite)
            {
                if (isOnBoard(kingSquare + 7) && isPawnCaptureValid(kingSquare, kingSquare + 7, false))
                {
                    if ((board.BlackPawns & (1UL << (kingSquare + 7))) != 0)
                        return true;
                }
                if (isOnBoard(kingSquare + 9) && isPawnCaptureValid(kingSquare, kingSquare + 9, false))
                {
                    if ((board.BlackPawns & (1UL << (kingSquare + 9))) != 0)
                        return true;
                }
            }
            else
            {
                if (isOnBoard(kingSquare - 7) && isPawnCaptureValid(kingSquare, kingSquare - 7, true))
                {
                    if ((board.WhitePawns & (1UL << (kingSquare - 7))) != 0)
                        return true;
                }
                if (isOnBoard(kingSquare - 9) && isPawnCaptureValid(kingSquare, kingSquare - 9, true))
                {
                    if ((board.WhitePawns & (1UL << (kingSquare - 9))) != 0)
                        return true;
                }
            }

            int[] knightOffsets = Constants.KnightOffsets;
            foreach (int offset in knightOffsets)
            {
                int target = kingSquare + offset;
                if (!isKnightMoveInBounds(kingSquare, target))
                    continue;
                if (isWhite)
                {
                    if ((board.BlackKnights & (1UL << target)) != 0)
                        return true;
                }
                else
                {
                    if ((board.WhiteKnights & (1UL << target)) != 0)
                        return true;                        
                }
            }

            int[] rookDirections = Constants.RookDirections;
            foreach (int dir in rookDirections)
            {
                int target = kingSquare;
                while (true)
                {
                    target += dir;
                    if (!isSlidingMoveInBounds(kingSquare, target, dir))
                        break;
                    ulong targetMask = 1UL << target;
                    if ((board.AllPieces & targetMask) != 0)
                    {
                        if (isWhite)
                        {
                            if (((board.BlackRooks | board.BlackQueens) & targetMask) != 0)
                                return true;
                        }
                        else
                        {
                            if (((board.WhiteRooks | board.WhiteQueens) & targetMask) != 0)
                                return true;
                        }
                        break;
                    }
                }
            }

            int[] bishopDirections = Constants.BishopDirections;
            foreach (int dir in bishopDirections)
            {
                int target = kingSquare;
                while (true)
                {
                    target += dir;
                    if (!isSlidingMoveInBounds(kingSquare, target, dir))
                        break;
                    ulong targetMask = 1UL << target;
                    if ((board.AllPieces & targetMask) != 0)
                    {
                        if (isWhite)
                        {
                            if (((board.BlackBishops | board.BlackQueens) & targetMask) != 0)
                                return true;
                        }
                        else
                        {
                            if (((board.WhiteBishops | board.WhiteQueens) & targetMask) != 0)
                                return true;
                        }
                        break;
                    }
                }
            }

            int[] kingOffsets = Constants.KingOffsets;
            foreach (int offset in kingOffsets)
            {
                int target = kingSquare + offset;
                if (!IsKingMoveInBounds(kingSquare, target))
                    continue;
                if (isWhite)
                {
                    if ((board.BlackKing & (1UL << target)) != 0)
                        return true;
                }
                else
                {
                    if ((board.WhiteKing & (1UL << target)) != 0)
                        return true;
                }
            }

            return false;
        }
        public static List<int> GetPieceMoves(Chessboard board, int startSquare, bool isWhite)
        {
            List<int> targetSquares = new List<int>();
            List<Move> possibleList = new List<Move>();
            ulong startMask = 1UL << startSquare;
            if (isWhite)
            {
                if ((board.WhitePawns & startMask) != 0)
                {
                    possibleList = board.LegalMoves.WhitePawnMoves;
                }
                else if ((board.WhiteKnights & startMask) != 0)
                {
                    possibleList = board.LegalMoves.WhiteKnightMoves;
                }
                else if ((board.WhiteBishops & startMask) != 0)
                {
                    possibleList = board.LegalMoves.WhiteBishopMoves;
                }
                else if ((board.WhiteRooks & startMask) != 0)
                {
                    possibleList = board.LegalMoves.WhiteRookMoves;
                }
                else if ((board.WhiteQueens & startMask) != 0)
                {
                    possibleList = board.LegalMoves.WhiteQueenMoves;
                }
                else if ((board.WhiteKing & startMask) != 0)
                {
                    possibleList = board.LegalMoves.WhiteKingMoves;
                }
            }
            else
            {
                if ((board.BlackPawns & startMask) != 0)
                {
                    possibleList = board.LegalMoves.BlackPawnMoves;
                }
                else if ((board.BlackKnights & startMask) != 0)
                {
                    possibleList = board.LegalMoves.BlackKnightMoves;
                }
                else if ((board.BlackBishops & startMask) != 0)
                {
                    possibleList = board.LegalMoves.BlackBishopMoves;
                }
                else if ((board.BlackRooks & startMask) != 0)
                {
                    possibleList = board.LegalMoves.BlackRookMoves;
                }
                else if ((board.BlackQueens & startMask) != 0)
                {
                    possibleList = board.LegalMoves.BlackQueenMoves;
                }
                else if ((board.BlackKing & startMask) != 0)
                {
                    possibleList = board.LegalMoves.BlackKingMoves;
                }
            }

            foreach (Move move in possibleList)
            {
                if (move.From == startSquare)
                {
                    targetSquares.Add(move.To);
                }
            }
            return targetSquares;
        }
        public static int GetMoveCount(Chessboard board)
        {
            int moveCount = 0;
            LegalMoves moves = board.LegalMoves;
            if (board.isWhiteTurn)
            {
                moveCount += moves.WhitePawnMoves.Count;
                moveCount += moves.WhiteKnightMoves.Count;
                moveCount += moves.WhiteBishopMoves.Count;
                moveCount += moves.WhiteRookMoves.Count;
                moveCount += moves.WhiteQueenMoves.Count;
                moveCount += moves.WhiteKingMoves.Count;
            }
            else
            {
                moveCount += moves.BlackPawnMoves.Count;
                moveCount += moves.BlackKnightMoves.Count;
                moveCount += moves.BlackBishopMoves.Count;
                moveCount += moves.BlackRookMoves.Count;
                moveCount += moves.BlackQueenMoves.Count;
                moveCount += moves.BlackKingMoves.Count;
            }
            return moveCount;
        }
        #region Displaying the pieces
        public static char[] GetPieceArray(Chessboard board)
        {
            char[] pieces = new char[64];
            for (int target = 0 ; target < 64; target++)
            {
                ulong targetMask = 1UL << target;
                // White pieces
                if ((targetMask & board.WhitePawns) != 0)
                    pieces[target] = 'P';
                else if ((targetMask & board.WhiteKnights) != 0)
                    pieces[target] = 'N';
                else if ((targetMask & board.WhiteBishops) != 0)
                    pieces[target] = 'B';
                else if ((targetMask & board.WhiteRooks) != 0)
                    pieces[target] = 'R';
                else if ((targetMask & board.WhiteQueens) != 0)
                    pieces[target] = 'Q';
                else if ((targetMask & board.WhiteKing) != 0)
                    pieces[target] = 'K';
                // Black pieces
                else if ((targetMask & board.BlackPawns) != 0)
                    pieces[target] = 'p';
                else if ((targetMask & board.BlackKnights) != 0)
                    pieces[target] = 'n';
                else if ((targetMask & board.BlackBishops) != 0)
                    pieces[target] = 'b';
                else if ((targetMask & board.BlackRooks) != 0)
                    pieces[target] = 'r';
                else if ((targetMask & board.BlackQueens) != 0)
                    pieces[target] = 'q';
                else if ((targetMask & board.BlackKing) != 0)
                    pieces[target] = 'k';
            }
            return pieces;
        }
        public static Rectangle GeneratePiece(char pieceChar)
        {
            string resource = Pieces.ResourceNames[pieceChar];
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resource))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                ImageBrush brush = new ImageBrush(bitmap);
                Rectangle piece = new Rectangle();
                piece.Fill = brush;
                piece.Width = Constants.SquareSize;
                piece.Height = Constants.SquareSize;
                return piece;
            }
        }
        #endregion
    }
}
