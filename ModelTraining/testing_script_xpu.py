import torch
import torch.nn as nn
from torch.utils.data import DataLoader
from torch.amp import autocast
from model import ChessValueNet
from dataset_manager import ChunkedChessDataset
from datetime import datetime

device = torch.device("xpu" if torch.xpu.is_available() else "cpu")
model = ChessValueNet().to(device)

model.load_state_dict(torch.load("chess_model_weights.pth", map_location=device, weights_only=True))
model.eval()

dataset = ChunkedChessDataset(folder_path="./validation_dataset_chunks")
test_loader = DataLoader(dataset, batch_size=1024)

print(f"Starting training loop on device: {device}")

criterion = nn.MSELoss()
running_loss = 0.0
batch_count = 0

with torch.no_grad():
    for batch_boards, batch_scores in test_loader:
        batch_boards = batch_boards.to(device)
        batch_scores = batch_scores.to(device).float()

        with autocast(device_type="xpu", dtype=torch.float16):
            predictions = model(batch_boards)
            loss = criterion(predictions, batch_scores)

        running_loss += loss.item()
        batch_count += 1

        if batch_count % 50 == 0:
            print(f"Batch {batch_count} | Current Loss: {loss.item():.4f} | Time: {datetime.now()}")

print(f"=== Training complete! Average Loss: {running_loss / batch_count:.4f} ===")