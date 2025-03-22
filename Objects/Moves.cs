using Chess.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Objects
{
    public struct Position
    {
        public int row;
        public int column;
        public Position(int row, int column)
        {
            this.row = row;
            this.column = column;
        }
        public static Position operator +(Position a, Position b)
        {
            return new Position(a.row + b.row, a.column + b.column);
        }
        public static bool operator ==(Position a, Position b)
        {
            return a.row == b.row && a.column == b.column;
        }
        public static bool operator !=(Position a, Position b)
        {
            return !(a == b);
        }
    }
    public struct Pin
    {
        public Position start;
        public Position pinned;
    }
    public struct SquareDangerType
    {
        public Position dangerPosition;
        public Position attackerPosition;
        public int attackerColor;
    }
    public struct Move
    {
        public Position startPosition;
        public Position targetPosition;
        public bool capture;
    }
    public struct Moveset
    {
        public List<Move> moves;
        public List<SquareDangerType> dangerSquares;
        public List<Pin> pins;
    }
    class Moves
    {
        public static Moveset GetAllMoves(Chessboard board, int color)
        {
            Piece[,] pieces = board.pieces;
            List<Move> moves = new List<Move>();
            for (int row = 0; row < 8; row++)
                for (int col = 0; col < 8; col++)
                {
                    int pieceValue = pieces[row, col].value;
                    int pieceColor = pieceValue & 24;
                    if (pieceColor == color)
                    {
                        switch (pieceValue & 7)
                        {
                            case Pieces.Pawn:
                                moves.AddRange(GetPawnMoves(new Position(row, col), board));
                                break;
                            case Pieces.Knight:
                                moves.AddRange(GetKnightMoves(new Position(row, col), board));
                                break;
                            case Pieces.Bishop:
                            case Pieces.Rook:
                            case Pieces.Queen:
                                moves.AddRange(GetSlidingMoves(new Position(row, col), board));
                                break;
                            case Pieces.King:
                                moves.AddRange(GetKingMoves(new Position(row, col), board));
                                break;
                        }
                    }
                }
            Moveset moveset = ValidateMoves(moves, board);
            return moveset;
        }
        private static Moveset ValidateMoves(List<Move> moves, Chessboard board)
        {
            List<Pin> pins = new List<Pin>();
            List<SquareDangerType> dangerSquares = new List<SquareDangerType>();
            List<Move> possibleMoves = new List<Move>();

            Piece[,] pieces = board.pieces;
            foreach (Move move in moves)
            {
                int startRow = move.startPosition.row;
                int startColumn = move.startPosition.column;
                int currentPiece = pieces[startRow, startColumn].value;
                int currentColor = currentPiece & 24;
                if ((currentPiece & 7) != Pieces.Pawn)
                {
                    dangerSquares.Add(new SquareDangerType { dangerPosition = move.targetPosition, attackerPosition = move.startPosition, attackerColor = currentColor });
                }
                else
                {
                    if (move.capture)
                    {
                        dangerSquares.Add(new SquareDangerType { dangerPosition = move.targetPosition, attackerPosition = move.startPosition, attackerColor = currentColor });
                    }
                }
            }
            Moveset moveset = new Moveset { moves = moves, dangerSquares = dangerSquares, pins = pins };
            return moveset;
        }
        private static List<Move> GetPawnMoves(Position startSquare, Chessboard board)
        {
            Piece[,] pieces = board.pieces;
            List<Move> moves = new List<Move>();
            Position consideredPosition;

            int startRow = startSquare.row;
            int startColumn = startSquare.column;

            int currentPiece = pieces[startRow, startColumn].value;
            int currentColor = currentPiece & 24;

            int direction = currentColor == Pieces.White ? -1 : 1;
            int[] colVars = [-1, 1];

            // Moving one square forward
            consideredPosition = new Position(startRow + direction, startColumn);
            if (Helpers.InBounds(consideredPosition) && Helpers.OccupationType(consideredPosition, pieces) == 0)
            {
                Move move = new Move
                {
                    startPosition = startSquare,
                    targetPosition = consideredPosition,
                    capture = false
                };
                moves.Add(move);
            }
            // Moving two squares forward
            consideredPosition = new Position(startRow + 2 * direction, startColumn);
            if (Helpers.InBounds(consideredPosition) && Helpers.OccupationType(consideredPosition, pieces) == 0 && startRow == (currentColor == Pieces.White ? 6 : 1) && Helpers.CheckPathClear(startSquare, consideredPosition, pieces))
            {
                Move move = new Move
                {
                    startPosition = startSquare,
                    targetPosition = consideredPosition,
                    capture = false
                };
                moves.Add(move);
            }
            // Capturing
            foreach(int var in colVars)
            {
                consideredPosition = new Position(startRow + direction, startColumn + var);
                if (Helpers.InBounds(consideredPosition) && (Helpers.OccupationType(consideredPosition, pieces) == Pieces.GetOppositeColor(currentColor) || Helpers.CheckEnPassant(startSquare, consideredPosition, board)))
                {
                    Move move = new Move
                    {
                        startPosition = startSquare,
                        targetPosition = consideredPosition,
                        capture = true
                    };
                    moves.Add(move);
                }
            }
            return moves;
        }
        private static List<Move> GetSlidingMoves(Position startSquare, Chessboard board)
        {
            Piece[,] pieces = board.pieces;
            List<Move> moves = new List<Move>();
            Position consideredPosition;

            int startRow = startSquare.row;
            int startColumn = startSquare.column;

            int currentPiece = pieces[startRow, startColumn].value;
            int currentColor = currentPiece & 24;

            int directionRangeStart;
            int directionRangeEnd;

            switch (currentPiece & 7)
            {
                case Pieces.Bishop:
                    directionRangeStart = 4;
                    directionRangeEnd = 8;
                    break;
                case Pieces.Rook:
                    directionRangeStart = 0;
                    directionRangeEnd = 4;
                    break;
                case Pieces.Queen:
                    directionRangeStart = 0;
                    directionRangeEnd = 8;
                    break;
                default:
                    directionRangeStart = 0;
                    directionRangeEnd = 0;
                    break;
            }

            for (int direction = directionRangeStart; direction < directionRangeEnd; direction++)
            {
                int step = 1;
                int rowDiff = Constants.Directions[direction].row;
                int colDiff = Constants.Directions[direction].column;
                while (true)
                {
                    consideredPosition = new Position(rowDiff * step + startRow, colDiff * step + startColumn);
                    if (Helpers.InBounds(consideredPosition) && Helpers.CheckPathClear(startSquare, consideredPosition, pieces))
                    {
                        if (Helpers.OccupationType(consideredPosition, pieces) != currentColor)
                        {
                            Move move = new Move
                            {
                                startPosition = startSquare,
                                targetPosition = consideredPosition,
                                capture = Helpers.OccupationType(consideredPosition, pieces) != 0
                            };
                            moves.Add(move);
                        }
                    }
                    else
                    {
                        break;
                    }
                    step++;
                }
            }
            return moves;
        }
        private static List<Move> GetKnightMoves(Position startSquare, Chessboard board)
        {
            Piece[,] pieces = board.pieces;
            List<Move> moves = new List<Move>();
            Position consideredPosition;

            int startRow = startSquare.row;
            int startColumn = startSquare.column;

            int currentPiece = pieces[startRow, startColumn].value;
            int currentColor = currentPiece & 24;

            foreach (Position direction in Constants.KnightDirections)
            {
                consideredPosition = startSquare + direction;
                if (Helpers.InBounds(consideredPosition) && Helpers.OccupationType(consideredPosition, pieces) != currentColor)
                {
                    Move move = new Move
                    {
                        startPosition = startSquare,
                        targetPosition = consideredPosition,
                        capture = Helpers.OccupationType(consideredPosition, pieces) != 0
                    };
                    moves.Add(move);
                }
            }
            return moves;
        }
        private static List<Move> GetKingMoves(Position startSquare, Chessboard board)
        {
            Piece[,] pieces = board.pieces;
            List<Move> moves = new List<Move>();
            Position consideredPosition;

            int startRow = startSquare.row;
            int startColumn = startSquare.column;

            int currentPiece = pieces[startRow, startColumn].value;
            int currentColor = currentPiece & 24;
            for (int direction = 0; direction < 8; direction++)
            {
                consideredPosition = startSquare + Constants.Directions[direction];
                if (Helpers.InBounds(consideredPosition) && Helpers.OccupationType(consideredPosition, pieces) != currentColor)
                {
                    Move move = new Move
                    {
                        startPosition = startSquare,
                        targetPosition = consideredPosition,
                        capture = Helpers.OccupationType(consideredPosition, pieces) != 0
                    };
                    moves.Add(move);
                }
            }
            return moves;
        }
    }
}
