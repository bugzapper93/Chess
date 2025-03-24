using Chess.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Chess.Objects
{
    public class Chessboard
    {
        public Piece[,] pieces;
        public bool isWhiteTurn;
        public bool isCheckMate;
        public bool isStaleMate;
        public Moveset moveset = new Moveset
        {
            moves = new List<Move>(),
            pins = new List<Pin>(),
            checks = new List<Check>()
        };
        public Position? enPassantTarget;
        public Chessboard(string FENstring = Pieces.DefaultPosition, bool whiteStarts = true)
        {
            isWhiteTurn = whiteStarts;
            pieces = new Piece[8, 8];

            Initialize(FENstring);
        }
        public void Initialize(string position)
        {
            pieces = Pieces.Parse_FEN(position);
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    pieces[row, col].Cache = Moves.UpdatePieceCache(new Position(row, col), this);
                }
            }
            moveset = Moves.GetAllMoves(this, isWhiteTurn ? Pieces.White : Pieces.Black, Constants.AllPositions);
            UpdateDanger();
        }
        public void MakeMove(Move move)
        {
            enPassantTarget = null;

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

            List<Position> affectedPositions = Moves.GetAffectedPositions(move, this);

            // Always update the moves for the king
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((pieces[i, j].value & 7) == Pieces.King)
                        affectedPositions.Add(new Position(i, j));
                }
            }

            // Update the piece that moved
            if (!affectedPositions.Contains(move.startPosition))
                affectedPositions.Add(move.startPosition);

            if (!affectedPositions.Contains(move.targetPosition))
                affectedPositions.Add(move.targetPosition);

            foreach (Position piecePos in affectedPositions)
            {
                if (Helpers.InBounds(piecePos) && pieceValue != 0)
                    pieces[piecePos.row, piecePos.column].Cache = Moves.UpdatePieceCache(piecePos, this);
            }

            moveset = new Moveset
            {
                moves = new List<Move>(),
                pins = new List<Pin>(),
                checks = new List<Check>()
            };

            moveset = Moves.GetAllMoves(this, currentColor, affectedPositions);

            if (moveset.checks.Count > 0)
            {
                affectedPositions = Constants.AllPositions;
            }

            // Pinned pieces need updating
            List<Position> pinnedPieces = UpdateDanger();
            affectedPositions.AddRange(pinnedPieces);

            isWhiteTurn = !isWhiteTurn;

            int checkCount = moveset.checks.Count;
            currentColor = isWhiteTurn ? Pieces.White : Pieces.Black;

            moveset = Moves.GetAllMoves(this, currentColor, affectedPositions);
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
       
        private List<Position> UpdateDanger()
        {
            List<Position> danger = new List<Position>();
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    pieces[row, col].isPinned = false;
                }
            }

            // Aktualizacja pinów
            foreach (Pin pin in moveset.pins)
            {

                int row = pin.pinned.row;
                int col = pin.pinned.column;

                // Sprawdź, czy pozycja jest w zakresie 0-7
                if (row >= 0 && row < 8 && col >= 0 && col < 8)
                {
                    pieces[row, col].isPinned = true;
                    danger.Add(new Position(row, col));
                }
            }
            return danger;
        }
        public Chessboard Clone()
        {
            Chessboard clone = new Chessboard
            {
                pieces = (Piece[,])this.pieces.Clone(),
                enPassantTarget = this.enPassantTarget,
                isWhiteTurn = this.isWhiteTurn,
                moveset = new Moveset
                {
                    moves = new List<Move>(this.moveset.moves),
                    pins = new List<Pin>(this.moveset.pins),
                    checks = new List<Check>(this.moveset.checks)
                }
            };
            return clone;
        }
    }
}