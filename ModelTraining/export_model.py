import torch
from model import ChessValueNet

model = ChessValueNet()
model.load_state_dict(torch.load("chess_model_weights.pth", map_location="cpu", weights_only=True))

model.eval()

dummy_input = torch.randn(1, 13, 64, dtype=torch.float32)

torch.onnx.export(
    model,
    dummy_input,
    "ChessValueNet.onnx",
    export_params=True,
    opset_version=14,
    input_names=['board_input'],
    output_names=['eval_output'],
    dynamic_axes={
        'board_input': {0: 'batch_size'}, 
        'eval_output': {0: 'batch_size'}
    }
)

print("ChessValueNet.onnx has been generated.")