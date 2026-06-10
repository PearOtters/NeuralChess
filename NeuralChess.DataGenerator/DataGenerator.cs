using NeuralChess.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NeuralChess.DataGenerator
{
    public class DataGenerator
    {
        private static Process? stockfishProcess;
        private static StreamWriter? stockfishIn;
        private static StreamReader? stockfishOut;

        public static void PopulateData(int numOfPositions, int? seed = null)
        {
            Random rng = seed.HasValue ? new Random(seed.Value) : new Random();

            InitializeStockfish();

            using StreamWriter writer = new("chess_dataset.csv", true);

            DateTime startedAt = DateTime.Now;

            Console.Clear();
            Console.WriteLine($"0% completed\nStarted calculations at {startedAt}\nLast updated {startedAt}\nCurrently at 0/{numOfPositions:N0} positions");

            HashSet<int> seperations = [];
            int numOfSeperations = 10_000;
            int seperation = numOfPositions / numOfSeperations;

            for (int i = 1; i <= numOfSeperations; i++)
            {
                seperations.Add(seperation * i);
            }

            for (int i = 0; i < numOfPositions; i++)
            {
                string? fen = GenerateRandomBoardFEN(rng);

                if (fen != null)
                {
                    string? stockfishScore = GetStockfishEvaluation(fen);
                    writer.WriteLine($"{fen},{stockfishScore}");
                }

                if (seperations.Contains(i))
                {
                    Console.Clear();
                    DateTime currentTime = DateTime.Now;
                    TimeSpan timeSpent = currentTime - startedAt;
                    double percentageDone = (double)i / (double)numOfPositions * 100;
                    double totalEstimatedMinutes = (100.0 / percentageDone) * timeSpent.TotalMinutes;
                    double remainingMinutes = totalEstimatedMinutes - timeSpent.TotalMinutes;
                    DateTime estimatedFinishTime = currentTime.AddMinutes(remainingMinutes);

                    Console.WriteLine($"{percentageDone:F2}% completed\nStarted calculations at {startedAt}\n" +
                        $"Last updated {currentTime}\nCurrently at {i:N0}/{numOfPositions:N0} positions");
                    Console.WriteLine($"Total time spent: {timeSpent.TotalMinutes:F2} minutes");
                    Console.WriteLine($"Estimated time to finish: {estimatedFinishTime:HH:mm:ss} on {estimatedFinishTime:yyyy-MM-dd}");
                }
            }
            if (stockfishIn != null && stockfishProcess != null)
            {
                stockfishIn.WriteLine("quit");
                stockfishProcess.WaitForExit();
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

        private static string? GetStockfishEvaluation(string fen)
        {
            if (stockfishIn != null && stockfishOut != null)
            {
                stockfishIn.WriteLine($"position fen {fen}");
                stockfishIn.WriteLine("go depth 10");

                string bestScore = "0";

                while (true)
                {
                    string? line = stockfishOut.ReadLine();
                    if (line == null) break;

                    if (line.StartsWith("info depth 10") && line.Contains("score"))
                    {
                        string[] tokens = line.Split(' ');
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            if (tokens[i] == "score")
                            {
                                if (tokens[i + 1] == "cp")
                                {
                                    bestScore = tokens[i + 2];
                                }
                                else if (tokens[i + 1] == "mate")
                                {
                                    int mateIn = int.Parse(tokens[i + 2]);
                                    bestScore = mateIn > 0 ? "10000" : "-10000";
                                }
                                break;
                            }
                        }
                    }

                    if (line.StartsWith("bestmove"))
                    {
                        break;
                    }
                }
                return bestScore;
            }
            return null;
        }

        private static void InitializeStockfish()
        {
            ProcessStartInfo psi = new()
            {
                FileName = "stockfish.exe",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            stockfishProcess = Process.Start(psi);
            if (stockfishProcess != null)
            {
                stockfishIn = stockfishProcess.StandardInput;
                stockfishOut = stockfishProcess.StandardOutput;

                stockfishIn.WriteLine("uci");
                stockfishIn.WriteLine("isready");

                string? line;
                while ((line = stockfishOut.ReadLine()) != null)
                {
                    if (line == "readyok") break;
                }
            }
        }
    }
}
