import torch
import torch.nn as nn

class ChessValueNet(nn.Module):
    def __init__(self):
        super().__init__()
        self.flatten = nn.Flatten()
        
        self.layer1 = nn.Linear(12 * 64, 256)
        self.layer2 = nn.Linear(256, 32)
        self.output_layer = nn.Linear(32, 1)

    def forward(self, x):
        x = self.flatten(x)
        
        x = self.layer1(x)
        x = torch.clamp(x, min=0.0, max=1.0) 
        
        x = self.layer2(x)
        x = torch.clamp(x, min=0.0, max=1.0)
        
        x = self.output_layer(x)
        
        return x