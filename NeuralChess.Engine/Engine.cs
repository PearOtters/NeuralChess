using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.Engine
{
    public abstract class Engine
    {
        public readonly string Name;

        public Engine(string name)
        {
            Name = name;
        }

        public abstract void Play(Board board, int maximumTime);
    }
}
