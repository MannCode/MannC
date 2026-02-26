import time
import random
import argparse
import sys



# Paragraph to be typed take from typeText.txt in same folder

# get the path of the current script
import os
script_dir = os.path.dirname(os.path.abspath(__file__))
# construct the path to typeText.txt
text_file_path = os.path.join(script_dir, "typeText.txt")
with open(text_file_path, "r") as f:
    PARAGRAPH = f.read().strip()


def type_with_wpm_range(text, cpm, start_delay=5):
    try:
        import pyautogui
    except Exception:
        print("pyautogui is required. Install with: pip3 install pyautogui")
        sys.exit(1)
    
    pyautogui.PAUSE = 0  # Small pause between actions to prevent overwhelming the system

    print(f"Place the cursor where you want the text to appear.")
    
    i = start_delay
    while i > 0:
        print(f"Starting in {i} seconds...")
        time.sleep(1)
        print("\033[F\033[K", end="")  # Move cursor up and clear line
        i -= 1
    print("")


    for ch in text:            
        pyautogui.write(ch, interval=0)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Type a paragraph at 70-80 WPM (approx).")
    parser.add_argument("--cpm", type=float, default=1050, help="CPM")
    parser.add_argument("--delay", type=float, default=3, help="seconds before typing starts")
    args = parser.parse_args()

    if args.cpm <= 0:
        print("Invalid CPM.")
        sys.exit(1)

    # macOS note: you may need to grant Accessibility permission to your terminal/Python app.
    type_with_wpm_range(PARAGRAPH, args.cpm, start_delay=args.delay)