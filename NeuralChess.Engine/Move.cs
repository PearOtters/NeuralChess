using System;
using System.Collections.Generic;
using System.Numerics;
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
        internal int CapturedPiece = -1;
        internal uint PrevCastleRights = 0;
        internal int PrevEnPassant = -1;
        internal int Score = 0;

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
            PrevCastleRights = board.CastleRights;
            PrevEnPassant = board.EnPassantSquare;

            board.EnPassantSquare = -1;
            ulong clearMask = ~(1UL << ToSquare);
            for (int i = 0; i < 12; i++)
            {
                if ((board.Pieces[i] & ~clearMask) != 0)
                {
                    board.Pieces[i] &= clearMask;
                    CapturedPiece = i;
                }
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
                CapturedPiece = SelectedPiece == Piece.WhitePawn ? Piece.BlackPawn : Piece.WhitePawn;
                board.Pieces[CapturedPiece] &= ~(1UL << capturedPawnSquare);
            }

            if (Special == SpecialMove.CASTLE)
            {
                int rookPiece = SelectedPiece - 2;
                if (ToSquare > FromSquare)
                {
                    board.Pieces[rookPiece] ^= 1UL << (ToSquare + 1);
                    board.Pieces[rookPiece] |= 1UL << (ToSquare - 1);
                }
                else
                {
                    board.Pieces[rookPiece] ^= 1UL << (ToSquare - 2);
                    board.Pieces[rookPiece] |= 1UL << (ToSquare + 1);
                }
            }

            board.Colours[Colour.White] = board.Pieces[Piece.WhitePawn] | board.Pieces[Piece.WhiteKnight] | board.Pieces[Piece.WhiteBishop] |
                board.Pieces[Piece.WhiteRook] | board.Pieces[Piece.WhiteQueen] | board.Pieces[Piece.WhiteKing];
            board.Colours[Colour.Black] = board.Pieces[Piece.BlackPawn] | board.Pieces[Piece.BlackKnight] | board.Pieces[Piece.BlackBishop] |
                board.Pieces[Piece.BlackRook] | board.Pieces[Piece.BlackQueen] | board.Pieces[Piece.BlackKing];
            board.AllPieces = board.Colours[Colour.White] | board.Colours[Colour.Black];

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

        public void ReverseMove(Board board)
        {
            if (CapturedPiece != -1)
            {
                if (Special == SpecialMove.EN_PASSANT)
                {
                    int capturedPawnSquare = SelectedPiece == Piece.WhitePawn ? ToSquare - 8 : ToSquare + 8;
                    board.Pieces[CapturedPiece] |= 1UL << capturedPawnSquare;
                }
                else
                {
                    board.Pieces[CapturedPiece] |= 1UL << ToSquare;
                }
            }

            board.Pieces[SelectedPiece] |= 1UL << FromSquare;
            if (Special == SpecialMove.PROMOTION)
            {
                board.Pieces[PromotionPiece] ^= 1UL << ToSquare;
            }
            else
            {
                board.Pieces[SelectedPiece] ^= 1UL << ToSquare;
            }

            if (Special == SpecialMove.CASTLE)
            {
                int rookPiece = SelectedPiece - 2;
                if (ToSquare > FromSquare)
                {
                    board.Pieces[rookPiece] ^= 1UL << (ToSquare - 1);
                    board.Pieces[rookPiece] |= 1UL << (ToSquare + 1);
                }
                else
                {
                    board.Pieces[rookPiece] ^= 1UL << (ToSquare + 1);
                    board.Pieces[rookPiece] |= 1UL << (ToSquare - 2);
                }
            }

            board.Colours[Colour.White] = board.Pieces[Piece.WhitePawn] | board.Pieces[Piece.WhiteKnight] | board.Pieces[Piece.WhiteBishop] |
                board.Pieces[Piece.WhiteRook] | board.Pieces[Piece.WhiteQueen] | board.Pieces[Piece.WhiteKing];
            board.Colours[Colour.Black] = board.Pieces[Piece.BlackPawn] | board.Pieces[Piece.BlackKnight] | board.Pieces[Piece.BlackBishop] |
                board.Pieces[Piece.BlackRook] | board.Pieces[Piece.BlackQueen] | board.Pieces[Piece.BlackKing];
            board.AllPieces = board.Colours[Colour.White] | board.Colours[Colour.Black];

            board.CastleRights = PrevCastleRights;
            board.EnPassantSquare = PrevEnPassant;
        }

        public string ToUCI()
        {
            char fromFile = (char)('a' + (FromSquare % 8));
            char fromRank = (char)('1' + (FromSquare / 8));
            char toFile = (char)('a' + (ToSquare % 8));
            char toRank = (char)('1' + (ToSquare / 8));

            string uci = $"{fromFile}{fromRank}{toFile}{toRank}";

            if (Special == SpecialMove.PROMOTION)
            {
                int pieceType = PromotionPiece % 6;
                if (pieceType == Piece.WhiteQueen) uci += "q";
                else if (pieceType == Piece.WhiteRook) uci += "r";
                else if (pieceType == Piece.WhiteKnight) uci += "n";
                else if (pieceType == Piece.WhiteBishop) uci += "b";
            }

            return uci;
        }

        public static Move GetMoveFromUCI(Board board, string uciString)
        {
            char fromFile = uciString[0];
            char fromRank = uciString[1];
            char toFile = uciString[2];
            char toRank = uciString[3];

            int fromSquare = fromFile - 'a' + (fromRank - '1') * 8;
            int toSquare = toFile - 'a' + (toRank - '1') * 8;

            int promotionPiece = -1;

            if (uciString.Length > 4)
            {
                char promPiece = uciString[4];
                if (promPiece == 'q') promotionPiece = Piece.WhiteQueen + board.ActiveColour * 6;
                else if (promPiece == 'r') promotionPiece = Piece.WhiteRook + board.ActiveColour * 6;
                else if (promPiece == 'n') promotionPiece = Piece.WhiteKnight + board.ActiveColour * 6;
                else if (promPiece == 'b') promotionPiece = Piece.WhiteBishop + board.ActiveColour * 6;
            }

            List<Move> legalMoves = MoveGenerator.GenerateAllMoves(board);

            return legalMoves.First(m =>
                m.FromSquare == fromSquare &&
                m.ToSquare == toSquare &&
                m.PromotionPiece == promotionPiece);
        }

        public bool IsLegal(Board board)
        {
            MovePiece(board);
            int kingIndex = BitOperations.TrailingZeroCount(board.ActiveColour == Colour.White ? board.Pieces[Piece.WhiteKing] : board.Pieces[Piece.BlackKing]);
            int attackingColour = board.ActiveColour == Colour.White ? Colour.Black : Colour.White;
            bool squareAttacked = !board.IsSquareAttacked(kingIndex, attackingColour);
            ReverseMove(board);
            return squareAttacked;
        }

        public int GetValue(Board board)
        {
            int originalVal = board.GetBoardValue();
            MovePiece(board);
            int moveVal = board.GetBoardValue() - originalVal;
            ReverseMove(board);
            return moveVal;
        }
    }
}
