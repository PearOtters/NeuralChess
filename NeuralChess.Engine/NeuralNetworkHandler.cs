using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace NeuralChess.Engine
{
    public class NeuralNetworkHandler
    {
        private static readonly InferenceSession session = new("ChessValueNet.onnx");

        private static readonly ThreadLocal<float[]> threadLocalBoardState =
            new(() => new float[832]);

        private static readonly ThreadLocal<DenseTensor<float>> threadLocalInputTensor =
            new(() => new DenseTensor<float>(threadLocalBoardState.Value, [1, 13, 64]));

        private static readonly ThreadLocal<List<NamedOnnxValue>> threadLocalInputs =
            new(() => [NamedOnnxValue.CreateFromTensor("board_input", threadLocalInputTensor.Value)]);

        public static int GetBoardValue(Board board)
        {
            float[]? currentBuffer = threadLocalBoardState.Value;
            if (currentBuffer == null) return 0;
            board.CalculateBoardState(currentBuffer);

            float output = GetNeuralNetworkOutput();

            output = Math.Clamp(output, -0.99999f, 0.99999f);
            return (int)Math.Round(Math.Atanh(output) * 400);
        }

        public static float GetNeuralNetworkOutput()
        {
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(threadLocalInputs.Value);
            return results[0].AsTensor<float>().First();
        }
    }
}
