using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace NeuralChess.Engine
{
    public static class Constants
    {
        public const ulong NotAFile = 0xFEFEFEFEFEFEFEFEUL;
        public const ulong NotABFile = 0xFCFCFCFCFCFCFCFCUL;
        public const ulong NotHFile = 0x7F7F7F7F7F7F7F7FUL;
        public const ulong NotGHFile = 0x3F3F3F3F3F3F3F3FUL;

        public static readonly string regular_start = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    }

    public class MoveGenerator
    {
        public static List<Move> GenerateAllMoves(Board board)
        {
            List<Move> moves = [];
            if (board.ActiveColour == Colour.White)
            {
                GenerateWhitePawnMoves(board, moves);
                GenerateWhiteKnightMoves(board, moves);
                GenerateWhiteBishopMoves(board, moves);
                GenerateWhiteRookMoves(board, moves);
                GenerateWhiteQueenMoves(board, moves);
                GenerateWhiteKingMoves(board, moves);
            }
            else
            {
                GenerateBlackPawnMoves(board, moves);
                GenerateBlackKnightMoves(board, moves);
                GenerateBlackBishopMoves(board, moves);
                GenerateBlackRookMoves(board, moves);
                GenerateBlackQueenMoves(board, moves);
                GenerateBlackKingMoves(board, moves);
            }
            
            return moves;
        }

        public static void GenerateWhitePawnMoves(Board board, List<Move> moves)
        {
            int piece = Piece.WhitePawn;
            ulong pawns = board.Pieces[Piece.WhitePawn];
            ulong u1 = (pawns << 8) & ~board.AllPieces;
            ExtractMoves(piece, u1, -8, moves);

            ulong u2 = (u1 << 8) & ~board.AllPieces & 0x00000000FF000000UL;
            ExtractMoves(piece, u2, -16, moves, SpecialMove.DOUBLE_PUSH);

            ulong u1l1 = ((pawns & Constants.NotAFile)  << 7) & board.Colours[Colour.Black];
            ExtractMoves(piece, u1l1, -7, moves);

            ulong u1r1 = ((pawns & Constants.NotHFile) << 9) & board.Colours[Colour.Black];
            ExtractMoves(piece, u1r1, -9, moves);

            if (board.EnPassantSquare != -1)
            {
                ulong EnPassantMask = 1UL << board.EnPassantSquare;

                ulong capLeft = ((pawns & Constants.NotAFile) << 7) & EnPassantMask;
                if (capLeft != 0) moves.Add(new Move(piece, board.EnPassantSquare - 7, board.EnPassantSquare, SpecialMove.EN_PASSANT));

                ulong capRight = ((pawns & Constants.NotHFile) << 9) & EnPassantMask;
                if (capRight != 0) moves.Add(new Move(piece, board.EnPassantSquare - 9, board.EnPassantSquare, SpecialMove.EN_PASSANT));
            }
        }

        public static void GenerateBlackPawnMoves(Board board, List<Move> moves)
        {
            int piece = Piece.BlackPawn;
            ulong pawns = board.Pieces[Piece.BlackPawn];
            ulong d1 = (pawns >> 8) & ~board.AllPieces;
            ExtractMoves(piece, d1, 8, moves);

            ulong d2 = (d1 >> 8) & ~board.AllPieces & 0x000000FF00000000UL;
            ExtractMoves(piece, d2, 16, moves, SpecialMove.DOUBLE_PUSH);

            ulong d1r1 = ((pawns & Constants.NotHFile) >> 7) & board.Colours[Colour.White];
            ExtractMoves(piece, d1r1, 7, moves);

            ulong d1l1 = ((pawns & Constants.NotAFile) >> 9) & board.Colours[Colour.White];
            ExtractMoves(piece, d1l1, 9, moves);

            if (board.EnPassantSquare != -1)
            {
                ulong EnPassantMask = 1UL << board.EnPassantSquare;

                ulong capLeft = ((pawns & Constants.NotAFile) >> 9) & EnPassantMask;
                if (capLeft != 0) moves.Add(new Move(piece, board.EnPassantSquare + 9, board.EnPassantSquare, SpecialMove.EN_PASSANT));

                ulong capRight = ((pawns & Constants.NotHFile) >> 7) & EnPassantMask;
                if (capRight != 0) moves.Add(new Move(piece, board.EnPassantSquare + 7, board.EnPassantSquare, SpecialMove.EN_PASSANT));
            }
        }

        public static void GenerateWhiteKnightMoves(Board board, List<Move> moves)
        {
            int piece = Piece.WhiteKnight;
            ulong knights = board.Pieces[Piece.WhiteKnight];
            ulong validSquares = ~board.Colours[Colour.White];

            ulong u2l1 = ((knights & Constants.NotAFile) << 15) & validSquares;
            ExtractMoves(piece, u2l1, -15, moves);

            ulong u2r1 = ((knights & Constants.NotHFile) << 17) & validSquares;
            ExtractMoves(piece, u2r1, -17, moves);

            ulong d2l1 = ((knights & Constants.NotAFile) >> 17) & validSquares;
            ExtractMoves(piece, d2l1, 17, moves);

            ulong d2r1 = ((knights & Constants.NotHFile) >> 15) & validSquares;
            ExtractMoves(piece, d2r1, 15, moves);

            ulong u1l2 = ((knights & Constants.NotABFile) << 6) & validSquares;
            ExtractMoves(piece, u1l2, -6, moves);

            ulong u1r2 = ((knights & Constants.NotGHFile) << 10) & validSquares;
            ExtractMoves(piece, u1r2, -10, moves);

            ulong d1l2 = ((knights & Constants.NotABFile) >> 10) & validSquares;
            ExtractMoves(piece, d1l2, 10, moves);

            ulong d1r2 = ((knights & Constants.NotGHFile) >> 6) & validSquares;
            ExtractMoves(piece, d1r2, 6, moves);
        }

        public static void GenerateBlackKnightMoves(Board board, List<Move> moves)
        {
            int piece = Piece.BlackKnight;
            ulong knights = board.Pieces[Piece.BlackKnight];
            ulong validSquares = ~board.Colours[Colour.Black];

            ulong u2l1 = ((knights & Constants.NotAFile) << 15) & validSquares;
            ExtractMoves(piece, u2l1, -15, moves);

            ulong u2r1 = ((knights & Constants.NotHFile) << 17) & validSquares;
            ExtractMoves(piece, u2r1, -17, moves);

            ulong d2l1 = ((knights & Constants.NotAFile) >> 17) & validSquares;
            ExtractMoves(piece, d2l1, 17, moves);

            ulong d2r1 = ((knights & Constants.NotHFile) >> 15) & validSquares;
            ExtractMoves(piece, d2r1, 15, moves);

            ulong u1l2 = ((knights & Constants.NotABFile) << 6) & validSquares;
            ExtractMoves(piece, u1l2, -6, moves);

            ulong u1r2 = ((knights & Constants.NotGHFile) << 10) & validSquares;
            ExtractMoves(piece, u1r2, -10, moves);

            ulong d1l2 = ((knights & Constants.NotABFile) >> 10) & validSquares;
            ExtractMoves(piece, d1l2, 10, moves);

            ulong d1r2 = ((knights & Constants.NotGHFile) >> 6) & validSquares;
            ExtractMoves(piece, d1r2, 6, moves);
        }

        public static void GenerateWhiteBishopMoves(Board board, List<Move> moves)
        {
            int piece = Piece.WhiteBishop;
            ulong bishops = board.Pieces[Piece.WhiteBishop];
            ulong notWhite = ~board.Colours[Colour.White];
            ulong notBlack = ~board.Colours[Colour.Black];

            ulong posu1l1 = bishops;
            ulong posu1r1 = bishops;
            ulong posd1l1 = bishops;
            ulong posd1r1 = bishops;

            for (int i = 1; i < 8; i++)
            {
                ulong u1l1 = ((posu1l1 & Constants.NotAFile) << 7) & notWhite;
                ExtractMoves(piece, u1l1, -7*i, moves);
                posu1l1 = u1l1 & notBlack;

                ulong u1r1 = ((posu1r1 & Constants.NotHFile) << 9) & notWhite;
                ExtractMoves(piece, u1r1, -9*i, moves);
                posu1r1 = u1r1 & notBlack;

                ulong d1l1 = ((posd1l1 & Constants.NotAFile) >> 9) & notWhite;
                ExtractMoves(piece, d1l1, 9*i, moves);
                posd1l1 = d1l1 & notBlack;

                ulong d1r1 = ((posd1r1 & Constants.NotHFile) >> 7) & notWhite;
                ExtractMoves(piece, d1r1, 7*i, moves);
                posd1r1 = d1r1 & notBlack;

                if ((posu1l1 | posu1r1 | posd1l1 | posd1r1) == 0)
                {
                    break;
                }
            }
        }

        public static void GenerateBlackBishopMoves(Board board, List<Move> moves)
        {
            int piece = Piece.BlackBishop;
            ulong bishops = board.Pieces[Piece.BlackBishop];
            ulong notWhite = ~board.Colours[Colour.White];
            ulong notBlack = ~board.Colours[Colour.Black];

            ulong posu1l1 = bishops;
            ulong posu1r1 = bishops;
            ulong posd1l1 = bishops;
            ulong posd1r1 = bishops;

            for (int i = 1; i < 8; i++)
            {
                ulong u1l1 = ((posu1l1 & Constants.NotAFile) << 7) & notBlack;
                ExtractMoves(piece, u1l1, -7 * i, moves);
                posu1l1 = u1l1 & notWhite;

                ulong u1r1 = ((posu1r1 & Constants.NotHFile) << 9) & notBlack;
                ExtractMoves(piece, u1r1, -9 * i, moves);
                posu1r1 = u1r1 & notWhite;

                ulong d1l1 = ((posd1l1 & Constants.NotAFile) >> 9) & notBlack;
                ExtractMoves(piece, d1l1, 9 * i, moves);
                posd1l1 = d1l1 & notWhite;

                ulong d1r1 = ((posd1r1 & Constants.NotHFile) >> 7) & notBlack;
                ExtractMoves(piece, d1r1, 7 * i, moves);
                posd1r1 = d1r1 & notWhite;

                if ((posu1l1 | posu1r1 | posd1l1 | posd1r1) == 0)
                {
                    break;
                }
            }
        }

        public static void GenerateWhiteRookMoves(Board board, List<Move> moves)
        {
            int piece = Piece.WhiteRook;
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
                ExtractMoves(piece, u1, -8 * i, moves);
                posu1 = u1 & notBlack;

                ulong d1 = (posd1 >> 8) & notWhite;
                ExtractMoves(piece, d1, 8 * i, moves);
                posd1 = d1 & notBlack;

                ulong l1 = ((posl1 & Constants.NotAFile) >> 1) & notWhite;
                ExtractMoves(piece, l1, 1 * i, moves);
                posl1 = l1 & notBlack;

                ulong r1 = ((posr1 & Constants.NotHFile) << 1) & notWhite;
                ExtractMoves(piece, r1, -1 * i, moves);
                posr1 = r1 & notBlack;

                if ((posu1 | posd1 | posl1 | posr1) == 0)
                {
                    break;
                }
            }
        }

        public static void GenerateBlackRookMoves(Board board, List<Move> moves)
        {
            int piece = Piece.BlackRook;
            ulong rooks = board.Pieces[Piece.BlackRook];
            ulong notWhite = ~board.Colours[Colour.White];
            ulong notBlack = ~board.Colours[Colour.Black];

            ulong posu1 = rooks;
            ulong posd1 = rooks;
            ulong posl1 = rooks;
            ulong posr1 = rooks;

            for (int i = 1; i < 8; i++)
            {
                ulong u1 = (posu1 << 8) & notBlack;
                ExtractMoves(piece, u1, -8 * i, moves);
                posu1 = u1 & notWhite;

                ulong d1 = (posd1 >> 8) & notBlack;
                ExtractMoves(piece, d1, 8 * i, moves);
                posd1 = d1 & notWhite;

                ulong l1 = ((posl1 & Constants.NotAFile) >> 1) & notBlack;
                ExtractMoves(piece, l1, 1 * i, moves);
                posl1 = l1 & notWhite;

                ulong r1 = ((posr1 & Constants.NotHFile) << 1) & notBlack;
                ExtractMoves(piece, r1, -1 * i, moves);
                posr1 = r1 & notWhite;

                if ((posu1 | posd1 | posl1 | posr1) == 0)
                {
                    break;
                }
            }
        }

        public static void GenerateWhiteQueenMoves(Board board, List<Move> moves)
        {
            int piece = Piece.WhiteQueen;
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
                ExtractMoves(piece, u1, -8 * i, moves);
                posu1 = u1 & notBlack;

                ulong d1 = (posd1 >> 8) & notWhite;
                ExtractMoves(piece, d1, 8 * i, moves);
                posd1 = d1 & notBlack;

                ulong l1 = ((posl1 & Constants.NotAFile) >> 1) & notWhite;
                ExtractMoves(piece, l1, 1 * i, moves);
                posl1 = l1 & notBlack;

                ulong r1 = ((posr1 & Constants.NotHFile) << 1) & notWhite;
                ExtractMoves(piece, r1, -1 * i, moves);
                posr1 = r1 & notBlack;

                ulong u1l1 = ((posu1l1 & Constants.NotAFile) << 7) & notWhite;
                ExtractMoves(piece, u1l1, -7 * i, moves);
                posu1l1 = u1l1 & notBlack;

                ulong u1r1 = ((posu1r1 & Constants.NotHFile) << 9) & notWhite;
                ExtractMoves(piece, u1r1, -9 * i, moves);
                posu1r1 = u1r1 & notBlack;

                ulong d1l1 = ((posd1l1 & Constants.NotAFile) >> 9) & notWhite;
                ExtractMoves(piece, d1l1, 9 * i, moves);
                posd1l1 = d1l1 & notBlack;

                ulong d1r1 = ((posd1r1 & Constants.NotHFile) >> 7) & notWhite;
                ExtractMoves(piece, d1r1, 7 * i, moves);
                posd1r1 = d1r1 & notBlack;

                if ((posu1 | posd1 | posl1 | posr1 | posu1l1 | posu1r1 | posd1l1 | posd1r1) == 0)
                {
                    break;
                }
            }
        }

        public static void GenerateBlackQueenMoves(Board board, List<Move> moves)
        {
            int piece = Piece.BlackQueen;
            ulong queens = board.Pieces[Piece.BlackQueen];
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
                ulong u1 = (posu1 << 8) & notBlack;
                ExtractMoves(piece, u1, -8 * i, moves);
                posu1 = u1 & notWhite;

                ulong d1 = (posd1 >> 8) & notBlack;
                ExtractMoves(piece, d1, 8 * i, moves);
                posd1 = d1 & notWhite;

                ulong l1 = ((posl1 & Constants.NotAFile) >> 1) & notBlack;
                ExtractMoves(piece, l1, 1 * i, moves);
                posl1 = l1 & notWhite;

                ulong r1 = ((posr1 & Constants.NotHFile) << 1) & notBlack;
                ExtractMoves(piece, r1, -1 * i, moves);
                posr1 = r1 & notWhite;

                ulong u1l1 = ((posu1l1 & Constants.NotAFile) << 7) & notBlack;
                ExtractMoves(piece, u1l1, -7 * i, moves);
                posu1l1 = u1l1 & notWhite;

                ulong u1r1 = ((posu1r1 & Constants.NotHFile) << 9) & notBlack;
                ExtractMoves(piece, u1r1, -9 * i, moves);
                posu1r1 = u1r1 & notWhite;

                ulong d1l1 = ((posd1l1 & Constants.NotAFile) >> 9) & notBlack;
                ExtractMoves(piece, d1l1, 9 * i, moves);
                posd1l1 = d1l1 & notWhite;

                ulong d1r1 = ((posd1r1 & Constants.NotHFile) >> 7) & notBlack;
                ExtractMoves(piece, d1r1, 7 * i, moves);
                posd1r1 = d1r1 & notWhite;

                if ((posu1 | posd1 | posl1 | posr1 | posu1l1 | posu1r1 | posd1l1 | posd1r1) == 0)
                {
                    break;
                }
            }
        }

        public static void GenerateWhiteKingMoves(Board board, List<Move> moves)
        {
            int piece = Piece.WhiteKing;
            ulong kings = board.Pieces[Piece.WhiteKing];
            ulong notWhite = ~board.Colours[Colour.White];

            ulong u1 = (kings << 8) & notWhite;
            ExtractMoves(piece, u1, -8, moves);

            ulong d1 = (kings >> 8) & notWhite;
            ExtractMoves(piece, d1, 8, moves);

            ulong l1 = ((kings & Constants.NotAFile) >> 1) & notWhite;
            ExtractMoves(piece, l1, 1, moves);

            ulong r1 = ((kings & Constants.NotHFile) << 1) & notWhite;
            ExtractMoves(piece, r1, -1, moves);

            ulong u1l1 = ((kings & Constants.NotAFile) << 7) & notWhite;
            ExtractMoves(piece, u1l1, -7, moves);

            ulong u1r1 = ((kings & Constants.NotHFile) << 9) & notWhite;
            ExtractMoves(piece, u1r1, -9, moves);

            ulong d1l1 = ((kings & Constants.NotAFile) >> 9) & notWhite;
            ExtractMoves(piece, d1l1, 9, moves);

            ulong d1r1 = ((kings & Constants.NotHFile) >> 7) & notWhite;
            ExtractMoves(piece, d1r1, 7, moves);

            if ((board.CastleRights & CastlingRights.WK) != 0)
            {
                ulong emptyMask = (1UL << 5) | (1UL << 6);
                if ((board.AllPieces & emptyMask) == 0)
                {
                    GenerateCastleMoves(board, emptyMask, 6, Colour.White, moves);
                }
            }

            if ((board.CastleRights & CastlingRights.WQ) != 0)
            {
                ulong emptyMask = (1UL << 1) | (1UL << 2) | (1UL << 3);
                if ((board.AllPieces & emptyMask) == 0)
                {
                    ulong attackPath = (1UL << 2) | (1UL << 3);
                    GenerateCastleMoves(board, attackPath, 2, Colour.White, moves);
                }
            }
        }

        public static void GenerateBlackKingMoves(Board board, List<Move> moves)
        {
            int piece = Piece.BlackKing;
            ulong kings = board.Pieces[Piece.BlackKing];
            ulong notBlack= ~board.Colours[Colour.Black];

            ulong u1 = (kings << 8) & notBlack;
            ExtractMoves(piece, u1, -8, moves);

            ulong d1 = (kings >> 8) & notBlack;
            ExtractMoves(piece, d1, 8, moves);

            ulong l1 = ((kings & Constants.NotAFile) >> 1) & notBlack;
            ExtractMoves(piece, l1, 1, moves);

            ulong r1 = ((kings & Constants.NotHFile) << 1) & notBlack;
            ExtractMoves(piece, r1, -1, moves);

            ulong u1l1 = ((kings & Constants.NotAFile) << 7) & notBlack;
            ExtractMoves(piece, u1l1, -7, moves);

            ulong u1r1 = ((kings & Constants.NotHFile) << 9) & notBlack;
            ExtractMoves(piece, u1r1, -9, moves);

            ulong d1l1 = ((kings & Constants.NotAFile) >> 9) & notBlack;
            ExtractMoves(piece, d1l1, 9, moves);

            ulong d1r1 = ((kings & Constants.NotHFile) >> 7) & notBlack;
            ExtractMoves(piece, d1r1, 7, moves);

            if ((board.CastleRights & CastlingRights.BK) != 0)
            {
                ulong emptyMask = (1UL << 61) | (1UL << 62);
                if ((board.AllPieces & emptyMask) == 0)
                {
                    GenerateCastleMoves(board, emptyMask, 62, Colour.Black, moves);
                }
            }

            if ((board.CastleRights & CastlingRights.BQ) != 0)
            {
                ulong emptyMask = (1UL << 57) | (1UL << 58) | (1UL << 59);
                if ((board.AllPieces & emptyMask) == 0)
                {
                    ulong attackPath = (1UL << 58) | (1UL << 59);
                    GenerateCastleMoves(board, attackPath, 58, Colour.Black, moves);
                }
            }
        }

        public static void GenerateCastleMoves(Board board, ulong transitPath, int destination, int colour, List<Move> moves)
        {
            ulong king = board.Pieces[5 + colour * 6];
            int attackColour = Colour.White == colour ? Colour.Black : Colour.White;

            if (board.IsSquareAttacked(BitOperations.TrailingZeroCount(king), attackColour)) return;
            while (transitPath != 0)
            {
                int pathSquare = BitOperations.TrailingZeroCount(transitPath);
                transitPath &= (transitPath - 1);
                if (board.IsSquareAttacked(pathSquare, attackColour)) return;
                if (((1UL << pathSquare) & board.AllPieces) != 0) return;
            }
            int piece = colour == Colour.White ? Piece.WhiteKing : Piece.BlackKing;
            moves.Add(new Move(piece, BitOperations.TrailingZeroCount(king), destination, SpecialMove.CASTLE));
        }

        public static List<Move> GenerateAllCaptures(Board board)
        {
            List<Move> moves = [];
            if (board.ActiveColour == Colour.White)
            {
                GenerateWhitePawnCaptures(board, moves);
                GenerateWhiteKnightCaptures(board, moves);
                GenerateWhiteBishopCaptures(board, moves);
                GenerateWhiteRookCaptures(board, moves);
                GenerateWhiteQueenCaptures(board, moves);
                GenerateWhiteKingCaptures(board, moves);
            }
            else
            {
                GenerateBlackPawnCaptures(board, moves);
                GenerateBlackKnightCaptures(board, moves);
                GenerateBlackBishopCaptures(board, moves);
                GenerateBlackRookCaptures(board, moves);
                GenerateBlackQueenCaptures(board, moves);
                GenerateBlackKingCaptures(board, moves);
            }

            return moves;
        }

        public static void GenerateWhitePawnCaptures(Board board, List<Move> moves)
        {
            int piece = Piece.WhitePawn;
            ulong pawns = board.Pieces[Piece.WhitePawn];

            ulong u1l1 = ((pawns & Constants.NotAFile) << 7) & board.Colours[Colour.Black];
            ExtractMoves(piece, u1l1, -7, moves);

            ulong u1r1 = ((pawns & Constants.NotHFile) << 9) & board.Colours[Colour.Black];
            ExtractMoves(piece, u1r1, -9, moves);

            if (board.EnPassantSquare != -1)
            {
                ulong EnPassantMask = 1UL << board.EnPassantSquare;

                ulong capLeft = ((pawns & Constants.NotAFile) << 7) & EnPassantMask;
                if (capLeft != 0) moves.Add(new Move(piece, board.EnPassantSquare - 7, board.EnPassantSquare, SpecialMove.EN_PASSANT));

                ulong capRight = ((pawns & Constants.NotHFile) << 9) & EnPassantMask;
                if (capRight != 0) moves.Add(new Move(piece, board.EnPassantSquare - 9, board.EnPassantSquare, SpecialMove.EN_PASSANT));
            }
        }

        public static void GenerateBlackPawnCaptures(Board board, List<Move> moves)
        {
            int piece = Piece.BlackPawn;
            ulong pawns = board.Pieces[Piece.BlackPawn];

            ulong d1r1 = ((pawns & Constants.NotHFile) >> 7) & board.Colours[Colour.White];
            ExtractMoves(piece, d1r1, 7, moves);

            ulong d1l1 = ((pawns & Constants.NotAFile) >> 9) & board.Colours[Colour.White];
            ExtractMoves(piece, d1l1, 9, moves);

            if (board.EnPassantSquare != -1)
            {
                ulong EnPassantMask = 1UL << board.EnPassantSquare;

                ulong capLeft = ((pawns & Constants.NotAFile) >> 9) & EnPassantMask;
                if (capLeft != 0) moves.Add(new Move(piece, board.EnPassantSquare + 9, board.EnPassantSquare, SpecialMove.EN_PASSANT));

                ulong capRight = ((pawns & Constants.NotHFile) >> 7) & EnPassantMask;
                if (capRight != 0) moves.Add(new Move(piece, board.EnPassantSquare + 7, board.EnPassantSquare, SpecialMove.EN_PASSANT));
            }
        }

        public static void GenerateWhiteKnightCaptures(Board board, List<Move> moves)
        {
            int piece = Piece.WhiteKnight;
            ulong knights = board.Pieces[Piece.WhiteKnight];
            ulong attack = board.Colours[Colour.Black];

            ulong u2l1 = ((knights & Constants.NotAFile) << 15) & attack;
            ExtractMoves(piece, u2l1, -15, moves);

            ulong u2r1 = ((knights & Constants.NotHFile) << 17) & attack;
            ExtractMoves(piece, u2r1, -17, moves);

            ulong d2l1 = ((knights & Constants.NotAFile) >> 17) & attack;
            ExtractMoves(piece, d2l1, 17, moves);

            ulong d2r1 = ((knights & Constants.NotHFile) >> 15) & attack;
            ExtractMoves(piece, d2r1, 15, moves);

            ulong u1l2 = ((knights & Constants.NotABFile) << 6) & attack;
            ExtractMoves(piece, u1l2, -6, moves);

            ulong u1r2 = ((knights & Constants.NotGHFile) << 10) & attack;
            ExtractMoves(piece, u1r2, -10, moves);

            ulong d1l2 = ((knights & Constants.NotABFile) >> 10) & attack;
            ExtractMoves(piece, d1l2, 10, moves);

            ulong d1r2 = ((knights & Constants.NotGHFile) >> 6) & attack;
            ExtractMoves(piece, d1r2, 6, moves);
        }

        public static void GenerateBlackKnightCaptures(Board board, List<Move> moves)
        {
            int piece = Piece.BlackKnight;
            ulong knights = board.Pieces[Piece.BlackKnight];
            ulong attack = board.Colours[Colour.White];

            ulong u2l1 = ((knights & Constants.NotAFile) << 15) & attack;
            ExtractMoves(piece, u2l1, -15, moves);

            ulong u2r1 = ((knights & Constants.NotHFile) << 17) & attack;
            ExtractMoves(piece, u2r1, -17, moves);

            ulong d2l1 = ((knights & Constants.NotAFile) >> 17) & attack;
            ExtractMoves(piece, d2l1, 17, moves);

            ulong d2r1 = ((knights & Constants.NotHFile) >> 15) & attack;
            ExtractMoves(piece, d2r1, 15, moves);

            ulong u1l2 = ((knights & Constants.NotABFile) << 6) & attack;
            ExtractMoves(piece, u1l2, -6, moves);

            ulong u1r2 = ((knights & Constants.NotGHFile) << 10) & attack;
            ExtractMoves(piece, u1r2, -10, moves);

            ulong d1l2 = ((knights & Constants.NotABFile) >> 10) & attack;
            ExtractMoves(piece, d1l2, 10, moves);

            ulong d1r2 = ((knights & Constants.NotGHFile) >> 6) & attack;
            ExtractMoves(piece, d1r2, 6, moves);
        }

        public static void GenerateWhiteBishopCaptures(Board board, List<Move> moves)
        {
            int piece = Piece.WhiteBishop;
            ulong bishops = board.Pieces[Piece.WhiteBishop];
            ulong notPiece = ~board.AllPieces;
            ulong attack = board.Colours[Colour.Black];

            ulong posu1l1 = bishops;
            ulong posu1r1 = bishops;
            ulong posd1l1 = bishops;
            ulong posd1r1 = bishops;

            for (int i = 1; i < 8; i++)
            {
                ulong u1l1 = (posu1l1 & Constants.NotAFile) << 7;
                ExtractMoves(piece, u1l1 & attack, -7 * i, moves);
                posu1l1 = u1l1 & notPiece;

                ulong u1r1 = (posu1r1 & Constants.NotHFile) << 9;
                ExtractMoves(piece, u1r1 & attack, -9 * i, moves);
                posu1r1 = u1r1 & notPiece;

                ulong d1l1 = (posd1l1 & Constants.NotAFile) >> 9;
                ExtractMoves(piece, d1l1 & attack, 9 * i, moves);
                posd1l1 = d1l1 & notPiece;

                ulong d1r1 = (posd1r1 & Constants.NotHFile) >> 7;
                ExtractMoves(piece, d1r1 & attack, 7 * i, moves);
                posd1r1 = d1r1 & notPiece;

                if ((posu1l1 | posu1r1 | posd1l1 | posd1r1) == 0) break;
            }
        }

        public static void GenerateBlackBishopCaptures(Board board, List<Move> moves)
        {
            int piece = Piece.BlackBishop;
            ulong bishops = board.Pieces[Piece.BlackBishop];
            ulong notPiece = ~board.AllPieces;
            ulong attack = board.Colours[Colour.White];

            ulong posu1l1 = bishops;
            ulong posu1r1 = bishops;
            ulong posd1l1 = bishops;
            ulong posd1r1 = bishops;

            for (int i = 1; i < 8; i++)
            {
                ulong u1l1 = (posu1l1 & Constants.NotAFile) << 7;
                ExtractMoves(piece, u1l1 & attack, -7 * i, moves);
                posu1l1 = u1l1 & notPiece;

                ulong u1r1 = (posu1r1 & Constants.NotHFile) << 9;
                ExtractMoves(piece, u1r1 & attack, -9 * i, moves);
                posu1r1 = u1r1 & notPiece;

                ulong d1l1 = (posd1l1 & Constants.NotAFile) >> 9;
                ExtractMoves(piece, d1l1 & attack, 9 * i, moves);
                posd1l1 = d1l1 & notPiece;

                ulong d1r1 = (posd1r1 & Constants.NotHFile) >> 7;
                ExtractMoves(piece, d1r1 & attack, 7 * i, moves);
                posd1r1 = d1r1 & notPiece;

                if ((posu1l1 | posu1r1 | posd1l1 | posd1r1) == 0) break;
            }
        }

        public static void GenerateWhiteRookCaptures(Board board, List<Move> moves)
        {
            int piece = Piece.WhiteRook;
            ulong rooks = board.Pieces[Piece.WhiteRook];
            ulong notPiece = ~board.AllPieces;
            ulong attack = board.Colours[Colour.Black];

            ulong posu1 = rooks;
            ulong posd1 = rooks;
            ulong posl1 = rooks;
            ulong posr1 = rooks;

            for (int i = 1; i < 8; i++)
            {
                ulong u1 = posu1 << 8;
                ExtractMoves(piece, u1 & attack, -8 * i, moves);
                posu1 = u1 & notPiece;

                ulong d1 = posd1 >> 8;
                ExtractMoves(piece, d1 & attack, 8 * i, moves);
                posd1 = d1 & notPiece;

                ulong l1 = (posl1 & Constants.NotAFile) >> 1;
                ExtractMoves(piece, l1 & attack, 1 * i, moves);
                posl1 = l1 & notPiece;

                ulong r1 = (posr1 & Constants.NotHFile) << 1;
                ExtractMoves(piece, r1 & attack, -1 * i, moves);
                posr1 = r1 & notPiece;

                if ((posu1 | posd1 | posl1 | posr1) == 0) break;
            }
        }

        public static void GenerateBlackRookCaptures(Board board, List<Move> moves)
        {
            int piece = Piece.BlackRook;
            ulong rooks = board.Pieces[Piece.BlackRook];
            ulong notPiece = ~board.AllPieces;
            ulong attack = board.Colours[Colour.White];

            ulong posu1 = rooks;
            ulong posd1 = rooks;
            ulong posl1 = rooks;
            ulong posr1 = rooks;

            for (int i = 1; i < 8; i++)
            {
                ulong u1 = posu1 << 8;
                ExtractMoves(piece, u1 & attack, -8 * i, moves);
                posu1 = u1 & notPiece;

                ulong d1 = posd1 >> 8;
                ExtractMoves(piece, d1 & attack, 8 * i, moves);
                posd1 = d1 & notPiece;

                ulong l1 = (posl1 & Constants.NotAFile) >> 1;
                ExtractMoves(piece, l1 & attack, 1 * i, moves);
                posl1 = l1 & notPiece;

                ulong r1 = (posr1 & Constants.NotHFile) << 1;
                ExtractMoves(piece, r1 & attack, -1 * i, moves);
                posr1 = r1 & notPiece;

                if ((posu1 | posd1 | posl1 | posr1) == 0) break;
            }
        }

        public static void GenerateWhiteQueenCaptures(Board board, List<Move> moves)
        {
            int piece = Piece.WhiteQueen;
            ulong queens = board.Pieces[Piece.WhiteQueen];
            ulong notPiece = ~board.AllPieces;
            ulong attack = board.Colours[Colour.Black];

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
                ulong u1 = posu1 << 8;
                ExtractMoves(piece, u1 & attack, -8 * i, moves);
                posu1 = u1 & notPiece;

                ulong d1 = posd1 >> 8;
                ExtractMoves(piece, d1 & attack, 8 * i, moves);
                posd1 = d1 & notPiece;

                ulong l1 = (posl1 & Constants.NotAFile) >> 1;
                ExtractMoves(piece, l1 & attack, 1 * i, moves);
                posl1 = l1 & notPiece;

                ulong r1 = (posr1 & Constants.NotHFile) << 1;
                ExtractMoves(piece, r1 & attack, -1 * i, moves);
                posr1 = r1 & notPiece;

                ulong u1l1 = (posu1l1 & Constants.NotAFile) << 7;
                ExtractMoves(piece, u1l1 & attack, -7 * i, moves);
                posu1l1 = u1l1 & notPiece;

                ulong u1r1 = (posu1r1 & Constants.NotHFile) << 9;
                ExtractMoves(piece, u1r1 & attack, -9 * i, moves);
                posu1r1 = u1r1 & notPiece;

                ulong d1l1 = (posd1l1 & Constants.NotAFile) >> 9;
                ExtractMoves(piece, d1l1 & attack, 9 * i, moves);
                posd1l1 = d1l1 & notPiece;

                ulong d1r1 = (posd1r1 & Constants.NotHFile) >> 7;
                ExtractMoves(piece, d1r1 & attack, 7 * i, moves);
                posd1r1 = d1r1 & notPiece;

                if ((posu1 | posd1 | posl1 | posr1 | posu1l1 | posu1r1 | posd1l1 | posd1r1) == 0) break;
            }
        }

        public static void GenerateBlackQueenCaptures(Board board, List<Move> moves)
        {
            int piece = Piece.BlackQueen;
            ulong queens = board.Pieces[Piece.BlackQueen];
            ulong notPiece = ~board.AllPieces;
            ulong attack = board.Colours[Colour.White];

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
                ulong u1 = posu1 << 8;
                ExtractMoves(piece, u1 & attack, -8 * i, moves);
                posu1 = u1 & notPiece;

                ulong d1 = posd1 >> 8;
                ExtractMoves(piece, d1 & attack, 8 * i, moves);
                posd1 = d1 & notPiece;

                ulong l1 = (posl1 & Constants.NotAFile) >> 1;
                ExtractMoves(piece, l1 & attack, 1 * i, moves);
                posl1 = l1 & notPiece;

                ulong r1 = (posr1 & Constants.NotHFile) << 1;
                ExtractMoves(piece, r1 & attack, -1 * i, moves);
                posr1 = r1 & notPiece;

                ulong u1l1 = (posu1l1 & Constants.NotAFile) << 7;
                ExtractMoves(piece, u1l1 & attack, -7 * i, moves);
                posu1l1 = u1l1 & notPiece;

                ulong u1r1 = (posu1r1 & Constants.NotHFile) << 9;
                ExtractMoves(piece, u1r1 & attack, -9 * i, moves);
                posu1r1 = u1r1 & notPiece;

                ulong d1l1 = (posd1l1 & Constants.NotAFile) >> 9;
                ExtractMoves(piece, d1l1 & attack, 9 * i, moves);
                posd1l1 = d1l1 & notPiece;

                ulong d1r1 = (posd1r1 & Constants.NotHFile) >> 7;
                ExtractMoves(piece, d1r1 & attack, 7 * i, moves);
                posd1r1 = d1r1 & notPiece;

                if ((posu1 | posd1 | posl1 | posr1 | posu1l1 | posu1r1 | posd1l1 | posd1r1) == 0) break;
            }
        }

        public static void GenerateWhiteKingCaptures(Board board, List<Move> moves)
        {
            int piece = Piece.WhiteKing;
            ulong kings = board.Pieces[Piece.WhiteKing];
            ulong attack = board.Colours[Colour.Black];

            ulong u1 = (kings << 8) & attack;
            ExtractMoves(piece, u1, -8, moves);

            ulong d1 = (kings >> 8) & attack;
            ExtractMoves(piece, d1, 8, moves);

            ulong l1 = ((kings & Constants.NotAFile) >> 1) & attack;
            ExtractMoves(piece, l1, 1, moves);

            ulong r1 = ((kings & Constants.NotHFile) << 1) & attack;
            ExtractMoves(piece, r1, -1, moves);

            ulong u1l1 = ((kings & Constants.NotAFile) << 7) & attack;
            ExtractMoves(piece, u1l1, -7, moves);

            ulong u1r1 = ((kings & Constants.NotHFile) << 9) & attack;
            ExtractMoves(piece, u1r1, -9, moves);

            ulong d1l1 = ((kings & Constants.NotAFile) >> 9) & attack;
            ExtractMoves(piece, d1l1, 9, moves);

            ulong d1r1 = ((kings & Constants.NotHFile) >> 7) & attack;
            ExtractMoves(piece, d1r1, 7, moves);
        }

        public static void GenerateBlackKingCaptures(Board board, List<Move> moves)
        {
            int piece = Piece.BlackKing;
            ulong kings = board.Pieces[Piece.BlackKing];
            ulong attack = board.Colours[Colour.White];

            ulong u1 = (kings << 8) & attack;
            ExtractMoves(piece, u1, -8, moves);

            ulong d1 = (kings >> 8) & attack;
            ExtractMoves(piece, d1, 8, moves);

            ulong l1 = ((kings & Constants.NotAFile) >> 1) & attack;
            ExtractMoves(piece, l1, 1, moves);

            ulong r1 = ((kings & Constants.NotHFile) << 1) & attack;
            ExtractMoves(piece, r1, -1, moves);

            ulong u1l1 = ((kings & Constants.NotAFile) << 7) & attack;
            ExtractMoves(piece, u1l1, -7, moves);

            ulong u1r1 = ((kings & Constants.NotHFile) << 9) & attack;
            ExtractMoves(piece, u1r1, -9, moves);

            ulong d1l1 = ((kings & Constants.NotAFile) >> 9) & attack;
            ExtractMoves(piece, d1l1, 9, moves);

            ulong d1r1 = ((kings & Constants.NotHFile) >> 7) & attack;
            ExtractMoves(piece, d1r1, 7, moves);
        }

        public static void ExtractMoves(int piece, ulong bitboard, int offset, List<Move> moves, SpecialMove special = SpecialMove.NONE)
        {
            int colour = piece < 6 ? Colour.White : Colour.Black;
            while (bitboard != 0)
            {
                int toSquare = BitOperations.TrailingZeroCount(bitboard);
                int fromSquare = toSquare + offset;
                if ((colour == Colour.White ? toSquare > 55 : toSquare < 8) && (piece == Piece.WhitePawn || piece == Piece.BlackPawn))
                {
                    moves.Add(new Move(piece, fromSquare, toSquare, SpecialMove.PROMOTION) { PromotionPiece = Piece.WhiteQueen + colour * 6 });
                    moves.Add(new Move(piece, fromSquare, toSquare, SpecialMove.PROMOTION) { PromotionPiece = Piece.WhiteRook + colour * 6 });
                    moves.Add(new Move(piece, fromSquare, toSquare, SpecialMove.PROMOTION) { PromotionPiece = Piece.WhiteKnight + colour * 6 });
                    moves.Add(new Move(piece, fromSquare, toSquare, SpecialMove.PROMOTION) { PromotionPiece = Piece.WhiteBishop + colour * 6 });
                }
                else
                {
                    moves.Add(new Move(piece, fromSquare, toSquare, special));
                }
                bitboard &= (bitboard - 1);
            }
        }
    }
}
