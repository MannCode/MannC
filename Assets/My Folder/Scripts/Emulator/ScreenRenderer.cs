
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.Rendering;

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

        
    }

    // Update is called once per frame
    public void Update()
    {
        mem = emu.mem_Backup;
        if(updateScreen) {
            for(int i=0; i < texture.height*texture.width; i++) {
                
                ushort data_short = mem.Data[0x7000+i];
                // bool val = (data_byte&(byte)Mathf.Pow(2, j))==Mathf.Pow(2, j);
                if(data_short == 0x0001) {
                    pixColorList[i] = new Color(0.0003f, 1.0f, 0.121f, 1.0f);
                } else pixColorList[i] = Color.black;
            }
            int index = 0;
            for(int y=texture.height-1; y >= 0; y--) {
            // for(int y=0; y < texture.height; y++) {
                for(int x=0; x < texture.width; x++) {
                    texture.SetPixel(x, y, pixColorList[index]);
                    index++;
                }
            }
            updateScreen = false;
            texture.Apply();
        }
    }
}
