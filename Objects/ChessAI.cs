using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Objects
{
    public class ChessAI
    {
        private int maxDepth;

        public ChessAI(int searchDepth)
        {
            maxDepth = searchDepth;
        }

        public async Task<Move> GetBestMove(Chessboard board, int color)
        {
            int bestValue = int.MinValue;
            Move bestMove = new Move();
            object lockObj = new object();

            Moveset moveset = board.moveset;

            var tasks = moveset.moves.Select(async move =>
            {
                Chessboard clone = board.Clone();
                clone.MakeMove(move);

                int moveValue = await Task.Run(() => Minimax(clone, maxDepth - 1, int.MinValue, int.MaxValue, false, color));

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
            if (depth == 0 || board.moveset.moves.Count == 0)
                return EvaluateBoard(board, aiColor);

            Moveset moveset = board.moveset;
            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (var move in moveset.moves)
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
                foreach (var move in moveset.moves)
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

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = board.pieces[row, col];
                    if (piece.value != 0)
                    {
                        int pieceType = piece.value & 7;
                        int pieceColor = piece.value & 24;

                        int pieceValue = 0;
                        switch (pieceType)
                        {
                            case Pieces.Pawn:
                                pieceValue = PawnValue;
                                break;
                            case Pieces.Knight:
                                pieceValue = KnightValue;
                                break;
                            case Pieces.Bishop:
                                pieceValue = BishopValue;
                                break;
                            case Pieces.Rook:
                                pieceValue = RookValue;
                                break;
                            case Pieces.Queen:
                                pieceValue = QueenValue;
                                break;
                            case Pieces.King:
                                pieceValue = KingValue;
                                break;
                        }

                        if (pieceColor == aiColor)
                            evaluation += pieceValue;
                        else
                            evaluation -= pieceValue;
                    }
                }
            }
            if (board.isCheckMate)
            {
                if ((board.isWhiteTurn ? Pieces.White : Pieces.Black) == aiColor)
                {
                    evaluation += CheckMateValue;
                }
                else
                {
                    evaluation -= CheckMateValue;
                }
            }
            else if (board.isStaleMate)
            {
                evaluation += StaleMateValue;
            }
            return evaluation;
        }
    }
}