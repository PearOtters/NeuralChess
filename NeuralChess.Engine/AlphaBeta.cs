using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace NeuralChess.Engine
{
    public class AlphaBeta(int MaxDepth, bool UseNNUE = true) : Engine($"AlphaBeta {(UseNNUE ? "NNUE" : "")} Def {MaxDepth}")
    {
        private const int CheckmateScore = 1000000;
        private static readonly int[] MVV_LVA_Values =
        [
            100, 300, 300, 500, 900, 10000,
            100, 300, 300, 500, 900, 10000
        ];
        private int NodesSearched;
        private Stopwatch SearchTimer = Stopwatch.StartNew();
        private int MaximumTime;
        private bool TimeIsUp;

        public override void Play(Board board, int maximumTime, int maximumDepth)
        {
            SearchTimer.Restart();
            NodesSearched = 0;
            TimeIsUp = false;

            if (UseNNUE) NNUE.GenerateAccumulatorFromBoard(board);

            List<Move> pseudoLegalMoves = MoveGenerator.GenerateAllMoves(board);
            OrderMoves(pseudoLegalMoves, board);

            int toDepth = int.MaxValue;
            if (maximumTime == -1)
            {
                toDepth = maximumDepth != -1 ? maximumDepth : MaxDepth;
                MaximumTime = int.MaxValue;
            }
            else MaximumTime = maximumTime;

            List<Move> rootLegalMoves = [];
            foreach (Move move in pseudoLegalMoves)
            {
                if (move.IsLegal(board)) rootLegalMoves.Add(move);
            }

            if (rootLegalMoves.Count == 0)
            {
                Console.WriteLine("bestmove (none)");
                return;
            }

            Move currentBestMove = rootLegalMoves[0];

            int aiColour = board.ActiveColour;
            int multiplier = board.ActiveColour == Colour.White ? 1 : -1;

            bool abortSearch = false;
            int completedDepth = 0;

            for (int depth = 1; depth <= toDepth; depth++)
            {
                Move? depthBestMove = null;
                int bestGain = int.MinValue;

                int alpha = int.MinValue;
                int beta = int.MaxValue;

                if (depth > 1 && currentBestMove != null)
                {
                    rootLegalMoves.Remove(currentBestMove);
                    rootLegalMoves.Insert(0, currentBestMove);
                }

                foreach (Move move in rootLegalMoves)
                {
                    if (TimeIsUp)
                    {
                        abortSearch = true;
                        break;
                    }

                    move.MovePiece(board);
                    if (UseNNUE) NNUE.UpdateAccumulator(move);
                    board.ActiveColour ^= 1;

                    int moveValue = RecursiveMinMaxed(board, depth - 1, aiColour, multiplier, alpha, beta);

                    board.ActiveColour ^= 1;
                    move.ReverseMove(board);
                    if (UseNNUE) NNUE.ReverseAccumulator(move);

                    if (moveValue > bestGain)
                    {
                        depthBestMove = move;
                        bestGain = moveValue;
                    }

                    alpha = Math.Max(alpha, bestGain);
                }

                if (abortSearch)
                {
                    break;
                }

                if (depthBestMove != null)
                {
                    currentBestMove = depthBestMove;
                    completedDepth = depth;
                }
            }
            if (currentBestMove == null)
            {
                Console.WriteLine("bestmove (none)");
                return;
            }
            Console.WriteLine($"bestmove {currentBestMove.ToUCI()}");

            double timeTaken = SearchTimer.ElapsedMilliseconds / 1000d;
            File.AppendAllText("log.txt", $"time taken: {timeTaken}\n");
            File.AppendAllText("log.txt", $"depth completed: {completedDepth}\n");
            if (UseNNUE) File.AppendAllText("log.txt", $"pre move NNUE evaluation: {(board.ActiveColour == aiColour ? NNUE.GetBoardValue(board.ActiveColour) : -NNUE.GetBoardValue(board.ActiveColour)) / 100d}\n");
            File.AppendAllText("log.txt", $"pre move neural network evaluation: {NeuralNetworkHandler.GetBoardValue(board, aiColour) / 100d}\n");
            File.AppendAllText("log.txt", $"pre move static evaluation: {board.GetBoardValue() * multiplier / 100d}\n");
            currentBestMove.MovePiece(board);
            board.ActiveColour ^= 1;
            if (UseNNUE) NNUE.UpdateAccumulator(currentBestMove);
            if (UseNNUE) File.AppendAllText("log.txt", $"post move NNUE evaluation: {(board.ActiveColour == aiColour ? NNUE.GetBoardValue(board.ActiveColour) : -NNUE.GetBoardValue(board.ActiveColour)) / 100d}\n");
            File.AppendAllText("log.txt", $"post move neural network evaluation: {NeuralNetworkHandler.GetBoardValue(board, aiColour) / 100d}\n");
            File.AppendAllText("log.txt", $"post move static evaluation: {board.GetBoardValue() * multiplier / 100d}\n\n");
            currentBestMove.ReverseMove(board);
            board.ActiveColour ^= 1;
            if (UseNNUE) NNUE.ReverseAccumulator(currentBestMove);
        }

        private int RecursiveMinMaxed(Board board, int depth, int aiColour, int multiplier, int alpha, int beta)
        {
            NodesSearched++;

            if ((NodesSearched & 2047) == 0)
            {
                if (SearchTimer.ElapsedMilliseconds >= MaximumTime)
                {
                    TimeIsUp = true;
                }
            }
            if (TimeIsUp)
            {
                return 0;
            }

            if (depth == 0)
            {
                return QuiescenceSearch(board, aiColour, multiplier, alpha, beta);
            }

            bool isMaximising = (aiColour == board.ActiveColour);
            int bestGain = aiColour == board.ActiveColour ? int.MinValue : int.MaxValue;

            List<Move> pseudoLegalMoves = MoveGenerator.GenerateAllMoves(board);

            OrderMoves(pseudoLegalMoves, board);

            List<Move> legalMoves = [];

            foreach (Move move in pseudoLegalMoves)
            {
                if (move.IsLegal(board))
                {
                    legalMoves.Add(move);

                    move.MovePiece(board);
                    if (UseNNUE) NNUE.UpdateAccumulator(move);
                    board.ActiveColour ^= 1;

                    int moveValue = RecursiveMinMaxed(board, depth - 1, aiColour, multiplier, alpha, beta);

                    board.ActiveColour ^= 1;
                    move.ReverseMove(board);
                    if (UseNNUE) NNUE.ReverseAccumulator(move);

                    if (isMaximising)
                    {
                        bestGain = Math.Max(moveValue, bestGain);
                        alpha = Math.Max(alpha, bestGain);
                    }
                    else
                    {
                        bestGain = Math.Min(moveValue, bestGain);
                        beta = Math.Min(beta, bestGain);
                    }

                    if (beta <= alpha)
                    {
                        break;
                    }
                }
            }

            if (legalMoves.Count == 0)
            {
                int kingSquare = BitOperations.TrailingZeroCount(board.Pieces[Piece.WhiteKing + board.ActiveColour * 6]);
                bool inCheck = board.IsSquareAttacked(kingSquare, board.ActiveColour ^ 1);

                if (inCheck)
                {
                    return board.ActiveColour == aiColour ? -CheckmateScore - depth : CheckmateScore + depth;
                }
                else
                {
                    return 0;
                }
            }

            return bestGain;
        }

        private static void OrderMoves(List<Move> moves, Board board)
        {
            foreach (Move move in moves)
            {
                move.Score = 0;
                ulong toSquareMask = 1UL << move.ToSquare;

                if ((board.AllPieces & toSquareMask) != 0)
                {
                    int victimType = -1;
                    for (int i = 0; i < 12; i++)
                    {
                        if ((board.Pieces[i] & toSquareMask) != 0)
                        {
                            victimType = i;
                            break;
                        }
                    }

                    if (victimType != -1)
                    {
                        move.Score = 10 * MVV_LVA_Values[victimType] - MVV_LVA_Values[move.SelectedPiece];
                    }
                }

                if (move.Special == SpecialMove.PROMOTION)
                {
                    move.Score += 9000;
                }
            }

            moves.Sort((m1, m2) => m2.Score.CompareTo(m1.Score));
        }

        private int QuiescenceSearch(Board board, int aiColour, int multiplier, int alpha, int beta)
        {
            int standPat;

            if (UseNNUE)
            {
                int activePlayerScore = NNUE.GetBoardValue(board.ActiveColour);
                standPat = (board.ActiveColour == aiColour) ? activePlayerScore : -activePlayerScore;
            }
            else standPat = board.GetBoardValue() * multiplier;
            bool isMaximising = (aiColour == board.ActiveColour);

            if (isMaximising)
            {
                if (standPat >= beta) return beta;
                if (standPat > alpha) alpha = standPat;
            }
            else
            {
                if (standPat <= alpha) return alpha;
                if (standPat < beta) beta = standPat;
            }

            int bestGain = standPat;

            List<Move> captures = MoveGenerator.GenerateAllCaptures(board);

            OrderMoves(captures, board);

            foreach (Move move in captures)
            {
                if (move.IsLegal(board))
                {
                    move.MovePiece(board);
                    if (UseNNUE) NNUE.UpdateAccumulator(move);
                    board.ActiveColour ^= 1;

                    int moveValue = QuiescenceSearch(board, aiColour, multiplier, alpha, beta);

                    board.ActiveColour ^= 1;
                    move.ReverseMove(board);
                    if (UseNNUE) NNUE.ReverseAccumulator(move);

                    if (isMaximising)
                    {
                        bestGain = Math.Max(moveValue, bestGain);
                        alpha = Math.Max(alpha, bestGain);
                    }
                    else
                    {
                        bestGain = Math.Min(moveValue, bestGain);
                        beta = Math.Min(beta, bestGain);
                    }

                    if (beta <= alpha)
                    {
                        break;
                    }
                }
            }

            return bestGain;
        }
    }
}
