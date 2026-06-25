using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace NeuralChess.Engine
{
    public static class Zobrist
    {
        public static readonly ulong[,] PieceKeys = new ulong[12, 64];
        public static readonly ulong SideToMove;
        public static readonly ulong[] CastlingRights = new ulong[16];
        public static readonly ulong[] EnPassantSquares = new ulong[8];

        public const int Seed = 1000;

        public static void Initialise()
        {
            RandomUlong(new Random(Seed));
        }

        static Zobrist()
        {
            Random rand = new(Seed);

            for (int piece = 0; piece < 12; piece++)
            {
                for (int square = 0; square < 64; square++)
                {
                    PieceKeys[piece, square] = RandomUlong(rand);
                }
            }

            SideToMove = RandomUlong(rand);

            for (int i = 0; i < 16; i++) CastlingRights[i] = RandomUlong(rand);
            for (int i = 0; i < 16; i++) EnPassantSquares[i] = RandomUlong(rand);
        }

        static ulong RandomUlong(Random rand)
        {
            byte[] bytes = new byte[8];
            rand.NextBytes(bytes);
            return BitConverter.ToUInt64(bytes);
        }
    }
}
