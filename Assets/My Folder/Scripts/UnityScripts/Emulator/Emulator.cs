using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using TMPro;
using UnityEngine.UI;
using Unity.Mathematics;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class Emulator : MonoBehaviour
{

    public ScreenRenderer screenRenderer;

    public Toggle toggle;


    public struct MEM
    {
        public static int maxMem = 65536;
        private ushort[] _data;
        public ushort[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public MEM(ushort[] Data)
        {
            _data = Data;
        }

        public ushort[] getMemory(string f)
        {
            BinaryReader reader = new BinaryReader(File.Open(f, FileMode.Open), Encoding.UTF8);
            ushort[] _Data = new ushort[maxMem];
            int i = 0;
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                _Data[i] = reader.ReadUInt16();
                i++;
            }
            reader.Close();
            return _Data;
        }
    }


    public struct CPU
    {
        public ushort PC;
        public ushort SP;
        public ushort A;
        public ushort X;
        public ushort Y;
        public bool F_C;
        public bool F_Z;
        public bool HLT;



        public void reset()
        {
            PC = 0x00;
            SP = 0x6FFE;
            A = X = Y = 0;
            F_C = false;
            F_Z = false;
            HLT = false;
        }

        // publicfetchByte(MEM mem) {
        //     byte code = mem.Data[PC];
        //     PC++;
        //     return code;
        // }

        public ushort fetchShort(MEM mem)
        {
            ushort code = mem.Data[PC];
            PC += 1;
            return code;
        }

        // public byte readByte(MEM mem, ushort address) {
        //     return mem.Data[address];
        // }

        public ushort readShort(MEM mem, ushort address)
        {
            // ushort code = (ushort)(mem.Data[address] << 8 | mem.Data[address+1]);
            ushort code = mem.Data[address];
            return code;
        }

        public void writeShort(MEM mem, ushort address, ushort value)
        {
            mem.Data[address] = value;
        }

        public void updateFlag(int val)
        {
            if (val > 0xFFFF || val < 0)
            {
                F_C = true;
            }
            else F_C = false;
            if (val == 0)
            {
                F_Z = true;
            }
            else F_Z = false;
        }

        public void decStack()
        {
            if (SP == 0x6F00) SP = 0x6FFE;
            else SP--;
        }

        public void incStack()
        {
            if (SP == 0x6FFE) SP = 0x6F00;
            else SP++;
        }

        public void PushStackShort(MEM mem, ushort val)
        {
            writeShort(mem, SP, val);
            decStack();
        }

        public ushort PullStackShort(MEM mem)
        {
            incStack();
            return readShort(mem, SP);
        }

        public void UpdateHLT()
        {
            if (HLT)
            {
                HLT = false;
            }
            else HLT = true;
        }

        public int getBitMapCode(KeyCode kcode)
        {
            switch (kcode)
            {
                case KeyCode.Alpha0:
                    return 10;
                case KeyCode.Alpha1:
                    return 11;
                case KeyCode.Alpha2:
                    return 12;
                case KeyCode.Alpha3:
                    return 13;
                case KeyCode.Alpha4:
                    return 14;
                case KeyCode.Alpha5:
                    return 15;
                case KeyCode.Alpha6:
                    return 16;
                case KeyCode.Alpha7:
                    return 17;
                case KeyCode.Alpha8:
                    return 18;
                case KeyCode.Alpha9:
                    return 19;
                case KeyCode.A:
                    return 42;
                case KeyCode.B:
                    return 43;
                case KeyCode.C:
                    return 44;
                case KeyCode.D:
                    return 45;
                case KeyCode.E:
                    return 46;
                case KeyCode.F:
                    return 47;
                case KeyCode.G:
                    return 48;
                case KeyCode.H:
                    return 49;
                case KeyCode.I:
                    return 50;
                case KeyCode.J:
                    return 51;
                case KeyCode.K:
                    return 52;
                case KeyCode.L:
                    return 53;
                case KeyCode.M:
                    return 54;
                case KeyCode.N:
                    return 55;
                case KeyCode.O:
                    return 56;
                case KeyCode.P:
                    return 57;
                case KeyCode.Q:
                    return 58;
                case KeyCode.R:
                    return 59;
                case KeyCode.S:
                    return 60;
                case KeyCode.T:
                    return 61;
                case KeyCode.U:
                    return 62;
                case KeyCode.V:
                    return 63;
                case KeyCode.W:
                    return 64;
                case KeyCode.X:
                    return 65;
                case KeyCode.Y:
                    return 66;
                case KeyCode.Z:
                    return 67;
                case KeyCode.Space:
                    return 0;
                case KeyCode.Return:
                    return 1;
                case KeyCode.Backspace:
                    return 2;
                default:
                    return -1;
            }
        }


        public void execute(CPU cpu, MEM mem, MEM mem_Backup, ScreenRenderer sc, ref bool intrupt)
        {
            if (intrupt)
            {
                intrupt = false;
                // print("INT detected");
                PushStackShort(mem, PC);
                PC = 0x6DF0; // Address of the interrupt handler
            }



            ushort instruction = fetchShort(mem);
            ushort imidiate;
            ushort address;

            ushort val;
            int int_val;

            switch (instruction)
            {
                case 0x0: //NOP
                    break;

                case 0x4: //LDA imidiate
                    imidiate = fetchShort(mem);
                    A = imidiate;
                    updateFlag(A);
                    break;
                case 0x5: //LDA address
                    address = fetchShort(mem);
                    A = readShort(mem, address);
                    updateFlag(A);
                    break;
                case 0x6: //LDA (address+X)
                    address = (ushort)(fetchShort(mem) + X);
                    A = readShort(mem, address);
                    updateFlag(A);
                    break;
                case 0x7: //LDA (address+Y)
                    address = (ushort)(fetchShort(mem) + Y);
                    A = readShort(mem, address);
                    updateFlag(A);
                    break;
                case 0x8: //LDX imidiate
                    imidiate = fetchShort(mem);
                    X = imidiate;
                    updateFlag(X);
                    break;
                case 0x9: //LDX address
                    address = fetchShort(mem);
                    X = readShort(mem, address);
                    updateFlag(X);
                    break;
                case 0xB: //LDX (address+Y)
                    address = (ushort)(fetchShort(mem) + Y);
                    X = readShort(mem, address);
                    updateFlag(X);
                    break;
                case 0xC: //LDY imidiate
                    imidiate = fetchShort(mem);
                    Y = imidiate;
                    updateFlag(Y);
                    break;
                case 0xD: //LDY address
                    address = fetchShort(mem);
                    Y = readShort(mem, address);
                    updateFlag(Y);
                    break;
                case 0xE: //LDY (address+X)
                    address = (ushort)(fetchShort(mem) + X);
                    Y = readShort(mem, address);
                    updateFlag(Y);
                    break;
                case 0xA0: //LDA (indirect), 0
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    A = readShort(mem, address);
                    updateFlag(A);
                    break;
                case 0xA1: //LDA (indirect),X
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    A = readShort(mem, (ushort)(address + X));
                    updateFlag(A);
                    break;
                case 0xA2: //LDA (indirect),Y
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    A = readShort(mem, (ushort)(address + Y));
                    updateFlag(A);
                    break;
                case 0xA4: //LDX (indirect), 0
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    X = readShort(mem, address);
                    updateFlag(X);
                    break;
                case 0xA5: //LDX (indirect),Y
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    X = readShort(mem, (ushort)(address + Y));
                    updateFlag(X);
                    break;
                case 0xA8: //LDY (indirect), 0
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    Y = readShort(mem, address);
                    updateFlag(Y);
                    break;
                case 0xA9: //LDY (indirect),X
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    Y = readShort(mem, (ushort)(address + X));
                    updateFlag(Y);
                    break;


                case 0x11: //STA address
                    address = fetchShort(mem);
                    writeShort(mem, address, A);
                    break;
                case 0x12: //STA (address+X)
                    address = fetchShort(mem);
                    writeShort(mem, (ushort)(address + X), A);
                    break;
                case 0x13: //STA (address+Y)
                    address = fetchShort(mem);
                    writeShort(mem, (ushort)(address + Y), A);
                    break;
                case 0x15: //STX address
                    address = fetchShort(mem);
                    writeShort(mem, address, X);
                    break;
                case 0x17: //STX (address+Y)
                    address = fetchShort(mem);
                    writeShort(mem, (ushort)(address + Y), X);
                    break;
                case 0x19: //STY address
                    address = fetchShort(mem);
                    writeShort(mem, address, Y);
                    break;
                case 0x1A: //STY (address+X)
                    address = fetchShort(mem);
                    writeShort(mem, (ushort)(address + X), Y);
                    break;
                case 0x94: //STA (indirect), 0
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    writeShort(mem, address, A);
                    break;
                case 0x95: //STA (indirect),X
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    writeShort(mem, (ushort)(address + X), A);
                    break;
                case 0x96: //STA (indirect),Y
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    writeShort(mem, (ushort)(address + Y), A);
                    break;
                case 0x98: //STX (indirect), 0
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    writeShort(mem, address, X);
                    break;
                case 0x99: //STX (indirect),Y
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    writeShort(mem, (ushort)(address + Y), X);
                    break;
                case 0x9C: //STY (indirect), 0
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    writeShort(mem, address, Y);
                    break;
                case 0x9D: //STY (indirect),X
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    writeShort(mem, (ushort)(address + X), Y);
                    break;

                case 0x1C: //TAX
                    X = A;
                    updateFlag(X);
                    break;
                case 0x20: //TAY
                    Y = A;
                    updateFlag(Y);
                    break;
                case 0x24: //TXA
                    A = X;
                    updateFlag(A);
                    break;
                case 0x28: //TYA
                    A = Y;
                    updateFlag(A);
                    break;

                case 0x2C: //TSX
                    X = (ushort)(8 << SP);
                    updateFlag(X);
                    break;
                case 0x30: //TXS
                    SP = X;
                    break;
                case 0x34: //PHA
                    PushStackShort(mem, A);
                    break;
                case 0x38: //PLA
                    A = PullStackShort(mem);
                    updateFlag(A);
                    break;

                case 0x3C: //AND imidiate
                    imidiate = fetchShort(mem);
                    A = (ushort)(A & imidiate);
                    updateFlag(A);
                    break;
                case 0x3D: //AND address
                    address = fetchShort(mem);
                    A = (ushort)(A & readShort(mem, address));
                    updateFlag(A);
                    break;
                case 0x3E: //AND (address+X)
                    address = fetchShort(mem);
                    A = (ushort)(A & readShort(mem, (ushort)(address + X)));
                    updateFlag(A);
                    break;
                case 0x3F: //AND (address+Y)
                    address = fetchShort(mem);
                    A = (ushort)(A & readShort(mem, (ushort)(address + Y)));
                    updateFlag(A);
                    break;
                case 0x40: //OR imidiate
                    imidiate = fetchShort(mem);
                    A = (ushort)(A | imidiate);
                    updateFlag(A);
                    break;
                case 0x41: //OR address
                    address = fetchShort(mem);
                    A = (ushort)(A | readShort(mem, address));
                    updateFlag(A);
                    break;
                case 0x42: //OR (address+X)
                    address = fetchShort(mem);
                    A = (ushort)(A | readShort(mem, (ushort)(address + X)));
                    updateFlag(A);
                    break;
                case 0x43: //OR (address+Y)
                    address = fetchShort(mem);
                    A = (ushort)(A | readShort(mem, (ushort)(address + Y)));
                    updateFlag(A);
                    break;
                case 0x44: //XOR imidiate
                    imidiate = fetchShort(mem);
                    A = (ushort)(A ^ imidiate);
                    updateFlag(A);
                    break;
                case 0x45: //XOR address
                    address = fetchShort(mem);
                    A = (ushort)(A ^ readShort(mem, address));
                    updateFlag(A);
                    break;
                case 0x46: //XOR (address+X)
                    address = fetchShort(mem);
                    A = (ushort)(A ^ readShort(mem, (ushort)(address + X)));
                    updateFlag(A);
                    break;
                case 0x47: //XOR (address+Y)
                    address = fetchShort(mem);
                    A = (ushort)(A ^ readShort(mem, (ushort)(address + Y)));
                    updateFlag(A);
                    break;

                case 0x48: //ADD imidiate
                    imidiate = fetchShort(mem);
                    int_val = A + imidiate;
                    A = (ushort)int_val;
                    updateFlag(int_val);
                    break;
                case 0x49: //ADD address
                    address = fetchShort(mem);
                    int_val = A + readShort(mem, address);
                    A = (ushort)int_val;
                    updateFlag(int_val);
                    break;
                case 0x4A: //ADD (address+X)
                    address = fetchShort(mem);
                    int_val = A + readShort(mem, (ushort)(address + X));
                    A = (ushort)int_val;
                    updateFlag(int_val);
                    break;
                case 0x4B: //ADD (address+Y)
                    address = fetchShort(mem);
                    int_val = A + readShort(mem, (ushort)(address + Y));
                    A = (ushort)int_val;
                    updateFlag(int_val);
                    break;
                case 0x4C: //SUB imidiate
                    imidiate = fetchShort(mem);
                    int_val = A - imidiate;
                    A = (ushort)int_val;
                    updateFlag(int_val);
                    break;
                case 0x4D: //SUB address
                    address = fetchShort(mem);
                    int_val = A - readShort(mem, address);
                    A = (ushort)int_val;
                    updateFlag(int_val);
                    break;
                case 0x4E: //SUB (address+X)
                    address = fetchShort(mem);
                    int_val = A - readShort(mem, (ushort)(address + X));
                    A = (ushort)int_val;
                    updateFlag(int_val);
                    break;
                case 0x4F: //SUB (address+Y)
                    address = fetchShort(mem);
                    int_val = A - readShort(mem, (ushort)(address + Y));
                    A = (ushort)int_val;
                    updateFlag(int_val);
                    break;
                case 0x50: //CMP A imidiate
                    imidiate = fetchShort(mem);
                    val = imidiate;
                    if (A >= val)
                    {
                        F_C = true;
                    }
                    else F_C = false;

                    if (A == val)
                    {
                        F_Z = true;
                    }
                    else F_Z = false;
                    break;
                case 0x51: //CMP A address
                    address = fetchShort(mem);
                    val = readShort(mem, address);
                    if (A >= val)
                    {
                        F_C = true;
                    }
                    else F_C = false;
                    if (A == val)
                    {
                        F_Z = true;
                    }
                    else F_Z = false;
                    break;
                case 0x52: //CMP A (address+X)
                    address = fetchShort(mem);
                    val = readShort(mem, (ushort)(address + X));
                    if (A >= val)
                    {
                        F_C = true;
                    }
                    else F_C = false;
                    if (A == val)
                    {
                        F_Z = true;
                    }
                    else F_Z = false;
                    break;
                case 0x53: //CMP A (address+Y)
                    address = fetchShort(mem);
                    val = readShort(mem, (ushort)(address + Y));
                    if (A >= val)
                    {
                        F_C = true;
                    }
                    else F_C = false;
                    if (A == val)
                    {
                        F_Z = true;
                    }
                    else F_Z = false;
                    break;
                case 0x54: //CMP X imidiate
                    imidiate = fetchShort(mem);
                    val = imidiate;
                    if (X >= val)
                    {
                        F_C = true;
                    }
                    else F_C = false;
                    if (X == val)
                    {
                        F_Z = true;
                    }
                    else F_Z = false;
                    break;
                case 0x55: //CMP X address
                    address = fetchShort(mem);
                    val = readShort(mem, address);
                    if (X >= val)
                    {
                        F_C = true;
                    }
                    else F_C = false;
                    if (X == val)
                    {
                        F_Z = true;
                    }
                    else F_Z = false;
                    break;
                case 0x57: //CMP X (address+Y)
                    address = fetchShort(mem);
                    val = readShort(mem, (ushort)(address + Y));
                    if (X >= val)
                    {
                        F_C = true;
                    }
                    else F_C = false;
                    if (X == val)
                    {
                        F_Z = true;
                    }
                    else F_Z = false;
                    break;
                case 0x58: //CMP Y imidiate
                    imidiate = fetchShort(mem);
                    val = imidiate;
                    if (Y >= val)
                    {
                        F_C = true;
                    }
                    else F_C = false;
                    if (Y == val)
                    {
                        F_Z = true;
                    }
                    else F_Z = false;
                    break;
                case 0x59: //CMP Y address
                    address = fetchShort(mem);
                    val = readShort(mem, address);
                    if (Y >= val)
                    {
                        F_C = true;
                    }
                    else F_C = false;
                    if (Y == val)
                    {
                        F_Z = true;
                    }
                    else F_Z = false;
                    break;
                case 0x5A: //CMP Y (address+X)
                    address = fetchShort(mem);
                    val = readShort(mem, (ushort)(address + X));
                    if (Y >= val)
                    {
                        F_C = true;
                    }
                    else F_C = false;
                    if (Y == val)
                    {
                        F_Z = true;
                    }
                    else F_Z = false;
                    break;

                case 0x5D: //INC address
                    address = fetchShort(mem);
                    int_val = readShort(mem, address) + 1;
                    writeShort(mem, address, (ushort)int_val);
                    updateFlag(int_val);
                    break;
                case 0x5E: //INC (address,X)
                    address = fetchShort(mem);
                    int_val = readShort(mem, (ushort)(address + X)) + 1;
                    writeShort(mem, (ushort)(address + X), (ushort)int_val);
                    updateFlag(int_val);
                    break;
                case 0x5F: //INC (address+Y)
                    address = fetchShort(mem);
                    int_val = readShort(mem, (ushort)(address + Y)) + 1;
                    writeShort(mem, (ushort)(address + Y), (ushort)int_val);
                    updateFlag(int_val);
                    break;
                case 0x60: //INC X
                    int_val = X + 1;
                    X = (ushort)int_val;
                    updateFlag(int_val);
                    break;
                case 0x64: //INC Y
                    int_val = Y + 1;
                    Y = (ushort)int_val;
                    updateFlag(int_val);
                    break;
                case 0x69: //DEC address
                    address = fetchShort(mem);
                    int_val = readShort(mem, address) - 1;
                    writeShort(mem, address, (ushort)int_val);
                    updateFlag(int_val);
                    break;
                case 0x6A: //DEC (address,X)
                    address = fetchShort(mem);
                    int_val = readShort(mem, (ushort)(address + X)) - 1;
                    writeShort(mem, (ushort)(address + X), (ushort)int_val);
                    updateFlag(int_val);
                    break;
                case 0x6B: //DEC (address+Y)
                    address = fetchShort(mem);
                    int_val = readShort(mem, (ushort)(address + Y)) - 1;
                    writeShort(mem, (ushort)(address + Y), (ushort)int_val);
                    updateFlag(int_val);
                    break;
                case 0x6C: //DEC X
                    int_val = X - 1;
                    X = (ushort)int_val;
                    updateFlag(int_val);
                    break;
                case 0x70: //DEC Y
                    int_val = Y - 1;
                    Y = (ushort)int_val;
                    updateFlag(int_val);
                    break;

                case 0x75: //JMP address
                    address = fetchShort(mem);
                    PC = address;
                    break;
                case 0x76: //JMP [indirect]
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    PC = address;
                    break;
                case 0x77: //JMP [indirect],X
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    PC = (ushort)(address + X);
                    break;

                case 0x79: //JSR address
                    address = fetchShort(mem);
                    PushStackShort(mem, PC);
                    PC = address;
                    break;
                case 0x7A: //JSR [Indirect]
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    PushStackShort(mem, PC);
                    PC = address;
                    break;
                case 0x7B: //JSR [Indirect],X
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    PushStackShort(mem, PC);
                    PC = (ushort)(address + X);
                    break;
                case 0x7C: //RTS
                    PC = PullStackShort(mem);
                    break;

                case 0x81: //BCC address
                    address = fetchShort(mem);
                    if (!F_C) PC = address;
                    break;
                case 0x85: //BCS address
                    address = fetchShort(mem);
                    if (F_C) PC = address;
                    break;
                case 0x89: //BNE address
                    address = fetchShort(mem);
                    if (!F_Z) PC = address;
                    break;
                case 0x8D: //BEQ address
                    address = fetchShort(mem);
                    if (F_Z) PC = address;
                    break;
                case 0xAC: //SLA
                    val = fetchShort(mem);
                    if(A >> (16 - val) != 0)
                    {
                        F_C = true;
                    }
                    else F_C = false;

                    A = (ushort)(A << val);
                    
                    if(A == 0)
                    {
                        F_Z = true;
                    }
                    else F_Z = false;
                    break;
                case 0xB0: //SRA
                    val = fetchShort(mem);
                    if((A & (1 << (val - 1))) != 0)
                    {
                        F_C = true;
                    }
                    else F_C = false;
                    
                    A = (ushort)(A >> val);
                    
                    if(A == 0)
                    {
                        F_Z = true;
                    }
                    else F_Z = false;
                    updateFlag(A);
                    break;
                case 0xfffc: //CLS
                    sc.deleteBackupScreens();
                    Array.Clear(mem.Data, 0x7000, 256 * 144);
                    break;
                case 0xfffd: //CNP (Clear to new page)
                    sc.storeCurrentScreenBackup();
                    sc.currentScreenIndex++;
                    sc.totalScreens++;
                    Array.Clear(mem.Data, 0x7000, 256 * 144);
                    break;
                case 0xfffe: //DUP
                    Array.Copy(mem.Data, mem_Backup.Data, mem.Data.Length);
                    sc.updateScreen = true;
                    if (!sc.isHomeScreen)
                    {
                        sc.currentScreenIndex = sc.bitmapScreenBackup.Length - 1;
                        sc.isHomeScreen = true;
                    }
                    break;
                case 0xffff: //HLT
                    UpdateHLT();
                    break;
                default:
                    print("Invalid Opcode " + Convert.ToString(PC, 16) + " " + instruction);
                    break;
            }

        }
    }

    public CPU cpu;
    public bool INT; // intrupt flag
    public bool isCPUinitiated = false;
    public MEM mem;
    public MEM mem_Backup;


    public bool isMemLoaded = false;

    //Clock
    public UnityEngine.UI.Slider cpuSpeedNoDelay;
    public TMP_InputField maxSpeed;



    public bool isDelay;
    bool RunCPUbool;
    public float clock_speed; // [value] Hz
    public int clock_step;
    public int frequency;
    int rate = 100;

    float timesincecurrentstepsiszero;
    float currentsteps;

    //Scene
    public TMP_Text OutputText;
    public Assembler assembler;

    public ushort[] memDataShower;

    public void Start()
    {
        currentsteps = 0;
        timesincecurrentstepsiszero = Time.realtimeSinceStartup;
        // Application.targetFrameRate = 500;
        cpu = new CPU();
        // mem = new MEM(mem.getMemory(programFilePath));
        cpu.HLT = true;
        cpuSpeedNoDelay.onValueChanged.AddListener(OnCputSpeedNoDelayValueChanged);
        maxSpeed.onValueChanged.AddListener(OnMaxSpeedValueChanged);

    }

    public void loadMem(string programFilePath)
    {
        mem = new MEM(mem.getMemory(programFilePath));
        mem_Backup = new MEM(mem_Backup.getMemory(programFilePath));
        cpu.reset();
        isMemLoaded = true;
        memDataShower = mem.Data;
    }


    public void StartCPU()
    {
        RunCPUbool = false;
        //Get The instruction set from the file and store it in his memory
        clock_step = 0;
        // mem = new MEM(mem.getMemory(programFilePath));
        // loadMem(assembler.binaryFilePath);
        // print(assembler.binaryFilePath);
        cpu.reset();

        cpu.HLT = false;
        //Run The loop
        if (isDelay) StartCoroutine(RunCPU());
        else RunCPUbool = true;
    }

    public void run1Tick()
    {
        //Run One Instruction
        if (!cpu.HLT && toggle.isOn)
            cpu.execute(cpu, mem, mem_Backup, screenRenderer, ref INT);
        clock_step += 8;
        if (cpu.PC == 0x6EFF)
        {
            cpu.HLT = true;
        }
    }
    [HideInInspector]
    public int index;

    int resetIndex = 300;


    public void Update()
    {
        int i = index;
        if (RunCPUbool && !toggle.isOn)
        {
            if (!cpu.HLT)
            {
                while (i != 0)
                {
                    if (toggle.isOn)
                        break;

                    if (!cpu.HLT)
                    {
                        //Run One Instruction
                        cpu.execute(cpu, mem, mem_Backup, screenRenderer, ref INT);
                        clock_step += 8;
                        if (cpu.PC > 0x6EFF)
                        {
                            cpu.HLT = true;
                        }
                    }
                    currentsteps++;
                    i--;
                }
            }
        }
    }

    public void FixedUpdate()
    {
        if (!cpu.HLT)
        {
            if (rate == 0)
            {
                float timediff = Time.realtimeSinceStartup - timesincecurrentstepsiszero;
                if (timediff == 0)
                {
                    timediff = 0.00001f;
                }
                rate = 100;
                frequency = (int)(currentsteps / timediff);
            }

            if (resetIndex == 0)
            {
                currentsteps = 0;
                timesincecurrentstepsiszero = Time.realtimeSinceStartup;
                resetIndex = 300;
            }
            resetIndex--;
            rate--;
        }
    }

    void OnMaxSpeedValueChanged(string newHoursValue)
    {

        if (string.IsNullOrWhiteSpace(newHoursValue))
            return;

        if (int.TryParse(newHoursValue, out int result))
        {
            if (result < 0)
            {
                index = 0;
            }
            else
            {
                index = (int)(int.Parse(maxSpeed.text) * cpuSpeedNoDelay.value);
                // if(index == 1) index = 0;
            }
        }
    }

    void OnCputSpeedNoDelayValueChanged(float newValue)
    {
        OnMaxSpeedValueChanged(maxSpeed.text);
    }

    IEnumerator RunCPU()
    {
        while (true)
        {
            while (!cpu.HLT && !toggle.isOn)
            {
                //Run One Instruction
                cpu.execute(cpu, mem, mem_Backup, screenRenderer, ref INT);
                clock_step += 8;
                yield return new WaitForSeconds(1 / clock_speed);
                if (cpu.PC == 0x6EFF)
                {
                    cpu.HLT = true;
                }
                currentsteps++;
            }
            yield return new WaitForEndOfFrame();
        }
    }
}