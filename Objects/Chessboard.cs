using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Chess.Objects
{
    class Chessboard
    {
        public Piece[,] pieces;
        private bool isWhiteTurn;
        public List<Move> possibleMoves;
        public Position? enPassantTarget;
        public Chessboard(string FENstring = Pieces.DefaultPosition, bool whiteStarts = true)
        {
            isWhiteTurn = whiteStarts;
            pieces = new Piece[8, 8];
            possibleMoves = new List<Move>();
            Initialize(FENstring);
        }
        public void Initialize(string position)
        {
            pieces = Pieces.Parse_FEN(position);
            GetMoves();
        }
        public void MakeMove(Move move)
        {
            int currentColor = isWhiteTurn ? Pieces.White : Pieces.Black;

            pieces[move.targetPosition.row, move.targetPosition.column] = pieces[move.startPosition.row, move.startPosition.column];
            pieces[move.startPosition.row, move.startPosition.column] = new Piece { value = 0 };

            int pieceValue = pieces[move.targetPosition.row, move.targetPosition.column].value;
            if ((pieceValue & 7) == Pieces.Pawn && Math.Abs(move.targetPosition.row - move.startPosition.row) == 2)
                enPassantTarget = new Position(move.startPosition.row + (move.targetPosition.row - move.startPosition.row) / 2, move.startPosition.column);

            isWhiteTurn = !isWhiteTurn;

            GetMoves();
        }
        public void GetMoves()
        {
            int currentColor = isWhiteTurn ? Pieces.White : Pieces.Black;
            possibleMoves = Moves.GetAllMoves(this, currentColor);
        }
    }
}
