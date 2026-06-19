import torch
import torch.nn as nn
from torch.utils.data import DataLoader
import torch.optim as optim
from torch.optim.lr_scheduler import CosineAnnealingLR
from torch.amp import GradScaler, autocast
from model import ChessValueNet
from loss_function import WDL_BCE_Loss
from dataset_manager import ChunkedChessDataset
from datetime import datetime

GPU = "xpu" if torch.xpu.is_available() else "cpu"
EPOCHS = 20

device = torch.device(GPU)
model = ChessValueNet().to(device)

criterion = WDL_BCE_Loss(scaling_factor=4.0)

optimizer = optim.Adam(model.parameters(), lr=0.001)
scheduler = CosineAnnealingLR(optimizer, T_max=EPOCHS, eta_min=1e-6)

scaler = GradScaler("xpu")

dataset = ChunkedChessDataset(folder_path="./train_dataset_chunks")
train_loader = DataLoader(dataset, batch_size=4096)

model.train()
print(f"Starting training loop on device: {device}")

for epoch in range(1, EPOCHS + 1):
    running_loss = 0.0
    batch_count = 0

    for batch_boards, batch_scores in train_loader:
        batch_boards = batch_boards.to(device)
        batch_scores = batch_scores.to(device).float().view(-1, 1)

        optimizer.zero_grad()

        with autocast(device_type="xpu", dtype=torch.float16):
            predictions = model(batch_boards)
            loss = criterion(predictions, batch_scores)

        scaler.scale(loss).backward()
        scaler.step(optimizer)
        scaler.update()

        running_loss += loss.item()
        batch_count += 1

        if batch_count % 500 == 0:
            print(f"Epoch {epoch} | Batch {batch_count} | Current Loss: {loss.item():.4f} | Time: {datetime.now()}")

    scheduler.step()
    print(f"=== Epoch {epoch} Complete! Average Loss: {running_loss / batch_count:.4f} ===")

torch.save(model.state_dict(), "chess_model_weights.pth")
print("Weights successfully saved to disk!")