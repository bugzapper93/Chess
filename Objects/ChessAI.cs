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

        /// <summary>
        /// Initializes the AI with a given search depth.
        /// </summary>
        /// <param name="searchDepth">The depth to search in the game tree.</param>
        public ChessAI(int searchDepth)
        {
            maxDepth = searchDepth;
        }

        /// <summary>
        /// Returns the best move for the current board and color using minimax with alpha-beta pruning.
        /// </summary>
        /// <param name="board">The current chessboard.</param>
        /// <param name="color">The color to move (e.g. Pieces.White or Pieces.Black).</param>
        /// <returns>The best move found.</returns>
        public Move GetBestMove(Chessboard board, int color)
        {
            int bestValue = int.MinValue;
            Move bestMove = new Move();// = null;

            // Generate all legal moves for the current color.
            Moveset moveset = Moves.GetAllMoves(board, color);
            foreach (var move in moveset.moves)
            {
                // Clone the board to simulate the move.
                Chessboard clone = board.Clone();
                clone.MakeMove(move);

                // Evaluate the move using minimax.
                int moveValue = Minimax(clone, maxDepth - 1, int.MinValue, int.MaxValue, false, color);

                if (moveValue > bestValue)
                {
                    bestValue = moveValue;
                    bestMove = move;
                }
            }
            return bestMove;
        }

        /// <summary>
        /// Minimax search with alpha-beta pruning.
        /// </summary>
        /// <param name="board">The current board state.</param>
        /// <param name="depth">Remaining search depth.</param>
        /// <param name="alpha">Alpha value for pruning.</param>
        /// <param name="beta">Beta value for pruning.</param>
        /// <param name="maximizingPlayer">True if it is the maximizing player’s turn.</param>
        /// <param name="aiColor">The AI’s color to evaluate positions from its perspective.</param>
        /// <returns>The evaluated score for the position.</returns>
        private int Minimax(Chessboard board, int depth, int alpha, int beta, bool maximizingPlayer, int aiColor)
        {
            // Terminal condition: search depth reached or no moves available.
            if (depth == 0)
                return EvaluateBoard(board, aiColor);

            int currentColor = maximizingPlayer ? aiColor : (aiColor == Pieces.White ? Pieces.Black : Pieces.White);
            Moveset moveset = Moves.GetAllMoves(board, currentColor);

            if (moveset.moves.Count == 0)
                return EvaluateBoard(board, aiColor);

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
                        break; // Beta cutoff
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
                        break; // Alpha cutoff
                }
                return minEval;
            }
        }

        /// <summary>
        /// Evaluates the board using a simple material count.
        /// </summary>
        /// <param name="board">The board to evaluate.</param>
        /// <param name="aiColor">The color perspective for evaluation.</param>
        /// <returns>A score indicating how favorable the board is for the given color.</returns>
        private int EvaluateBoard(Chessboard board, int aiColor)
        {
            int evaluation = 0;

            // Define piece values.
            const int PawnValue = 100;
            const int KnightValue = 320;
            const int BishopValue = 330;
            const int RookValue = 500;
            const int QueenValue = 900;
            const int KingValue = 20000;

            // Loop through the board and sum up the values.
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = board.pieces[row, col];
                    if (piece.value != 0)
                    {
                        // Assume the lower three bits represent the piece type.
                        int pieceType = piece.value & 7;
                        // Assume a defined mask for color (e.g., Pieces.ColorMask) is used.
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

                        // Add or subtract value based on color.
                        if (pieceColor == aiColor)
                            evaluation += pieceValue;
                        else
                            evaluation -= pieceValue;
                    }
                }
            }
            return evaluation;
        }
    }
}
