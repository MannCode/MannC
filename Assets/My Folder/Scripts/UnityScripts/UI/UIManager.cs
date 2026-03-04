using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Unity.Mathematics;

public class UIManager : MonoBehaviour
{
    public TMP_Text clock;
    public TMP_Text currentClockSpeed;

    public Emulator emu;
    Emulator.CPU cpu;
    Emulator.MEM mem;
    public ScreenRenderer sc;
    

    [Header("Resitors")]
    public TMP_Text r_PC;
    public TMP_Text r_A;
    public TMP_Text r_X;
    public TMP_Text r_Y;
    public TMP_Text r_SP;
    public TMP_Text F_C;
    public TMP_Text F_Z;
    public TMP_Text r_OUT;
    public Image HLT;

    [Header("Memory")]
    public TMP_InputField m_address;
    public TMP_Text m_data;

    [Header("Screen")]
    public TMP_Text screenPageIndex;

    void Start() {
        // 60 fps
        // Application.targetFrameRate = 60;

        HLT.color = Color.black;
        clock.text = "Clock -> 0";
    }

    void Update()
    {
        cpu = emu.cpu;
        mem = emu.mem;

        r_PC.text = Convert.ToString(cpu.PC, 16);
        r_A.text = Convert.ToString(cpu.A, 16);
        r_X.text = Convert.ToString(cpu.X, 16);
        r_Y.text = Convert.ToString(cpu.Y, 16);
        r_SP.text = Convert.ToString(cpu.SP, 16);
        F_C.text = cpu.F_C.ToString();
        F_Z.text = cpu.F_Z.ToString();
        // cpu.readByte(mem, 0x6FFF);
        r_OUT.text = Convert.ToString(cpu.readShort(mem, 0x6FFF), 16);

        if(cpu.HLT) {
            HLT.color = Color.black;
        }else HLT.color = Color.green;
        if(m_address.text != "") {
            ushort add = (ushort)Convert.ToUInt64(Convert.ToString(m_address.text), 16);
            m_data.text = Convert.ToString(cpu.readShort(mem, add), 16);
        }
        
        string frequency = FormatClockSpeed(emu.clock_speed);

        clock.text = "Clock -> "+ frequency.ToString();
        if(emu.cpuSpeedCap == 5) {
            currentClockSpeed.text = Char.ToString((char)0x263A);   // Smiley face for unlimited speed
        } else
        currentClockSpeed.text = FormatClockSpeed(emu.TargetClockSpeed).ToString();

        //screen
        screenPageIndex.text = (sc.currentScreenIndex+1).ToString() + "/" + sc.totalScreens.ToString();
    }

    string FormatClockSpeed(float frequency)
{
    // if 0.99 then convert to 1
    int int_frequency = (int)Math.Round(frequency);
    if (int_frequency >= 1_000_000_000) // 1 GHz
    {
        return (int_frequency / 1_000_000_000).ToString() + " GHz";
    }
    if (int_frequency >= 1_000_000) // 1 MHz
    {
        return (int_frequency / 1_000_000).ToString() + " MHz";
    }
    else if (int_frequency >= 1_000) // 1 KHz
    {
        return (int_frequency / 1_000).ToString() + " KHz";
    }
    else // Hz
    {
        return int_frequency.ToString() + " Hz";
    }
}
}
