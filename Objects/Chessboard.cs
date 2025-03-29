using Chess.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        public LegalMoves Clone()
        {
            LegalMoves clone = new LegalMoves
            {
                WhitePawnMoves = new List<Move>(this.WhitePawnMoves),
                WhiteKnightMoves = new List<Move>(this.WhiteKnightMoves),
                WhiteBishopMoves = new List<Move>(this.WhiteBishopMoves),
                WhiteRookMoves = new List<Move>(this.WhiteRookMoves),
                WhiteQueenMoves = new List<Move>(this.WhiteQueenMoves),
                WhiteKingMoves = new List<Move>(this.WhiteKingMoves),

                BlackPawnMoves = new List<Move>(this.BlackPawnMoves),
                BlackKnightMoves = new List<Move>(this.BlackKnightMoves),
                BlackBishopMoves = new List<Move>(this.BlackBishopMoves),
                BlackRookMoves = new List<Move>(this.BlackRookMoves),
                BlackQueenMoves = new List<Move>(this.BlackQueenMoves),
                BlackKingMoves = new List<Move>(this.BlackKingMoves)
            };
            return clone;
        }
        public List<Move> GetAllMoves()
        {
            List<Move> allMoves = new List<Move>();
            allMoves.AddRange(WhitePawnMoves);
            allMoves.AddRange(WhiteKnightMoves);
            allMoves.AddRange(WhiteBishopMoves);
            allMoves.AddRange(WhiteRookMoves);
            allMoves.AddRange(WhiteQueenMoves);
            allMoves.AddRange(WhiteKingMoves);

            allMoves.AddRange(BlackPawnMoves);
            allMoves.AddRange(BlackKnightMoves);
            allMoves.AddRange(BlackBishopMoves);
            allMoves.AddRange(BlackRookMoves);
            allMoves.AddRange(BlackQueenMoves);
            allMoves.AddRange(BlackKingMoves);

            return allMoves;
        }
        public List<Move> GetAllCaptureMoves(Chessboard board)
        {
            List<Move> allMoves = GetAllMoves();
            List<Move> captureMoves = new List<Move>();
            foreach (Move move in allMoves)
                if (Helpers.IsMoveCapture(board, move))
                    captureMoves.Add(move);
            return captureMoves;
        }
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

        public int? EnPassantSquare;

        // Indices 0 and 1 for the leftmost and rightmost white rooks,
        // similarly indices 2 and 3 for the leftmost and rightmost black rooks.
        public bool[] RooksMoved;
        public bool WhiteKingMoved;
        public bool BlackKingMoved;

        public bool ValidatingMove = false;

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

            RooksMoved = [false, false, false, false];
            WhiteKingMoved = false;
            BlackKingMoved = false;

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
                EnPassantSquare = this.EnPassantSquare,
                RooksMoved = (bool[])this.RooksMoved.Clone(),
                WhiteKingMoved = this.WhiteKingMoved,
                BlackKingMoved = this.BlackKingMoved,
                isWhiteTurn = this.isWhiteTurn,
                LegalMoves = this.LegalMoves.Clone(),
            };
        }
        public ulong ComputeHash()
        {
            ulong hash = 0;

            hash ^= Helpers.ComputePieceHash(WhitePawns, 0);
            hash ^= Helpers.ComputePieceHash(WhiteKnights, 1);
            hash ^= Helpers.ComputePieceHash(WhiteBishops, 2);
            hash ^= Helpers.ComputePieceHash(WhiteRooks, 3);
            hash ^= Helpers.ComputePieceHash(WhiteQueens, 4);
            hash ^= Helpers.ComputePieceHash(WhiteKing, 5);

            hash ^= Helpers.ComputePieceHash(BlackPawns, 6);
            hash ^= Helpers.ComputePieceHash(BlackKnights, 7);
            hash ^= Helpers.ComputePieceHash(BlackBishops, 8);
            hash ^= Helpers.ComputePieceHash(BlackRooks, 9);
            hash ^= Helpers.ComputePieceHash(BlackQueens, 10);
            hash ^= Helpers.ComputePieceHash(BlackKing, 11);

            return hash;
        }
        public void UpdateMoves()
        {
            LegalMoves = Moves.GenerateLegalMoves(this);
        }
        
        public MoveData MakeMove(Move move)
        {
            MoveData moveData = new MoveData
            {
                move = move,
                piece = 0,
                isWhite = isWhiteTurn,
                capture = false,
                enPassant = false
            };

            ulong fromMask = 1UL << move.From;
            ulong toMask = 1UL << move.To;
            bool EnPassant = false;
            EnPassantSquare = null;

            if (isWhiteTurn)
            {
                if ((WhitePawns & fromMask) != 0)
                {
                    moveData.piece = Pieces.Pawn;
                    if (move.To - move.From == 16)
                    {
                        EnPassantSquare = move.From + 8;
                        EnPassant = true;
                    }
                    WhitePawns &= ~fromMask;
                    WhitePawns |= toMask;
                }
                else if ((WhiteKnights & fromMask) != 0)
                {
                    moveData.piece = Pieces.Knight;
                    WhiteKnights &= ~fromMask;
                    WhiteKnights |= toMask;
                }
                else if ((WhiteBishops & fromMask) != 0)
                {
                    moveData.piece = Pieces.Bishop;
                    WhiteBishops &= ~fromMask;
                    WhiteBishops |= toMask;
                }
                else if ((WhiteRooks & fromMask) != 0)
                {
                    moveData.piece = Pieces.Rook;
                    // Update status for castling
                    if (move.From == 0 && !RooksMoved[0])
                        RooksMoved[0] = true;
                    else if (move.From == 7 && !RooksMoved[1])
                        RooksMoved[1] = true;

                    WhiteRooks &= ~fromMask;
                    WhiteRooks |= toMask;
                }
                else if ((WhiteQueens & fromMask) != 0)
                {
                    moveData.piece = Pieces.Queen;
                    WhiteQueens &= ~fromMask;
                    WhiteQueens |= toMask;
                }
                else if ((WhiteKing & fromMask) != 0)
                {
                    moveData.piece = Pieces.King;
                    // Handle castling - move the rook
                    // Kingside
                    if (move.To - move.From == 2)
                    {
                        WhiteRooks &= ~(1UL << 7);
                        WhiteRooks |= (1UL << (move.To - 1));
                        WhiteKingMoved = true;
                    }
                    // Queenside
                    else if (move.To - move.From == -2)
                    {
                        WhiteRooks &= ~(1UL << 0);
                        WhiteRooks |= (1UL << (move.To + 1));
                        WhiteKingMoved = true;
                    }
                    WhiteKing &= ~fromMask;
                    WhiteKing |= toMask;
                }
                if ((AllBlackPieces & toMask) != 0)
                {
                    moveData.capture = true;
                    moveData.enPassant = EnPassant;
                    CapturePiece(move.To, EnPassant);
                }
            }
            else
            {
                if ((BlackPawns & fromMask) != 0)
                {
                    moveData.piece = Pieces.Pawn;
                    if (move.To - move.From == -16)
                    {
                        EnPassantSquare = move.From - 8;
                        EnPassant = true;
                    }
                    BlackPawns &= ~fromMask;
                    BlackPawns |= toMask;
                }
                else if ((BlackKnights & fromMask) != 0)
                {
                    moveData.piece = Pieces.Knight;
                    BlackKnights &= ~fromMask;
                    BlackKnights |= toMask;
                }
                else if ((BlackBishops & fromMask) != 0)
                {
                    moveData.piece = Pieces.Bishop;
                    BlackBishops &= ~fromMask;
                    BlackBishops |= toMask;
                }
                else if ((BlackRooks & fromMask) != 0)
                {
                    moveData.piece = Pieces.Rook;
                    // Update status for castling
                    if (move.From == 56 && !RooksMoved[2])
                        RooksMoved[2] = true;
                    else if (move.From == 63 && !RooksMoved[3])
                        RooksMoved[3] = true;

                    BlackRooks &= ~fromMask;
                    BlackRooks |= toMask;
                }
                else if ((BlackQueens & fromMask) != 0)
                {
                    moveData.piece = Pieces.Queen;
                    BlackQueens &= ~fromMask;
                    BlackQueens |= toMask;
                }
                else if ((BlackKing & fromMask) != 0)
                {
                    moveData.piece = Pieces.King;
                    // Handle castling - move the rook
                    // Kingside
                    if (move.To - move.From == 2)
                    {
                        BlackRooks &= ~(1UL << 63);
                        BlackRooks |= (1UL << (move.To - 1));
                        BlackKingMoved = true;
                    }
                    // Queenside
                    else if (move.To - move.From == -2)
                    {
                        BlackRooks &= ~(1UL << 56);
                        BlackRooks |= (1UL << (move.To + 1));
                        BlackKingMoved = true;
                    }
                    BlackKing &= ~fromMask;
                    BlackKing |= toMask;
                }
                if ((AllWhitePieces & toMask) != 0)
                {
                    moveData.capture = true;
                    moveData.enPassant = EnPassant;
                    CapturePiece(move.To, EnPassant);
                }
            }
            isWhiteTurn = !isWhiteTurn;
            if (!ValidatingMove)
                UpdateMoves();
            return moveData;
        }
        public void CapturePiece(int square, bool enPassant = false)
        {
            bool captureWhite = isWhiteTurn ? false : true;
            ulong mask = 1UL << square;

            if (captureWhite)
            {
                if (enPassant)
                    mask = 1UL << (square + 8);
                WhitePawns &= ~mask;
                WhiteKnights &= ~mask;
                WhiteBishops &= ~mask;
                WhiteRooks &= ~mask;
                WhiteQueens &= ~mask;
                WhiteKing &= ~mask;
            }
            else
            {
                if (enPassant)
                    mask = 1UL << (square - 8);
                BlackPawns &= ~mask;
                BlackKnights &= ~mask;
                BlackBishops &= ~mask;
                BlackRooks &= ~mask;
                BlackQueens &= ~mask;
                BlackKing &= ~mask;
            }
        }
        public void UnmakeMove(MoveData moveData)
        {
            ulong fromMask = 1UL << moveData.move.From;
            ulong toMask = 1UL << moveData.move.To;

            if (moveData.isWhite)
            {
                switch (moveData.piece)
                {
                    case Pieces.Pawn:
                        WhitePawns &= ~toMask;
                        WhitePawns |= fromMask;
                        break;
                    case Pieces.Knight:
                        WhiteKnights &= ~toMask;
                        WhiteKnights |= fromMask;
                        break;
                    case Pieces.Bishop:
                        WhiteBishops &= ~toMask;
                        WhiteBishops |= fromMask;
                        break;
                    case Pieces.Rook:
                        WhiteRooks &= ~toMask;
                        WhiteRooks |= fromMask;
                        if (moveData.move.From == 0)
                            RooksMoved[0] = false;
                        else if (moveData.move.From == 7)
                            RooksMoved[1] = false;
                        break;
                    case Pieces.Queen:
                        WhiteQueens &= ~toMask;
                        WhiteQueens |= fromMask;
                        break;
                    case Pieces.King:
                        WhiteKing &= ~toMask;
                        WhiteKing |= fromMask;
                        if (moveData.move.To - moveData.move.From == 2)
                        {
                            WhiteRooks &= ~(1UL << (moveData.move.To - 1));
                            WhiteRooks |= (1UL << 7);
                            WhiteKingMoved = false;
                        }
                        else if (moveData.move.To - moveData.move.From == -2)
                        {
                            WhiteRooks &= ~(1UL << (moveData.move.To + 1));
                            WhiteRooks |= (1UL << 0);
                            WhiteKingMoved = false;
                        }
                        break;
                }
                if (moveData.capture)
                {
                    UncapturePiece(moveData.move.To, moveData.enPassant);
                }
            }
            else
            {
                switch (moveData.piece)
                {
                    case Pieces.Pawn:
                        BlackPawns &= ~toMask;
                        BlackPawns |= fromMask;
                        break;
                    case Pieces.Knight:
                        BlackKnights &= ~toMask;
                        BlackKnights |= fromMask;
                        break;
                    case Pieces.Bishop:
                        BlackBishops &= ~toMask;
                        BlackBishops |= fromMask;
                        break;
                    case Pieces.Rook:
                        BlackRooks &= ~toMask;
                        BlackRooks |= fromMask;
                        if (moveData.move.From == 56)
                            RooksMoved[2] = false;
                        else if (moveData.move.From == 63)
                            RooksMoved[3] = false;
                        break;
                    case Pieces.Queen:
                        BlackQueens &= ~toMask;
                        BlackQueens |= fromMask;
                        break;
                    case Pieces.King:
                        BlackKing &= ~toMask;
                        BlackKing |= fromMask;
                        if (moveData.move.To - moveData.move.From == 2)
                        {
                            BlackRooks &= ~(1UL << (moveData.move.To - 1));
                            BlackRooks |= (1UL << 63);
                            BlackKingMoved = false;
                        }
                        else if (moveData.move.To - moveData.move.From == -2)
                        {
                            BlackRooks &= ~(1UL << (moveData.move.To + 1));
                            BlackRooks |= (1UL << 56);
                            BlackKingMoved = false;
                        }
                        break;
                }
                if (moveData.capture)
                {
                    UncapturePiece(moveData.move.To, moveData.enPassant);
                }
            }
            isWhiteTurn = !isWhiteTurn;
            EnPassantSquare = null;
        }

        private void UncapturePiece(int square, bool enPassant = false)
        {
            bool captureWhite = isWhiteTurn ? true : false;
            ulong mask = 1UL << square;

            if (captureWhite)
            {
                if (enPassant)
                    mask = 1UL << (square - 8);
                WhitePawns |= mask;
                WhiteKnights |= mask;
                WhiteBishops |= mask;
                WhiteRooks |= mask;
                WhiteQueens |= mask;
                WhiteKing |= mask;
            }
            else
            {
                if (enPassant)
                    mask = 1UL << (square + 8);
                BlackPawns |= mask;
                BlackKnights |= mask;
                BlackBishops |= mask;
                BlackRooks |= mask;
                BlackQueens |= mask;
                BlackKing |= mask;
            }
        }
    }
}