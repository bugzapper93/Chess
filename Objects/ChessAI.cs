using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Chess.Tools;

namespace Chess.Objects
{
    public class ChessAI
    {
        private int maxDepth;

        public ChessAI(int searchDepth)
        {
            maxDepth = searchDepth;
        }

        public async Task<Move> GetBestMove(Chessboard board, int aiColor)
        {
            int bestValue = int.MinValue;
            Move bestMove = new Move();
            object lockObj = new object();

            List<Move> moveset = board.LegalMoves.GetAllMoves();
            if (moveset.Count == 0)
                return bestMove;

            var tasks = moveset.Select(async move =>
            {
                Chessboard clone = board.Clone();

                clone.MakeMove(move);

                int moveValue = await Task.Run(() =>
                    Minimax(clone, maxDepth - 1, int.MinValue, int.MaxValue, false, aiColor));

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
            return bestMove;
        }

        private int Minimax(Chessboard board, int depth, int alpha, int beta, bool maximizingPlayer, int aiColor)
        {
            board.UpdateMoves();

            if (depth == 0 || Helpers.GetMoveCount(board) == 0)
                return EvaluateBoard(board, aiColor);

            List<Move> moves = board.LegalMoves.GetAllMoves();

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (var move in moves)
                {
                    Chessboard clone = board.Clone();
                    clone.MakeMove(move);
                    int eval = Minimax(clone, depth - 1, alpha, beta, false, aiColor);
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
                    Chessboard clone = board.Clone();
                    clone.MakeMove(move);
                    int eval = Minimax(clone, depth - 1, alpha, beta, true, aiColor);
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
    }
}