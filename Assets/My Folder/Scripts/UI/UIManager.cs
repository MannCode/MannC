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
    public RawImage HLT;

    [Header("Memory")]
    public TMP_InputField m_address;
    public TMP_Text m_data;

    void Start() {
        HLT.color = Color.red;
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
            HLT.color = Color.red;
        }else HLT.color = Color.green;
        if(m_address.text != "") {
            ushort add = (ushort)Convert.ToUInt64(Convert.ToString(m_address.text), 16);
            m_data.text = Convert.ToString(cpu.readShort(mem, add), 16);
        }
        
        clock.text = "Clock -> "+emu.frequency.ToString();
        setClockSpeed.text = emu.index.ToString();
    }
}
