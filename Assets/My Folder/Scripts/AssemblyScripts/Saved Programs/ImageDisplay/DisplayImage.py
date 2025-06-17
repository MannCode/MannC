from PIL import Image

brighness_map = [0, 68, 69, 70, 71, 1, 2, 3]

program_1 = '''.line
.col
.indexStart
.line_length = 36
.val

start:
    LDA #20
    STA val
    LDA #$7000
    STA indexStart

'''

program_2 = '''    HLT

~add_char 1 {
.index
    LDA ^1
    PHA
    @mul #(line_length) line
    ADD col
    ADD indexStart
    STA index
    PLA
    STA [index], 0
    LDA col
    SUB #(line_length)
    BEQ else
    INC col
    DUP
    JMP end
else:
    JSR new_line
end:
}

new_line:
    INC line
    LDA #0
    STA col
    RTS

~mul 2 {
; A, B
    LDA #0
loop_mul:
    LDX ^2
    BEQ end_mul
    ADD ^1
    DEC ^2
    JMP loop_mul
end_mul:
}'''


def convert_img_to_indexes(image_path):
    with Image.open(image_path) as img:
        print("Converting image to indexes")
        img = img.convert("L")
        img_data = img.getdata()
        img_width, img_height = img.size

        display_width = 36
        display_height = 18
        scale_x = img_width / display_width
        scale_y = img_height / display_height
        program = program_1
        for y in range(display_height):
            for x in range(display_width):
                pixel_x = int(x * scale_x)
                pixel_y = int(y * scale_y)
                pixel_value = img_data[pixel_y * img_width + pixel_x]
                if pixel_value < 200: pixel_value = 0
                else: pixel_value = 255
                brightness_value = brighness_map[int(pixel_value / 255 * (len(brighness_map) - 1))]
                # print(f"@add_char #{brightness_value}")
                program += f"@add_char #{brightness_value}\n"
            program += "JSR new_line\n"

        program += program_2
        with open("/Users/mandeepsingh/MyStuff/Programming/Unity/Projects/MannC/Assets/My Folder/Scripts/Saved Programs/ImageDisplay/DisplayImage.asm", "w") as output_file:
            print("Writing to file")
            output_file.write(program)

    
                

def new_line():
    print("JSR new_line")

convert_img_to_indexes("/Users/mandeepsingh/MyStuff/Programming/Unity/Projects/Singh.jpeg")