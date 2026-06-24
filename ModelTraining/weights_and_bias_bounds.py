import torch
import numpy as np
from model import ChessValueNet

def analyze_tensor(tensor_name, tensor, k=10):
    # Extract the raw floats and flatten them into a 1D array
    flat_tensor = tensor.detach().cpu().numpy().flatten()
    
    # Ensure we don't try to pull more values than exist in the tensor
    k = min(k, len(flat_tensor))
    
    # Sort the array from lowest to highest
    sorted_tensor = np.sort(flat_tensor)
    
    # Slice the top and bottom k values
    top_max = sorted_tensor[-k:][::-1]  # Highest first
    top_min = sorted_tensor[:k]         # Lowest first
    
    print(f"=== {tensor_name} ({len(flat_tensor):,} parameters) ===")
    print(f"Overall Min: {sorted_tensor[0]:.5f} | Overall Max: {sorted_tensor[-1]:.5f}")
    
    # Format the arrays for clean console reading
    max_str = ", ".join([f"{val:.4f}" for val in top_max])
    min_str = ", ".join([f"{val:.4f}" for val in top_min])
    
    print(f"Top {k} Max : [{max_str}]")
    print(f"Top {k} Min : [{min_str}]\n")

def inspect_model(model_path):
    print(f"Loading model from {model_path}...\n")
    device = torch.device("cpu")
    model = ChessValueNet()
    
    try:
        model.load_state_dict(torch.load(model_path, map_location=device))
        model.eval()
    except FileNotFoundError:
        print(f"Error: Could not find '{model_path}'. Ensure the file is in the same directory.")
        return

    L1_SCALE = 127.0
    L2_SCALE = 16.0
    OUT_SCALE = 127.0

    L2_CSHARP_SHIFT_DIVISOR = 16.0

    # Pass each layer's weights and biases into the analyzer
    analyze_tensor("Layer 1 Weights (W1)", model.layer1.weight)
    analyze_tensor("Layer 1 Scaled Weights (W1)", model.layer1.weight * L1_SCALE)
    print()
    analyze_tensor("Layer 1 Biases  (B1)", model.layer1.bias)
    analyze_tensor("Layer 1 Scaled Biases  (B1)", model.layer1.bias * L1_SCALE)
    print()
    analyze_tensor("Layer 2 Weights (W2)", model.layer2.weight)
    analyze_tensor("Layer 2 Scaled Weights (W2)", model.layer2.weight * L2_SCALE)
    print()
    analyze_tensor("Layer 2 Biases  (B2)", model.layer2.bias)
    analyze_tensor("Layer 2 Scaled Biases  (B2)", model.layer2.bias * L1_SCALE * L2_SCALE)
    print()
    analyze_tensor("Output Layer Weights (W3)", model.output_layer.weight)
    analyze_tensor("Output Layer Scaled Weights (W3)", model.output_layer.weight * OUT_SCALE)
    print()
    analyze_tensor("Output Layer Biases  (B3)", model.output_layer.bias)
    analyze_tensor("Output Layer Scaled Biases  (B3)", model.output_layer.bias * L1_SCALE * L2_SCALE / L2_CSHARP_SHIFT_DIVISOR * OUT_SCALE)

inspect_model("chess_model_weights.pth")