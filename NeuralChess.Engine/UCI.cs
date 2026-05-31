using System;
using System.Collections.Generic;

namespace NeuralChess.Engine
{
    public static class UCI
    {
        public static void Loop()
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
                    Console.WriteLine("id name NeuralChess");
                    Console.WriteLine("id author Pierre Outters");
                    Console.WriteLine("uciok");
                }
                else if (command == "isready")
                {
                    Console.WriteLine("readyok");
                }
                else if (command == "position")
                {
                    ParsePosition(ref board, tokens);
                }
                else if (command == "go")
                {
                    List<Move> pseudoMoves = MoveGenerator.GenerateAllMoves(board);
                    List<Move> legalMoves = [];

                    foreach (Move m in pseudoMoves)
                    {
                        if (MoveParser.IsLegal(board, m)) legalMoves.Add(m);
                    }

                    if (legalMoves.Count > 0)
                    {
                        Random rng = new Random();
                        Move bestMove = legalMoves[rng.Next(legalMoves.Count)];
                        Console.WriteLine($"bestmove {MoveToUCIString(bestMove)}");
                    }
                    else
                    {
                        Console.WriteLine("bestmove (none)");
                    }
                }
                else if (command == "quit")
                {
                    break;
                }
            }
        }

        internal static void ParsePosition(ref Board board, string[] tokens)
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
                    Move move = Move.getMoveFromUCI(board, tokens[i]);
                    move.MovePiece(board);
                    board.ActiveColour ^= 1;
                }
            }
        }

        private static string MoveToUCIString(Move move)
        { 
            return move.ToUCI();
        }
    }
}