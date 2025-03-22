using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    struct Position
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
    }
    struct Move
    {
        public Position startPosition;
        public Position targetPosition;
        public bool capture;
    }
    class Moves
    {
        public static List<Move> GetSlidingMoves(Position startSquare, Piece piece)
        {
            int startRow = startSquare.row;
            int startColumn = startSquare.column;
            List<Move> moves = new List<Move>();
            for (int direction = 0; direction < 8; direction++)
            {
                for (int num = 0; num < Constants.SquaresToSide[startRow, startColumn][direction]; num++)
                {
                    Position offset = new Position { row = Constants.Directions[direction].row * (num + 1), column = Constants.Directions[direction].column * (num + 1) };
                    Position targetSquare = startSquare + offset;
                    moves.Add(new Move { startPosition = startSquare, targetPosition = targetSquare, capture = false });
                }
            }
            return moves;
        }
    }
}
