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
using System.Text.Json;
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
        #region Getting piece and move data
        public static int GetPiece(Chessboard board, int square)
        {
            ulong mask = 1UL << square;
            if (((board.WhitePawns | board.BlackPawns) & mask) != 0)
                return Pieces.Pawn;
            if (((board.WhiteKnights | board.BlackKnights) & mask) != 0)
                return Pieces.Knight;
            if (((board.WhiteBishops | board.BlackBishops) & mask) != 0)
                return Pieces.Bishop;
            if (((board.WhiteRooks | board.BlackRooks) & mask) != 0)
                return Pieces.Rook;
            if (((board.WhiteQueens | board.BlackQueens) & mask) != 0)
                return Pieces.Queen;
            if (((board.WhiteKing | board.BlackKing) & mask) != 0)
                return Pieces.King;
            return 0;
        }
        public static int GetPieceColor(Chessboard board, int square)
        {
            ulong mask = 1UL << square;
            if ((board.AllWhitePieces & mask) != 0)
                return Pieces.White;
            else if ((board.AllBlackPieces & mask) != 0)
                return Pieces.Black;
            return 0;
        }
        public static char GetPieceAt(int square, Chessboard board)
        {
            ulong mask = 1UL << square;
            if (board.isWhiteTurn)
            {
                if ((board.WhitePawns & mask) != 0) return 'P';
                if ((board.WhiteKnights & mask) != 0) return 'N';
                if ((board.WhiteBishops & mask) != 0) return 'B';
                if ((board.WhiteRooks & mask) != 0) return 'R';
                if ((board.WhiteQueens & mask) != 0) return 'Q';
                if ((board.WhiteKing & mask) != 0) return 'K';
            }
            else
            {
                if ((board.BlackPawns & mask) != 0) return 'P';
                if ((board.BlackKnights & mask) != 0) return 'N';
                if ((board.BlackBishops & mask) != 0) return 'B';
                if ((board.BlackRooks & mask) != 0) return 'R';
                if ((board.BlackQueens & mask) != 0) return 'Q';
                if ((board.BlackKing & mask) != 0) return 'K';
            }
            return ' ';
        }
        public static bool IsMoveCapture(Chessboard board, Move move)
        {
            ulong mask = 1UL << move.To;
            if ((board.AllPieces & mask) != 0)
                return true;
            return false;
        }
        private static int BitScanForward(ulong bitboard)
        {
            if (bitboard == 0) return -1;
            int index = 0;
            while ((bitboard & 1) != 0)
            {
                bitboard >>= 1;
                index++;
            }
            return index;
        }
        public static ulong ComputePieceHash(ulong bitboard, int index)
        {
            ulong hash = 0;
            ulong bitboardCopy = bitboard;
            while (bitboardCopy != 0)
            {
                int square = BitScanForward(bitboardCopy);
                hash ^= Constants.ZobristTable[index, square];
                bitboardCopy &= bitboardCopy - 1;
            }
            return hash;
        }
        #endregion
        #region Move validation
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

            if (isKingInCheck(board, isWhite))
                return [false, false];

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
        #endregion
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
        #region Grandmaster mode helpers
        /// <summary>
        /// Function for getting all the game records of a chosen grandmaster
        /// </summary>
        /// <param name="grandmasterName"></param>
        /// <returns></returns>
        public static List<string> GetPlayerRecords(string grandmasterName, int playerColor)
        {
            string path = Constants.Grandmasters[grandmasterName];
            string json = File.ReadAllText(path);

            var games = JsonSerializer.Deserialize<List<GameRecord>>(json);

            if (games == null)
                return new List<string>();

            string[] temp = grandmasterName.Split(" ");
            string grandmasterNameFormatted = $"{temp[1]}, {temp[0]}";

            List<string> wonGames = new List<string>();
            List<string> lostGames = new List<string>();
            List<string> stalemates = new List<string>();

            for (int i = 0; i < games.Count; i++)
            {
                var game = games[i];
                
                if (playerColor == Pieces.Black && game.white == grandmasterNameFormatted)
                {
                    if (game.result == "1-0")
                        wonGames.Add(string.Join(",", game.moves));
                    else if (game.result == "1-1")
                        stalemates.Add(string.Join(",", game.moves));
                    else
                        lostGames.Add(string.Join(",", game.moves));
                }
                else if (playerColor == Pieces.White && game.black == grandmasterNameFormatted)
                {
                    if (game.result == "0-1")
                        wonGames.Add(string.Join(",", game.moves));
                    else if (game.result == "1-1")
                        stalemates.Add(string.Join(",", game.moves));
                    else
                        lostGames.Add(string.Join(",", game.moves));
                }
            }

            List<string> allMoves = new List<string>();
            allMoves.AddRange(wonGames);
            allMoves.AddRange(stalemates);
            allMoves.AddRange(lostGames);
            return allMoves;
        }
        #endregion
    }
}
