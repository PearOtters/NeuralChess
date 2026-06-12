import torch
import torch.nn as nn
from torch.utils.data import DataLoader
import torch.optim as optim
from torch.amp import GradScaler, autocast
from model import ChessValueNet
from dataset_manager import ChunkedChessDataset

device = torch.device("xpu" if torch.xpu.is_available() else "cpu")
model = ChessValueNet().to(device)

criterion = nn.MSELoss()
optimizer = optim.Adam(model.parameters(), lr=0.001)
scaler = GradScaler("xpu")

dataset = ChunkedChessDataset(folder_path="./dataset_chunks_5mil")
train_loader = DataLoader(dataset, batch_size=1024)

model.train()
print(f"Starting training loop on device: {device}")

for epoch in range(1, 4):
    running_loss = 0.0
    batch_count = 0

    for batch_boards, batch_scores in train_loader:
        batch_boards = batch_boards.to(device)
        batch_scores = batch_scores.to(device).float()

        optimizer.zero_grad()

        with autocast(device_type="xpu", dtype=torch.float16):
            predictions = model(batch_boards)
            loss = criterion(predictions, batch_scores)

        scaler.scale(loss).backward()
        scaler.step(optimizer)
        scaler.update()

        running_loss += loss.item()
        batch_count += 1

        if batch_count % 50 == 0:
            print(f"Epoch {epoch} | Batch {batch_count} | Current Loss: {loss.item():.4f}")

    print(f"=== Epoch {epoch} Complete! Average Loss: {running_loss / batch_count:.4f} ===")