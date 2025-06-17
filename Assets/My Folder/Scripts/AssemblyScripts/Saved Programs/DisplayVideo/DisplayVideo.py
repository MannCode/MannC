import cv2

brighness_map = [0, 68, 69, 70, 71, 1, 2, 3]

program_1 = '''
.screen_size = $7288
.pixel_index
.frame_count
.index

start:
    LDA #$40
    STA pixel_index
    LDA #(frames)
    STA frame_count
    LDA #$7000
    STA index

loop:
    JSR DrawMultiple
    INC pixel_index
    LDA index
    CMP #(screen_size)
    BEQ reset
    JMP loop

reset:
    DUP
    LDA #$7000
    STA index
    DEC frame_count
    BEQ start
    JMP loop

DrawMultiple:
.MultipleCount
    LDA [pixel_index], 0
    SRA #8
    STA MultipleCount
    LDA [pixel_index], 0
    AND #$FF
DrawMultipleLoop:
    STA [index], 0
    INC index
    DEC MultipleCount
    BEQ DrawMultipleEnd
    JMP DrawMultipleLoop
DrawMultipleEnd:
    RTS
` $40
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
        if count < 44 or count % 5 != 0:
            success, image = cap.read()
            continue
        if count > 400 * 5:
            break           
        frames += 1
        final_pixel_data += "\n: "
        last_brightness_val = 0
        repeat_brightness_val_count = 1
        print("{}th image from the movie ".format(count))
        for y in range(display_height):
            # final_pixel_data += "\n: "
            for x in range(display_width):
                pixel_x = int(x * scale_x)
                pixel_y = int(y * scale_y)

                pixel_value = image[pixel_y, pixel_x]
                pixel_value = pixel_value[0]
                # if pixel_value > 127: pixel_value = 255
                # else: pixel_value = 0

                brightness_value = brighness_map[int(pixel_value / 255 * (len(brighness_map) - 1))]

                if x == 0 and y == 0:
                    last_brightness_val = brightness_value

                if(brightness_value != last_brightness_val or x == display_width - 1 and y == display_height - 1 or repeat_brightness_val_count >= 255):
                    _val = (repeat_brightness_val_count << 8) | last_brightness_val
                    final_pixel_data += f"{_val} "
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

convert_vid_to_indexes("/Users/mandeepsingh/MyStuff/Programming/Unity/Projects/bad_apple.mp4")