using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace NeuralChess.Engine
{
    public class AlphaBeta(int maxDepth) : MinMax(maxDepth, true)
    {
    }
}
