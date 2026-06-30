using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace NeuralChess.Engine
{
    public static class Piece
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

    public static class Colour
    {
        public const int White = 0;
        public const int Black = 1;     
    }

    public static class CastlingRights
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
        public ulong ZobristHash;
        public int totalPieces;

        private static readonly Dictionary<char, int> PieceMap = new()
        {
            {'P', Piece.WhitePawn}, {'N', Piece.WhiteKnight}, {'B', Piece.WhiteBishop},
            {'R', Piece.WhiteRook}, {'Q', Piece.WhiteQueen}, {'K', Piece.WhiteKing},
            {'p', Piece.BlackPawn}, {'n', Piece.BlackKnight}, {'b', Piece.BlackBishop},
            {'r', Piece.BlackRook}, {'q', Piece.BlackQueen}, {'k', Piece.BlackKing}
        };

        private static readonly int[] PieceValue =
        [
            100, 300, 300, 500, 900, 10000,
            -100, -300, -300, -500, -900, -10000
        ];

        private static readonly int[][] PSTs =
        [
            // Pawn
            [
                 0,  0,  0,  0,  0,  0,  0,  0,
                 5, 10, 10,-20,-20, 10, 10,  5,
                 5, -5,-10,  0,  0,-10, -5,  5,
                 0,  0,  0, 20, 20,  0,  0,  0,
                 5,  5, 10, 25, 25, 10,  5,  5,
                10, 10, 20, 30, 30, 20, 10, 10,
                50, 50, 50, 50, 50, 50, 50, 50,
                 0,  0,  0,  0,  0,  0,  0,  0
            ],
            // Knight
            [
                -50,-40,-30,-30,-30,-30,-40,-50,
                -40,-20,  0,  5,  5,  0,-20,-40,
                -30,  5, 10, 15, 15, 10,  5,-30,
                -30,  0, 15, 20, 20, 15,  0,-30,
                -30,  5, 15, 20, 20, 15,  5,-30,
                -30,  0, 10, 15, 15, 10,  0,-30,
                -40,-20,  0,  0,  0,  0,-20,-40,
                -50,-40,-30,-30,-30,-30,-40,-50
            ],
            // Bishop
            [
                -20,-10,-10,-10,-10,-10,-10,-20,
                -10,  5,  0,  0,  0,  0,  5,-10,
                -10, 10, 10, 10, 10, 10, 10,-10,
                -10,  0, 10, 10, 10, 10,  0,-10,
                -10,  5,  5, 10, 10,  5,  5,-10,
                -10,  0,  5, 10, 10,  5,  0,-10,
                -10,  0,  0,  0,  0,  0,  0,-10,
                -20,-10,-10,-10,-10,-10,-10,-20
            ],
            // Rook
            [
                 0,  0,  0,  5,  5,  0,  0,  0,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                 5, 10, 10, 10, 10, 10, 10,  5,
                 0,  0,  0,  0,  0,  0,  0,  0
            ],
            // Queen
            [
                -20,-10,-10, -5, -5,-10,-10,-20,
                -10,  0,  5,  0,  0,  0,  0,-10,
                -10,  5,  5,  5,  5,  5,  0,-10,
                  0,  0,  5,  5,  5,  5,  0, -5,
                 -5,  0,  5,  5,  5,  5,  0, -5,
                -10,  0,  5,  5,  5,  5,  0,-10,
                -10,  0,  0,  0,  0,  0,  0,-10,
                -20,-10,-10, -5, -5,-10,-10,-20
            ],
            // King
            [
                 20, 30, 10,  0,  0, 10, 30, 20,
                 20, 20,  0,  0,  0,  0, 20, 20,
                -10,-20,-20,-20,-20,-20,-20,-10,
                -20,-30,-30,-40,-40,-30,-30,-20,
                -30,-40,-40,-50,-50,-40,-40,-30,
                -30,-40,-40,-50,-50,-40,-40,-30,
                -30,-40,-40,-50,-50,-40,-40,-30,
                -30,-40,-40,-50,-50,-40,-40,-30
            ]
        ];

        public Board(string starting_pos)
        {
            LoadPositionFromFen(starting_pos);
        }

        public Board() : this(Constants.regular_start)
        {

        }

        private void LoadPositionFromFen(string fen)
        {
            ZobristHash = 0;
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
                    ZobristHash ^= Zobrist.PieceKeys[pieceType, squareIndex];
                    totalPieces++;
                    file++;
                }
            }

            if (fenParts.Length > 1)
            {
                ActiveColour = (fenParts[1] == "w") ? Colour.White : Colour.Black;
                if (ActiveColour == Colour.Black)
                {
                    ZobristHash ^= Zobrist.SideToMove;
                }
            }

            CastleRights = 0;
            if (fenParts.Length > 2 && fenParts[2] != "-")
            {
                if (fenParts[2].Contains('K')) CastleRights |= CastlingRights.WK;
                if (fenParts[2].Contains('Q')) CastleRights |= CastlingRights.WQ;
                if (fenParts[2].Contains('k')) CastleRights |= CastlingRights.BK;
                if (fenParts[2].Contains('q')) CastleRights |= CastlingRights.BQ;
            }
            ZobristHash ^= Zobrist.CastlingRights[CastleRights];

            EnPassantSquare = -1;
            if (fenParts.Length > 3 && fenParts[3] != "-")
            {
                char f = char.ToUpper(fenParts[3][0]);
                char r = fenParts[3][1];

                EnPassantSquare = (r - '1') * 8 + (f - 'A');
                ZobristHash ^= Zobrist.EnPassantSquares[EnPassantSquare % 8];
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
                EnPassantSquare = this.EnPassantSquare,
                ZobristHash = this.ZobristHash,
                Colours = (ulong[])this.Colours.Clone(),
                Pieces = (ulong[])this.Pieces.Clone()
            };
        }

        public bool IsSquareAttacked(int squareIndex, int attackingColour)
        {
            ulong attackSquare = 1UL << squareIndex;
            ulong attackPawn = Pieces[Piece.WhitePawn + attackingColour * 6];
            ulong attackKnight = Pieces[Piece.WhiteKnight + attackingColour * 6];
            ulong attackBishop = Pieces[Piece.WhiteBishop + attackingColour * 6];
            ulong attackRook = Pieces[Piece.WhiteRook + attackingColour * 6];
            ulong attackQueen = Pieces[Piece.WhiteQueen + attackingColour * 6];
            ulong attackKing = Pieces[Piece.WhiteKing + attackingColour * 6];
            ulong notPiece = ~AllPieces;

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

        public bool IsInCheck(int defendingColour)
        {
            return IsSquareAttacked(BitOperations.TrailingZeroCount(Pieces[Piece.WhiteKing + 6 * defendingColour]), defendingColour ^ 1);
        }

        public int GetBoardValue()
        {
            int totalValue = 0;
            for (int i = 0; i < 12; i++)
            {
                ulong piece = Pieces[i];

                bool isWhite = i < 6;

                while (piece != 0)
                {
                    int square = BitOperations.TrailingZeroCount(piece);

                    totalValue += PieceValue[i];

                    if (isWhite)
                    {
                        totalValue += PSTs[i][square];
                    }
                    else
                    {
                        totalValue -= PSTs[i - 6][square ^ 56];
                    }

                    piece &= piece - 1;
                }
            }
            return totalValue;
        }

        public bool IsCalm()
        {
            int kingSquare = BitOperations.TrailingZeroCount(Pieces[Piece.WhiteKing + ActiveColour * 6]);
            if (IsSquareAttacked(kingSquare, ActiveColour ^ 1)) return false;
            Span<Move> moves = stackalloc Move[79];
            int numOfMoves = 0;
            MoveGenerator.GenerateAllCaptures(this, ref moves, ref numOfMoves);
            return numOfMoves == 0;
        }

        public string ToFEN()
        {
            StringBuilder fen = new();

            char[] pieceChars = ['P', 'N', 'B', 'R', 'Q', 'K', 'p', 'n', 'b', 'r', 'q', 'k'];

            for (int rank = 7; rank >= 0; rank--)
            {
                int emptySquares = 0;
                for (int file = 0; file < 8; file++)
                {
                    int squareIndex = (rank * 8) + file;
                    ulong squareMask = 1UL << squareIndex;

                    if ((AllPieces & squareMask) != 0)
                    {
                        if (emptySquares > 0)
                        {
                            fen.Append(emptySquares);
                            emptySquares = 0;
                        }

                        for (int i = 0; i < 12; i++)
                        {
                            if ((Pieces[i] & squareMask) != 0)
                            {
                                fen.Append(pieceChars[i]);
                                break;
                            }
                        }
                    }
                    else
                    {
                        emptySquares++;
                    }
                }

                if (emptySquares > 0)
                {
                    fen.Append(emptySquares);
                }

                if (rank > 0)
                {
                    fen.Append('/');
                }
            }

            fen.Append(ActiveColour == Colour.White ? " w " : " b ");

            if (CastleRights == 0)
            {
                fen.Append('-');
            }
            else
            {
                if ((CastleRights & CastlingRights.WK) != 0) fen.Append('K');
                if ((CastleRights & CastlingRights.WQ) != 0) fen.Append('Q');
                if ((CastleRights & CastlingRights.BK) != 0) fen.Append('k');
                if ((CastleRights & CastlingRights.BQ) != 0) fen.Append('q');
            }

            fen.Append(' ');
            if (EnPassantSquare == -1)
            {
                fen.Append('-');
            }
            else
            {
                char fileChar = (char)('a' + (EnPassantSquare % 8));
                char rankChar = (char)('1' + (EnPassantSquare / 8));
                fen.Append(fileChar).Append(rankChar);
            }

            fen.Append(" 0 1");

            return fen.ToString();
        }

        public void CalculateBoardState(float[] boardState)
        {
            Array.Clear(boardState, 0, 768);
            if (ActiveColour == Colour.White)
            {
                for (int i = 0; i < 12; i++)
                {
                    ulong pieces = Pieces[i];
                    while (pieces != 0)
                    {
                        int square = BitOperations.TrailingZeroCount(pieces);

                        boardState[(i * 64) + square] = 1f;

                        pieces &= pieces - 1;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 12; i++)
                {
                    ulong pieces = Pieces[i];

                    int povPiece = (i + 6) % 12;

                    while (pieces != 0)
                    {
                        int square = BitOperations.TrailingZeroCount(pieces);

                        int povSquare = square ^ 63;

                        boardState[(povPiece * 64) + povSquare] = 1f;

                        pieces &= pieces - 1;
                    }
                }
            }
        }
    }
}
