using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.Engine
{
    public enum SpecialMove
    {
        NONE,
        CASTLE,
        PROMOTION,
        EN_PASSANT
    }

    public class Move
    {
        internal SpecialMove Special;
        internal int SelectedPiece;
        internal int FromSquare, ToSquare;
        internal int PromotionPiece = -1;

        public Move(int piece, int fromSquare, int toSquare)
        {
            SelectedPiece = piece;
            FromSquare = fromSquare;
            ToSquare = toSquare;
            Special = SpecialMove.NONE;
        }

        public Move(int piece, int fromSquare, int toSquare, SpecialMove special)
        {
            SelectedPiece = piece;
            FromSquare = fromSquare;
            ToSquare = toSquare;
            Special = special;
        }

        public void MovePiece(Board board)
        {
            board.EnPassantSquare = -1;
            ulong clearMask = ~(1UL << ToSquare);
            for (int i = 0; i < 12; i++)
            {
                board.Pieces[i] &= clearMask;
            }

            board.Pieces[SelectedPiece] ^= 1UL << FromSquare;
            if (Special == SpecialMove.PROMOTION)
            {
                board.Pieces[PromotionPiece] |= (1UL << ToSquare);
            }
            else
            {
                board.Pieces[SelectedPiece] |= (1UL << ToSquare);
            }

            if (Special == SpecialMove.EN_PASSANT)
            {
                int capturedPawnSquare = SelectedPiece == Piece.WhitePawn ? ToSquare - 8 : ToSquare + 8;
                int capturedPiece = SelectedPiece == Piece.WhitePawn ? Piece.BlackPawn : Piece.WhitePawn;
                board.Pieces[capturedPiece] &= ~(1UL << capturedPawnSquare);
            }

            board.Colours[Colour.White] = board.Pieces[Piece.WhitePawn] | board.Pieces[Piece.WhiteKnight] | board.Pieces[Piece.WhiteBishop] |
                board.Pieces[Piece.WhiteRook] | board.Pieces[Piece.WhiteQueen] | board.Pieces[Piece.WhiteKing];
            board.Colours[Colour.Black] = board.Pieces[Piece.BlackPawn] | board.Pieces[Piece.BlackKnight] | board.Pieces[Piece.BlackBishop] |
                board.Pieces[Piece.BlackRook] | board.Pieces[Piece.BlackQueen] | board.Pieces[Piece.BlackKing];
            board.AllPieces = board.Colours[Colour.White] | board.Colours[Colour.Black];

            if (Special == SpecialMove.CASTLE)
            {
                if (ToSquare > FromSquare)
                {
                    new Move(SelectedPiece - 2, ToSquare + 1, ToSquare - 1).MovePiece(board);
                }
                else
                {
                    new Move(SelectedPiece - 2, ToSquare - 2, ToSquare + 1).MovePiece(board);
                }
            }

            if (SelectedPiece == Piece.WhiteKing) board.CastleRights &= ~(CastlingRights.WK | CastlingRights.WQ);
            if (SelectedPiece == Piece.BlackKing) board.CastleRights &= ~(CastlingRights.BK | CastlingRights.BQ);

            if (FromSquare == 0 || ToSquare == 0) board.CastleRights &= ~CastlingRights.WQ;
            if (FromSquare == 7 || ToSquare == 7) board.CastleRights &= ~CastlingRights.WK;
            if (FromSquare == 56 || ToSquare == 56) board.CastleRights &= ~CastlingRights.BQ;
            if (FromSquare == 63 || ToSquare == 63) board.CastleRights &= ~CastlingRights.BK;

            if (SelectedPiece == Piece.WhitePawn && FromSquare + 16 == ToSquare)
            {
                board.EnPassantSquare = FromSquare + 8;
            }
            else if (SelectedPiece == Piece.BlackPawn && FromSquare - 16 == ToSquare)
            {
                board.EnPassantSquare = FromSquare - 8;
            }
        }

        public static void Promote(Board board, int fromSquare, int promoteToPiece)
        {
            ulong pawn = 1UL << fromSquare;
            board.Pieces[promoteToPiece < 6 ? Piece.WhitePawn : Piece.BlackPawn] &= ~pawn;
            board.Pieces[promoteToPiece] |= pawn;

            board.Colours[Colour.White] = board.Pieces[Piece.WhitePawn] | board.Pieces[Piece.WhiteKnight] | board.Pieces[Piece.WhiteBishop] |
               board.Pieces[Piece.WhiteRook] | board.Pieces[Piece.WhiteQueen] | board.Pieces[Piece.WhiteKing];
            board.Colours[Colour.Black] = board.Pieces[Piece.BlackPawn] | board.Pieces[Piece.BlackKnight] | board.Pieces[Piece.BlackBishop] |
                board.Pieces[Piece.BlackRook] | board.Pieces[Piece.BlackQueen] | board.Pieces[Piece.BlackKing];
            board.AllPieces = board.Colours[Colour.White] | board.Colours[Colour.Black];
        }
    }
}
