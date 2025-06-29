from PIL import Image
import numpy as np

# # 1407-604=
# width = 803
# height = 919
# start_x = 604
# start_y = 290

# width *= 2
# height *= 2
# start_x *= 2
# start_y *= 2


char_width = 7
char_height = 8

def save_pixel_data(pixel_data):
    with open("pixel_data.txt", "w") as f:
        i = 0
        for pixel in pixel_data:
            # same character space
            f.write(f"{int(pixel)} ")
            i += 1
            if(i % 56 == 0 and i != 0):
                f.write("\n")

    print("Pixel data saved to pixel_data.txt")

    with open("BitMap_bin.bin", "wb") as f:
        for i in range(0, len(pixel_data), 8):
            byte = 0
            for j in range(8):
                if i + j < len(pixel_data):
                    byte |= (int(pixel_data[i + j]) << (7 - j))
            f.write(bytes([byte]))
    
    print("Pixel data saved to pixel_data.bin")


def main():

    # os.system('say "Capturing in"')
    # # time.sleep(0.1)
    # os.system('say "3"')
    # # time.sleep(0.1)
    # os.system('say "2"')
    # # time.sleep(0.1)
    # os.system('say "1"')



    # Get the pixel data from the image
    img = Image.open("Bitmap.png")
    width, height = img.size

    CharCountX = int(width / char_width)
    CharCountY = int(height / char_height)
    
    pixel_data = np.empty(0)
    for y in range(CharCountY):
        for x in range(CharCountX):
            row = np.empty(0)
            for j in range(char_height):
                for i in range(char_width):
                    pixel = img.getpixel((x * char_width + i, y * char_height + j))
                    val = 0
                    if(pixel[0] == 255 and pixel[1] == 255 and pixel[2] == 255):
                        val = 1
                    row = np.append(row, val)
            pixel_data = np.append(pixel_data, row)
    

    save_pixel_data(pixel_data)
    # save_image(pixel_data, 1, 1)
    


main()