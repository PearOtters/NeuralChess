using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.Engine
{
    public abstract class Engine(string name)
    {
        public readonly string Name = name;
        public byte IrreversibleTurn = 1;


        public abstract void Play(Board board, int maximumTime, int maximumDepth);
    }
}
