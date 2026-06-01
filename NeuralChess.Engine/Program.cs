namespace NeuralChess.Engine
{
    public class Program
    {
        static void Main(string[] args)
        {
            Engine engine = new MinMax(5);
            UCI.Loop(engine);
        }
    }
}
