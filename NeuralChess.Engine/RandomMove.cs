using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.Engine
{
    public class RandomMove : Engine
    {
        public RandomMove() : base("Random")
        {

        }

        public override void Play(Board board, int maximumTime, int maximumDepth)
        {
            List<Move> pseudoMoves = MoveGenerator.GenerateAllMoves(board);
            List<Move> legalMoves = [];

            foreach (Move m in pseudoMoves)
            {
                if (m.IsLegal(board)) legalMoves.Add(m);
            }

            if (legalMoves.Count > 0)
            {
                Random rng = new();
                Move bestMove = legalMoves[rng.Next(legalMoves.Count)];
                Console.WriteLine($"bestmove {bestMove.ToUCI()}");
            }
            else
            {
                Console.WriteLine("bestmove (none)");
            }
        }
    }
}
