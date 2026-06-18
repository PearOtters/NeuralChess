import torch
import numpy as np
from model import ChessValueNet

def scramble_w2_weights(weight_tensor):
    scrambled_tensor = weight_tensor.clone()
    for block in range(256 // 32):
        start_idx = block * 32
        end_idx = start_idx + 32
        chunk = weight_tensor[:, start_idx:end_idx]
        
        scrambled_chunk = torch.cat([
            chunk[:, 0:8],   
            chunk[:, 16:24], 
            chunk[:, 8:16],  
            chunk[:, 24:32]  
        ], dim=1)
        
        scrambled_tensor[:, start_idx:end_idx] = scrambled_chunk
        
    return scrambled_tensor

def export_to_binary(model_path, output_path):
    print("Loading PyTorch model...")
    device = torch.device("cpu")
    model = ChessValueNet()
    model.load_state_dict(torch.load(model_path, map_location=device))
    model.eval()

    L1_SCALE = 127.0
    L2_SCALE = 16.0
    OUT_SCALE = 16.0

    L2_CSHARP_SHIFT_DIVISOR = 128.0

    print("Extracting and Quantising Layer 1 (Weights)...")
    w1_floats = model.layer1.weight.data.numpy().T
    w1_quantised = np.clip(np.round(w1_floats * L1_SCALE), -32768, 32767).astype(np.int16)

    b1_floats = model.layer1.bias.data.numpy()
    b1_quantised = np.clip(np.round(b1_floats * L1_SCALE), -32768, 32767).astype(np.int16)

    print("Extracting, Scrambling, and Quantising Layer 2 (Weights & Biases)...")
    w2_scrambled = scramble_w2_weights(model.layer2.weight.data)
    w2_quantised = np.clip(np.round(w2_scrambled.numpy() * L2_SCALE), -128, 127).astype(np.int8)
    
    b2_floats = model.layer2.bias.data.numpy()
    b2_quantised = np.clip(np.round(b2_floats * L1_SCALE * L2_SCALE), -2147483648, 2147483647).astype(np.int32)

    print("Extracting and Quantising Layer 3 (Weights & Biases)...")
    w3_floats = model.output_layer.weight.data.numpy()
    w3_quantised = np.clip(np.round(w3_floats * OUT_SCALE), -2147483648, 2147483647).astype(np.int32)
    
    b3_floats = model.output_layer.bias.data.numpy()
    B3_SCALE = (L1_SCALE * L2_SCALE / L2_CSHARP_SHIFT_DIVISOR) * OUT_SCALE
    b3_quantised = np.clip(np.round(b3_floats * B3_SCALE), -2147483648, 2147483647).astype(np.int32)

    print(f"Writing binary data to {output_path}...")
    with open(output_path, "wb") as f:
        f.write(w1_quantised.tobytes())
        f.write(b1_quantised.tobytes())
        f.write(b2_quantised.tobytes())
        f.write(w2_quantised.tobytes())
        f.write(b3_quantised.tobytes())
        f.write(w3_quantised.tobytes())

    print("Export complete! Your engine is ready to load the binary.")

export_to_binary("chess_model_weights.pth", "nnue_network.bin")