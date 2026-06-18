using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace NeuralChess.Engine
{
    public static class NNUE
    {
        // Note to later:
        // Unless switching to NEGAMAX for simplicity later on W and B Accumulators can be simplified into one as only one perspective is needed

        private static readonly Vector256<short>[] W1 = new Vector256<short>[12_288];
        private static readonly Vector256<short>[] B1 = new Vector256<short>[16];

        private static readonly Vector256<sbyte>[] W2 = new Vector256<sbyte>[256];
        private static readonly int[] B2 = new int[32];

        private static readonly int[] W3 = new int[32];
        private static readonly int B3;

        private static readonly Vector256<short>[] WAccumulator = new Vector256<short>[16];
        private static readonly Vector256<short>[] BAccumulator = new Vector256<short>[16];

        private static readonly Vector256<short> LowerBound = Vector256<short>.Zero;
        private static readonly Vector256<short> UpperBound = Vector256<short>.One * 127;
        private static readonly Vector256<short> Ones = Vector256<short>.One;

        static NNUE()
        {
            byte[] rawBytes = File.ReadAllBytes("nnue_network.bin");

            if (rawBytes.Length != 402_180)
            {
                throw new Exception($"Invalid NNUE file size. Expected 402,084 bytes but found {rawBytes.Length}.");
            }

            int offset = 0;

            unsafe
            {
                fixed (byte* ptr = rawBytes)
                {
                    short* w1Ptr = (short*)(ptr + offset);
                    for (int i = 0; i < 12_288; i++)
                    {
                        W1[i] = Vector256.Load(w1Ptr + (i * 16));
                    }
                    offset += 393_216;

                    short* b1Ptr = (short*)(ptr + offset);
                    for (int i = 0; i < 16; i++)
                    {
                        B1[i] = Vector256.Load(b1Ptr + (i * 16));
                    }
                    offset += 512;

                    int* b2Ptr = (int*)(ptr + offset);
                    for (int i = 0; i < 32; i++)
                    {
                        B2[i] = b2Ptr[i];
                    }
                    offset += 128;

                    sbyte* w2Ptr = (sbyte*)(ptr + offset);
                    for (int i = 0; i < 256; i++)
                    {
                        W2[i] = Vector256.Load(w2Ptr + (i * 32));
                    }
                    offset += 8_192;

                    int* b3Ptr = (int*)(ptr + offset);
                    B3 = b3Ptr[0];
                    offset += 4;

                    int* w3Ptr = (int*)(ptr + offset);
                    for (int i = 0; i < 32; i++)
                    {
                        W3[i] = w3Ptr[i];
                    }
                }
            }
        }

        public static void Initialise()
        {
            Board dummyBoard = new(Constants.regular_start);
            GenerateAccumulatorFromBoard(dummyBoard);
            GetBoardValue(Colour.White);
            GetBoardValue(Colour.Black);
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

        public static int GetBoardValue(int colour)
        {
            Span<Vector256<byte>> ClampedAccumulator = stackalloc Vector256<byte>[8];

            if (colour == Colour.White)
            {
                for (int v = 0; v < 8; v++)
                {
                    ClampedAccumulator[v] = Avx2.PackUnsignedSaturate(Vector256.Clamp(WAccumulator[v * 2], LowerBound, UpperBound), Vector256.Clamp(WAccumulator[v * 2 + 1], LowerBound, UpperBound));
                }
            }
            else
            {
                for (int v = 0; v < 8; v++)
                {
                    ClampedAccumulator[v] = Avx2.PackUnsignedSaturate(Vector256.Clamp(BAccumulator[v * 2], LowerBound, UpperBound), Vector256.Clamp(BAccumulator[v * 2 + 1], LowerBound, UpperBound));
                }
            }


            int weightIndex = 0;

            int runningScaledTotal = 0;
            int shift = 4;
            int finalScale = 161;

            for (int n = 0; n < 32; n++)
            {
                Vector256<int> nodeSum = Vector256<int>.Zero;

                for (int i = 0; i < 8; i++)
                {
                    Vector256<byte> inputs = ClampedAccumulator[i];
                    Vector256<sbyte> weights = W2[weightIndex++];

                    Vector256<short> mult16 = Avx2.MultiplyAddAdjacent(inputs, weights);
                    Vector256<int> mult32 = Avx2.MultiplyAddAdjacent(mult16, Ones);

                    nodeSum = Avx2.Add(nodeSum, mult32);
                }

                int currentSum = Vector256.Sum(nodeSum) + B2[n];
                sbyte clampedSum = (sbyte)int.Clamp(currentSum >> shift, 0, 127);

                runningScaledTotal += clampedSum * W3[n];
            }

            return (B3 + runningScaledTotal) / finalScale;
        }
    }
}
