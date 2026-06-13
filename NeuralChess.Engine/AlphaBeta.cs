using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace NeuralChess.Engine
{
    public class AlphaBeta(int MaxDepth, bool UseNeuralNetwork = true) : MinMax(MaxDepth, true, UseNeuralNetwork)
    {
    }
}
