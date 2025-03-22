using Chess.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

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
        public int captured_piece;
        public Position capture_pos;
        public bool isEnPassant;
    }
    public struct Moveset
    {
        public List<Move> moves;
        public List<SquareDangerType> dangerSquares;
        public List<Pin> pins;
        public List<Check> checks;
    }
    class Moves
    {
        public static int pieceValue = 0;
        public static Position friendlyKingPos = new Position();
        public static Position enemyKingPos = new Position();
        public static Moveset GetAllMoves(Chessboard board, int color)
        {
            Piece[,] pieces = board.pieces;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (pieces[row, col].value == (Pieces.King | Pieces.GetOppositeColor(color)))
                    {
                        enemyKingPos = new Position(row, col);
                    }
                    else if (pieces[row, col].value == (Pieces.King | color))
                    {
                        friendlyKingPos = new Position(row, col);
                    }
                }
            }

            List<Move> moves = new List<Move>();
            List<SquareDangerType> squares = new List<SquareDangerType>();
            List<Pin> pins = new List<Pin>();

            for (int row = 0; row < 8; row++)
                for (int col = 0; col < 8; col++)
                {
                    pieceValue = pieces[row, col].value;
                    int pieceColor = pieceValue & 24;
                    if (pieceColor == color)
                    {
                        Moveset temp;
                        switch (pieceValue & 7)
                        {
                            case Pieces.Pawn:
                                temp = GetPawnMoves(new Position(row, col), board);
                                moves.AddRange(temp.moves);
                                squares.AddRange(temp.dangerSquares);
                                break;
                            case Pieces.Knight:
                                temp = GetKnightMoves(new Position(row, col), board);
                                moves.AddRange(temp.moves);
                                squares.AddRange(temp.dangerSquares);
                                break;
                            case Pieces.Bishop:
                            case Pieces.Rook:
                            case Pieces.Queen:
                                temp = GetSlidingMoves(new Position(row, col), board);
                                moves.AddRange(temp.moves);
                                squares.AddRange(temp.dangerSquares);
                                if (temp.pins != null)
                                    pins.AddRange(temp.pins);
                                break;
                            case Pieces.King:
                                temp = GetKingMoves(new Position(row, col), board);
                                moves.AddRange(temp.moves);
                                squares.AddRange(temp.dangerSquares);
                                break;
                        }
                    }
                }

            Moveset inputSet = new Moveset { moves = moves, dangerSquares = squares };
            Moveset temp_moveset = ValidateMoves(inputSet, board);
            return new Moveset { moves = temp_moveset.moves, dangerSquares = temp_moveset.dangerSquares, pins = pins, checks = temp_moveset.checks };
        }
        private static Moveset ValidateMoves(Moveset moveset, Chessboard board)
        {
            List<Move> moves = moveset.moves;
            List<Check> checks = new List<Check>();
            List<SquareDangerType> dangerSquares = new List<SquareDangerType>();
            List<Move> possibleMoves = new List<Move>();

            Piece[,] pieces = board.pieces;

            foreach (Move move in moves)
            {
                int startRow = move.startPosition.row;
                int startColumn = move.startPosition.column;

                Piece currentPiece = board.pieces[startRow, startColumn];

                // Handle dangerous areas
                int currentPieceValue = pieces[startRow, startColumn].value;
                int currentColor = currentPieceValue & 24;
                if ((currentPieceValue & 7) != Pieces.Pawn)
                {
                    dangerSquares.Add(new SquareDangerType { dangerPosition = move.targetPosition, attackerPosition = move.startPosition, attackerColor = currentColor });
                }
                dangerSquares.AddRange(moveset.dangerSquares);

                if (move.targetPosition == enemyKingPos)
                {
                    int[] slidingPieces = [Pieces.Bishop, Pieces.Rook, Pieces.Queen];
                    bool isSliding = slidingPieces.Contains(currentPieceValue & 7);
                    checks.Add(new Check { checkingPiece = move.startPosition, kingPosition = enemyKingPos, slidingPiece = isSliding });
                    continue;
                }

                // Handle special movement
                if ((currentPieceValue & 7) == Pieces.King)
                {
                    if (currentColor == Pieces.Black && board.squares[move.targetPosition.row, move.targetPosition.column].dangerWhite)
                        continue;

                    if (currentColor == Pieces.White && board.squares[move.targetPosition.row, move.targetPosition.column].dangerBlack)
                        continue;

                    possibleMoves.Add(move);
                    continue;
                }

                // Handle movement along pins
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

            // Handle checks

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

            return new Moveset { moves = legalMoves, dangerSquares = dangerSquares, checks = checks };
        }
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
            Position default_capture = new Position { row = -1, column = -1 };
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
                    capture = false,
                    capture_pos = default_capture
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
                    capture = false,
                    capture_pos = default_capture
                };
                moves.Add(move);
            }
            // Capturing
            foreach (int var in colVars)
            {
                consideredPosition = new Position(startRow + direction, startColumn + var);
               // int squareValue = pieces[consideredPosition.row, consideredPosition.column].value;
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
                            isEnPassant = isEnPassant
                        };

                        if (isEnPassant)
                        {
                            Position pawnTargetPos = new Position(startRow, startColumn + var);
                            move.captured_piece = Pieces.Get_Value(pieces, pawnTargetPos);
                            move.capture_pos = pawnTargetPos;
                        }
                        else
                        {
                            move.captured_piece = pieces[consideredPosition.row, consideredPosition.column].value;
                            move.capture_pos = consideredPosition;
                        }
                        moves.Add(move);
                    }
                    squares.Add(new SquareDangerType { dangerPosition = consideredPosition, attackerPosition = startSquare, attackerColor = currentColor });
                }
            }
            return new Moveset { moves = moves, dangerSquares = squares };
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
                        int squareValue = pieces[consideredPosition.row, consideredPosition.column].value;
                        if (Helpers.OccupationType(consideredPosition, pieces) != currentColor)
                        {
                            // Checking positions for pins
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
                            if (Helpers.OccupationType(consideredPosition, pieces) != 0)
                            {
                                move.captured_piece = squareValue;
                                move.capture_pos = consideredPosition;
                            }
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
            return new Moveset { moves = moves, dangerSquares = squares, pins = pins };
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
                        if (Helpers.OccupationType(consideredPosition, pieces) != 0)
                        {
                            move.captured_piece = pieces[consideredPosition.row, consideredPosition.column].value;
                            move.capture_pos = consideredPosition;
                        }
                        moves.Add(move);
                    }
                    squares.Add(new SquareDangerType { dangerPosition = consideredPosition, attackerPosition = startSquare, attackerColor = currentColor });
                }
            }
            return new Moveset { moves = moves, dangerSquares = squares };
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
                        if (Helpers.OccupationType(consideredPosition, pieces) != 0)
                        {
                            move.captured_piece = pieces[consideredPosition.row, consideredPosition.column].value;
                            move.capture_pos = consideredPosition;
                        }
                        moves.Add(move);
                    }
                    squares.Add(new SquareDangerType { dangerPosition = consideredPosition, attackerPosition = startSquare, attackerColor = currentColor });
                }
            }
            // Handle castling
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
            return new Moveset { moves = moves, dangerSquares = squares };
        }
        public static bool Compare_Positions(Position pos_1, Position pos_2)
        {
            if (pos_1.row == pos_2.row && pos_1.column == pos_2.column)
                return true;
            return false;
        }
    }
}