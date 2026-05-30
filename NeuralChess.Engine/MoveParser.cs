using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace NeuralChess.Engine
{
    public class MoveParser
    {
        public static bool TryPlayMove(Board board, string moveStr)
        {
            if (string.IsNullOrWhiteSpace(moveStr) || moveStr.Length < 5) return false;
            char fromFile = char.ToUpper(moveStr[0]);
            char fromRank = moveStr[1];

            char toFile;
            char toRank;
            int promotionPiece = -1;
            if (Char.ToLower(moveStr[^1]) == 'q' || Char.ToLower(moveStr[^1]) == 'n' || Char.ToLower(moveStr[^1]) == 'k' || Char.ToLower(moveStr[^1]) == 'b' || Char.ToLower(moveStr[^1]) == 'r')
            {
                toFile = char.ToUpper(moveStr[^3]);
                toRank = moveStr[^2];

                char prom = Char.ToLower(moveStr[^1]);
                if (prom == 'q') promotionPiece = Piece.WhiteQueen + board.ActiveColour * 6;
                else if (prom == 'r') promotionPiece = Piece.WhiteRook + board.ActiveColour * 6;
                else if (prom == 'k' || prom == 'n') promotionPiece = Piece.WhiteKnight + board.ActiveColour * 6;
                else promotionPiece = Piece.WhiteBishop + board.ActiveColour * 6;
            }
            else
            {
                toFile = char.ToUpper(moveStr[^2]);
                toRank = moveStr[^1];
            }

            int fromSquare = (fromRank - '1') * 8 + (fromFile - 'A');
            int toSquare = (toRank - '1') * 8 + (toFile - 'A');

            List<Move> moves = MoveGenerator.GenerateAllMoves(board);
            Move? validMove = moves.FirstOrDefault(m => m.ToSquare == toSquare && m.FromSquare == fromSquare && m.PromotionPiece == promotionPiece);

            if (validMove == null) return false;
            if (!IsLegal(board, validMove)) return false;

            validMove.MovePiece(board);
            board.ActiveColour = board.ActiveColour == Colour.White ? Colour.Black : Colour.White;

            return true;
        }

        public static bool IsLegal(Board board, Move validMove)
        {
            Board clone = board.CloneBoard();
            validMove.MovePiece(clone);
            int kingIndex = BitOperations.TrailingZeroCount(clone.ActiveColour == Colour.White ? clone.Pieces[Piece.WhiteKing] : clone.Pieces[Piece.BlackKing]);
            int attackingColour = clone.ActiveColour == Colour.White ? Colour.Black : Colour.White;
            return !Board.IsSquareAttacked(kingIndex, attackingColour, clone);
        }
    }
}
