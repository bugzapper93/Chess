using Chess.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Chess.Objects
{
    public struct Move
    {
        public int From;
        public int To;

        public Move(int from, int to)
        {
            From = from;
            To = to;
        }
        public static string SquareToString(int square)
        {
            int file = square % 8;
            int rank = square / 8;
            return $"{(char)('a' + file)}{rank + 1}";
        }
        public static int SquareStringToIndex(string square)
        {
            int file = square[0] - 'a';
            int rank = square[1] - '1';
            return rank * 8 + file;
        }
    }
    public class GameRecord
    {
        public string white { get; set; }
        public string black { get; set; }
        public string result { get; set; }
        public List<string> moves { get; set; }
    }
    public struct GrandmasterMoveset
    {
        public List<List<Move>> moves;
    }
    public static class Moves
    {
        public static LegalMoves GenerateLegalMoves(Chessboard board)
        {
            LegalMoves legalMoves = new LegalMoves();
            bool isWhite = board.isWhiteTurn;

            foreach (Move move in GenerateKingMoves(board, isWhite))
            {
                if (ValidateMove(board, move, isWhite))
                {
                    if (isWhite)
                        legalMoves.WhiteKingMoves.Add(move);
                    else
                        legalMoves.BlackKingMoves.Add(move);
                }
            }

            foreach (Move move in GeneratePawnMoves(board, isWhite))
            {
                if (ValidateMove(board, move, isWhite))
                {
                    if (isWhite)
                        legalMoves.WhitePawnMoves.Add(move);
                    else
                        legalMoves.BlackPawnMoves.Add(move);
                }
            }

            foreach (Move move in GenerateKnightMoves(board, isWhite))
            {
                if (ValidateMove(board, move, isWhite))
                {
                    if (isWhite)
                        legalMoves.WhiteKnightMoves.Add(move);
                    else
                        legalMoves.BlackKnightMoves.Add(move);
                }
            }

            foreach (Move move in GenerateBishopMoves(board, isWhite))
            {
                if (ValidateMove(board, move, isWhite))
                {
                    if (isWhite)
                        legalMoves.WhiteBishopMoves.Add(move);
                    else
                        legalMoves.BlackBishopMoves.Add(move);
                }
            }

            foreach (Move move in GenerateRookMoves(board, isWhite))
            {
                if (ValidateMove(board, move, isWhite))
                {
                    if (isWhite)
                        legalMoves.WhiteRookMoves.Add(move);
                    else
                        legalMoves.BlackRookMoves.Add(move);
                }
            }

            foreach (Move move in GenerateQueenMoves(board, isWhite))
            {
                if (ValidateMove(board, move, isWhite))
                {
                    if (isWhite)
                        legalMoves.WhiteQueenMoves.Add(move);
                    else
                        legalMoves.BlackQueenMoves.Add(move);
                }
            }
            
            return legalMoves;
        }

        public static bool ValidateMove(Chessboard board, Move move, bool isWhite)
        {
            Chessboard boardClone = board.Clone();
            boardClone.ValidatingMove = true;
            boardClone.MakeMove(move);
            boardClone.ValidatingMove = false;
            return !Helpers.isKingInCheck(boardClone, isWhite);
        }
       
        private static List<Move> GeneratePawnMoves(Chessboard board, bool isWhite)
        {
            List<Move> moves = new List<Move>();

            ulong allPieces = board.AllPieces;
            ulong whitePieces = board.AllWhitePieces;
            ulong blackPieces = board.AllBlackPieces;

            ulong pawns = isWhite ? board.WhitePawns : board.BlackPawns;
            if (isWhite)
            {
                while (pawns != 0)
                {
                    int fromSquare = Helpers.BitScan(pawns);
                    pawns &= pawns - 1;

                    int toSquare = fromSquare + 8;
                    if (toSquare < 64 && (allPieces & (1UL << toSquare)) == 0)
                    {
                        moves.Add(new Move(fromSquare, toSquare));

                        if (fromSquare >= 8 && fromSquare < 16)
                        {
                            int doubleSquare = fromSquare + 16;
                            if ((allPieces & (1UL << doubleSquare)) == 0)
                                moves.Add(new Move(fromSquare, doubleSquare));
                        }
                    }

                    int[] captureOffsets = { 7, 9 };
                    foreach (int offset in captureOffsets)
                    {
                        int captureSquare = fromSquare + offset;
                        if (captureSquare < 64 && Helpers.isPawnCaptureValid(fromSquare, captureSquare, false))
                        {
                            if ((blackPieces & (1UL << captureSquare)) != 0)
                            {
                                moves.Add(new Move(fromSquare, captureSquare));
                            }
                            if (board.EnPassantSquare != null && captureSquare == board.EnPassantSquare)
                            {
                                if ((allPieces & (1UL << captureSquare)) == 0)
                                    moves.Add(new Move(fromSquare, captureSquare));
                            }
                        }
                    }
                }
            }
            else
            {
                while (pawns != 0)
                {
                    int fromSquare = Helpers.BitScan(pawns);
                    pawns &= pawns - 1;

                    int toSquare = fromSquare - 8;
                    if (toSquare >= 0 && (allPieces & (1UL << toSquare)) == 0)
                    {
                        moves.Add(new Move(fromSquare, toSquare));

                        if (fromSquare >= 48 && fromSquare < 56)
                        {
                            int doubleSquare = fromSquare - 16;
                            if ((allPieces & (1UL << doubleSquare)) == 0)
                                moves.Add(new Move(fromSquare, doubleSquare));
                        }
                    }
                    int[] captureOffsets = { -7, -9 };
                    foreach (int offset in captureOffsets)
                    {
                        int captureSquare = fromSquare + offset;
                        if (captureSquare >= 0 && Helpers.isPawnCaptureValid(fromSquare, captureSquare, true))
                        {
                            if ((whitePieces & (1UL << captureSquare)) != 0)
                            {
                                moves.Add(new Move(fromSquare, captureSquare));
                            }
                            if (board.EnPassantSquare != null && captureSquare == board.EnPassantSquare)
                            {
                                if ((allPieces & (1UL << captureSquare)) == 0)
                                    moves.Add(new Move(fromSquare, captureSquare));
                            }
                        }
                    }
                }
            }
            return moves;
        }
        private static List<Move> GenerateKingMoves(Chessboard board, bool isWhite)
        {
            List<Move> moves = new List<Move>();
            ulong kingTP = isWhite ? board.WhiteKing : board.BlackKing;
            int kingSquare = Helpers.BitScan(kingTP);
            if (kingSquare == -1)
                return moves;

            int[] offsets = Constants.KingOffsets;

            foreach (int offset in offsets)
            {
                int target = kingSquare + offset;
                if (target < 0 || target >= 64)
                    continue;
                if (!Helpers.IsKingMoveInBounds(kingSquare, target))
                    continue;
                ulong ownPieces = isWhite ? board.AllWhitePieces : board.AllBlackPieces;
                if ((ownPieces & (1UL << target)) != 0)
                    continue;

                moves.Add(new Move(kingSquare, target));
            }

            bool[] castling = Helpers.isCastlingValid(board);
            if (castling[0])
                moves.Add(new Move(kingSquare, kingSquare - 2));
            if (castling[1])
                moves.Add(new Move(kingSquare, kingSquare + 2));
            return moves;
        }
        private static List<Move> GenerateKnightMoves(Chessboard board, bool isWhite)
        {
            List<Move> moves = new List<Move>();
            ulong knights = isWhite ? board.WhiteKnights : board.BlackKnights;
            while (knights != 0)
            {
                int fromSquare = Helpers.BitScan(knights);
                knights &= knights - 1;
                int[] offsets = Constants.KnightOffsets;
                foreach (int offset in offsets)
                {
                    int target = fromSquare + offset;
                    if (target < 0 || target >= 64)
                        continue;
                    if (!Helpers.isKnightMoveInBounds(fromSquare, target))
                        continue;
                    ulong targetMask = 1UL << target;
                    ulong ownPieces = isWhite ? board.AllWhitePieces : board.AllBlackPieces;
                    if ((targetMask & ownPieces) != 0)
                        continue;

                    moves.Add(new Move(fromSquare, target));
                }
            }
            return moves;
        }
        private static List<Move> GenerateBishopMoves(Chessboard board, bool isWhite)
        {
            List<Move> moves = new List<Move>();
            ulong bishops = isWhite ? board.WhiteBishops : board.BlackBishops;
            while (bishops != 0)
            {
                int fromSquare = Helpers.BitScan(bishops);
                bishops &= bishops - 1;
                int[] directions = Constants.BishopDirections;
                moves.AddRange(GenerateSlidingMoves(board, fromSquare, directions, isWhite));
            }
            return moves;
        }
        private static List<Move> GenerateRookMoves(Chessboard board, bool isWhite)
        {
            List<Move> moves = new List<Move>();
            ulong rooks = isWhite ? board.WhiteRooks : board.BlackRooks;
            while (rooks != 0)
            {
                int fromSquare = Helpers.BitScan(rooks);
                rooks &= rooks - 1;
                int[] directions = Constants.RookDirections;
                moves.AddRange(GenerateSlidingMoves(board, fromSquare, directions, isWhite));
            }
            return moves;
        }
        private static List<Move> GenerateQueenMoves(Chessboard board, bool isWhite)
        {
            List<Move> moves = new List<Move>();
            ulong queens = isWhite ? board.WhiteQueens : board.BlackQueens;
            while (queens != 0)
            {
                int fromSquare = Helpers.BitScan(queens);
                queens &= queens - 1;
                int[] directions = Constants.QueenDirections;
                moves.AddRange(GenerateSlidingMoves(board, fromSquare, directions, isWhite));
            }
            return moves;
        }
        private static List<Move> GenerateSlidingMoves(Chessboard board, int from, int[] directions, bool isWhite)
        {
            List<Move> moves = new List<Move>();
            foreach (int direction in directions)
            {
                int target = from;
                while (true)
                {
                    target += direction;
                    if (target < 0 || target > 64) 
                        break;
                    if (!Helpers.isSlidingMoveInBounds(from, target, direction))
                        break;
                    ulong targetMask = 1UL << target;
                    if ((board.AllPieces & targetMask) == 0)
                    {
                        moves.Add(new Move(from, target));
                    }
                    else
                    {
                        if (isWhite && ((board.AllBlackPieces) & targetMask) != 0)
                            moves.Add(new Move(from, target));
                        else if (!isWhite && (board.AllWhitePieces & targetMask) != 0)
                            moves.Add(new Move(from, target));
                        break;
                    }
                }
            }
            return moves;
        }
        public static string GetNextMove(string currentMoves, List<string> grandmasterMoves)
        {
            int index = grandmasterMoves.FindIndex(move => move.Contains(currentMoves));

            MessageBox.Show(index.ToString());

            MessageBox.Show(currentMoves);
            MessageBox.Show($"{grandmasterMoves[index]}");

            List<string> fullMoveList = grandmasterMoves[index].Split(',').ToList();
            List<string> currentMoveList = currentMoves.Split(',').ToList();

            //MessageBox.Show($"{currentMoves} {grandmasterMoves}");

            int startIndex = -1;
            for (int i = 0; i <= fullMoveList.Count - currentMoveList.Count; i++)
            {
                if (fullMoveList.Skip(i).Take(currentMoveList.Count).SequenceEqual(currentMoveList))
                {
                    startIndex = i;
                    break;
                }
            }
            if (startIndex != -1 && startIndex + currentMoveList.Count < fullMoveList.Count)
            {
                return fullMoveList[startIndex + currentMoveList.Count];
            }
            else
            {
                return "none";
            }
        }

        public static Move GetMoveFromNotation(string notation, Chessboard board)
        {
            notation = notation.Replace("+", "").Replace("#", "");

            char pieceChar = 'P';
            int pos = 0;
            if (notation.Length > 0 && "NBRQK".Contains(notation[0]))
            {
                pieceChar = notation[0];
                pos = 1;
            }

            string destSquareStr = notation.Substring(notation.Length - 2);
            int destSquare = Move.SquareStringToIndex(destSquareStr);

            string disambiguation = notation.Substring(pos, notation.Length - pos - 2);

            List<Move> allMoves = board.LegalMoves.GetAllMoves();
            List<Move> candidates = new List<Move>();
            foreach (var move in allMoves)
            {
                if (move.To == destSquare)
                {
                    if (Helpers.GetPieceAt(move.From, board) == pieceChar)
                    {
                        string fromSquareStr = Move.SquareToString(move.From);
                        if (string.IsNullOrEmpty(disambiguation) || fromSquareStr.Contains(disambiguation))
                        {
                            candidates.Add(move);
                        }
                    }
                }
            }
            if (candidates.Count == 1)
            {
                return candidates[0];
            }
            else if (candidates.Count == 0)
            {
                throw new Exception("No legal move found for notation: " + notation);
            }
            else
            {
                throw new Exception("Ambiguous move notation: " + notation);
            }
        }
    }
}