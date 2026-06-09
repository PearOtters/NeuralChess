import numpy as np

def parse_fen(fen: str) -> np.ndarray:
    piece_map = {"P":0, "N":1, "B":2, "R":3, "Q":4, "K":5,
              "p":6, "n":7, "b":8, "r":9, "q":10, "k":11}
    
    board = np.zeros((13,64), dtype=np.float32)

    fen_parts = fen.split(' ')
    board_layout = fen_parts[0]

    rank = 7
    file = 0

    for c in board_layout:
        if c == '/':
            file = 0
            rank -= 1
        elif (c.isdigit()):
            file += int(c)
        else:
            square_index = (rank * 8) + file
            piece_type = piece_map[c]
            
            board[piece_type, square_index] = 1
            file += 1

    is_black_turn = fen_parts[1] == 'b'

    board[12, :] = 1.0 if is_black_turn else 0.0

    return board