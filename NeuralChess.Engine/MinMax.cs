using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace NeuralChess.Engine
{
    public class MinMax : Engine
    {
        private readonly int Depth = 5;
        private const int CheckmateScore = 1000000;

        public MinMax(int depth) : base("MinMax")
        {
            Depth = depth;
        }

        public MinMax() : base("MinMax")
        {

        }

        public override void Play(Board board)
        {
            List<Move> pseudoLegalMoves = MoveGenerator.GenerateAllMoves(board);
            List<Move> legalMoves = [];
            List<Move> bestMoves = [];

            int bestGain = int.MinValue;

            int aiColour = board.ActiveColour;
            int multiplier = board.ActiveColour == Colour.White ? 1 : -1;

            foreach (Move move in pseudoLegalMoves)
            {
                if (move.IsLegal(board))
                {
                    legalMoves.Add(move);
                    move.MovePiece(board);
                    board.ActiveColour ^= 1;

                    int moveValue = RecursiveMinMaxed(board, Depth - 1, aiColour, multiplier);

                    board.ActiveColour ^= 1;
                    move.ReverseMove(board);

                    if (moveValue > bestGain)
                    {
                        bestMoves = [];
                        bestMoves.Add(move);
                        bestGain = moveValue;
                    }
                    else if (moveValue == bestGain)
                    {
                        bestMoves.Add(move);
                    }
                }
            }
            Console.WriteLine(legalMoves.Count);
            if (legalMoves.Count > 0)
            {
                Random rng = new();
                Move bestMove = bestMoves[rng.Next(bestMoves.Count)];
                Console.WriteLine($"bestmove {bestMove.ToUCI()}");
            }
            else
            {
                Console.WriteLine("bestmove (none)");
            }
        }

        private static int RecursiveMinMaxed(Board board, int depth, int AIColour, int multiplier)
        {
            if (depth == 0)
            {
                return board.GetBoardValue() * multiplier;
            }

            int bestGain = AIColour == board.ActiveColour ? int.MinValue : int.MaxValue;
            List<Move> pseudoLegalMoves = MoveGenerator.GenerateAllMoves(board);
            List<Move> legalMoves = [];

            foreach (Move move in pseudoLegalMoves)
            {
                if (move.IsLegal(board))
                {
                    legalMoves.Add(move);

                    move.MovePiece(board);
                    board.ActiveColour ^= 1;

                    int moveValue = RecursiveMinMaxed(board, depth - 1, AIColour, multiplier);

                    board.ActiveColour ^= 1;
                    move.ReverseMove(board);

                    bestGain = board.ActiveColour == AIColour ? Math.Max(moveValue, bestGain) : Math.Min(moveValue, bestGain);
                }
            }

            if (legalMoves.Count == 0)
            {
                int kingSquare = BitOperations.TrailingZeroCount(board.Pieces[Piece.WhiteKing + board.ActiveColour * 6]);
                bool inCheck = Board.IsSquareAttacked(kingSquare, board.ActiveColour ^ 1, board);

                if (inCheck)
                {
                    return board.ActiveColour == AIColour ? -CheckmateScore - depth : CheckmateScore + depth;
                }
                else
                {
                    return 0;
                }
            }

            return bestGain;
        }
    }
}
