char_map = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
            "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
            "U", "V", "W", "X", "Y", "Z"]

def get_char_index(char):
    if char == ' ':
        return 0
    else:
        return 42 + int(char_map.index(char))

def convert_to_indexes(text):
    for char in text:
        if char in char_map or char == ' ':
            print(f"@add_char #{get_char_index(char)}")
        else:
            print(f"Character '{char}' not found in char_map.")

def new_line():
    print("JSR new_line")

convert_to_indexes("JAYANSH IS A FUCKING PIECE OF SHIT")
# new_line()
# convert_to_indexes("IS A")
# new_line()
# convert_to_indexes("FUCKING")
# new_line()
# convert_to_indexes("PIECE OF")
# new_line()
# convert_to_indexes("SHIT")