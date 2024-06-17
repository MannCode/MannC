
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class ScreenRenderer : MonoBehaviour
{
    public Texture2D texture;
    public RawImage image;
    public GameObject screenCover;
    public Emulator emu;
    public Emulator.MEM mem;
    public Emulator.CPU cpu;

    public Color pixcolor;
    public Color[] pixColorList;
    // Start is called before the first frame update
    void Start()
    {
        mem = emu.mem;
        cpu = emu.cpu;

        screenCover.SetActive(false);
        texture = new Texture2D(256, 144)
        {
            filterMode = FilterMode.Point
        };
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
    public void ScreenUpdate()
    {   
        for(int i=0; i < texture.height*texture.width; i++) {
            ushort data_short = cpu.readShort(mem, (ushort)(0x7000+i));
            // bool val = (data_byte&(byte)Mathf.Pow(2, j))==Mathf.Pow(2, j);
            if(data_short == 0x0001) {
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
