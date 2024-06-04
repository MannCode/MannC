
using System;
using UnityEngine;
using UnityEngine.UI;

public class ScreenRenderer : MonoBehaviour
{
    Texture2D texture;
    public RawImage image;
    public GameObject screenCover;
    public Emulator emu;
    public Emulator.MEM mem;
    public Emulator.CPU cpu;

    Color pixcolor;
    Color[] pixColorList;
    // Start is called before the first frame update
    void Start()
    {
        mem = emu.mem;
        cpu = emu.cpu;

        screenCover.SetActive(false);
        texture = new Texture2D(256, 144);
        texture.filterMode = FilterMode.Point;
        image.material.mainTexture = texture;
        pixColorList = new Color[texture.width*texture.height];

        for(int y=0; y < texture.height; y++) {
            for(int x=0; x < texture.width; x++) {
                texture.SetPixel(x, y, Color.black);
            }
        }
        texture.Apply();
    }

    // Update is called once per frame
    public void Update()
    {   
        if(true) {
            for(int i=0; i < texture.height*texture.width; i++) {
                byte data_byte = cpu.readByte(mem, (ushort)(i+0x7000));
                // bool val = (data_byte&(byte)Mathf.Pow(2, j))==Mathf.Pow(2, j);
                if(data_byte == 0x01) {
                    pixcolor = Color.white;
                } else pixcolor = Color.black;
                pixColorList[i] = pixcolor;
            }

            int index = 0;
            for(int y=texture.height-1; y >= 0; y--) {
                for(int x=0; x < texture.width; x++) {
                    texture.SetPixel(x, y, pixColorList[index]);
                    index++;
                }
            }

            texture.Apply();
        }
    }
}
