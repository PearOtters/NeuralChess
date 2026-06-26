using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.Engine
{
    public struct TTEntry()
    {
        public ulong ZobristKey;
        public int Move;
        public short Score;
        public byte Depth;
        public byte NodeFlag;

        public TTEntry(ulong zobristKey, int move, short score, byte depth, byte nodeFlag) : this()
        {
            ZobristKey = zobristKey;
            Move = move;
            Score = score;
            Depth = depth;
            NodeFlag = nodeFlag;
        }
    }
}
