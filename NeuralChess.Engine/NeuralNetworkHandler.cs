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
        private static readonly float[] boardState = new float[832];
        private static readonly DenseTensor<float> inputTensor;
        private static readonly List<NamedOnnxValue> inputs;

        static NeuralNetworkHandler()
        {
            inputTensor = new DenseTensor<float>(boardState, [1, 13, 64]);
            inputs = [NamedOnnxValue.CreateFromTensor("board_input", inputTensor)];
        }

        public static int GetBoardValue(Board board)
        {
            board.CalculateBoardState(boardState);
            float output = GetNeuralNetworkOutput();
            output = Math.Clamp(output, -0.99999f, 0.99999f);
            return (int)Math.Round(Math.Atanh(output) * 400);
        }

        public static float GetNeuralNetworkOutput()
        {
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
            return results[0].AsTensor<float>().First();
        }
    }
}
