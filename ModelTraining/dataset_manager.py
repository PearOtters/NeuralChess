import numpy as np
from torch.utils.data import IterableDataset, get_worker_info
import os
import glob
import math

class ChunkedChessDataset(IterableDataset):
    def __init__(self, folder_path):
        super().__init__()
        search_path = os.path.join(folder_path, "*.npz")
        self.chunk_files = sorted(glob.glob(search_path))

        if len(self.chunk_files) == 0:
            raise ValueError(f"No chunk files found in {folder_path}")

    def __iter__(self):
        worker_info = get_worker_info()
        
        if worker_info is None:
            files_to_process = self.chunk_files
        else:
            per_worker = int(math.ceil(len(self.chunk_files) / float(worker_info.num_workers)))
            worker_id = worker_info.id
            start = worker_id * per_worker
            end = min(start + per_worker, len(self.chunk_files))
            files_to_process = self.chunk_files[start:end]

        for file_path in files_to_process:
            data = np.load(file_path)
            packed_boards = data['boards']

            raw_scores = data['scores']
            scaled_scores = (raw_scores / 100.0).astype(np.float32)

            unpacked_boards = np.unpackbits(packed_boards).reshape(-1, 12, 64).astype(np.float32)

            for i in range(len(scaled_scores)):
                yield unpacked_boards[i], scaled_scores[i]