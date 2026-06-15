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
            Span<short> wAccumBuffer = stackalloc short[16];
            Span<short> bAccumBuffer = stackalloc short[16];

            Span<int> onePositions = stackalloc int[32];
            int pieceCount = 0;

            for (int i = 0; i < 12; i++)
            {
                ulong piece = board.Pieces[i];
                while (piece != 0)
                {
                    int square = BitOperations.TrailingZeroCount(piece);
                    onePositions[pieceCount++] = 64 * i + square;
                    piece &= piece - 1;
                }
            }
            // 256 * 48 vectors
            for (int i = 0; i < 256; i++)
            {
                short wRunningTotal = 0;
                short bRunningTotal = 0;

                for (int j = 0; j < pieceCount; i++)
                {
                    int wPosition = onePositions[pieceCount];
                    int wSquare = wPosition % 64;
                    int wPiece = (wPosition - wSquare) / 64;

                    int bSquare = wSquare ^ 63;
                    int bPiece = (wPiece + 6) % 12;
                    int bPosition = bSquare + bPiece * 64;

                    Vector256<short> wPos = W1[i * 48 + wPosition / 16];
                    Vector256<short> bPos = W1[i * 48 + bPosition / 16];

                    wRunningTotal += wPos[wPosition % 16];
                    bRunningTotal += bPos[bPosition % 16];
                }

                wAccumBuffer[i % 16] = wRunningTotal;
                bAccumBuffer[i % 16] = bRunningTotal;

                if (i % 16 == 15)
                {
                    Vector256<short> wTempVec = Vector256.Create(wAccumBuffer);
                    Vector256<short> bTempVec = Vector256.Create(bAccumBuffer);

                    WAccumulator[i / 16] = wTempVec;
                    BAccumulator[i / 16] = bTempVec;

                    wAccumBuffer.Clear();
                    bAccumBuffer.Clear();
                }
                
            }
        }

        public static void UpdateAccumulator(Move move)
        {

        }
    }
}
