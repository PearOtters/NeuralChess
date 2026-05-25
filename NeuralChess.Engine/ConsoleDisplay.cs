using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.Engine
{
    public class ConsoleDisplay
    {
        public static void PrintBoard(Board board)
        {
            Console.WriteLine("\n  +------------------------+");
            for (int rank = 7; rank >= 0; rank--)
            {
                Console.Write($"{rank + 1} |");
                for (int file = 0; file < 8; file++)
                {
                    char piece = '.';
                    if ((1UL << (rank * 8 + file) & board.Pieces[Piece.WhitePawn]) > 0) piece = 'P';
                    else if ((1UL << (rank * 8 + file) & board.Pieces[Piece.BlackPawn]) > 0) piece = 'p';
                    else if ((1UL << (rank * 8 + file) & board.Pieces[Piece.WhiteKnight]) > 0) piece = 'N';
                    else if ((1UL << (rank * 8 + file) & board.Pieces[Piece.BlackKnight]) > 0) piece = 'n';
                    else if ((1UL << (rank * 8 + file) & board.Pieces[Piece.WhiteBishop]) > 0) piece = 'B';
                    else if ((1UL << (rank * 8 + file) & board.Pieces[Piece.BlackBishop]) > 0) piece = 'b';
                    else if ((1UL << (rank * 8 + file) & board.Pieces[Piece.WhiteRook]) > 0) piece = 'R';
                    else if ((1UL << (rank * 8 + file) & board.Pieces[Piece.BlackRook]) > 0) piece = 'r';
                    else if ((1UL << (rank * 8 + file) & board.Pieces[Piece.WhiteQueen]) > 0) piece = 'Q';
                    else if ((1UL << (rank * 8 + file) & board.Pieces[Piece.BlackQueen]) > 0) piece = 'q';
                    else if ((1UL << (rank * 8 + file) & board.Pieces[Piece.WhiteKing]) > 0) piece = 'K';
                    else if ((1UL << (rank * 8 + file) & board.Pieces[Piece.BlackKing]) > 0) piece = 'k';

                    Console.Write($" {piece} ");
                }
                Console.WriteLine("|");
            }
            Console.WriteLine("  +------------------------+");
            Console.WriteLine("  | A  B  C  D  E  F  G  H |\n");
        }
    }
}
