using Chess.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public struct Check
    {
        public bool slidingPiece;
        public Position checkingPiece;
        public Position kingPosition;
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
        public int piece;
        public Position startPosition;
        public Position targetPosition;
        public bool capture;
    }
    public struct Moveset
    {
        public List<Move> moves;
        public List<Pin> pins;
        public List<Check> checks;
    }
    class Moves
    {
        
        public static int pieceValue = 0;
        public static Position friendlyKingPos = new Position();
        public static Position enemyKingPos = new Position();
        public static List<Position> GetAffectedPositions(Move move, Chessboard board)
        {
            List<Position> affectedPositions = new List<Position>();

            affectedPositions.Add(move.startPosition);
            affectedPositions.Add(move.targetPosition);
            affectedPositions.Add(friendlyKingPos);

            // Checking whether sliding pieces were affected
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = board.pieces[row, col];
                    if (piece.value != 0)
                    {
                        int pieceType = piece.value & 7;
                        if (pieceType == Pieces.Bishop || pieceType == Pieces.Rook || pieceType == Pieces.Queen)
                        {
                            Position checkPosition = new Position(row, col);
                            if (Helpers.IsInline(move.startPosition, checkPosition) || Helpers.IsInline(move.targetPosition, checkPosition))
                            {
                                affectedPositions.Add(new Position(row, col));
                            }
                        }
                    }
                }
            }

            // Checking whether pawns and knights were affected
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = board.pieces[row, col];
                    if (piece.value != 0)
                    {
                        int pieceType = piece.value & 7;
                        Position currentPos = new Position(row, col);
                        if (pieceType == Pieces.Knight)
                        {
                            foreach (Position direction in Constants.KnightDirections)
                            {
                                Position checkPosition = currentPos + direction;
                                if (Helpers.InBounds(checkPosition) && (checkPosition == move.startPosition || checkPosition == move.targetPosition))
                                {
                                    affectedPositions.Add(currentPos);
                                    break;
                                }
                            }
                        }
                        else if (pieceType == Pieces.Pawn)
                        {
                            int direction = (piece.value & 24) == Pieces.White ? -1 : 1;
                            Position forwardOne = new Position(row + direction, col);
                            Position forwardTwo = new Position(row + 2 * direction, col);
                            Position attackLeft = new Position(row + direction, col - 1);
                            Position attackRight = new Position(row + direction, col + 1);

                            if ((Helpers.InBounds(forwardOne) && (forwardOne == move.startPosition || forwardOne == move.targetPosition)) ||
                                (Helpers.InBounds(forwardTwo) && (forwardTwo == move.startPosition || forwardTwo == move.targetPosition)) ||
                                (Helpers.InBounds(attackRight) && (attackRight == move.startPosition || attackRight == move.targetPosition)) ||
                                (Helpers.InBounds(attackLeft) && (attackLeft == move.startPosition || attackLeft == move.targetPosition)))
                            {
                                affectedPositions.Add(currentPos);
                            }
                        }
                    }
                }
            }
            return affectedPositions;
        }
        public static Moveset UpdatePieceCache(Position pos, Chessboard board)
        {
            Piece piece = board.pieces[pos.row, pos.column];
            int row = pos.row;
            int col = pos.column;
            int pieceValue = piece.value;
            Moveset temp = new Moveset();
            switch (pieceValue & 7)
            {
                case Pieces.Pawn:
                    temp = GetPawnMoves(new Position(row, col), board);
                    break;
                case Pieces.Knight:
                    temp = GetKnightMoves(new Position(row, col), board);
                    break;
                case Pieces.Bishop:
                case Pieces.Rook:
                case Pieces.Queen:
                    temp = GetSlidingMoves(new Position(row, col), board);
                    break;
                case Pieces.King:
                    temp = GetKingMoves(new Position(row, col), board);
                    break;
            }
            return temp;
        }
        public static Moveset GetAllMoves(Chessboard board, int color)
        {
            List<Move> moves = new List<Move>();
            List<Pin> pins = new List<Pin>();
            List<Check> checks = new List<Check>();

            int oppositeColor = Pieces.GetOppositeColor(color);
            List<Position> slidingPieces = new List<Position>();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (board.pieces[row, col].value == (Pieces.King | oppositeColor))
                    {
                        enemyKingPos = new Position(row, col);
                    }
                    else if (board.pieces[row, col].value == (Pieces.King | color))
                    {
                        friendlyKingPos = new Position(row, col);
                    }
                    else if (board.pieces[row, col].value == (Pieces.Bishop | oppositeColor) || board.pieces[row, col].value == (Pieces.Rook | oppositeColor) || board.pieces[row, col].value == (Pieces.Queen | oppositeColor))
                    {
                        slidingPieces.Add(new Position(row, col));
                    }
                }
            }

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = board.pieces[row, col];
                    int pieceValue = piece.value;
                    if (pieceValue != 0 && (pieceValue & 24) == color)
                    {
                        Moveset tempMoveset = board.pieces[row, col].Cache;
                        moves.AddRange(tempMoveset.moves);
                        if (tempMoveset.pins != null)
                            pins.AddRange(tempMoveset.pins);
                        if (tempMoveset.checks != null)
                            checks.AddRange(tempMoveset.checks);
                    }
                }
            }
            Moveset legalSet = ValidateMoves(new Moveset { moves = moves, pins = pins, checks = checks }, board, slidingPieces);
            return legalSet;
        }
        private static Moveset ValidateMoves(Moveset moveset, Chessboard board, List<Position> slidingPieces)
        {
            List<Move> moves = moveset.moves;
            List<Check> checks = new List<Check>();
            List<Move> possibleMoves = new List<Move>();

            Piece[,] pieces = board.pieces;

            foreach (Move move in moves)
            {
                int startRow = move.startPosition.row;
                int startColumn = move.startPosition.column;

                if (!Helpers.InBounds(move.targetPosition) || !Helpers.InBounds(move.startPosition))
                    continue;

                Piece currentPiece = board.pieces[startRow, startColumn];

                int currentPieceValue = pieces[startRow, startColumn].value;
                int currentColor = currentPieceValue & 24;
                    
                if (move.targetPosition == enemyKingPos)
                {
                    int[] sliding = [Pieces.Bishop, Pieces.Rook, Pieces.Queen];
                    bool isSliding = sliding.Contains(currentPieceValue & 7);
                    checks.Add(new Check { checkingPiece = move.startPosition, kingPosition = enemyKingPos, slidingPiece = isSliding });
                    continue;
                }

                // Handling possibly moving into a check
                if ((currentPieceValue & 7) == Pieces.King)
                {
                    bool validMove = true;
                    Position checkPosition = move.targetPosition;

                    // Unable to move into a knight attack
                    foreach (Position direction in Constants.KnightDirections)
                    {
                        Position position = checkPosition + direction;
                        if (Helpers.InBounds(position) && pieces[position.row, position.column].value == (Pieces.Knight | Pieces.GetOppositeColor(currentColor)))
                        {
                            validMove = false;
                            break;
                        }
                    }

                    foreach (Position slidingPiece in slidingPieces)
                    {
                        int attackerType = pieces[slidingPiece.row, slidingPiece.column].value & 7;
                        int type = 0;
                        if (attackerType == Pieces.Bishop)
                            type = 1;
                        else if (attackerType == Pieces.Rook)
                            type = 2;

                        if (Helpers.IsInline(checkPosition, slidingPiece, type) && checkPosition != slidingPiece && Helpers.CheckPathClear(checkPosition, slidingPiece, board.pieces))
                        {
                            validMove = false;
                            break;
                        }
                    }

                    if (validMove)
                        possibleMoves.Add(move);
                    continue;
                }

                if (!currentPiece.isPinned)
                {
                    possibleMoves.Add(move);
                    continue;
                }
                else
                {
                    List<Pin> piecePins = Helpers.GetAllPiecePins(move.startPosition, board.moveset.pins);
                    foreach (Pin pin in piecePins)
                    {
                        if (Helpers.ColinearPaths(move.startPosition, move.targetPosition, pin.start))
                        {
                            possibleMoves.Add(move);
                            break;
                        }
                    }
                    continue;
                }
            }

            List<Move> legalMoves = new List<Move>();
            if (board.moveset.checks.Count > 0)
            {
                foreach (Check check in board.moveset.checks)
                {
                    foreach (Move move in possibleMoves)
                    {
                        if ((board.pieces[move.startPosition.row, move.startPosition.column].value & 7) == Pieces.King && Helpers.ValidForAllChecks(move.startPosition, move.targetPosition, board.moveset.checks))
                        {
                            legalMoves.Add(move);
                            continue;
                        }
                        else
                        {
                            if (Helpers.ColinearPaths(friendlyKingPos, move.targetPosition, check.checkingPiece) && Helpers.IsBetween(friendlyKingPos, check.checkingPiece, move.targetPosition))
                            {
                                legalMoves.Add(move);
                                continue;
                            }
                        }
                    }
                }
            }
            else
            {
                legalMoves = possibleMoves;
            }

            return new Moveset { moves = legalMoves, checks = checks, pins = new List<Pin>() };
        }
        #region Getting moves for each piece
        private static Moveset GetPawnMoves(Position startSquare, Chessboard board)
        {
            Piece[,] pieces = board.pieces;
            List<Move> moves = new List<Move>();
            List<SquareDangerType> squares = new List<SquareDangerType>();

            Position consideredPosition;

            int startRow = startSquare.row;
            int startColumn = startSquare.column;

            int currentPiece = pieces[startRow, startColumn].value;
            int currentColor = currentPiece & 24;
            int direction = currentColor == Pieces.White ? -1 : 1;
            int[] colVars = [-1, 1];

            consideredPosition = new Position(startRow + direction, startColumn);
            if (Helpers.InBounds(consideredPosition) && Helpers.OccupationType(consideredPosition, pieces) == 0)
            {
                Move move = new Move
                {
                    startPosition = startSquare,
                    targetPosition = consideredPosition,
                    capture = false,
                };
                moves.Add(move);
            }

            consideredPosition = new Position(startRow + 2 * direction, startColumn);
            if (Helpers.InBounds(consideredPosition) && Helpers.OccupationType(consideredPosition, pieces) == 0 && startRow == (currentColor == Pieces.White ? 6 : 1) && Helpers.CheckPathClear(startSquare, consideredPosition, pieces))
            {
                Move move = new Move
                {
                    startPosition = startSquare,
                    targetPosition = consideredPosition,
                    capture = false,
                };
                moves.Add(move);
            }

            foreach (int var in colVars)
            {
                consideredPosition = new Position(startRow + direction, startColumn + var);
                if (Helpers.InBounds(consideredPosition))
                {
                    bool isEnPassant = Helpers.CheckEnPassant(startSquare, consideredPosition, board);
                    if ((Helpers.OccupationType(consideredPosition, pieces) == Pieces.GetOppositeColor(currentColor) || Helpers.CheckEnPassant(startSquare, consideredPosition, board) || isEnPassant))
                    {
                        Move move = new Move
                        {
                            startPosition = startSquare,
                            targetPosition = consideredPosition,
                            capture = true,
                        };

                        if (isEnPassant)
                        {
                            Position pawnTargetPos = new Position(startRow, startColumn + var);
                        }
                        moves.Add(move);
                    }
                    squares.Add(new SquareDangerType { dangerPosition = consideredPosition, attackerPosition = startSquare, attackerColor = currentColor });
                }
            }
            return new Moveset { moves = moves };
        }
        private static Moveset GetSlidingMoves(Position startSquare, Chessboard board)
        {
            Piece[,] pieces = board.pieces;
            List<Move> moves = new List<Move>();
            List<SquareDangerType> squares = new List<SquareDangerType>();
            List<Pin> pins = new List<Pin>();
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
                            if (Helpers.OccupationType(consideredPosition, pieces) != 0)
                            {
                                int nextStep = step + 1;
                                while (true)
                                {
                                    Position checkPosition = new Position(rowDiff * nextStep + startRow, colDiff * nextStep + startColumn);
                                    if (!Helpers.InBounds(checkPosition))
                                    {
                                        break;
                                    }
                                    int nextPiece = pieces[checkPosition.row, checkPosition.column].value;
                                    if (nextPiece == 0)
                                    {
                                        nextStep++;
                                        continue;
                                    }
                                    else
                                    {
                                        if ((nextPiece & 7) == Pieces.King)
                                        {
                                            Pin pin = new Pin
                                            {
                                                start = startSquare,
                                                pinned = consideredPosition
                                            };
                                            pins.Add(pin);
                                            break;
                                        }
                                        break;
                                    }
                                }
                            }
                            Move move = new Move
                            {
                                startPosition = startSquare,
                                targetPosition = consideredPosition,
                                capture = Helpers.OccupationType(consideredPosition, pieces) != 0
                            };
                            moves.Add(move);

                        }
                        squares.Add(new SquareDangerType { dangerPosition = consideredPosition, attackerPosition = startSquare, attackerColor = currentColor });
                        if (Helpers.OccupationType(consideredPosition, pieces) != 0)
                            break;
                    }
                    else
                    {
                        break;
                    }
                    step++;
                }
            }
            return new Moveset { moves = moves, pins = pins };
        }
        private static Moveset GetKnightMoves(Position startSquare, Chessboard board)
        {
            Piece[,] pieces = board.pieces;

            List<Move> moves = new List<Move>();
            List<SquareDangerType> squares = new List<SquareDangerType>();

            Position consideredPosition;

            int startRow = startSquare.row;
            int startColumn = startSquare.column;

            int currentPiece = pieces[startRow, startColumn].value;
            int currentColor = currentPiece & 24;

            foreach (Position direction in Constants.KnightDirections)
            {
                consideredPosition = startSquare + direction;
                if (Helpers.InBounds(consideredPosition))
                {
                    if (Helpers.OccupationType(consideredPosition, pieces) != currentColor)
                    {
                        Move move = new Move
                        {
                            startPosition = startSquare,
                            targetPosition = consideredPosition,
                            capture = Helpers.OccupationType(consideredPosition, pieces) != 0,                      
                        };
                        moves.Add(move);
                    }
                    squares.Add(new SquareDangerType { dangerPosition = consideredPosition, attackerPosition = startSquare, attackerColor = currentColor });
                }
            }
            return new Moveset { moves = moves };
        }
        private static Moveset GetKingMoves(Position startSquare, Chessboard board)
        {
            Piece[,] pieces = board.pieces;

            List<Move> moves = new List<Move>();
            List<SquareDangerType> squares = new List<SquareDangerType>();

            Position consideredPosition;

            int startRow = startSquare.row;
            int startColumn = startSquare.column;

            int currentPiece = pieces[startRow, startColumn].value;
            int currentColor = currentPiece & 24;
            for (int direction = 0; direction < 8; direction++)
            {
                consideredPosition = startSquare + Constants.Directions[direction];
                if (Helpers.InBounds(consideredPosition))
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
                    squares.Add(new SquareDangerType { dangerPosition = consideredPosition, attackerPosition = startSquare, attackerColor = currentColor });
                }
            }
            int[] possibleColumns = { 0, 7 };
            int row = currentColor == Pieces.White ? 7 : 0;
            if (!pieces[startRow, startColumn].hasMoved)
            {
                foreach (int column in possibleColumns)
                {
                    if (!pieces[row, column].hasMoved)
                    {
                        if (Helpers.CheckPathClear(startSquare, new Position(row, column), pieces) && Helpers.CheckPathCheck(startSquare, new Position(row, column), board))
                        {
                            int direction = column == 0 ? -1 : 1;
                            consideredPosition = new Position(row, startColumn + 2 * direction);
                            Move move = new Move
                            {
                                startPosition = startSquare,
                                targetPosition = consideredPosition,
                                capture = false
                            };
                            moves.Add(move);
                        }
                    }
                }
            }
            return new Moveset { moves = moves };
        }
        #endregion
    }
}