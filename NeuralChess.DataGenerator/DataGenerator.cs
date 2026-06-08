using NeuralChess.Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.DataGenerator
{
    public class DataGenerator
    {
        public static void PopulateData(int numOfPositions, int? seed = null)
        {
            Random rng = seed.HasValue ? new Random(seed.Value) : new Random();

            using StreamWriter writer = new("chess_dataset.csv", true);
            for (int i = 0; i < numOfPositions; i++)
            {
                string? fen = GenerateRandomBoardFEN(rng);

                if (fen != null)
                {
                    string stockfishScore = GetStockfishEvaluation(fen);
                    writer.WriteLine($"{fen},{stockfishScore}");
                }
            }
        }

        private static bool MakeRandomMove(Board board, Random rng)
        {
            List<Move> legalMoves = [];
            List<Move> moves = MoveGenerator.GenerateAllMoves(board);

            foreach (Move move in moves)
            {
                if (move.IsLegal(board))
                {
                    legalMoves.Add(move);
                }
            }

            if (legalMoves.Count == 0)
            {
                return false;
            }

            Move toMove = legalMoves[rng.Next(legalMoves.Count)];
            toMove.MovePiece(board);
            board.ActiveColour ^= 1;
            return true;
        }

        private static string? GenerateRandomBoardFEN(Random rng)
        {
            Board board = new();

            int numberOfMoves = rng.Next(10, 60);

            for (int i = 0; i < numberOfMoves; i++)
            {
                if (!MakeRandomMove(board, rng)) return null;
            }

            while (!board.IsCalm())
            {
                if (!MakeRandomMove(board, rng)) return null;
            }

            return board.ToFEN();
        }

        private static string GetStockfishEvaluation(string fen)
        {
            return "";
        }
    }
}
