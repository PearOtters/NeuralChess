import re
import sys
import time

INPUT_FILE = "chess_dataset.csv"
LOG_FILE = "corrupt_lines_report.txt"
DELIMITER = ","
PROGRESS_INTERVAL = 5_000_000

FEN_REGEX = re.compile(
    r"^[rnbqkpRNBQKP1-8]+(/[rnbqkpRNBQKP1-8]+){7}\s+[wb]\s+([KQkq]+|-)\s+([a-h][36]|-)\s+\d+\s+\d+$"
)


def validate_dataset():
    print(f"Starting verification of {INPUT_FILE}...")
    start_time = time.time()

    line_count = 0
    corrupt_count = 0

    with open(INPUT_FILE, "r", encoding="utf-8") as infile, open(
        LOG_FILE, "w", encoding="utf-8"
    ) as logfile:

        for line in infile:
            line_count += 1
            clean_line = line.strip()

            if not clean_line:
                continue

            if DELIMITER not in clean_line:
                corrupt_count += 1
                logfile.write(
                    f"Line {line_count}: Missing delimiter '{DELIMITER}' -> {clean_line}\n"
                )
                continue

            parts = clean_line.rsplit(DELIMITER, 1)
            fen = parts[0].strip()
            score_str = parts[1].strip()

            if not FEN_REGEX.match(fen):
                corrupt_count += 1
                logfile.write(
                    f"Line {line_count}: Invalid FEN structure -> {fen}\n"
                )
                continue

            try:
                _ = int(score_str)
            except ValueError:
                corrupt_count += 1
                logfile.write(
                    f"Line {line_count}: Invalid score integer -> {score_str}\n"
                )
                continue

            if line_count % PROGRESS_INTERVAL == 0:
                elapsed = time.time() - start_time
                print(
                    f"Processed {line_count:,} lines... Found {corrupt_count:,} errors so far. ({elapsed:.1f}s elapsed)"
                )

    total_time = time.time() - start_time
    print("\n" + "=" * 40)
    print("VERIFICATION COMPLETE")
    print("=" * 40)
    print(f"Total Lines Scanned: {line_count:,}")
    print(f"Total Corrupt Lines: {corrupt_count:,}")
    print(f"Execution Time:      {total_time:.2f} seconds")

    if corrupt_count > 0:
        print(f"Action Required: Detailed errors written to: {LOG_FILE}")
    else:
        print("Success! Every single line matches the perfect format.")


if __name__ == "__main__":
    validate_dataset()