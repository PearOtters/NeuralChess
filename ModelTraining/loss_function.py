import torch
import torch.nn as nn

class WDL_Loss(nn.Module):
    def __init__(self, scaling_factor=4.0):
        super().__init__()
        self.scaling_factor = scaling_factor
        self.mse = nn.MSELoss()

    def forward(self, predictions, targets):
        pred_wdl = torch.sigmoid(predictions / self.scaling_factor)
        
        target_wdl = torch.sigmoid(targets / self.scaling_factor)
        
        return self.mse(pred_wdl, target_wdl)