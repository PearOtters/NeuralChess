using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace NeuralChess.Engine
{
    public static class UCI
    {
        private static Engine LoadedEngine = new RandomMove();
        public static void Loop(Engine engine, bool toLog)
        {
            Board board = new(Constants.regular_start);
            LoadedEngine = engine;

            while (true)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;
                if (toLog)
                {
                    File.AppendAllText("log.txt", input + "\n");
                }
                string[] tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = tokens[0].ToLower();

                if (command == "uci")
                {
                    Console.WriteLine($"id name {engine.Name}");
                    Console.WriteLine("id author Pierre Outters");
                    Console.WriteLine("option name SyzygyPath type string default <empty>");
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
                    engine.Play(board, maximumTime, maximumDepth);
                }
                else if (command == "setoption")
                {
                    if (tokens.Contains("SyzygyPath"))
                    {
                        int nameIndex = Array.IndexOf(tokens, "name");
                        int valueIndex = Array.IndexOf(tokens, "value");

                        if (nameIndex != -1 && valueIndex != -1)
                        {
                            string optionName = tokens[nameIndex + 1];
                            
                            if (optionName.Equals("SyzygyPath", StringComparison.OrdinalIgnoreCase))
                            {
                                string tbPath = string.Join(" ", tokens.Skip(valueIndex + 1));
                                
                                bool success = SyzygyLocal.tb_init(tbPath);
                                if (success)
                                {
                                    Console.WriteLine($"info string Syzygy tablebases successfully loaded from {tbPath}");
                                }
                                else
                                {
                                    Console.WriteLine("info string Warning: Failed to load Syzygy tablebases from provided path.");
                                }
                            }
                        }
                    }
                }
                else if (command == "quit")
                {
                    break;
                }
            }
        }

        private static void ParsePosition(ref Board board, string[] tokens)
        {
            byte irreversibleTurn = 1;
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
                    if (move.CapturedPiece != -1 || move.SelectedPiece == (Piece.WhitePawn % 6)) irreversibleTurn++;
                    board.ActiveColour ^= 1;
                }
                irreversibleTurn = (byte)((irreversibleTurn % 63) + 1);
                LoadedEngine.IrreversibleTurn = irreversibleTurn;
            }
        }
    }
}