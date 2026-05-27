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

            char toFile = char.ToUpper(moveStr[moveStr.Length - 2]);
            char toRank = moveStr[moveStr.Length - 1];

            int fromSquare = (fromRank - '1') * 8 + (fromFile - 'A');
            int toSquare = (toRank - '1') * 8 + (toFile - 'A');

            List<Move> moves = MoveGenerator.GenerateAllMoves(board);
            Move? validMove = moves.FirstOrDefault(m => m.ToSquare == toSquare && m.FromSquare == fromSquare);

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
