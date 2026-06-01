using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.Engine
{
    public abstract class Engine
    {
        public abstract void Play(Board board);
    }
}
