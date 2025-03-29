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
            const int KingValue = 10000;
            const int CheckMateValue = 100000;
            const int StaleMateValue = 0;

            int whitePawns = BitOperations.PopCount(board.WhitePawns);
            int whiteKnights = BitOperations.PopCount(board.WhiteKnights);
            int whiteBishops = BitOperations.PopCount(board.WhiteBishops);
            int whiteRooks = BitOperations.PopCount(board.WhiteRooks);
            int whiteQueens = BitOperations.PopCount(board.WhiteQueens);
            int whiteKing = BitOperations.PopCount(board.WhiteKing);

            int blackPawns = BitOperations.PopCount(board.BlackPawns);
            int blackKnights = BitOperations.PopCount(board.BlackKnights);
            int blackBishops = BitOperations.PopCount(board.BlackBishops);
            int blackRooks = BitOperations.PopCount(board.BlackRooks);
            int blackQueens = BitOperations.PopCount(board.BlackQueens);
            int blackKing = BitOperations.PopCount(board.BlackKing);

            evaluation += whitePawns * PawnValue;
            evaluation += whiteKnights * KnightValue;
            evaluation += whiteBishops * BishopValue;
            evaluation += whiteRooks * RookValue;
            evaluation += whiteQueens * QueenValue;
            evaluation += whiteKing * KingValue;

            evaluation -= blackPawns * PawnValue;
            evaluation -= blackKnights * KnightValue;
            evaluation -= blackBishops * BishopValue;
            evaluation -= blackRooks * RookValue;
            evaluation -= blackQueens * QueenValue;
            evaluation -= blackKing * KingValue;

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

            if (Helpers.GetMoveCount(board) == 0)
            {
                if (Helpers.isKingInCheck(board, board.isWhiteTurn))
                {
                    evaluation = (board.isWhiteTurn == (aiColor == Pieces.White))
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
    }
}