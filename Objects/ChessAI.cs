using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Chess.Tools;

namespace Chess.Objects
{
    public class ChessAI
    {
        private int MaxDepth;
        private int FutilityMargin;
        private bool GrandmasterMode;
        private List<string> GameRecords;
        public ChessAI(int searchDepth, int playerColor, bool grandmaster = false, string grandmasterName = "", int futilityMargin = 100)
        {
            MaxDepth = searchDepth;
            GrandmasterMode = grandmaster;
            FutilityMargin = futilityMargin;
            if (GrandmasterMode)
                GameRecords = Helpers.GetPlayerRecords(grandmasterName, playerColor);
        }
        public async Task<Move> GetBestMove(Chessboard board, int aiColor)
        {
            if (GrandmasterMode)
            {
                string move = Moves.GetNextMove(board.CurrentMoves, GameRecords);
                if (move != "none")
                {
                    Move targetMove = Moves.GetMoveFromNotation(move, board);
                    return targetMove;
                }
            }

            int bestValue = int.MinValue;
            List<(Move move, int pieceValue)> bestMoves = new List<(Move, int)>(); // Lista ruchów z wartością figury
            object lockObj = new object();
            board.UpdateMoves();
            List<Move> moveset = board.LegalMoves.GetAllMoves();
            if (moveset.Count == 0)
                return new Move();

            for (int depth = 1; depth <= MaxDepth; depth++)
            {
                bestValue = int.MinValue;
                bestMoves.Clear();

                var tasks = moveset.Select(async move =>
                {
                    Chessboard clone = board.Clone();
                    clone.MakeMove(move);

                    int moveValue = await Task.Run(() =>
                        Minimax(clone, depth - 1, int.MinValue, int.MaxValue, false, aiColor));
                    Trace.WriteLine("MOVEVALUE: " + moveValue);
                    lock (lockObj)
                    {
                        int pieceValue = GetPieceValue(Helpers.GetPiece(board, move.From)); // Wartość figury wykonującej ruch
                        if (moveValue > bestValue)
                        {
                            bestValue = moveValue;
                            Trace.WriteLine("moveValue > bestValue" + moveValue + " " + bestValue);
                            bestMoves.Clear();
                            bestMoves.Add((move, pieceValue));
                        }
                        else if (moveValue == bestValue)
                        {
                            bestMoves.Add((move, pieceValue)); // Dodaj ruch o tej samej ocenie
                        }
                    }
                });

                await Task.WhenAll(tasks);
            }

            // Wybór najlepszego ruchu
            if (bestMoves.Count == 0)
                return new Move();
            if (bestMoves.Count == 1)
                return bestMoves[0].move;

            // Jeśli jest więcej niż jeden najlepszy ruch
            // 1. Szukamy ruchu z biciem i wybieramy najtańszą figurę
            var captureMoves = bestMoves.Where(m => Helpers.IsMoveCapture(board, m.move)).ToList();
            if (captureMoves.Any())
            {
                return captureMoves.OrderBy(m => m.pieceValue).First().move; // Najtańsza figura
            }

            // 2. Jeśli nie ma bicia, zwracamy dowolny (np. pierwszy)
            return bestMoves[0].move;
        }
        private int Minimax(Chessboard board, int depth, int alpha, int beta, bool maximizingPlayer, int aiColor)
        {
            board.UpdateMoves();

            if (depth == 0 || Helpers.GetMoveCount(board) == 0)
                return EvaluateBoard(board, aiColor);

            List<Move> moves = board.LegalMoves.GetAllMoves();
            moves.Sort((move1, move2) =>
            {
                int score1 = MoveScore(board, move1);
                int score2 = MoveScore(board, move2);
                return score2.CompareTo(score1);
            });
            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (var move in moves)
                {
                    MoveData data = board.MakeMove(move);
                    int eval = Minimax(board, depth - 1, alpha, beta, false, aiColor);
                    board.UnmakeMove(data);
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break;
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var move in moves)
                {
                    MoveData data = board.MakeMove(move);
                    int eval = Minimax(board, depth - 1, alpha, beta, true, aiColor);
                    board.UnmakeMove(data);
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                        break;
                }
                return minEval;
            }
        }

        public int EvaluateBoard(Chessboard board, int aiColor)
        {
            int evaluation = 0;

            // Stałe wartości figur i sytuacji
            const int PawnValue = 100;
            const int KnightValue = 320;
            const int BishopValue = 330;
            const int RookValue = 500;
            const int QueenValue = 900;
            const int KingValue = 20000;
            const int CheckMateValue = 100000; // Szach-mat
            const int StaleMateValue = 0;

            // Bonusy za ogólne aspekty pozycji (usunięto specyficzne priorytety z MoveScore)
            const int PawnStructureBonus = 10;
            const int MobilityBonus = 5;
            const int DevelopmentBonus = 10;

            // Ocena materiału
            int whiteMaterial = 0;
            int blackMaterial = 0;

            whiteMaterial += BitOperations.PopCount(board.WhitePawns) * PawnValue;
            whiteMaterial += BitOperations.PopCount(board.WhiteKnights) * KnightValue;
            whiteMaterial += BitOperations.PopCount(board.WhiteBishops) * BishopValue;
            whiteMaterial += BitOperations.PopCount(board.WhiteRooks) * RookValue;
            whiteMaterial += BitOperations.PopCount(board.WhiteQueens) * QueenValue;

            blackMaterial += BitOperations.PopCount(board.BlackPawns) * PawnValue;
            blackMaterial += BitOperations.PopCount(board.BlackKnights) * KnightValue;
            blackMaterial += BitOperations.PopCount(board.BlackBishops) * BishopValue;
            blackMaterial += BitOperations.PopCount(board.BlackRooks) * RookValue;
            blackMaterial += BitOperations.PopCount(board.BlackQueens) * QueenValue;

            evaluation += whiteMaterial - blackMaterial;

            // Ocena pozycji figur (piece-square tables)
            evaluation += EvaluatePiecePositions(board.WhitePawns, Constants.WhitePawnTable);
            evaluation += EvaluatePiecePositions(board.WhiteKnights, Constants.WhiteKnightTable);
            evaluation += EvaluatePiecePositions(board.WhiteBishops, Constants.WhiteBishopTable);
            evaluation += EvaluatePiecePositions(board.WhiteRooks, Constants.WhiteRookTable);
            evaluation += EvaluatePiecePositions(board.WhiteQueens, Constants.WhiteQueenTable);
            evaluation += EvaluatePiecePositions(board.WhiteKing, Constants.WhiteKingTable);

            evaluation -= EvaluatePiecePositions(board.BlackPawns, Constants.BlackPawnTable);
            evaluation -= EvaluatePiecePositions(board.BlackKnights, Constants.BlackKnightTable);
            evaluation -= EvaluatePiecePositions(board.BlackBishops, Constants.BlackBishopTable);
            evaluation -= EvaluatePiecePositions(board.BlackRooks, Constants.BlackRookTable);
            evaluation -= EvaluatePiecePositions(board.BlackQueens, Constants.BlackQueenTable);
            evaluation -= EvaluatePiecePositions(board.BlackKing, Constants.BlackKingTable);

            // Usunięto specyficzne priorytety (CaptureBonus, KingSafetyBonus, CastlingBonus, CenterControlBonus),
            // bo są teraz w MoveScore. Zostawiamy ogólne aspekty pozycji:

            // Struktura pionów
            evaluation += EvaluatePawnStructure(board.WhitePawns) * PawnStructureBonus;
            evaluation -= EvaluatePawnStructure(board.BlackPawns) * PawnStructureBonus;

            // Mobilność
            evaluation += (board.LegalMoves.GetAllMoves().Count(m => Helpers.GetPiece(board, m.From) != Pieces.White) -
                           board.LegalMoves.GetAllMoves().Count(m => Helpers.GetPiece(board, m.From) != Pieces.Black)) * MobilityBonus;

            // Dostosowanie dla koloru AI
            if (aiColor == Pieces.Black)
                evaluation = -evaluation;

            // Szach-mat lub pat (zachowane, bo to końcowa ocena pozycji)
            if (Helpers.GetMoveCount(board) == 0)
            {
                if (Helpers.isKingInCheck(board, board.isWhiteTurn))
                {
                    evaluation = board.isWhiteTurn == (aiColor == Pieces.White) ? -CheckMateValue : CheckMateValue;
                }
                else
                {
                    evaluation = StaleMateValue;
                }
            }

            return evaluation;
        }
        private int MoveScore(Chessboard board, Move move)
        {
            int score = 0;

            // Priorytet 1: Szach-mat
            Chessboard clone = board.Clone();
            clone.MakeMove(move);
            if (Helpers.GetMoveCount(clone) == 0 && Helpers.isKingInCheck(clone, !clone.isWhiteTurn))
            {
                score += 100000; // Najwyższy priorytet, zgodny z CheckMateValue w EvaluateBoard
                return score; // Wczesne zakończenie, bo szach-mat jest ostateczny
            }

            // Priorytet 2: Bicie
            int targetPiece = Helpers.GetPiece(board, move.To);
            if (targetPiece != 0)
            {
                switch (targetPiece)
                {
                    case Pieces.King: score += 10000; break; // Nie powinno się zdarzyć, ale zachowujemy
                    case Pieces.Queen: score += 900; break;
                    case Pieces.Rook: score += 500; break;
                    case Pieces.Bishop: score += 330; break;
                    case Pieces.Knight: score += 320; break;
                    case Pieces.Pawn: score += 100; break;
                }
                score += 2000; // Bonus za bicie
                Trace.WriteLine("Score za bicie: " + score);
            }

            // Priorytet 3: Obrona króla
            int kingPosBefore = board.isWhiteTurn ? BitOperations.TrailingZeroCount(board.WhiteKing) : BitOperations.TrailingZeroCount(board.BlackKing);
            int safetyBefore = KingShieldOptimized(board.isWhiteTurn ? board.WhitePawns : board.BlackPawns, kingPosBefore, board.isWhiteTurn);
            clone = board.Clone(); // Ponowne użycie klona
            clone.MakeMove(move);
            int kingPosAfter = clone.isWhiteTurn ? BitOperations.TrailingZeroCount(clone.WhiteKing) : BitOperations.TrailingZeroCount(clone.BlackKing);
            int safetyAfter = KingShieldOptimized(clone.isWhiteTurn ? clone.WhitePawns : clone.BlackPawns, kingPosAfter, clone.isWhiteTurn);
            if (safetyAfter > safetyBefore)
            {
                score += 100; // Bonus za poprawę bezpieczeństwa króla (zgodny z KingSafetyBonus)
                Trace.WriteLine("Score za obrone króla: " + score);
            }

            // Priorytet 4: Roszada
            if (IsCastlingMove(board, move))
            {
                score += 50; // Bonus za roszadę (zgodny z CastlingBonus)
                Trace.WriteLine("Score za roszade: " + score);
            }

            // Priorytet 5: Kontrola centrum
            const ulong CenterMask = (1UL << 27) | (1UL << 28) | (1UL << 35) | (1UL << 36); // d4, d5, e4, e5
            ulong moveToMask = 1UL << move.To;
            if ((CenterMask & moveToMask) != 0) // Czy ruch trafia do centrum
            {
                int centerControlBefore = EvaluateCenterControl(board, board.isWhiteTurn ? Pieces.White : Pieces.Black);
                int centerControlAfter = EvaluateCenterControl(clone, board.isWhiteTurn ? Pieces.White : Pieces.Black);
                if (centerControlAfter > centerControlBefore)
                {
                    score += 15; // Bonus za kontrolę centrum (zgodny z CenterControlBonus)
                    Trace.WriteLine("Score za centrum: " + score);
                }
            }

            return score;
        }
        private int EvaluatePiecePositions(ulong bitboard, int[] pieceSquareTable)
        {
            int evaluation = 0;
            ulong bitmask = 1;

            for (int index = 0; index < 64; index++)
            {
                if ((bitboard & bitmask) != 0)
                {
                    evaluation += pieceSquareTable[index];
                }
                bitmask <<= 1;
            }
            return evaluation;
        }
        private int EvaluateCenterControl(Chessboard board, int aiColor)
        {
            const ulong CenterMask = (1UL << 27) | (1UL << 28) | (1UL << 35) | (1UL << 36);

            int whiteControl = BitOperations.PopCount(
                (board.WhitePawns | board.WhiteKnights | board.WhiteBishops |
                 board.WhiteRooks | board.WhiteQueens) & CenterMask);

            int blackControl = BitOperations.PopCount(
                (board.BlackPawns | board.BlackKnights | board.BlackBishops |
                 board.BlackRooks | board.BlackQueens) & CenterMask);

            int control = whiteControl - blackControl;
            return aiColor == Pieces.White ? control : -control;
        }

        // Optimized king safety evaluation with cached king positions
        private int EvaluateKingSafety(Chessboard board, int aiColor)
        {
            int safety = 0;

            // Cache king positions
            int whiteKingPos = BitOperations.TrailingZeroCount(board.WhiteKing);
            int blackKingPos = BitOperations.TrailingZeroCount(board.BlackKing);

            // Precompute pawn shields
            safety += KingShieldOptimized(board.WhitePawns, whiteKingPos, true);
            safety -= KingShieldOptimized(board.BlackPawns, blackKingPos, false);

            return aiColor == Pieces.White ? safety : -safety;
        }
        private int GetPieceValue(int piece)
        {
            switch (piece)
            {
                case Pieces.Pawn: return 100;
                case Pieces.Knight: return 320;
                case Pieces.Bishop: return 330;
                case Pieces.Rook: return 500;
                case Pieces.Queen: return 900;
                case Pieces.King: return 20000;
                default: return 0;
            }
        }
        // Optimized pawn shield calculation
        private int KingShieldOptimized(ulong pawns, int kingPos, bool isWhite)
        {
            int rank = kingPos / 8;
            int file = kingPos % 8;

            if ((isWhite && rank <= 1) || (!isWhite && rank >= 6))
                return 0;

            int shieldRank = isWhite ? rank - 1 : rank + 1;
            ulong shieldMask = 0UL;

            if (file > 0) shieldMask |= 1UL << (shieldRank * 8 + (file - 1));
            shieldMask |= 1UL << (shieldRank * 8 + file);
            if (file < 7) shieldMask |= 1UL << (shieldRank * 8 + (file + 1));

            return BitOperations.PopCount(pawns & shieldMask) * 10;
        }

        // Optimized pawn structure evaluation using file counts
        private int EvaluatePawnStructure(ulong pawns)
        {
            Span<int> files = stackalloc int[8]; // Stack allocation for performance

            // Count pawns per file using bitwise operations
            ulong remainingPawns = pawns;
            while (remainingPawns != 0)
            {
                int pos = BitOperations.TrailingZeroCount(remainingPawns);
                files[pos % 8]++;
                remainingPawns &= remainingPawns - 1; // Clear least significant set bit
            }

            int score = 0;
            for (int file = 0; file < 8; file++)
            {
                int count = files[file];
                if (count == 0) continue;

                // Penalize doubled pawns
                if (count > 1) score -= 10 * (count - 1);

                // Reward connected pawns
                if (file > 0 && files[file - 1] > 0) score += 5;
                if (file < 7 && files[file + 1] > 0) score += 5;
            }

            return score;
        }
        private bool IsCastlingMove(Chessboard board, Move move)
        {
            int kingSquare = board.isWhiteTurn ? Helpers.BitScan(board.WhiteKing) : Helpers.BitScan(board.BlackKing);
            if (move.From != kingSquare || Helpers.GetPiece(board, kingSquare) != Pieces.King)
                return false;

            int distance = Math.Abs(move.To - move.From);
            return distance == 2; // Roszada krótka (0-0) lub długa (0-0-0) przesuwa króla o 2 pola
        }
    }
}