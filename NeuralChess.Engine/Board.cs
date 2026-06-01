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

    internal static class CastlingRights
    {
        public const uint WK = 1;
        public const uint WQ = 2;
        public const uint BK = 4;
        public const uint BQ = 8;
    }

    public class Board
    {
        public uint CastleRights;
        public ulong[] Pieces = new ulong[12];
        public ulong[] Colours = new ulong[2];
        public ulong AllPieces;
        public int ActiveColour = Colour.White;
        public int EnPassantSquare = -1;

        private static readonly Dictionary<char, int> PieceMap = new()
        {
            {'P', Piece.WhitePawn}, {'N', Piece.WhiteKnight}, {'B', Piece.WhiteBishop},
            {'R', Piece.WhiteRook}, {'Q', Piece.WhiteQueen}, {'K', Piece.WhiteKing},
            {'p', Piece.BlackPawn}, {'n', Piece.BlackKnight}, {'b', Piece.BlackBishop},
            {'r', Piece.BlackRook}, {'q', Piece.BlackQueen}, {'k', Piece.BlackKing}
        };

        private static readonly Dictionary<int, int> PieceValue = new()
        {
            {Piece.WhitePawn, 1}, {Piece.WhiteKnight, 3}, {Piece.WhiteBishop, 3},
            {Piece.WhiteRook, 5}, {Piece.WhiteQueen, 9}, {Piece.WhiteKing, 999},
            {Piece.BlackPawn, -1}, {Piece.BlackKnight, -3}, {Piece.BlackBishop, -3},
            {Piece.BlackRook, -5}, {Piece.BlackQueen, -9}, {Piece.BlackKing, -999}
        };

        public Board(string starting_pos)
        {
            LoadPositionFromFen(starting_pos);
        }

        public Board() : this(Constants.regular_start)
        {

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

            CastleRights = 0;
            if (fenParts.Length > 2 && fenParts[2] != "-")
            {
                if (fenParts[2].Contains('K')) CastleRights |= CastlingRights.WK;
                if (fenParts[2].Contains('Q')) CastleRights |= CastlingRights.WQ;
                if (fenParts[2].Contains('k')) CastleRights |= CastlingRights.BK;
                if (fenParts[2].Contains('q')) CastleRights |= CastlingRights.BQ;
            }

            EnPassantSquare = -1;
            if (fenParts.Length > 3 && fenParts[3] != "-")
            {
                char f = char.ToUpper(fenParts[3][0]);
                char r = fenParts[3][1];

                EnPassantSquare = (r - '1') * 8 + (f - 'A');
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

        public Board CloneBoard()
        {
            return new Board()
            {
                CastleRights = this.CastleRights,
                ActiveColour = this.ActiveColour,
                AllPieces = this.AllPieces,
                Colours = (ulong[])this.Colours.Clone(),
                Pieces = (ulong[])this.Pieces.Clone()
            };
        }

        public static bool IsSquareAttacked(int squareIndex, int attackingColour, Board board)
        {
            ulong attackSquare = 1UL << squareIndex;
            ulong attackPawn = board.Pieces[Piece.WhitePawn + attackingColour * 6];
            ulong attackKnight = board.Pieces[Piece.WhiteKnight + attackingColour * 6];
            ulong attackBishop = board.Pieces[Piece.WhiteBishop + attackingColour * 6];
            ulong attackRook = board.Pieces[Piece.WhiteRook + attackingColour * 6];
            ulong attackQueen = board.Pieces[Piece.WhiteQueen + attackingColour * 6];
            ulong attackKing = board.Pieces[Piece.WhiteKing + attackingColour * 6];
            ulong notPiece = ~board.AllPieces;

            ulong attackStraight = attackQueen | attackRook;
            ulong attackDiagonal = attackQueen | attackBishop;

            ulong posu1 = attackSquare;
            ulong posd1 = attackSquare;
            ulong posl1 = attackSquare;
            ulong posr1 = attackSquare;
            ulong posu1l1 = attackSquare;
            ulong posu1r1 = attackSquare;
            ulong posd1l1 = attackSquare;
            ulong posd1r1 = attackSquare;

            for (int i = 1; i < 8; i++)
            {
                ulong u1 = posu1 << 8;
                if ((u1 & (attackStraight | (i == 1 ? attackKing : 0))) != 0) return true;
                posu1 = u1 & notPiece;

                ulong d1 = posd1 >> 8;
                if ((d1 & (attackStraight | (i == 1 ? attackKing : 0))) != 0) return true;
                posd1 = d1 & notPiece;

                ulong l1 = (posl1 & Constants.NotAFile) >> 1;
                if ((l1 & (attackStraight | (i == 1 ? attackKing : 0))) != 0) return true;
                posl1 = l1 & notPiece;

                ulong r1 = (posr1 & Constants.NotHFile) << 1;
                if ((r1 & (attackStraight | (i == 1 ? attackKing : 0))) != 0) return true;
                posr1 = r1 & notPiece;

                ulong u1l1 = (posu1l1 & Constants.NotAFile) << 7;
                if ((u1l1 & (attackDiagonal | (i == 1 ? attackKing : 0))) != 0) return true;
                posu1l1 = u1l1 & notPiece;

                ulong u1r1 = (posu1r1 & Constants.NotHFile) << 9;
                if ((u1r1 & (attackDiagonal | (i == 1 ? attackKing : 0))) != 0) return true;
                posu1r1 = u1r1 & notPiece;

                ulong d1l1 = (posd1l1 & Constants.NotAFile) >> 9;
                if ((d1l1 & (attackDiagonal | (i == 1 ? attackKing : 0))) != 0) return true;
                posd1l1 = d1l1 & notPiece;

                ulong d1r1 = (posd1r1 & Constants.NotHFile) >> 7;
                if ((d1r1 & (attackDiagonal | (i == 1 ? attackKing : 0))) != 0) return true;
                posd1r1 = d1r1 & notPiece;

                if ((posu1 | posd1 | posl1 | posr1 | posu1l1 | posu1r1 | posd1l1 | posd1r1) == 0) break;
            }

            ulong u2l1 = (attackSquare & Constants.NotAFile) << 15;
            if ((u2l1 & attackKnight) != 0) return true;

            ulong u2r1 = (attackSquare & Constants.NotHFile) << 17;
            if ((u2r1 & attackKnight) != 0) return true;

            ulong d2l1 = (attackSquare & Constants.NotAFile) >> 17;
            if ((d2l1 & attackKnight) != 0) return true;

            ulong d2r1 = (attackSquare & Constants.NotHFile) >> 15;
            if ((d2r1 & attackKnight) != 0) return true;

            ulong u1l2 = (attackSquare & Constants.NotABFile) << 6;
            if ((u1l2 & attackKnight) != 0) return true;

            ulong u1r2 = (attackSquare & Constants.NotGHFile) << 10;
            if ((u1r2 & attackKnight) != 0) return true;

            ulong d1l2 = (attackSquare & Constants.NotABFile) >> 10;
            if ((d1l2 & attackKnight) != 0) return true;

            ulong d1r2 = (attackSquare & Constants.NotGHFile) >> 6;
            if ((d1r2 & attackKnight) != 0) return true;

            if (attackingColour == Colour.White)
            {
                ulong d1l1 = (attackSquare & Constants.NotAFile) >> 9;
                if ((d1l1 & attackPawn) != 0) return true;

                ulong d1r1 = (attackSquare & Constants.NotHFile) >> 7;
                if ((d1r1 & attackPawn) != 0) return true;
            }
            else
            {
                ulong u1l1 = (attackSquare & Constants.NotAFile) << 7;
                if ((u1l1 & attackPawn) != 0) return true;

                ulong u1r1 = (attackSquare & Constants.NotHFile) << 9;
                if ((u1r1 & attackPawn) != 0) return true;
            }

            return false;
        }

        public int GetBoardValue()
        {
            int totalValue = 0;
            for (int i = 0; i < 12; i++)
            {
                ulong piece = Pieces[i];
                while (piece != 0)
                {
                    totalValue += PieceValue[i];
                    piece &= piece - 1;
                }
            }
            return totalValue;
        }
    }
}
