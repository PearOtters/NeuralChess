import numpy as np
import csv
import os

def parse_fen(fen: str) -> np.ndarray:
    piece_map = {"P":0, "N":1, "B":2, "R":3, "Q":4, "K":5,
              "p":6, "n":7, "b":8, "r":9, "q":10, "k":11}
    
    board = np.zeros((12,64), dtype=np.bool_)

    fen_parts = fen.split(' ')
    board_layout = fen_parts[0]

    is_white_turn = fen_parts[1] == 'w'

    if is_white_turn:
        rank = 7
        file = 0
    else:
        rank = 0
        file = 7


    for c in board_layout:
        if c == '/':
            if is_white_turn:
                file = 0
                rank -= 1
            else:
                file = 7
                rank += 1

        elif (c.isdigit()):
            if is_white_turn:
                file += int(c)
            else:
                file -= int(c)
        else:
            char_to_map = c if is_white_turn else c.swapcase()
            square_index = (rank * 8) + file
            piece_type = piece_map[char_to_map]
            
            board[piece_type, square_index] = True
            if is_white_turn:
                file += 1
            else:
                file -= 1

    return board

def create_packed_datasets(csv_filepath: str, output_folder: str, total_positions: int, chunk_size: int = 1_000_000):
    
    os.makedirs(output_folder, exist_ok=True)
    
    ram_buffer_boards = np.zeros((chunk_size, 12, 64), dtype=np.bool_)
    ram_buffer_scores = np.zeros((chunk_size, 1), dtype=np.float32)
    
    buffer_index = 0
    chunk_number = 1
    
    with open(csv_filepath, mode='r', encoding='utf-8') as file:
        csv_reader = csv.reader(file)
        
        for global_index, row in enumerate(csv_reader):
            if global_index >= total_positions:
                break
                
            ram_buffer_boards[buffer_index] = parse_fen(row[0])
            ram_buffer_scores[buffer_index, 0] = float(row[1])
            
            buffer_index += 1
            
            if buffer_index == chunk_size:
                print(f"Packing and saving chunk {chunk_number}...")
                
                packed_boards = np.packbits(ram_buffer_boards)
                
                chunk_filename = os.path.join(output_folder, f"dataset_chunk_{chunk_number}.npz")
                np.savez_compressed(chunk_filename, boards=packed_boards, scores=ram_buffer_scores)
                
                chunk_number += 1
                buffer_index = 0 
                
        if buffer_index > 0:
            print(f"Packing and saving final chunk {chunk_number}...")
            packed_boards = np.packbits(ram_buffer_boards[:buffer_index])
            chunk_filename = os.path.join(output_folder, f"dataset_chunk_{chunk_number}.npz")
            np.savez_compressed(chunk_filename, boards=packed_boards, scores=ram_buffer_scores[:buffer_index])

    print("Creation complete")

create_packed_datasets("chess_dataset.csv", "train_dataset_chunks", 100_000_000, 250_000)