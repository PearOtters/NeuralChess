using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.Engine
{
    public class PlayGame
    {
        private static readonly string regular_start = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private int Winner = -1;

        public PlayGame(string starting_pos)
        {
            Board board = new(starting_pos);
            while (Winner == -1)
            {
                bool validMove = false;
                while (!validMove)
                {
                    ConsoleDisplay.PrintBoard(board);
                    int boardCol = board.ActiveColour;
                    string currentCol = board.ActiveColour == Colour.White ? "White" : "Black";
                    Console.WriteLine($"{currentCol} please make your first move in the format B3->C3");
                    string? move = Console.ReadLine();
                    if (move != null)
                    {
                        _ = new MakeMove(board, move);
                    }
                    if (boardCol == board.ActiveColour) Console.WriteLine("Invalid Move please try again");
                    else validMove = true;
                }
            }
        }

        public PlayGame() : this(regular_start) { }
    }
}
