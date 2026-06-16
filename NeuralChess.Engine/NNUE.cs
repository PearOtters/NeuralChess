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

        }
    }
}
