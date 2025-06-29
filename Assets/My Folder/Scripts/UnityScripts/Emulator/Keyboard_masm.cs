using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.Threading.Tasks;

public class Keyboard_masm : MonoBehaviour
{
    public Toggle KeyboardToggle;
    public Emulator emu;
    public Emulator.MEM mem;
    public Emulator.CPU cpu;
    public ushort port_address;

    KeyCode LastKeyPressed;

    private static readonly Dictionary<KeyCode, int> keyMap = new Dictionary<KeyCode, int>
    {
        { KeyCode.Alpha0, 10 },
        { KeyCode.Alpha1, 11 },
        { KeyCode.Alpha2, 12 },
        { KeyCode.Alpha3, 13 },
        { KeyCode.Alpha4, 14 },
        { KeyCode.Alpha5, 15 },
        { KeyCode.Alpha6, 16 },
        { KeyCode.Alpha7, 17 },
        { KeyCode.Alpha8, 18 },
        { KeyCode.Alpha9, 19 },
        { KeyCode.A, 42 }, { KeyCode.B, 43 }, { KeyCode.C, 44 }, { KeyCode.D, 45 },
        { KeyCode.E, 46 }, { KeyCode.F, 47 }, { KeyCode.G, 48 }, { KeyCode.H, 49 },
        { KeyCode.I, 50 }, { KeyCode.J, 51 }, { KeyCode.K, 52 }, { KeyCode.L, 53 },
        { KeyCode.M, 54 }, { KeyCode.N, 55 }, { KeyCode.O, 56 }, { KeyCode.P, 57 },
        { KeyCode.Q, 58 }, { KeyCode.R, 59 }, { KeyCode.S, 60 }, { KeyCode.T, 61 },
        { KeyCode.U, 62 }, { KeyCode.V, 63 }, { KeyCode.W, 64 }, { KeyCode.X, 65 },
        { KeyCode.Y, 66 }, { KeyCode.Z, 67 },
        { KeyCode.Period, 72 },
        { KeyCode.Semicolon, 74 },
        { KeyCode.Space, 78 },
        { KeyCode.Escape, 0xFD },
        { KeyCode.Return, 0xFE },
        { KeyCode.Backspace, 0xFF }
    };
    // Update is called once per frame
    void Update()
    {
    if (!KeyboardToggle.isOn)
        return;  
        
    foreach (var kvp in keyMap)
        {
            KeyCode key = kvp.Key;
            int keyCode = kvp.Value;

            if (Input.GetKeyUp(key) && LastKeyPressed != KeyCode.None)
            {
                LastKeyPressed = KeyCode.None;
                break;
            }

            if (Input.GetKeyDown(key) && key != LastKeyPressed)
            {
                // Debug.Log(key);
                LastKeyPressed = key;

                emu.INT = true;
                cpu = emu.cpu;
                mem = emu.mem;
                cpu.writeShort(mem, port_address, (ushort)keyCode); // write keycode to 0xF4
                break;
            }
        }
    }
}