using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.Engine
{
    public class Move
    {
        public int SelectedPiece;
        public int FromSquare, ToSquare;

        public Move(int piece, int fromSquare, int toSquare)
        {
            SelectedPiece = piece;
            this.FromSquare = fromSquare;
            this.ToSquare = toSquare;
        }

        public Move(int fromSquare, int toSquare)
        {
            this.FromSquare = fromSquare;
            this.ToSquare = toSquare;
        }

        public void MovePiece(Board board)
        {
            ulong clearMask = ~(1UL << ToSquare);
            for (int i = 0; i < 12; i++)
            {
                board.Pieces[i] &= clearMask;
            }

            ulong boardPiece = board.Pieces[SelectedPiece];
            boardPiece ^= 1UL << FromSquare;
            boardPiece |= 1UL << ToSquare;
            board.Pieces[SelectedPiece] = boardPiece;

            board.Colours[Colour.White] = board.Pieces[Piece.WhitePawn] | board.Pieces[Piece.WhiteKnight] | board.Pieces[Piece.WhiteBishop] |
                board.Pieces[Piece.WhiteRook] | board.Pieces[Piece.WhiteQueen] | board.Pieces[Piece.WhiteKing];
            board.Colours[Colour.Black] = board.Pieces[Piece.BlackPawn] | board.Pieces[Piece.BlackKnight] | board.Pieces[Piece.BlackBishop] |
                board.Pieces[Piece.BlackRook] | board.Pieces[Piece.BlackQueen] | board.Pieces[Piece.BlackKing];
            board.AllPieces = board.Colours[Colour.White] | board.Colours[Colour.Black];
        }
    }
}
