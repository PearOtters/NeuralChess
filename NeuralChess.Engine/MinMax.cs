using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace NeuralChess.Engine
{
    public class MinMax : Engine
    {
        private readonly int Depth = 5;

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

            int multiplier = board.ActiveColour == Colour.White ? 1 : -1;

            foreach (Move move in pseudoLegalMoves)
            {
                if (move.IsLegal(board))
                {
                    legalMoves.Add(move);
                    Board clone = board.CloneBoard();
                    move.MovePiece(clone);
                    int moveValue = RecursiveMinMaxed(clone, Depth - 1, board.ActiveColour, multiplier);
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

            board.ActiveColour ^= 1;

            int bestGain = AIColour == board.ActiveColour ? int.MinValue : int.MaxValue;
            List<Move> pseudoLegalMoves = MoveGenerator.GenerateAllMoves(board);
            List<Move> legalMoves = [];

            foreach (Move move in pseudoLegalMoves)
            {
                if (move.IsLegal(board))
                {
                    legalMoves.Add(move);
                    Board clone = board.CloneBoard();
                    move.MovePiece(clone);
                    int moveValue = RecursiveMinMaxed(clone, depth - 1, AIColour, multiplier);
                    bestGain = board.ActiveColour == AIColour ? Math.Max(moveValue, bestGain) : Math.Min(moveValue, bestGain);
                }
            }

            if (legalMoves.Count == 0)
            {
                if (Board.IsSquareAttacked(BitOperations.TrailingZeroCount(board.Pieces[Piece.WhiteKing + board.ActiveColour * 6]), board.ActiveColour ^ 1, board))
                {
                    return -999;
                }
                else if (Board.IsSquareAttacked(BitOperations.TrailingZeroCount(board.Pieces[Piece.WhiteKing + (board.ActiveColour ^ 1) * 6]), board.ActiveColour, board))
                {
                    return 999;
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
