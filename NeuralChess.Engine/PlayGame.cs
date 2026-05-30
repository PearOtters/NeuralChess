using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace NeuralChess.Engine
{
    public class PlayGame
    {
        private static int Winner = -1;

        public static void StartGame(string starting_pos)
        {
            Board board = new(starting_pos);
            while (Winner == -1)
            {
                bool validMove = false;
                string currentCol = board.ActiveColour == Colour.White ? "White" : "Black";
                ConsoleDisplay.PrintBoard(board);
                Console.WriteLine($"{currentCol} please make your first move in the format A2->A4. If promoting please add letter to change at the end e.g A7->A8Q");
                string? move = Console.ReadLine(); 

                if (move != null)
                {
                    validMove = MoveParser.TryPlayMove(board, move);
                }

                if (!validMove)
                {
                    Console.WriteLine("Invalid Move please try again");
                }
            }
        }

        public static void StartGame()
        {
            StartGame(Constants.regular_start);
        }
    }
}
