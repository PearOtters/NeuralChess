using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.Engine
{
    internal static class Piece
    {
        public const int WhitePawn = 0;
        public const int WhiteKnight = 1;
        public const int WhiteBishop = 2;
        public const int WhiteRook = 3;
        public const int WhiteQueen = 4;
        public const int WhiteKing = 5;

        public const int BlackPawn = 6;
        public const int BlackKnight = 7;
        public const int BlackBishop = 8;
        public const int BlackRook = 9;
        public const int BlackQueen = 10;
        public const int BlackKing = 11;
    }

    internal static class Colour
    {
        public const int White = 0;
        public const int Black = 1;
    }
    public class Board
    {
        internal ulong[] Pieces = new ulong[12];
        internal ulong[] Colours = new ulong[2];
        internal ulong AllPieces;
        internal int ActiveColour = Colour.White;


        private static readonly string regular_start = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private static readonly Dictionary<char, int> PieceMap = new Dictionary<char, int>
        {
            {'P', Piece.WhitePawn}, {'N', Piece.WhiteKnight}, {'B', Piece.WhiteBishop},
            {'R', Piece.WhiteRook}, {'Q', Piece.WhiteQueen}, {'K', Piece.WhiteKing},
            {'p', Piece.BlackPawn}, {'n', Piece.BlackKnight}, {'b', Piece.BlackBishop},
            {'r', Piece.BlackRook}, {'q', Piece.BlackQueen}, {'k', Piece.BlackKing}
        };

        public Board(string starting_pos)
        {
            LoadPositionFromFen(starting_pos);
        }

        public Board()
        {
            LoadPositionFromFen(regular_start);
        }

        private void LoadPositionFromFen(string fen)
        {
            Array.Clear(Pieces, 0, Pieces.Length);
            Array.Clear(Colours, 0, Colours.Length);

            string[] fenParts = fen.Split(' ');
            string boardLayout = fenParts[0];

            int rank = 7;
            int file = 0;

            foreach (char c in boardLayout)
            {
                if (c == '/')
                {
                    file = 0;
                    rank--;
                }
                else if (char.IsDigit(c))
                {
                    file += (int)char.GetNumericValue(c);
                }
                else
                {
                    int squareIndex = (rank * 8) + file;
                    int pieceType = PieceMap[c];

                    Pieces[pieceType] |= (1UL << squareIndex);

                    file++;
                }
            }

            if (fenParts.Length > 1)
            {
                ActiveColour = (fenParts[1] == "w") ? Colour.White : Colour.Black;
            }

            UpdateColours();
        }

        private void UpdateColours()
        {
            Colours[Colour.White] = Pieces[Piece.WhitePawn] | Pieces[Piece.WhiteKnight] |
                                    Pieces[Piece.WhiteBishop] | Pieces[Piece.WhiteRook] |
                                    Pieces[Piece.WhiteQueen] | Pieces[Piece.WhiteKing];

            Colours[Colour.Black] = Pieces[Piece.BlackPawn] | Pieces[Piece.BlackKnight] |
                                    Pieces[Piece.BlackBishop] | Pieces[Piece.BlackRook] |
                                    Pieces[Piece.BlackQueen] | Pieces[Piece.BlackKing];

            AllPieces = Colours[Colour.White] | Colours[Colour.Black];
        }
    }
}
