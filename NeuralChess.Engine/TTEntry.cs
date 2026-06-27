using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.Engine
{
    public struct TTEntry()
    {
        public ulong ZobristKey = 0;
        public int Move = 0;
        public short Score = 0;
        public byte Depth = 0;
        public byte NodeFlag = 0;

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
