using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NeuralChess.Engine
{
    public class MakeMove
    {
        private int FromSquare;
        private int ToSquare;
        private Move? move;

        public MakeMove(Board board, string moveStr)
        {
            if (!CheckFormat(moveStr)) return;
            SplitMove(moveStr);

            List<Move> moves = MoveGenerator.GenerateAllMoves(board);
            this.move = moves.FirstOrDefault(m => m.ToSquare == ToSquare && m.FromSquare == FromSquare);

            if (this.move == null) return;
            if (!CheckLegal()) return;

            move.MovePiece(board);
            board.ActiveColour = ~board.ActiveColour;
        }

        public static bool CheckFormat(string move)
        {
            string pattern = @"^[A-H][1-8]\s*->?\s*[A-H][1-8]$";
            return Regex.IsMatch(move, pattern, RegexOptions.IgnoreCase);
        }

        public void SplitMove(string move)
        {
            String[] fromTo = Regex.Split(move, @"\s*->?\s*");
            FromSquare = (int.Parse(fromTo[0][1].ToString())-1) * 8 + fromTo[0].ToUpper()[0] - 65;
            ToSquare = (int.Parse(fromTo[1][1].ToString()) - 1) * 8 + fromTo[1].ToUpper()[0] - 65;
        }

        public bool CheckLegal()
        {
            return true;
        }
    }
}
