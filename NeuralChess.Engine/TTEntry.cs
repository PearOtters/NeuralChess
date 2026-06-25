using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.Engine
{
    public readonly struct TTEntry
    {
        readonly ulong ZobristKey;
        readonly int Move;
        readonly short Score;
        readonly byte Depth;
        readonly byte NodeFlag;
    }
}
