using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace NeuralChess.Engine
{
    public class PlayGame
    {
        public static void StartGame(string starting_pos)
        {
            Board board = new(starting_pos);
            while (true)
            {
                bool validMove = false;
                string currentCol = board.ActiveColour == Colour.White ? "White" : "Black";
                ConsoleDisplay.PrintBoard(board);
                Console.WriteLine($"{currentCol} please make your move in the format A2->A4. If promoting please add letter to change at the end e.g A7->A8Q");
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
