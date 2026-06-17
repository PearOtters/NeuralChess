using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace NeuralChess.Engine
{
    public class AlphaBeta(int MaxDepth, bool UseNNUE = true) : MinMax(MaxDepth, true, UseNNUE)
    {
    }
}
