using NeuralChess.Engine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeuralChess.DataGenerator
{
    public class DataGenerator
    {
        public static void PopulateData(int numOfPositions, int? seed = null)
        {
            int workerThreads = Math.Max(1, Environment.ProcessorCount - 2);

            DateTime startedAt = DateTime.Now;
            long globalProcessedCount = 0;

            object fileLock = new();
            using StreamWriter writer = new("chess_dataset.csv", true, Encoding.UTF8, 65536);

            Console.Clear();
            Console.WriteLine($"Launching data generator with {workerThreads} parallel Stockfish instances...");

            Parallel.For(0, workerThreads, workerId =>
            {
                var (stockfishProcess, stockfishIn, stockfishOut) = InitializeStockfishInstance();

                Random rng = new(seed.HasValue ? seed.Value + workerId : Guid.NewGuid().GetHashCode());

                List<string> localBuffer = new(1000);

                int positionsPerWorker = numOfPositions / workerThreads;

                for (int i = 0; i < positionsPerWorker; i++)
                {
                    string? fen = GenerateRandomBoardFEN(rng);
                    if (fen != null)
                    {
                        string? stockfishScore = GetStockfishEvaluation(fen, stockfishIn, stockfishOut);
                        if (stockfishScore != null)
                        {
                            localBuffer.Add($"{fen},{stockfishScore}");
                        }
                    }

                    if (localBuffer.Count >= 500)
                    {
                        lock (fileLock)
                        {
                            foreach (var line in localBuffer)
                            {
                                writer.WriteLine(line);
                            }
                        }
                        localBuffer.Clear();

                        long currentProgress = Interlocked.Add(ref globalProcessedCount, 500);

                        if (currentProgress % 50_000 == 0)
                        {
                            PrintProgressUpdate(currentProgress, numOfPositions, startedAt);
                        }
                    }
                }

                if (localBuffer.Count > 0)
                {
                    lock (fileLock)
                    {
                        foreach (var line in localBuffer) writer.WriteLine(line);
                    }
                    Interlocked.Add(ref globalProcessedCount, localBuffer.Count);
                }

                stockfishIn.WriteLine("quit");
                stockfishProcess.WaitForExit();
                stockfishProcess.Dispose();
            });

            Console.WriteLine($"\nSuccessfully completed! Data saved to chess_dataset.csv");
        }

        private static void PrintProgressUpdate(long current, int total, DateTime start)
        {
            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - start;
            double percent = (double)current / total * 100;
            double positionsPerSecond = current / elapsed.TotalSeconds;

            string etaDisplay = "Calculating...";

            if (current > 0 && positionsPerSecond > 0)
            {
                double remainingPositions = total - current;
                double remainingSeconds = remainingPositions / positionsPerSecond;
                DateTime estimatedFinishTime = now.AddSeconds(remainingSeconds);

                etaDisplay = estimatedFinishTime.ToString("HH:mm:ss dd/MM/yy");
            }

            Console.Write($"\rProgress: {percent:F2}% | Processed: {current:N0}/{total:N0} | Speed: {positionsPerSecond:F0} pos/sec | Elapsed: {elapsed.TotalMinutes:F1}m | ETA: {etaDisplay}");
        }

        private static bool MakeRandomMove(Board board, Random rng)
        {
            List<Move> legalMoves = [];
            List<Move> moves = MoveGenerator.GenerateAllMoves(board);

            foreach (Move move in moves)
            {
                if (move.IsLegal(board)) legalMoves.Add(move);
            }

            if (legalMoves.Count == 0) return false;

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

        private static string? GetStockfishEvaluation(string fen, StreamWriter stockfishIn, StreamReader stockfishOut)
        {
            stockfishIn.WriteLine($"position fen {fen}");
            stockfishIn.WriteLine("go depth 10");

            string bestScore = "0";

            while (true)
            {
                string? line = stockfishOut.ReadLine();
                if (line == null) break;

                if (line.StartsWith("info depth 10 ") && line.Contains("score"))
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

                if (line.StartsWith("bestmove")) break;
            }
            return bestScore;
        }

        private static (Process, StreamWriter, StreamReader) InitializeStockfishInstance()
        {
            ProcessStartInfo psi = new()
            {
                FileName = "stockfish.exe",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to boot Stockfish.");
            StreamWriter si = process.StandardInput;
            StreamReader so = process.StandardOutput;

            si.WriteLine("uci");
            si.WriteLine("setoption name Threads value 1");
            si.WriteLine("setoption name Hash value 16");
            si.WriteLine("isready");

            string? line;
            while ((line = so.ReadLine()) != null)
            {
                if (line == "readyok") break;
            }

            return (process, si, so);
        }
    }
}