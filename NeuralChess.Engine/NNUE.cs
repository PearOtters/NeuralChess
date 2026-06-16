using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text;

namespace NeuralChess.Engine
{
    public static class NNUE
    {
        private static readonly Vector256<short>[] W1 = new Vector256<short>[12_288];
        private static readonly Vector256<short>[] B1 = new Vector256<short>[16];
        private static readonly Vector256<sbyte>[] W2 = new Vector256<sbyte>[256];
        private static readonly Vector256<int>[] B2 = new Vector256<int>[4];
        private static readonly Vector256<sbyte> W3;
        private static readonly int B3;

        private static readonly Vector256<short>[] WAccumulator = new Vector256<short>[16];
        private static readonly Vector256<short>[] BAccumulator = new Vector256<short>[16];

        static NNUE()
        {
        }

        public static void GenerateAccumulatorFromBoard(Board board)
        {
            for (int v = 0; v < 16; v++)
            {
                WAccumulator[v] = B1[v];
                BAccumulator[v] = B1[v];
            }

            for (int wPiece = 0; wPiece < 12; wPiece++)
            {
                ulong bitboard = board.Pieces[wPiece];

                while (bitboard != 0)
                {
                    int wSquare = BitOperations.TrailingZeroCount(bitboard);

                    int bSquare = wSquare ^ 63;
                    int bPiece = (wPiece + 6) % 12;

                    int wBaseIndex = (wPiece * 64 + wSquare) * 16;
                    int bBaseIndex = (bPiece * 64 + bSquare) * 16;

                    for (int v = 0; v < 16; v++)
                    {
                        WAccumulator[v] = Vector256.Add(WAccumulator[v], W1[wBaseIndex + v]);
                        BAccumulator[v] = Vector256.Add(BAccumulator[v], W1[bBaseIndex + v]);
                    }

                    bitboard &= bitboard - 1;
                }
            }
        }

        public static void UpdateAccumulator(Move move)
        {
            int wFromSquare = move.FromSquare;
            int wToSquare = move.ToSquare;

            int bFromSquare = wFromSquare ^ 63;
            int bToSquare = wToSquare ^ 63;

            int wPiece = move.SelectedPiece;
            int wPromotion = move.PromotionPiece;

            int bPiece = (wPiece + 6) % 12;
            int bPromotion = (wPromotion + 6) % 12;

            int wFromIndex = (wPiece * 64 + wFromSquare) * 16;
            int wToIndex = ((wPromotion != -1 ? wPromotion : wPiece) * 64 + wToSquare) * 16;

            int bFromIndex = (bPiece * 64 + bFromSquare) * 16;
            int bToIndex = ((bPromotion != -1 ? bPromotion : bPiece) * 64 + bToSquare) * 16;

            if (move.Special == SpecialMove.CASTLE)
            {
                int wRook = wPiece - 2;
                int bRook = (wRook + 6) % 12;

                int wRookFrom, wRookTo;

                if (wToSquare > wFromSquare)
                {
                    wRookFrom = wToSquare + 1;
                    wRookTo = wToSquare - 1;
                }
                else
                {
                    wRookFrom = wToSquare - 2;
                    wRookTo = wToSquare + 1;
                }

                int bRookFrom = wRookFrom ^ 63;
                int bRookTo = wRookTo ^ 63;

                int wRookFromIndex = (wRook * 64 + wRookFrom) * 16;
                int wRookToIndex = (wRook * 64 + wRookTo) * 16;

                int bRookFromIndex = (bRook * 64 + bRookFrom) * 16;
                int bRookToIndex = (bRook * 64 + bRookTo) * 16;

                for (int v = 0; v < 16; v++)
                {
                    WAccumulator[v] = Vector256.Add(Vector256.Subtract(Vector256.Add(Vector256.Subtract(WAccumulator[v], W1[wFromIndex + v]), W1[wToIndex + v]), W1[wRookFromIndex + v]), W1[wRookToIndex + v]);
                    BAccumulator[v] = Vector256.Add(Vector256.Subtract(Vector256.Add(Vector256.Subtract(BAccumulator[v], W1[bFromIndex + v]), W1[bToIndex + v]), W1[bRookFromIndex + v]), W1[bRookToIndex + v]);
                }
            }
            else
            {
                if (move.CapturedPiece != -1)
                {
                    int bCaptured = move.CapturedPiece;
                    int wCaptured = (bCaptured + 6) % 12;

                    int bCapturedIndex;
                    int wCapturedIndex;

                    if (move.Special == SpecialMove.EN_PASSANT)
                    {
                        int epOffset = (wPiece < 6) ? -8 : 8;
                        int wTargetSquare = wToSquare + epOffset;
                        int bTargetSquare = wTargetSquare ^ 63;

                        bCapturedIndex = (bCaptured * 64 + wTargetSquare) * 16;
                        wCapturedIndex = (wCaptured * 64 + bTargetSquare) * 16;
                    }
                    else
                    {
                        bCapturedIndex = (bCaptured * 64 + wToSquare) * 16;
                        wCapturedIndex = (wCaptured * 64 + bToSquare) * 16;
                    }

                    for (int v = 0; v < 16; v++)
                    {
                        WAccumulator[v] = Vector256.Add(Vector256.Subtract(Vector256.Subtract(WAccumulator[v], W1[bCapturedIndex + v]), W1[wFromIndex + v]), W1[wToIndex + v]);
                        BAccumulator[v] = Vector256.Add(Vector256.Subtract(Vector256.Subtract(BAccumulator[v], W1[wCapturedIndex + v]), W1[bFromIndex + v]), W1[bToIndex + v]);
                    }
                }
                else
                {
                    for (int v = 0; v < 16; v++)
                    {
                        WAccumulator[v] = Vector256.Add(Vector256.Subtract(WAccumulator[v], W1[wFromIndex + v]), W1[wToIndex + v]);
                        BAccumulator[v] = Vector256.Add(Vector256.Subtract(BAccumulator[v], W1[bFromIndex + v]), W1[bToIndex + v]);
                    }
                }
            }
        }

        public static void ReverseAccumulator(Move move)
        {
            int wFromSquare = move.FromSquare;
            int wToSquare = move.ToSquare;

            int bFromSquare = wFromSquare ^ 63;
            int bToSquare = wToSquare ^ 63;

            int wPiece = move.SelectedPiece;
            int wPromotion = move.PromotionPiece;

            int bPiece = (wPiece + 6) % 12;
            int bPromotion = (wPromotion + 6) % 12;

            int wFromIndex = (wPiece * 64 + wFromSquare) * 16;
            int wToIndex = ((wPromotion != -1 ? wPromotion : wPiece) * 64 + wToSquare) * 16;

            int bFromIndex = (bPiece * 64 + bFromSquare) * 16;
            int bToIndex = ((bPromotion != -1 ? bPromotion : bPiece) * 64 + bToSquare) * 16;

            if (move.Special == SpecialMove.CASTLE)
            {
                int wRook = wPiece - 2;
                int bRook = (wRook + 6) % 12;

                int wRookFrom, wRookTo;

                if (wToSquare > wFromSquare)
                {
                    wRookFrom = wToSquare + 1;
                    wRookTo = wToSquare - 1;
                }
                else
                {
                    wRookFrom = wToSquare - 2;
                    wRookTo = wToSquare + 1;
                }

                int bRookFrom = wRookFrom ^ 63;
                int bRookTo = wRookTo ^ 63;

                int wRookFromIndex = (wRook * 64 + wRookFrom) * 16;
                int wRookToIndex = (wRook * 64 + wRookTo) * 16;

                int bRookFromIndex = (bRook * 64 + bRookFrom) * 16;
                int bRookToIndex = (bRook * 64 + bRookTo) * 16;

                for (int v = 0; v < 16; v++)
                {
                    WAccumulator[v] = Vector256.Subtract(Vector256.Add(Vector256.Subtract(Vector256.Add(WAccumulator[v], W1[wFromIndex + v]), W1[wToIndex + v]), W1[wRookFromIndex + v]), W1[wRookToIndex + v]);
                    BAccumulator[v] = Vector256.Subtract(Vector256.Add(Vector256.Subtract(Vector256.Add(BAccumulator[v], W1[bFromIndex + v]), W1[bToIndex + v]), W1[bRookFromIndex + v]), W1[bRookToIndex + v]);
                }
            }
            else
            {
                if (move.CapturedPiece != -1)
                {
                    int bCaptured = move.CapturedPiece;
                    int wCaptured = (bCaptured + 6) % 12;

                    int bCapturedIndex;
                    int wCapturedIndex;

                    if (move.Special == SpecialMove.EN_PASSANT)
                    {
                        int epOffset = (wPiece < 6) ? -8 : 8;
                        int wTargetSquare = wToSquare + epOffset;
                        int bTargetSquare = wTargetSquare ^ 63;

                        bCapturedIndex = (bCaptured * 64 + wTargetSquare) * 16;
                        wCapturedIndex = (wCaptured * 64 + bTargetSquare) * 16;
                    }
                    else
                    {
                        bCapturedIndex = (bCaptured * 64 + wToSquare) * 16;
                        wCapturedIndex = (wCaptured * 64 + bToSquare) * 16;
                    }

                    for (int v = 0; v < 16; v++)
                    {
                        WAccumulator[v] = Vector256.Subtract(Vector256.Add(Vector256.Add(WAccumulator[v], W1[bCapturedIndex + v]), W1[wFromIndex + v]), W1[wToIndex + v]);
                        BAccumulator[v] = Vector256.Subtract(Vector256.Add(Vector256.Add(BAccumulator[v], W1[wCapturedIndex + v]), W1[bFromIndex + v]), W1[bToIndex + v]);
                    }
                }
                else
                {
                    for (int v = 0; v < 16; v++)
                    {
                        WAccumulator[v] = Vector256.Subtract(Vector256.Add(WAccumulator[v], W1[wFromIndex + v]), W1[wToIndex + v]);
                        BAccumulator[v] = Vector256.Subtract(Vector256.Add(BAccumulator[v], W1[bFromIndex + v]), W1[bToIndex + v]);
                    }
                }
            }
        }
    }
}
