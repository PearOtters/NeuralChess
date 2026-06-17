namespace NeuralChess.Engine
{
    public class Program
    {
        static void Main()
        {
            Engine engine = new AlphaBeta(3, false);
            UCI.Loop(engine, true);
        }
    }
}
