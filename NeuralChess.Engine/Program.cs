namespace NeuralChess.Engine
{
    public class Program
    {
        static void Main()
        {
            Engine engine = new MinMax(5);
            UCI.Loop(engine, false);
        }
    }
}
