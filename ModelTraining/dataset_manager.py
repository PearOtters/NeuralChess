import numpy as np
from torch.utils.data import IterableDataset
import os
import glob

class ChunkedChessDataset(IterableDataset):
    def __init__(self, folder_path):
        super().__init__()
        search_path = os.path.join(folder_path, "*.npz")
        self.chunk_files = sorted(glob.glob(search_path))

        if len(self.chunk_files) == 0:
            raise ValueError(f"No chunk files found in {folder_path}")

    def __iter__(self):
        for file_path in self.chunk_files:
            data = np.load(file_path)
            packed_boards = data['boards']

            raw_scores = data['scores']
            scaled_scores = (raw_scores / 100.0).astype(np.float32)
            scaled_scores = np.clip(scaled_scores, -9.0, 9.0)

            unpacked_boards = np.unpackbits(packed_boards).reshape(-1, 12, 64).astype(np.float32)

            for i in range(len(scaled_scores)):
                yield unpacked_boards[i], scaled_scores[i]