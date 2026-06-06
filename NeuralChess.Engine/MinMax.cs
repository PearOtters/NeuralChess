using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.IO;

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

        public override void Play(Board board, int maximumTime)
        {
            Stopwatch searchTimer = Stopwatch.StartNew();

            List<Move> pseudoLegalMoves = MoveGenerator.GenerateAllMoves(board);
            List<Move> rootLegalMoves = [];
            foreach (Move move in pseudoLegalMoves)
            {
                if (move.IsLegal(board)) rootLegalMoves.Add(move);
            }

            if (rootLegalMoves.Count == 0)
            {
                Console.WriteLine("bestmove (none)");
                return;
            }
            else if (rootLegalMoves.Count == 1)
            {
                Console.WriteLine(rootLegalMoves[0].ToUCI());
                return;
            }

            List<Move> currentBestMoves = [rootLegalMoves[0]];

            int aiColour = board.ActiveColour;
            int multiplier = board.ActiveColour == Colour.White ? 1 : -1;

            bool abortSearch = false;
            int completedDepth = 0;

            for (int depth = 1; depth <= Depth; depth++)
            {
                List<Move> depthBestMoves = [];
                int bestGain = int.MinValue;

                foreach (Move move in rootLegalMoves)
                {
                    if (searchTimer.ElapsedMilliseconds > maximumTime)
                    {
                        abortSearch = true;
                        break;
                    }

                    move.MovePiece(board);
                    board.ActiveColour ^= 1;

                    int moveValue = RecursiveMinMaxed(board, depth - 1, aiColour, multiplier);

                    board.ActiveColour ^= 1;
                    move.ReverseMove(board);

                    if (moveValue > bestGain)
                    {
                        depthBestMoves.Clear();
                        depthBestMoves.Add(move);
                        bestGain = moveValue;
                    }
                    else if (moveValue == bestGain)
                    {
                        depthBestMoves.Add(move);
                    }
                }

                if (abortSearch)
                {
                    break;
                }

                currentBestMoves.Clear();
                currentBestMoves.AddRange(depthBestMoves);
                completedDepth = depth;
            }

            Random rng = new();
            Move bestMove = currentBestMoves[rng.Next(currentBestMoves.Count)];
            Console.WriteLine($"bestmove {bestMove.ToUCI()}");

            double timeTaken = searchTimer.ElapsedMilliseconds / 1000d;
            File.AppendAllText("log.txt", $"time taken: {timeTaken}\n");
            File.AppendAllText("log.txt", $"depth completed: {completedDepth}\n");
        }

        private static int RecursiveMinMaxed(Board board, int depth, int aiColour, int multiplier)
        {
            if (depth == 0)
            {
                return board.GetBoardValue() * multiplier;
            }

            int bestGain = aiColour == board.ActiveColour ? int.MinValue : int.MaxValue;

            List<Move> pseudoLegalMoves = MoveGenerator.GenerateAllMoves(board);
            List<Move> legalMoves = [];

            foreach (Move move in pseudoLegalMoves)
            {
                if (move.IsLegal(board))
                {
                    legalMoves.Add(move);

                    move.MovePiece(board);
                    board.ActiveColour ^= 1;

                    int moveValue = RecursiveMinMaxed(board, depth - 1, aiColour, multiplier);

                    board.ActiveColour ^= 1;
                    move.ReverseMove(board);

                    bestGain = board.ActiveColour == aiColour ? Math.Max(moveValue, bestGain) : Math.Min(moveValue, bestGain);
                }
            }

            if (legalMoves.Count == 0)
            {
                int kingSquare = BitOperations.TrailingZeroCount(board.Pieces[Piece.WhiteKing + board.ActiveColour * 6]);
                bool inCheck = Board.IsSquareAttacked(kingSquare, board.ActiveColour ^ 1, board);

                if (inCheck)
                {
                    return board.ActiveColour == aiColour ? -CheckmateScore - depth : CheckmateScore + depth;
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
