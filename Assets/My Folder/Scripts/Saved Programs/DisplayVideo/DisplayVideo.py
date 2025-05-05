import cv2

'''
~add_char_chunk 2 {
    LDA ^2
    PHA
    @add_char ^1
    PLA
    STA ^2
    DEC ^2
    BEQ charChunkEnd
    JMP add_char_chunk
charChunkEnd:
}
'''




brighness_map = [0, 68, 69, 70, 71, 1, 2, 3]

program_1 = '''.line
.col
.indexStart
.line_length = 36
.pixel_index
.frame_count
.val

start:
    LDA #0
    STA line
    LDA #0
    STA col
    LDA #$100
    STA pixel_index
    LDA #(frames)
    STA frame_count
    LDA #$7000
    STA indexStart

loop:
    LDA [pixel_index], 0
    STA val
    CMP #6969
    BEQ DrawMultiple
    @add_char val
DrawMultipleEnd:
    INC pixel_index
    LDA line
    CMP #18
    BEQ reset
    JMP loop

reset:
    DUP
    LDA #0
    STA line
    LDA #0
    STA col
    DEC frame_count
    BEQ start
    JMP loop

DrawMultiple:
.MultipleCount
    INC pixel_index
    LDA [pixel_index], 0
    STA val
    INC pixel_index
    LDA [pixel_index], 0
    STA MultipleCount
DrawMultipleLoop:
    @add_char val
    DEC MultipleCount
    BEQ DrawMultipleEnd
    JMP DrawMultipleLoop

    
~add_char 1 {
.index
    LDA ^1
    PHA
    @mul #(line_length) line
    ADD col
    ADD indexStart
    STA index
    PLA
    STA ^1
    STA [index], 0
    LDA col
    SUB #(line_length-1)
    BEQ else
    INC col
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

    
sleep:
    LDA #100
sleep_loop:
    SUB #1
    BEQ sleep_end
    NOP
    JMP sleep_loop
sleep_end:
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
}

` $100
'''


def convert_vid_to_indexes(vid_path):
    final_pixel_data = ""
    cap = cv2.VideoCapture(vid_path)
    success, image = cap.read()
    img_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    img_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))

    display_width = 36
    display_height = 18

    scale_x = img_width / display_width
    scale_y = img_height / display_height

    count = 0
    frames = 0

    while success:
        count += 1
        if frames > 50:
            break
        frames += 1
        final_pixel_data += "\n: "
        last_brightness_val = 0
        repeat_brightness_val_count = 0
        print("{}th image from the movie ".format(count))
        for y in range(display_height):
            # final_pixel_data += "\n: "
            for x in range(display_width):
                pixel_x = int(x * scale_x)
                pixel_y = int(y * scale_y)

                pixel_value = image[pixel_y, pixel_x]
                pixel_value = pixel_value[0]
                # pixel_value = 0
                # print("Pixel at ({}, {}) - Value {} ".format(x,y,v))
                brightness_value = brighness_map[int(pixel_value / 255 * (len(brighness_map) - 1))]
                # final_pixel_data += str(brightness_value) + " "
                # program += f"@add_char #{brightness_value}\n"

                if x == 0 and y == 0:
                    final_pixel_data += f"{brightness_value} "
                    last_brightness_val = brightness_value
                    repeat_brightness_val_count = 1
                elif(brightness_value != last_brightness_val):
                    if repeat_brightness_val_count > 3:
                        final_pixel_data += f'6969 {last_brightness_val} {repeat_brightness_val_count} '
                    else:
                        for i in range(repeat_brightness_val_count):
                            final_pixel_data += f'{last_brightness_val} '
                    last_brightness_val = brightness_value
                    repeat_brightness_val_count = 1
                elif x == display_width - 1 and y == display_height - 1:
                    if repeat_brightness_val_count > 3:
                        final_pixel_data += f'6969 {last_brightness_val} {repeat_brightness_val_count} '
                    else:
                        for i in range(repeat_brightness_val_count):
                            final_pixel_data += f'{last_brightness_val} '
                    last_brightness_val = brightness_value
                    repeat_brightness_val_count = 1
                else:
                    repeat_brightness_val_count += 1
        success, image = cap.read()
    
    program = f".frames = {frames}\n" + program_1 + final_pixel_data
    cap.release()
    cv2.destroyAllWindows()

    print("Total frames generated: ", frames)

    with open("/Users/mandeepsingh/MyStuff/Programming/Unity/Projects/MannC/Assets/My Folder/Scripts/Saved Programs/DisplayVideo/DisplayVideo.asm", "w") as output_file:
        print("Writing to file")
        output_file.write(program)

    
                

def new_line():
    print("JSR new_line")

convert_vid_to_indexes("/Users/mandeepsingh/MyStuff/Programming/Unity/Projects/dancing-dance.gif")