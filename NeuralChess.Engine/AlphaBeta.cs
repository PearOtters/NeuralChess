using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace NeuralChess.Engine
{
    public class AlphaBeta(int MaxDepth, bool UseNNUE = true) : Engine($"AlphaBeta {(UseNNUE ? "NNUE" : "")} Def {MaxDepth}")
    {
        private const int CheckmateScore = 30000;
        private static readonly int[] MVV_LVA_Values =
        [
            100, 300, 300, 500, 900, 10000,
            100, 300, 300, 500, 900, 10000
        ];
        private int NodesSearched;
        private Stopwatch SearchTimer = Stopwatch.StartNew();
        private int MaximumTime;
        private bool TimeIsUp;
        private const int TTSize = 16_777_216;
        private const int TTMask = TTSize - 1;
        private static TTEntry[] TranspositionTable = new TTEntry[TTSize];

        public override void Play(Board board, int maximumTime, int maximumDepth)
        {
            SearchTimer.Restart();
            NodesSearched = 0;
            TimeIsUp = false;

            if (UseNNUE) NNUE.GenerateAccumulatorFromBoard(board);


            Span<Move> pseudoLegalMoves = stackalloc Move[218];
            int pseudoLegalMovesCount = 0;
            MoveGenerator.GenerateAllMoves(board, ref pseudoLegalMoves, ref pseudoLegalMovesCount);

            int toDepth = int.MaxValue;
            if (maximumTime == -1)
            {
                toDepth = maximumDepth != -1 ? maximumDepth : MaxDepth;
                MaximumTime = int.MaxValue;
            }
            else MaximumTime = maximumTime;

            Span<Move> rootLegalMoves = stackalloc Move[218];
            int legalMovesCount = 0;
            for (int i = 0; i < pseudoLegalMovesCount; i++)
            {
                if (pseudoLegalMoves[i].IsLegal(board)) rootLegalMoves[legalMovesCount++] = pseudoLegalMoves[i];
            }

            if (legalMovesCount == 0)
            {
                Console.WriteLine("bestmove (none)");
                return;
            }

            Move currentBestMove = rootLegalMoves[0];

            int aiColour = board.ActiveColour;
            int multiplier = board.ActiveColour == Colour.White ? 1 : -1;

            int bestDepth = 0;

            TTEntry tTEntry = TranspositionTable[board.ZobristHash & (TTMask)];
            if (tTEntry.ZobristKey == board.ZobristHash)
            {
                bestDepth = tTEntry.Depth;
                currentBestMove = new Move(tTEntry.Move);
                OrderMoves(ref rootLegalMoves, legalMovesCount, board, currentBestMove.MoveValue);
            }
            else OrderMoves(ref rootLegalMoves, legalMovesCount, board);

            int completedDepth = bestDepth;

            for (byte depth = 1; depth <= toDepth; depth++)
            {
                Move depthBestMove = rootLegalMoves[0];
                int bestGain = int.MinValue;

                int alpha = int.MinValue;
                int beta = int.MaxValue;

                if (depth > completedDepth)
                {
                    OrderMoves(ref rootLegalMoves, legalMovesCount, board, currentBestMove.MoveValue);
                }

                for (int m = 0; m < legalMovesCount; m++)
                {
                    Move move = rootLegalMoves[m];
                    if (TimeIsUp) break;

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

                if (TimeIsUp) break;

                if (depth > bestDepth)
                {
                    currentBestMove = depthBestMove;
                    completedDepth = depth;
                    TranspositionTable[board.ZobristHash & TTMask] = new TTEntry(board.ZobristHash, depthBestMove.MoveValue, (short)bestGain, depth, 0);
                }
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

            Span<Move> pseudoLegalMoves = stackalloc Move[218];
            int pseudoLegalMovesCount = 0;
            MoveGenerator.GenerateAllMoves(board, ref pseudoLegalMoves, ref pseudoLegalMovesCount);

            OrderMoves(ref pseudoLegalMoves, pseudoLegalMovesCount, board);

            int legalMoves = 0;

            for (int m = 0; m < pseudoLegalMovesCount; m++)
            {
                Move move = pseudoLegalMoves[m];
                if (move.IsLegal(board))
                {
                    legalMoves++;

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

            if (legalMoves == 0)
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

        private static void OrderMoves(ref Span<Move> moves, int movesCount, Board board, int hashMoveInt)
        {
            for (int m = 0; m < movesCount; m++)
            {
                Move move = moves[m];
                move.Score = 1;

                if (move.MoveValue == hashMoveInt)
                {
                    move.Score = 2000000;
                    continue;
                }

                if (move.Special == SpecialMove.EN_PASSANT)
                {
                    move.Score = 900;
                }

                else
                {
                    ulong toSquareMask = 1UL << move.ToSquare;

                    if ((board.AllPieces & toSquareMask) != 0)
                    {
                        int victimType = -1;

                        int startEnemy = move.SelectedPiece < 6 ? 6 : 0;
                        int endEnemy = move.SelectedPiece < 6 ? 12 : 6;

                        for (int i = startEnemy; i < endEnemy; i++)
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
                }

                if (move.Special == SpecialMove.PROMOTION)
                {
                    move.Score += 9000;
                }
            }

            moves.Sort((m1, m2) => m2.Score.CompareTo(m1.Score));
        }

        private static void OrderMoves(ref Span<Move> moves, int movesCount, Board board)
        {
            for (int m = 0; m < movesCount; m++)
            {
                Move move = moves[m];
                move.Score = 1;

                if (move.Special == SpecialMove.EN_PASSANT)
                {
                    move.Score = 900;
                }

                else
                {
                    ulong toSquareMask = 1UL << move.ToSquare;

                    if ((board.AllPieces & toSquareMask) != 0)
                    {
                        int victimType = -1;

                        int startEnemy = move.SelectedPiece < 6 ? 6 : 0;
                        int endEnemy = move.SelectedPiece < 6 ? 12 : 6;

                        for (int i = startEnemy; i < endEnemy; i++)
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

            Span<Move> captures = stackalloc Move[79];
            int capturesCount = 0;
            MoveGenerator.GenerateAllCaptures(board, ref captures, ref capturesCount);

            OrderMoves(ref captures, capturesCount, board);

            for (int m = 0; m < capturesCount; m++)
            {
                Move move = captures[m];
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
