using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Chess.Objects
{
    public struct Square
    {
        public Position position;
        public bool dangerWhite;
        public bool dangerBlack;
    }
    public class Chessboard
    {
        public Square[,] squares;
        public Piece[,] pieces;
        public bool isWhiteTurn;
        public bool isCheckMate;
        public bool isStaleMate;
        public int? en_passant_target_color = null;
        public Moveset moveset = new Moveset
        {
            moves = new List<Move>(),
            dangerSquares = new List<SquareDangerType>(),
            pins = new List<Pin>(),
            checks = new List<Check>()
        };
        public Position? enPassantTarget;
        public Chessboard(string FENstring = Pieces.DefaultPosition, bool whiteStarts = true)
        {
            isWhiteTurn = whiteStarts;
            pieces = new Piece[8, 8];
            squares = new Square[8, 8];

            Initialize(FENstring);
        }
        public void Initialize(string position)
        {
            pieces = Pieces.Parse_FEN(position);
            moveset = Moves.GetAllMoves(this, isWhiteTurn ? Pieces.White : Pieces.Black);
            UpdateDanger();
        }
        public void MakeMove(Move move)
        {
            isCheckMate = false;
            isStaleMate = false;
            int currentColor = isWhiteTurn ? Pieces.White : Pieces.Black;

            pieces[move.targetPosition.row, move.targetPosition.column] = pieces[move.startPosition.row, move.startPosition.column];
            pieces[move.startPosition.row, move.startPosition.column] = new Piece { value = 0 };
            pieces[move.targetPosition.row, move.targetPosition.column].hasMoved = true;

            int pieceValue = pieces[move.targetPosition.row, move.targetPosition.column].value;
            if ((pieceValue & 7) == Pieces.Pawn && Math.Abs(move.targetPosition.row - move.startPosition.row) == 2)
            {
                enPassantTarget = new Position(move.startPosition.row + (move.targetPosition.row - move.startPosition.row) / 2, move.startPosition.column);
                en_passant_target_color = pieceValue & 24;
            }
            if (enPassantTarget != null && (pieceValue & 7) == Pieces.Pawn && move.targetPosition == enPassantTarget.Value)
            {
                if (pieces[move.targetPosition.row, move.targetPosition.column].value == 0)
                {
                    pieces[move.startPosition.row, move.targetPosition.column] = new Piece { value = 0 };
                }
            }


            if ((pieceValue & 7) == Pieces.King)
            {
                if (move.targetPosition.column - move.startPosition.column == 2)
                {
                    pieces[move.targetPosition.row, move.targetPosition.column - 1] = pieces[move.targetPosition.row, 7];
                    pieces[move.targetPosition.row, 7] = new Piece { value = 0 };
                    pieces[move.targetPosition.row, move.targetPosition.column - 1].hasMoved = true;
                }
                if (move.targetPosition.column - move.startPosition.column == -2)
                {
                    pieces[move.targetPosition.row, move.targetPosition.column + 1] = pieces[move.targetPosition.row, 0];
                    pieces[move.targetPosition.row, 0] = new Piece { value = 0 };
                    pieces[move.targetPosition.row, move.targetPosition.column + 1].hasMoved = true;
                }
            }
            moveset = Moves.GetAllMoves(this, currentColor);
            UpdateDanger();
            isWhiteTurn = !isWhiteTurn;

            int checkCount = moveset.checks.Count;
            currentColor = isWhiteTurn ? Pieces.White : Pieces.Black;

            moveset = Moves.GetAllMoves(this, currentColor);
            if (moveset.moves.Count == 0)
            {
                if (checkCount > 0)
                {
                    isCheckMate = true;
                }
                else
                {
                    isStaleMate = true;
                }
            }
        }
       
        private void UpdateDanger()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    pieces[row, col].isPinned = false;
                    squares[row, col].dangerWhite = false;
                    squares[row, col].dangerBlack = false;
                }
            }

            foreach (SquareDangerType danger in moveset.dangerSquares)
            {
                if (danger.attackerColor == Pieces.White)
                    squares[danger.dangerPosition.row, danger.dangerPosition.column].dangerWhite = true;
                if (danger.attackerColor == Pieces.Black)
                    squares[danger.dangerPosition.row, danger.dangerPosition.column].dangerBlack = true;
            }

            foreach (Pin pin in moveset.pins)
            {
                pieces[pin.pinned.row, pin.pinned.column].isPinned = true;
            }
        }
        public Chessboard Clone()
        {
            Chessboard clone = new Chessboard
            {
                pieces = (Piece[,])this.pieces.Clone(),
                squares = (Square[,])this.squares.Clone(),
                enPassantTarget = this.enPassantTarget,
                en_passant_target_color = this.en_passant_target_color,
                isWhiteTurn = this.isWhiteTurn,
                moveset = new Moveset
                {
                    moves = new List<Move>(this.moveset.moves),
                    dangerSquares = new List<SquareDangerType>(this.moveset.dangerSquares),
                    pins = new List<Pin>(this.moveset.pins),
                    checks = new List<Check>(this.moveset.checks)
                }
            };
            return clone;
        }

        public bool CheckIfValidMove(Position startPos, Position endPos, out Move move)
        {
            move = default;
            foreach (var m in moveset.moves)
            {
                if (m.startPosition == startPos && m.targetPosition == endPos)
                {
                    move = m;
                    return true;
                }
            }
            return false;
        }
    }
}