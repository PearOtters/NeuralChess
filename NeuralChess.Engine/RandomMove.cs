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
            Span<Move> pseudoMoves = stackalloc Move[218];
            int pseudoMovesCount = 0;
            MoveGenerator.GenerateAllMoves(board, ref pseudoMoves, ref pseudoMovesCount);
            Span<Move> legalMoves = stackalloc Move[218];
            int legalMovesCount = 0;

            for (int i = 0; i < pseudoMovesCount; i++)
            {
                Move m = pseudoMoves[i];
                if (m.IsLegal(board)) legalMoves[legalMovesCount++] = m;
            }

            if (legalMovesCount > 0)
            {
                Random rng = new();
                Move bestMove = legalMoves[rng.Next(legalMovesCount)];
                Console.WriteLine($"bestmove {bestMove.ToUCI()}");
            }
            else
            {
                Console.WriteLine("bestmove (none)");
            }
        }
    }
}
