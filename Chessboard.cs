using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    class Chessboard
    {
        public Piece[,] pieces;
        public bool isWhiteTurn;
        public Chessboard(string FENstring = Pieces.DefaultPosition)
        {
            pieces = new Piece[8, 8];
            Initialize(FENstring);
        }
        public void Initialize(string position)
        {
            pieces = Pieces.Parse_FEN(position);
        }
    }
}
