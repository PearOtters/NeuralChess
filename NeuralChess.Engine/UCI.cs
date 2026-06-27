using System;
using System.Collections.Generic;
using System.IO;

namespace NeuralChess.Engine
{
    public static class UCI
    {
        public static void Loop(Engine engine, bool toLog)
        {
            Board board = new(Constants.regular_start);

            while (true)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                string[] tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = tokens[0].ToLower();

                if (command == "uci")
                {
                    Console.WriteLine($"id name {engine.Name}");
                    Console.WriteLine("id author Pierre Outters");
                    Console.WriteLine("uciok");
                }
                else if (command == "isready")
                {
                    NNUE.Initialise();
                    Zobrist.Initialise();
                    AlphaBeta.Initialise();
                    Console.WriteLine("readyok");
                }
                else if (command == "position")
                {
                    ParsePosition(ref board, tokens);
                }
                else if (command == "go")
                {
                    int maximumTime = -1;
                    int maxTimeIndex = Array.IndexOf(tokens, "movetime");

                    int maximumDepth = -1;
                    int maxDepthIndex = Array.IndexOf(tokens, "depth");

                    if (maxTimeIndex != -1)
                    {
                        maximumTime = int.Parse(tokens[maxTimeIndex + 1]);
                    }
                    if (maxDepthIndex != -1)
                    {
                        maximumDepth = int.Parse(tokens[maxDepthIndex + 1]);
                    }
                    if (toLog)
                    {
                        File.AppendAllText("log.txt", input + "\n");
                    }
                    engine.Play(board, maximumTime, maximumDepth);
                }
                else if (command == "quit")
                {
                    break;
                }
            }
        }

        private static void ParsePosition(ref Board board, string[] tokens)
        {
            int moveIndex = -1;

            if (tokens.Length > 1 && tokens[1] == "startpos")
            {
                board = new Board(Constants.regular_start);
                moveIndex = Array.IndexOf(tokens, "moves");
            }
            else if (tokens.Length > 1 && tokens[1] == "fen")
            {
                moveIndex = Array.IndexOf(tokens, "moves");
                int fenEndIndex = moveIndex != -1 ? moveIndex : tokens.Length;
                string fen = string.Join(" ", tokens.Skip(2).Take(fenEndIndex - 2));
                board = new Board(fen);
            }

            if (moveIndex != -1)
            {
                for (int i = moveIndex + 1; i < tokens.Length; i++)
                {
                    Move move = Move.GetMoveFromUCI(board, tokens[i]);
                    move.MovePiece(board);
                    board.ActiveColour ^= 1;
                }
            }
        }
    }
}