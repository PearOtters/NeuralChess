using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralChess.Engine
{
    public class Move
    {
        int fromSquare, toSquare;
        public Move(int fromSquare, int toSquare)
        {
            this.fromSquare = fromSquare;
            this.toSquare = toSquare;
        }
    }
}
