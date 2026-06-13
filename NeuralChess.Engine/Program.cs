namespace NeuralChess.Engine
{
    public class Program
    {
        static void Main()
        {
            Engine engine = new AlphaBeta(7);
            UCI.Loop(engine, true);
        }
    }
}
