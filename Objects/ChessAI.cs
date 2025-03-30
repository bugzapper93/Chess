using System;
using System.Collections.Generic;
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
            Move bestMove = new Move();
            object lockObj = new object();
            board.UpdateMoves();
            List<Move> moveset = board.LegalMoves.GetAllMoves();
            if (moveset.Count == 0)
                return bestMove;

            for (int depth = 1; depth <= MaxDepth; depth++)
            {
                var tasks = moveset.Select(async move =>
                {
                    Chessboard clone = board.Clone();
                    clone.MakeMove(move);

                    int moveValue = await Task.Run(() =>
                        Minimax(clone, depth - 1, int.MinValue, int.MaxValue, false, aiColor));

                    lock (lockObj)
                    {
                        if (moveValue > bestValue)
                        {
                            bestValue = moveValue;
                            bestMove = move;
                        }
                    }
                });

                await Task.WhenAll(tasks);
            }
            return bestMove;
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

        private int EvaluateBoard(Chessboard board, int aiColor)
        {
            int evaluation = 0;

            const int PawnValue = 100;
            const int KnightValue = 320;
            const int BishopValue = 330;
            const int RookValue = 500;
            const int QueenValue = 900;
            const int KingValue = 20000;
            const int CheckMateValue = 100000; // Added constant
            const int StaleMateValue = 0;

            const int CenterControlBonus = 15;
            const int KingSafetyBonus = 20;
            const int PawnStructureBonus = 10;
            const int MobilityBonus = 5;
            const int DevelopmentBonus = 10;

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

            evaluation += EvaluateCenterControl(board, aiColor) * CenterControlBonus;
            evaluation += EvaluateKingSafety(board, aiColor) * KingSafetyBonus;

            // Pawn structure evaluation
            evaluation += EvaluatePawnStructure(board.WhitePawns) * PawnStructureBonus;
            evaluation -= EvaluatePawnStructure(board.BlackPawns) * PawnStructureBonus;

            evaluation += (board.LegalMoves.GetAllMoves().Count(m => Helpers.GetPiece(board, m.From) != Pieces.White) -
                          board.LegalMoves.GetAllMoves().Count(m => Helpers.GetPiece(board, m.From) != Pieces.Black)) * MobilityBonus;

            if (aiColor == Pieces.Black)
                evaluation = -evaluation;

            if (Helpers.GetMoveCount(board) == 0)
            {
                if (Helpers.isKingInCheck(board, board.isWhiteTurn))
                {
                    evaluation = board.isWhiteTurn == (aiColor == Pieces.White)
                        ? -CheckMateValue
                        : CheckMateValue;
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
            int targetPiece = Helpers.GetPiece(board, move.To);
            if (targetPiece == 0)
                return score;
            switch (targetPiece)
            {
                case Pieces.King: score += 10000; break;
                case Pieces.Queen: score += 900; break;
                case Pieces.Rook: score += 500; break;
                case Pieces.Bishop: score += 330; break;
                case Pieces.Knight: score += 320; break;
                case Pieces.Pawn: score += 100; break;
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
    }
}