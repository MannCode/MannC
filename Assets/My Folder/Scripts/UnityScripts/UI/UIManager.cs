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
    public TMP_Text setClockSpeed;

    public Emulator emu;
    Emulator.CPU cpu;
    Emulator.MEM mem;
    

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
        
        string clockSpeed = FormatClockSpeed(emu.frequency);

        clock.text = "Clock -> "+ clockSpeed.ToString();
        setClockSpeed.text = emu.index.ToString();
    }

    string FormatClockSpeed(float frequency)
{
    if (frequency >= 1_000_000_000) // 1 GHz
    {
        return (frequency / 1_000_000_000).ToString("F2") + " GHz";
    }
    else if (frequency >= 1_000_000) // 1 MHz
    {
        return (frequency / 1_000_000).ToString("F2") + " MHz";
    }
    else if (frequency >= 1_000) // 1 KHz
    {
        return (frequency / 1_000).ToString("F2") + " KHz";
    }
    else if (frequency < 0) // Negative
    {
        return Char.ToString((char)0x263A);
    }
    else // Hz
    {
        return frequency.ToString("F2") + " Hz";
    }
}
}
