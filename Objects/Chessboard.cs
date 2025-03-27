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
    public class LegalMoves
    {
        public List<Move> WhitePawnMoves = new List<Move>();
        public List<Move> WhiteKnightMoves = new List<Move>();
        public List<Move> WhiteBishopMoves = new List<Move>();
        public List<Move> WhiteRookMoves = new List<Move>();
        public List<Move> WhiteQueenMoves = new List<Move>();
        public List<Move> WhiteKingMoves = new List<Move>();

        public List<Move> BlackPawnMoves = new List<Move>();
        public List<Move> BlackKnightMoves = new List<Move>();
        public List<Move> BlackBishopMoves = new List<Move>();
        public List<Move> BlackRookMoves = new List<Move>();
        public List<Move> BlackQueenMoves = new List<Move>();
        public List<Move> BlackKingMoves = new List<Move>();
    }
    public class Chessboard
    {
        public ulong WhitePawns;
        public ulong WhiteKnights;
        public ulong WhiteBishops;
        public ulong WhiteRooks;
        public ulong WhiteQueens;
        public ulong WhiteKing;

        public ulong BlackPawns;
        public ulong BlackKnights;
        public ulong BlackBishops;
        public ulong BlackRooks;
        public ulong BlackQueens;
        public ulong BlackKing;

        public ulong? EnPassantSquare;

        public bool isWhiteTurn;
        public LegalMoves LegalMoves;

        public Chessboard()
        {
            WhitePawns = Constants.DefaultWhitePawns;
            WhiteKnights = Constants.DefaultWhiteKnights;
            WhiteBishops = Constants.DefaultWhiteBishops;
            WhiteRooks = Constants.DefaultWhiteRooks;
            WhiteQueens = Constants.DefaultWhiteQueens;
            WhiteKing = Constants.DefaultWhiteKing;

            BlackPawns = Constants.DefaultBlackPawns;
            BlackKnights = Constants.DefaultBlackKnights;
            BlackBishops = Constants.DefaultBlackBishops;
            BlackRooks = Constants.DefaultBlackRooks;
            BlackQueens = Constants.DefaultBlackQueens;
            BlackKing = Constants.DefaultBlackKing;

            isWhiteTurn = true;
            LegalMoves = new LegalMoves();
        }
        public ulong AllWhitePieces => WhitePawns | WhiteKnights | WhiteBishops | WhiteRooks | WhiteQueens | WhiteKing;
        public ulong AllBlackPieces => BlackPawns | BlackKnights | BlackBishops | BlackRooks | BlackQueens | BlackKing;
        public ulong AllPieces => AllWhitePieces | AllBlackPieces;
        public Chessboard Clone()
        {
            return new Chessboard
            {
                WhiteKing = this.WhiteKing,
                WhitePawns = this.WhitePawns,
                WhiteKnights = this.WhiteKnights,
                WhiteBishops = this.WhiteBishops,
                WhiteRooks = this.WhiteRooks,
                WhiteQueens = this.WhiteQueens,
                BlackKing = this.BlackKing,
                BlackPawns = this.BlackPawns,
                BlackKnights = this.BlackKnights,
                BlackBishops = this.BlackBishops,
                BlackRooks = this.BlackRooks,
                BlackQueens = this.BlackQueens,
                isWhiteTurn = this.isWhiteTurn,
                LegalMoves = this.LegalMoves,
            };
        }
        public void UpdateMoves()
        {
            LegalMoves = Moves.GenerateLegalMoves(this);
        }
        public void MakeMove(Move move, bool redoMoves = true)
        {
            ulong fromMask = 1UL << move.From;
            ulong toMask = 1UL << move.To;

            if (isWhiteTurn)
            {
                if ((WhitePawns & fromMask) != 0)
                {
                    WhitePawns &= ~fromMask;
                    WhitePawns |= toMask;
                }
                else if ((WhiteKnights & fromMask) != 0)
                {
                    WhiteKnights &= ~fromMask;
                    WhiteKnights |= toMask;
                }
                else if ((WhiteBishops & fromMask) != 0)
                {
                    WhiteBishops &= ~fromMask;
                    WhiteBishops |= toMask;
                }
                else if ((WhiteRooks & fromMask) != 0)
                {
                    WhiteRooks &= ~fromMask;
                    WhiteRooks |= toMask;
                }
                else if ((WhiteQueens & fromMask) != 0)
                {
                    WhiteQueens &= ~fromMask;
                    WhiteQueens |= toMask;
                }
                else if ((WhiteKing & fromMask) != 0)
                {
                    WhiteKing &= ~fromMask;
                    WhiteKing |= toMask;
                }
                CapturePiece(move.To);
            }
            else
            {
                if ((BlackPawns & fromMask) != 0)
                {
                    BlackPawns &= ~fromMask;
                    BlackPawns |= toMask;
                }
                else if ((BlackKnights & fromMask) != 0)
                {
                    BlackKnights &= ~fromMask;
                    BlackKnights |= toMask;
                }
                else if ((BlackBishops & fromMask) != 0)
                {
                    BlackBishops &= ~fromMask;
                    BlackBishops |= toMask;
                }
                else if ((BlackRooks & fromMask) != 0)
                {
                    BlackRooks &= ~fromMask;
                    BlackRooks |= toMask;
                }
                else if ((BlackQueens & fromMask) != 0)
                {
                    BlackQueens &= ~fromMask;
                    BlackQueens |= toMask;
                }
                else if ((BlackKing & fromMask) != 0)
                {
                    BlackKing &= ~fromMask;
                    BlackKing |= toMask;
                }
                CapturePiece(move.To);
            }
            isWhiteTurn = !isWhiteTurn;
            if (redoMoves)
                UpdateMoves();
        }
        public void CapturePiece(int square)
        {
            bool captureWhite = isWhiteTurn ? false : true;
            ulong mask = 1UL << square;
            if (captureWhite)
            {
                WhitePawns &= ~mask;
                WhiteKnights &= ~mask;
                WhiteBishops &= ~mask;
                WhiteRooks &= ~mask;
                WhiteQueens &= ~mask;
                WhiteKing &= ~mask;
            }
            else
            {
                BlackPawns &= ~mask;
                BlackKnights &= ~mask;
                BlackBishops &= ~mask;
                BlackRooks &= ~mask;
                BlackQueens &= ~mask;
                BlackKing &= ~mask;
            }
        }
    }
}