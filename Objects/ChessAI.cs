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

        public Move GetBestMove(Chessboard board, int color)
        {
            int bestValue = int.MinValue;
            Move bestMove = new Move();
            object lockObj = new object();

            Moveset moveset = Moves.GetAllMoves(board, color);
            Parallel.ForEach(moveset.moves, move =>
            {
                Chessboard clone = board.Clone();
                clone.MakeMove(move);

                int moveValue = Minimax(clone, maxDepth - 1, int.MinValue, int.MaxValue, false, color);

                lock (lockObj)
                {
                    if (moveValue > bestValue)
                    {
                        bestValue = moveValue;
                        bestMove = move;
                    }
                }
            });

            return bestMove;
        }

        private int Minimax(Chessboard board, int depth, int alpha, int beta, bool maximizingPlayer, int aiColor)
        {
            if (depth == 0)
                return EvaluateBoard(board, aiColor);

            int currentColor = maximizingPlayer ? aiColor : (aiColor == Pieces.White ? Pieces.Black : Pieces.White);
            Moveset moveset = Moves.GetAllMoves(board, currentColor);

            if (moveset.moves.Count == 0)
                return EvaluateBoard(board, aiColor);

            int value = maximizingPlayer ? int.MinValue : int.MaxValue;
            object lockObj = new object();

            Parallel.ForEach(moveset.moves, move =>
            {
                Chessboard clone = board.Clone();
                clone.MakeMove(move);
                int eval = Minimax(clone, depth - 1, alpha, beta, !maximizingPlayer, aiColor);

                lock (lockObj)
                {
                    if (maximizingPlayer)
                    {
                        value = Math.Max(value, eval);
                        alpha = Math.Max(alpha, value);
                    }
                    else
                    {
                        value = Math.Min(value, eval);
                        beta = Math.Min(beta, value);
                    }
                }

                if (beta <= alpha)
                    return;
            });
            return value;
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