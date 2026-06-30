using System.Diagnostics;

namespace NeuralChess.Engine
{
    public class RandomMove() : Engine("Random")
    {
        private readonly Stopwatch SearchTimer = Stopwatch.StartNew();

        public override void Play(Board board, ref string response, bool toLog, int maximumTime, int maximumDepth)
        {
            SearchTimer.Restart();
            Span<Move> pseudoMoves = stackalloc Move[218];
            int pseudoMovesCount = 0;
            MoveGenerator.GenerateAllMoves(board, ref pseudoMoves, ref pseudoMovesCount);
            Span<Move> legalMoves = stackalloc Move[218];
            int legalMovesCount = 0;

            for (int i = 0; i < pseudoMovesCount; i++)
            {
                Move m = pseudoMoves[i];
                if (m.IsLegal(board)) legalMoves[legalMovesCount++] = m;
            }

            Move bestMove;
            if (legalMovesCount > 0)
            {
                Random rng = new();
                bestMove = legalMoves[rng.Next(legalMovesCount)];
                response = $"bestmove {bestMove.ToUCI()}";
                if (toLog)
                {
                    int multiplier = board.ActiveColour == Colour.White ? 1 : -1;
                    double timeTaken = SearchTimer.ElapsedMilliseconds / 1000d;
                    File.AppendAllText("log.txt", $"time taken: {timeTaken}\n");
                    File.AppendAllText("log.txt", $"pre move neural network evaluation: {NeuralNetworkHandler.GetBoardValue(board, board.ActiveColour) / 100d}\n");
                    File.AppendAllText("log.txt", $"pre move static evaluation: {board.GetBoardValue() * multiplier / 100d}\n");
                    bestMove.MovePiece(board);
                    board.ActiveColour ^= 1;
                    File.AppendAllText("log.txt", $"post move neural network evaluation: {NeuralNetworkHandler.GetBoardValue(board, board.ActiveColour ^ 1) / 100d}\n");
                    File.AppendAllText("log.txt", $"post move static evaluation: {board.GetBoardValue() * multiplier / 100d}\n\n");
                    bestMove.ReverseMove(board);
                    board.ActiveColour ^= 1;
                }
            }
            else
            {
                response = "bestmove (none)";
            }
        }
    }
}
