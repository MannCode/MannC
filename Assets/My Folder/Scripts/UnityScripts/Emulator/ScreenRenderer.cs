
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.Rendering;
using System.IO;
using System.Text;
using TMPro;

public class ScreenRenderer : MonoBehaviour
{
    public Texture2D texture;
    public RawImage image;
    public Emulator emu;
    public Emulator.MEM mem;
    public Emulator.CPU cpu;

    

    public Color pixcolor;
    public Color[] pixColorList;

    public Image screenCover;

    // [HideInInspector]
    public bool updateScreen = false;
    bool useBitMap = true;

    //bitmap configuration
    public int bitmapWidth = 7;
    public int bitmapHeight = 8;

    public string bitmapFilePath; //"../BitMapGenerator/Bitmap_bin.bin";
    
    public int[][][] bitmap;
    public int[][] bitmapScreenBackup;
    public int currentScreenIndex = 0;
    public int totalScreens = 1;
    public bool isHomeScreen = true;

    // all 0
    // public int[][] bitmap1 = new int[8][] {
    //     new int[7]{0,1,1,1,0,0,0},
    //     new int[7]{1,0,0,0,1,0,0},
    //     new int[7]{1,0,0,0,1,0,0},
    //     new int[7]{1,1,1,1,1,0,0},
    //     new int[7]{1,0,0,0,1,0,0},
    //     new int[7]{1,0,0,0,1,0,0},
    //     new int[7]{0,0,0,0,0,0,0},
    //     new int[7]{0,0,0,0,0,0,0}
    // };

    // Start is called before the first frame update
    public void Start()
    {
        screenCover.gameObject.SetActive(false);
        texture = new Texture2D(256, 144, TextureFormat.RGB24, false);
        texture.filterMode = FilterMode.Point;
        image.texture = texture;
        // image.material.mainTexture = texture;
        pixColorList = new Color[texture.width*texture.height];

        for(int y=0; y < texture.height; y++) {
            for(int x=0; x < texture.width; x++) {
                texture.SetPixel(x, y, Color.black);
            }
        }
        texture.Apply();

        
        // bitmap configuration
        BinaryReader reader = new BinaryReader(File.Open(bitmapFilePath, FileMode.Open), Encoding.UTF8);
        string _Data = "";
        while(reader.BaseStream.Position != reader.BaseStream.Length) {
            byte data = reader.ReadByte();
            _Data += Convert.ToString(data, 2).PadLeft(8, '0');
        }

        int bitmapsCount = _Data.Length / (bitmapWidth * bitmapHeight);
        bitmap = new int[bitmapsCount][][];
        for(int i=0; i < bitmapsCount; i++) {
            bitmap[i] = new int[bitmapHeight][];
            for(int y=0; y < bitmapHeight; y++) {
                bitmap[i][y] = new int[bitmapWidth];
                for(int x=0; x < bitmapWidth; x++) {
                    int index = (i * bitmapWidth * bitmapHeight) + (y * bitmapWidth) + x;
                    bitmap[i][y][x] = (_Data[index] == '1') ? 1 : 0;
                }
            }
        }

        // initialize bitmapScreenBackup
        bitmapScreenBackup = new int[1][];
        bitmapScreenBackup[0] = new int[(texture.width/bitmapWidth) * (texture.height/bitmapHeight)];
    }
    
    // Update is called once per frame
    public void Update()
    {
        if(updateScreen) {
            if(useBitMap) {
                UpdateEachPixelBitMap();
            } else {
                UpdateEachPixel();
            }
        }

        //check for input to navigate through screen backups
        if(Input.GetKeyDown(KeyCode.UpArrow)) {
            navigateScreenBackup(-1);
        } else if(Input.GetKeyDown(KeyCode.DownArrow)) {
            navigateScreenBackup(1);
        }

        // Debug.Log(bitmapScreenBackup.Length);
    }

    public void UpdateEachPixel()
    {
        mem = emu.mem_Backup;
        int index = 0x7000;
        for(int y=texture.height-1; y >= 0; y--) {
            for(int x=0; x < texture.width; x++) {
                ushort data_short = mem.Data[index];
                texture.SetPixel(x, y, (data_short == 0x0001) ? new Color(0.0003f, 1.0f, 0.121f, 1.0f) : Color.black);
                index++;
            }
        }
        updateScreen = false;
        texture.Apply();
    }

    public void UpdateEachPixelBitMap()
    {
        mem = emu.mem_Backup;

        int bitmapCountX = texture.width/bitmapWidth;
        int bitmapCountY = texture.height/bitmapHeight;
        int index = 0x7000;
        for(int b_y=bitmapCountY-1; b_y >= 0; b_y--) {
            for(int b_x=0; b_x < bitmapCountX; b_x++) {
                int bitmapIndex = mem.Data[index];
                for(int y=0; y < bitmapHeight; y++) {
                    for(int x=0; x < bitmapWidth; x++) {
                        try {
                        int val = bitmap[bitmapIndex][y][x];
                        texture.SetPixel((b_x*bitmapWidth) + x, (b_y*bitmapHeight) + bitmapHeight-y-1, (val == 1) ? new Color(0.0003f, 1.0f, 0.121f, 1.0f) : Color.black);
                        } catch (Exception e) {
                            Debug.Log("bitmapIndex: " + bitmapIndex + " y: " + y + " x: " + x);
                            Debug.Log(e);
                        }
                    }
                }
                index += 1;
            }
        }
        updateScreen = false;
        texture.Apply();
    }

    public void storeCurrentScreenBackup() {
        // store the current state of bitmap in new index of bitmapScreenBackup
        // bitmap[index of screen backup][index of bitmap pixel]
        mem = emu.mem_Backup;

        int bitmapCountX = texture.width/bitmapWidth;
        int bitmapCountY = texture.height/bitmapHeight;

        if(currentScreenIndex >= bitmapScreenBackup.Length)
        {
            // create new bitmapScreenBackup with size + 1
            int[][] newBitmapScreenBackup = new int[bitmapScreenBackup.Length + 1][];
            for(int i=0; i < bitmapScreenBackup.Length; i++) {
                newBitmapScreenBackup[i] = bitmapScreenBackup[i];
            }
            newBitmapScreenBackup[newBitmapScreenBackup.Length - 1] = new int[bitmapCountX * bitmapCountY];
            bitmapScreenBackup = newBitmapScreenBackup;
        }

        
        int index = 0x7000;

        for(int y=bitmapCountY-1; y >= 0; y--)
        {
            for(int x=0; x < bitmapCountX; x++)
            {
                bitmapScreenBackup[currentScreenIndex][(bitmapCountY-1-y)*bitmapCountX + x] = mem.Data[index];
                index++;
            }
        }
    }

    // use up and down arror keys to navigate through the screen backups
    public void navigateScreenBackup(int direction)
    {
        if(bitmapScreenBackup == null || bitmapScreenBackup.Length == 0) {
            return;
        }

        if(currentScreenIndex + direction < 0 || currentScreenIndex + direction >= bitmapScreenBackup.Length) {
            return;
        }

        storeCurrentScreenBackup();

        currentScreenIndex += direction;
        // isCurrent = false;

        // if(currentScreenIndex == bitmapScreenBackup.Length) {
        //     isCurrent = true;
        // }

        int index = 0x7000;
        int bitmapCountX = texture.width/bitmapWidth;
        int bitmapCountY = texture.height/bitmapHeight;
        for(int y=bitmapCountY-1; y >= 0; y--)
        {
            for(int x=0; x < bitmapCountX; x++)
            {
                mem.Data[index] = (ushort)bitmapScreenBackup[currentScreenIndex][(bitmapCountY-1-y)*bitmapCountX + x];
                index++;
            }
        }
        updateScreen = true;
        isHomeScreen = false;
    }

    public void deleteBackupScreens() {
        bitmapScreenBackup = new int[1][];
        bitmapScreenBackup[0] = new int[(texture.width/bitmapWidth) * (texture.height/bitmapHeight)];
        currentScreenIndex = 0;
        totalScreens = 1;
        isHomeScreen = true;
    }
}