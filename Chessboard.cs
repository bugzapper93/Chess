using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess
{
    internal class Chessboard
    {
        private int BoardSize;
        private int SquareSize;
        public Panel[,] squares;
        public Piece.PiecePanelPair[,] pieces;
        public Panel selectedPiece;
        public Point originalLocation;
        public int selectedRow;
        public int selectedCol;

        public Color backColor1 = Color.RosyBrown;
        public Color backColor2 = Color.Wheat;
        public Color highlightColor = Color.DarkBlue;

        public string currentMoves = "";

        public int? enPassantTargetRow = null;
        public int? enPassantTargetCol = null;
        public Chessboard(int boardSize, int squareSize)
        {
            BoardSize = boardSize;
            SquareSize = squareSize;
            squares = new Panel[BoardSize, BoardSize];
            pieces = new Piece.PiecePanelPair[BoardSize, BoardSize];

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    Panel square = new Panel
                    {
                        Size = new Size(SquareSize, SquareSize),
                        Location = new Point(col * SquareSize, row * SquareSize),
                    };
                    squares[row, col] = square;
                }
            }
            SetBackColor();
        }
        public void FillBoard(string layout)
        {
            pieces = Tools.ParseFEN(layout);
        }
        public bool CheckValidity(int pieceValue, int startRow, int startCol, int endRow, int endCol)
        {
            int rowDiff = Math.Abs(endRow - startRow);
            int colDiff = Math.Abs(endCol - startCol);
            int color = pieceValue & (Piece.White | Piece.Black);
            int piece = pieceValue & 7;

            if (rowDiff == 0 && colDiff == 0) return false;
            if ((pieces[endRow, endCol].number & (Piece.White | Piece.Black)) == color) return false;

            switch (piece)
            {
                case Piece.Pawn:
                    if (color == Piece.White)
                    {
                        if (rowDiff == 1 && startCol == endCol 
                            && endRow < startRow 
                            && pieces[endRow, endCol].number == 0) 
                            return true;
                        if ((rowDiff == 2 && startCol == endCol && startRow == 6 && endRow == 4)
                            && pieces[endRow, endCol].number == 0
                            && CheckPathValidity(startRow, startCol, endRow, endCol)) 
                            return true; // Pion może wykonać ruch o dwa pola do przodu, jeśli jest w pozycji startowej
                        if (colDiff == 1 && rowDiff == 1 && endRow < startRow)
                        {
                            if (pieces[endRow, endCol].number != 0)
                                return true;

                            // En passant
                            if (enPassantTargetRow.HasValue && enPassantTargetCol.HasValue &&
                                endRow == enPassantTargetRow.Value && endCol == enPassantTargetCol.Value
                                && pieces[endRow, endCol].number == 0)
                                return true;
                        }
                    }
                    else
                    {
                        if (rowDiff == 1 && startCol == endCol 
                            && endRow > startRow
                            && pieces[endRow, endCol].number == 0) 
                            return true;
                        if ((rowDiff == 2 && startCol == endCol && startRow == 1 && endRow == 3)
                            && pieces[endRow, endCol].number == 0
                            && CheckPathValidity(startRow, startCol, endRow, endCol)) 
                            return true;
                        if (colDiff == 1 && rowDiff == 1 && endRow > startRow)
                        {
                            if (pieces[endRow, endCol].number != 0)
                                return true;

                            // En passant
                            if (enPassantTargetRow.HasValue && enPassantTargetCol.HasValue && 
                                endRow == enPassantTargetRow.Value && endCol == enPassantTargetCol.Value
                                && pieces[endRow, endCol].number == 0)
                                return true;
                        }
                    }
                    break;
                case Piece.Knight:
                    if (rowDiff == 2 && colDiff == 1) 
                        return true;
                    if (rowDiff == 1 && colDiff == 2) 
                        return true;
                    break;
                case Piece.Bishop:
                    if (rowDiff == colDiff 
                        && CheckPathValidity(startRow, startCol, endRow, endCol)) 
                        return true;
                    break;
                case Piece.Rook:
                    if ((startRow == endRow || startCol == endCol) 
                        && CheckPathValidity(startRow, startCol, endRow, endCol)) 
                        return true;
                    break;
                case Piece.Queen:
                    if (rowDiff == colDiff
                        && CheckPathValidity(startRow, startCol, endRow, endCol))
                        return true;
                    if ((startRow == endRow || startCol == endCol) 
                        && CheckPathValidity(startRow, startCol, endRow, endCol)) 
                        return true;
                    break;
                case Piece.King:
                    if (rowDiff <= 1 && colDiff <= 1) 
                        return true;
                    if (rowDiff == 0 && colDiff == 2
                        && !pieces[startRow, startCol].hasMoved)
                    {
                        if (endCol == 6)
                        {
                            if (((pieces[startRow, 7].number & 7) == Piece.Rook) & !pieces[startRow, 7].hasMoved)
                            {
                                if (pieces[startRow, 5].number == 0
                                    && pieces[startRow, 6].number == 0)
                                    return true;
                            }
                        }
                        if (endCol == 2)
                        {
                            if (((pieces[startRow, 0].number & 7) == Piece.Rook) & !pieces[startRow, 0].hasMoved)
                            {
                                if (pieces[startRow, 1].number == 0
                                    && pieces[startRow, 2].number == 0
                                    && pieces[startRow, 3].number == 0)
                                    return true;
                            }
                        }
                    }
                    break;
            }
            return false;
        }
        private bool CheckPathValidity(int startRow, int startCol, int endRow, int endCol)
        {
            int rowSkip = (endRow > startRow) ? 1 : (endRow < startRow) ? -1 : 0;
            int colSkip = (endCol > startCol) ? 1 : (endCol < startCol) ? -1 : 0;

            int row = startRow + rowSkip;
            int col = startCol + colSkip;

            while (row != endRow || col != endCol)
            {
                if (pieces[row, col].number != 0)
                    return false;

                row += rowSkip;
                col += colSkip;
            }
            return true;
        }
        public void HighlightLegalMoves(int pieceValue, int startRow, int startCol)
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (CheckValidity(pieceValue, startRow, startCol, row, col))
                        squares[row, col].BackColor = highlightColor;
                }
            }
        }
        public void SetBackColor()
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    squares[row, col].BackColor = (row + col) % 2 == 0 ? backColor1 : backColor2;
                }
            }
        }
    }
}
