using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;

namespace NeuralChess.Engine
{
    public class MoveGenerator
    {
        private const ulong NotAFile = 0xFEFEFEFEFEFEFEFEUL;
        private const ulong NotABFile = 0xFCFCFCFCFCFCFCFCUL;
        private const ulong NotHFile = 0x7F7F7F7F7F7F7F7FUL;
        private const ulong NotGHFile = 0x3F3F3F3F3F3F3F3FUL;

        public static List<Move> GenerateAllMoves(Board board)
        {
            List<Move> moves = [];
            GenerateWhitePawnMoves(board, moves);
            GenerateWhiteKnightMoves(board, moves);
            return moves;
        }

        public static void GenerateWhitePawnMoves(Board board, List<Move> moves)
        {
            ulong pawns = board.Pieces[Piece.WhitePawn];
            ulong singlePushes = (pawns << 8) & ~board.AllPieces;
            ExtractMoves(singlePushes, -8, moves);

            ulong u1l1 = ((pawns & NotAFile)  << 7) & board.Colours[Colour.Black];
            ExtractMoves(u1l1, -7, moves);

            ulong u1r1 = ((pawns & NotHFile) << 9) & board.Colours[Colour.Black];
            ExtractMoves(u1r1, -9, moves);
        }

        public static void GenerateWhiteKnightMoves(Board board, List<Move> moves)
        {
            ulong knights = board.Pieces[Piece.WhiteKnight];
            ulong validSquares = ~board.Colours[Colour.White];

            ulong u2l1 = ((knights & NotAFile) << 15) & validSquares;
            ExtractMoves(u2l1, -15, moves);

            ulong u2r1 = ((knights & NotHFile) << 17) & validSquares;
            ExtractMoves(u2r1, -17, moves);

            ulong d2l1 = ((knights & NotAFile) >> 17) & validSquares;
            ExtractMoves(d2l1, 17, moves);

            ulong d2r1 = ((knights & NotHFile) >> 15) & validSquares;
            ExtractMoves(d2r1, 15, moves);

            ulong u1l2 = ((knights & NotABFile) << 6) & validSquares;
            ExtractMoves(u1l2, -6, moves);

            ulong u1r2 = ((knights & NotGHFile) << 10) & validSquares;
            ExtractMoves(u1r2, -10, moves);

            ulong d1l2 = ((knights & NotABFile) >> 10) & validSquares;
            ExtractMoves(d1l2, 10, moves);

            ulong d1r2 = ((knights & NotGHFile) >> 6) & validSquares;
            ExtractMoves(d1r2, 6, moves);
        }

        public static void GenerateWhiteBishopMoves(Board board, List<Move> moves)
        {
            ulong bishops = board.Pieces[Piece.WhiteBishop];
            ulong notWhite = ~board.Colours[Colour.White];
            ulong notBlack = ~board.Colours[Colour.Black];

            ulong posu1l1 = bishops;
            ulong posu1r1 = bishops;
            ulong posd1l1 = bishops;
            ulong posd1r1 = bishops;

            for (int i = 1; i < 8; i++)
            {
                ulong u1l1 = ((posu1l1 & NotAFile) << 7) & notWhite;
                ExtractMoves(u1l1, -7*i, moves);
                posu1l1 = u1l1 & notBlack;

                ulong u1r1 = ((posu1r1 & NotHFile) << 9) & notWhite;
                ExtractMoves(u1r1, -9*i, moves);
                posu1r1 = u1r1 & notBlack;

                ulong d1l1 = ((posd1l1 & NotAFile) >> 9) & notWhite;
                ExtractMoves(d1l1, 9*i, moves);
                posd1l1 = d1l1 & notBlack;

                ulong d1r1 = ((posd1r1 & NotHFile) >> 7) & notWhite;
                ExtractMoves(d1r1, 7*i, moves);
                posd1r1 = d1r1 & notBlack;

                if ((posu1l1 | posu1r1 | posd1l1 | posd1r1) == 0)
                {
                    break;
                }
            }
        }

        public static void GenerateWhiteRookMoves(Board board, List<Move> moves)
        {
            ulong rooks = board.Pieces[Piece.WhiteRook];
            ulong notWhite = ~board.Colours[Colour.White];
            ulong notBlack = ~board.Colours[Colour.Black];

            ulong posu1 = rooks;
            ulong posd1 = rooks;
            ulong posl1 = rooks;
            ulong posr1 = rooks;

            for (int i = 1; i < 8; i++)
            {
                ulong u1 = (posu1 << 8) & notWhite;
                ExtractMoves(u1, -8 * i, moves);
                posu1 = u1 & notBlack;

                ulong d1 = (posd1 >> 8) & notWhite;
                ExtractMoves(d1, 8 * i, moves);
                posd1 = d1 & notBlack;

                ulong l1 = ((posl1 & NotAFile) >> 1) & notWhite;
                ExtractMoves(l1, 1 * i, moves);
                posl1 = l1 & notBlack;

                ulong r1 = ((posr1 & NotHFile) << 1) & notWhite;
                ExtractMoves(r1, -1 * i, moves);
                posr1 = r1 & notBlack;

                if ((posu1 | posd1 | posl1 | posr1) == 0)
                {
                    break;
                }
            }
        }

        public static void GenerateWhiteQueenMoves(Board board, List<Move> moves)
        {
            ulong queens = board.Pieces[Piece.WhiteQueen];
            ulong notWhite = ~board.Colours[Colour.White];
            ulong notBlack = ~board.Colours[Colour.Black];

            ulong posu1 = queens;
            ulong posd1 = queens;
            ulong posl1 = queens;
            ulong posr1 = queens;
            ulong posu1l1 = queens;
            ulong posu1r1 = queens;
            ulong posd1l1 = queens;
            ulong posd1r1 = queens;

            for (int i = 1; i < 8; i++)
            {
                ulong u1 = (posu1 << 8) & notWhite;
                ExtractMoves(u1, -8 * i, moves);
                posu1 = u1 & notBlack;

                ulong d1 = (posd1 >> 8) & notWhite;
                ExtractMoves(d1, 8 * i, moves);
                posd1 = d1 & notBlack;

                ulong l1 = ((posl1 & NotAFile) >> 1) & notWhite;
                ExtractMoves(l1, 1 * i, moves);
                posl1 = l1 & notBlack;

                ulong r1 = ((posr1 & NotHFile) << 1) & notWhite;
                ExtractMoves(r1, -1 * i, moves);
                posr1 = r1 & notBlack;

                ulong u1l1 = ((posu1l1 & NotAFile) << 7) & notWhite;
                ExtractMoves(u1l1, -7 * i, moves);
                posu1l1 = u1l1 & notBlack;

                ulong u1r1 = ((posu1r1 & NotHFile) << 9) & notWhite;
                ExtractMoves(u1r1, -9 * i, moves);
                posu1r1 = u1r1 & notBlack;

                ulong d1l1 = ((posd1l1 & NotAFile) >> 9) & notWhite;
                ExtractMoves(d1l1, 9 * i, moves);
                posd1l1 = d1l1 & notBlack;

                ulong d1r1 = ((posd1r1 & NotHFile) >> 7) & notWhite;
                ExtractMoves(d1r1, 7 * i, moves);
                posd1r1 = d1r1 & notBlack;

                if ((posu1 | posd1 | posl1 | posr1 | posu1l1 | posu1r1 | posd1l1 | posd1r1) == 0)
                {
                    break;
                }
            }
        }

        public static void GenerateWhiteKingMoves(Board board, List<Move> moves)
        {
            ulong kings = board.Pieces[Piece.WhiteKing];
            ulong notWhite = ~board.Colours[Colour.White];

            ulong u1 = (kings << 8) & notWhite;
            ExtractMoves(u1, -8, moves);

            ulong d1 = (kings >> 8) & notWhite;
            ExtractMoves(d1, 8, moves);

            ulong l1 = ((kings & NotAFile) >> 1) & notWhite;
            ExtractMoves(l1, 1, moves);

            ulong r1 = ((kings & NotHFile) << 1) & notWhite;
            ExtractMoves(r1, -1, moves);

            ulong u1l1 = ((kings & NotAFile) << 7) & notWhite;
            ExtractMoves(u1l1, -7, moves);

            ulong u1r1 = ((kings & NotHFile) << 9) & notWhite;
            ExtractMoves(u1r1, -9, moves);

            ulong d1l1 = ((kings & NotAFile) >> 9) & notWhite;
            ExtractMoves(d1l1, 9, moves);

            ulong d1r1 = ((kings & NotHFile) >> 7) & notWhite;
            ExtractMoves(d1r1, 7, moves);
        }

        public static void ExtractMoves(ulong bitboard, int offset, List<Move> moves)
        {
            while (bitboard != 0)
            {
                int toSquare = BitOperations.TrailingZeroCount(bitboard);
                int fromSquare = toSquare + offset;
                moves.Add(new Move(fromSquare, toSquare));
                bitboard &= (bitboard - 1);
            }
        }
    }
}
