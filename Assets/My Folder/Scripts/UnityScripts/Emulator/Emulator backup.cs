using System.Collections;
using UnityEngine;
using System.IO;
using System;
using TMPro;
using UnityEngine.UI;
using Unity.Mathematics;

public class EmulatorBackup : MonoBehaviour
{
    // Program File Path
    public string programFilePath;
    public ScreenRenderer screenRenderer;

    public Toggle toggle;

    public struct MEM {
        public static int maxMem = 65536;
        private byte[] _data;
        public byte[] Data {
            get {return _data; }
            set {_data = value;}
        }

        public MEM(byte[] Data) {
            _data = Data;
        }

        public byte[] getMemory(string f) {
            StreamReader inp_stm = new StreamReader(f);
            int i=0;
            byte[] _Data = new byte[maxMem];

            while(!inp_stm.EndOfStream) {
                string[] inp_ln = inp_stm.ReadLine().Split();
                foreach(string _byte in inp_ln) {
                    _Data[i] = Convert.ToByte(_byte, 16);
                    i++;
                }
            }
            inp_stm.Close();
            return _Data;
        }
    }


    public struct CPU {
        public ushort PC;
        public ushort SP;
        public byte A;
        public byte X;
        public byte Y;
        public bool F_C;
        public bool F_Z;
        public bool HLT;

        public void reset() {
            PC = 0x00;
            SP = 0x6FFE;
            A = X = Y = 0;
            F_C = false;
            F_Z = false;
            HLT = false;
        }

        public byte fetchByte(MEM mem) {
            byte code = mem.Data[PC];
            PC++;
            return code;
        }

        public ushort fetchShort(MEM mem) {
            ushort code = (ushort)(mem.Data[PC] << 8 | mem.Data[PC+1]);
            PC += 2;
            return code;
        }

        public byte readByte(MEM mem, ushort address) {
            return mem.Data[address];
        }

        public ushort readShort(MEM mem, ushort address) {
            ushort code = (ushort)(mem.Data[address] << 8 | mem.Data[address+1]);
            return code;
        }

        public void writeByte(MEM mem, ushort address, byte value) {
            mem.Data[address] = value;
        }

        public void updateFlag(int val) {
            if(val > 0xFF) {
                F_C = true;
            }else F_C = false;
            if(val == 0) {
                F_Z = true;
            }else F_Z = false;
        }
        
        public void decStack() {
            if(SP == 0x6F00) SP = 0x6FFE;
            else SP--;
        }

        public void incStack() {
            if(SP == 0x6FFE) SP = 0x6F00;
            else SP++;
        }

        public void PushStackByte(MEM mem, byte val) {
            writeByte(mem, SP, val);
            decStack();
        }

        public byte PullStackByte(MEM mem) {
            incStack();
            return readByte(mem, SP);
        }

        public void PushStackShort(MEM mem, ushort val) {
            writeByte(mem, SP, (byte)val);
            decStack();
            writeByte(mem, SP, (byte)(val>>8));
            decStack();
        }

        public ushort PullStackShort(MEM mem) {
            byte val1;
            ushort val;
            incStack();
            val1 = (byte)(readByte(mem, SP)<<8);
            incStack();
            val = (ushort)(val1 | readByte(mem, SP));
            return val;
        }


        public void execute(CPU cpu, MEM mem , ScreenRenderer sc) {
            byte instruction = fetchByte(mem);
            byte imidiate;
            ushort address;

            int int_val;
            byte val;

            switch(instruction) {
                case 0x0: //NOP
                    break;

                case 0x4: //LDA imidiate
                    imidiate = fetchByte(mem);
                    A = imidiate;
                    updateFlag(A);
                    break;
                case 0x5: //LDA address
                    address = fetchShort(mem);
                    A = readByte(mem, address);
                    updateFlag(A);
                    break;
                case 0x6: //LDA (address+X)
                    address = (ushort)(fetchShort(mem) + X);
                    A = readByte(mem, address);
                    updateFlag(A);
                    break;
                case 0x7: //LDA (address+Y)
                    address = (ushort)(fetchShort(mem) + Y);
                    A = readByte(mem, address);
                    updateFlag(A);
                    break;
                case 0x8: //LDX imidiate
                    imidiate = fetchByte(mem);
                    X = imidiate;
                    updateFlag(X);
                    break;
                case 0x9: //LDX address
                    address = fetchShort(mem);
                    X = readByte(mem, address);
                    updateFlag(X);
                    break;
                case 0xB: //LDX (address+Y)
                    address = (ushort)(fetchShort(mem) + Y);
                    X = readByte(mem, address);
                    updateFlag(X);
                    break;
                case 0xC: //LDY imidiate
                    imidiate = fetchByte(mem);
                    Y = imidiate;
                    updateFlag(Y);
                    break;
                case 0xD: //LDY address
                    address = fetchShort(mem);
                    Y = readByte(mem, address);
                    updateFlag(Y);
                    break;
                case 0xE: //LDY (address+X)
                    address = (ushort)(fetchShort(mem) + X);
                    Y = readByte(mem, address);
                    updateFlag(Y);
                    break;
                case 0x11: //STA address
                    address = fetchShort(mem);
                    writeByte(mem, address, A);
                    break;
                case 0x12: //STA (address+X)
                    address = fetchShort(mem);
                    writeByte(mem, (ushort)(address+X), A);
                    break;
                case 0x13: //STA (address+Y)
                    address = fetchShort(mem);
                    writeByte(mem, (ushort)(address+Y), A);
                    break;
                case 0x15: //STX address
                    address = fetchShort(mem);
                    writeByte(mem, address, X);
                    break;
                case 0x17: //STX (address+Y)
                    address = fetchShort(mem);
                    writeByte(mem, (ushort)(address+Y), X);
                    break;
                case 0x19: //STY address
                    address = fetchShort(mem);
                    writeByte(mem, address, Y);
                    break;
                case 0x1A: //STY (address+X)
                    address = fetchShort(mem);
                    writeByte(mem, (ushort)(address+X), Y);
                    break;
                case 0x94: //STA (indirect),X
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    writeByte(mem, (ushort)(address+X), A);
                    break;
                case 0x95: //STA (indirect),Y
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    writeByte(mem, (ushort)(address+Y), A);
                    break;
                case 0x98: //STX (indirect),Y
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    writeByte(mem, (ushort)(address+Y), X);
                    break;
                case 0x9C: //STY (indirect),X
                    address = fetchShort(mem);
                    address = readShort(mem, address);
                    writeByte(mem, (ushort)(address+X), Y);
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
                    X = (byte)(8<<SP);
                    updateFlag(X);
                    break;
                case 0x30: //TXS
                    SP = X;
                    break;
                case 0x34: //PHA
                    PushStackByte(mem, A);
                    break;
                case 0x38: //PLA
                    A = PullStackByte(mem);
                    updateFlag(A);
                    break;

                case 0x3C: //AND imidiate
                    imidiate = fetchByte(mem);
                    A = (byte)(A & imidiate);
                    updateFlag(A);
                    break;
                case 0x3D: //AND address
                    address = fetchShort(mem);
                    A = (byte)(A & readByte(mem, address));
                    updateFlag(A);
                    break;
                case 0x3E: //AND (address+X)
                    address = fetchShort(mem);
                    A = (byte)(A & readByte(mem, (ushort)(address+X)));
                    updateFlag(A);
                    break;
                case 0x3F: //AND (address+Y)
                    address = fetchShort(mem);
                    A = (byte)(A & readByte(mem, (ushort)(address+Y)));
                    updateFlag(A);
                    break;
                case 0x40: //OR imidiate
                    imidiate = fetchByte(mem);
                    A = (byte)(A | imidiate);
                    updateFlag(A);
                    break;
                case 0x41: //OR address
                    address = fetchShort(mem);
                    A = (byte)(A | readByte(mem, address));
                    updateFlag(A);
                    break;
                case 0x42: //OR (address+X)
                    address = fetchShort(mem);
                    A = (byte)(A | readByte(mem, (ushort)(address+X)));
                    updateFlag(A);
                    break;
                case 0x43: //OR (address+Y)
                    address = fetchShort(mem);
                    A = (byte)(A | readByte(mem, (ushort)(address+Y)));
                    updateFlag(A);
                    break;
                case 0x44: //XOR imidiate
                    imidiate = fetchByte(mem);
                    A = (byte)(A ^ imidiate);
                    updateFlag(A);
                    break;
                case 0x45: //XOR address
                    address = fetchShort(mem);
                    A = (byte)(A ^ readByte(mem, address));
                    updateFlag(A);
                    break;
                case 0x46: //XOR (address+X)
                    address = fetchShort(mem);
                    A = (byte)(A ^ readByte(mem, (ushort)(address+X)));
                    updateFlag(A);
                    break;
                case 0x47: //XOR (address+Y)
                    address = fetchShort(mem);
                    A = (byte)(A ^ readByte(mem, (ushort)(address+Y)));
                    updateFlag(A);
                    break;
                
                case 0x48: //ADD imidiate
                    imidiate = fetchByte(mem);
                    int_val = A + imidiate;
                    A = (byte)int_val;
                    updateFlag(int_val);
                    break;
                case 0x49: //ADD address
                    address = fetchShort(mem);
                    int_val = A + readByte(mem, address);
                    A = (byte)int_val;
                    updateFlag(int_val);
                    break;
                case 0x4A: //ADD (address+X)
                    address = fetchShort(mem);
                    int_val = A + readByte(mem, (ushort)(address+X));
                    A = (byte)int_val;
                    updateFlag(int_val);
                    break;
                case 0x4B: //ADD (address+Y)
                    address = fetchShort(mem);
                    int_val = A + readByte(mem, (ushort)(address+Y));
                    A = (byte)int_val;
                    updateFlag(int_val);
                    break;
                case 0x4C: //SUB imidiate
                    imidiate = fetchByte(mem);
                    int_val = A - imidiate;
                    A = (byte)int_val;
                    updateFlag(int_val);
                    break;
                case 0x4D: //SUB address
                    address = fetchShort(mem);
                    int_val = A - readByte(mem, address);
                    A = (byte)int_val;
                    updateFlag(int_val);
                    break;
                case 0x4E: //SUB (address+X)
                    address = fetchShort(mem);
                    int_val = A - readByte(mem, (ushort)(address+X));
                    A = (byte)int_val;
                    updateFlag(int_val);
                    break;
                case 0x4F: //SUB (address+Y)
                    address = fetchShort(mem);
                    int_val = A - readByte(mem, (ushort)(address+Y));
                    A = (byte)int_val;
                    updateFlag(int_val);
                    break;
                case 0x50: //CMP A imidiate
                    imidiate = fetchByte(mem);
                    val = imidiate;
                    if(A>=val) {
                        F_C = true;
                    } else F_C = false;

                    if(A==val) {
                        F_Z = true;
                    }else F_Z = false;
                    break;
                case 0x51: //CMP A address
                    address = fetchShort(mem);
                    val = readByte(mem, address);
                    if(A>=val) {
                        F_C = true;
                    }else F_C = false;
                    if(A==val) {
                        F_Z = true;
                    }else F_Z = false;
                    break;
                case 0x52: //CMP A (address+X)
                    address = fetchShort(mem);
                    val = readByte(mem, (ushort)(address+X));
                    if(A>=val) {
                        F_C = true;
                    }else F_C = false;
                    if(A==val) {
                        F_Z = true;
                    }else F_Z = false;
                    break;
                case 0x53: //CMP A (address+Y)
                    address = fetchShort(mem);
                    val = readByte(mem, (ushort)(address+Y));
                    if(A>=val) {
                        F_C = true;
                    }else F_C = false;
                    if(A==val) {
                        F_Z = true;
                    }else F_Z = false;
                    break;
                case 0x54: //CMP X imidiate
                    imidiate = fetchByte(mem);
                    val = imidiate;
                    if(X>=val) {
                        F_C = true;
                    }else F_C = false;
                    if(X==val) {
                        F_Z = true;
                    }else F_Z = false;
                    break;
                case 0x55: //CMP X address
                    address = fetchShort(mem);
                    val = readByte(mem, address);
                    if(X>=val) {
                        F_C = true;
                    }else F_C = false;
                    if(X==val) {
                        F_Z = true;
                    }else F_Z = false;
                    break;
                case 0x57: //CMP X (address+Y)
                    address = fetchShort(mem);
                    val = readByte(mem, (ushort)(address+Y));
                    if(X>=val) {
                        F_C = true;
                    }else F_C = false;
                    if(X==val) {
                        F_Z = true;
                    }else F_Z = false;
                    break;
                case 0x58: //CMP Y imidiate
                    imidiate = fetchByte(mem);
                    val = imidiate;
                    if(Y>=val) {
                        F_C = true;
                    }else F_C = false;
                    if(Y==val) {
                        F_Z = true;
                    }else F_Z = false;
                    break;
                case 0x59: //CMP Y address
                    address = fetchShort(mem);
                    val = readByte(mem, address);
                    if(Y>=val) {
                        F_C = true;
                    }else F_C = false;
                    if(Y==val) {
                        F_Z = true;
                    }else F_Z = false;
                    break;
                case 0x5A: //CMP Y (address+X)
                    address = fetchShort(mem);
                    val = readByte(mem, (ushort)(address+X));
                    if(Y>=val) {
                        F_C = true;
                    }else F_C = false;
                    if(Y==val) {
                        F_Z = true;
                    }else F_Z = false;
                    break;

                case 0x5D: //INC address
                    address = fetchShort(mem);
                    int_val = readByte(mem, address) + 1;
                    writeByte(mem, address, (byte)int_val);
                    updateFlag(int_val);
                    break;
                case 0x5E: //INC (address,X)
                    address = fetchShort(mem);
                    int_val = readByte(mem, (ushort)(address+X)) + 1;
                    writeByte(mem, (ushort)(address+X), (byte)int_val);
                    updateFlag(int_val);
                    break;
                case 0x5F: //INC (address+Y)
                    address = fetchShort(mem);
                    int_val = readByte(mem, (ushort)(address+Y)) + 1;
                    writeByte(mem, (ushort)(address+Y), (byte)int_val);
                    updateFlag(int_val);
                    break;
                case 0x60: //INC X
                    int_val = X + 1;
                    X = (byte)int_val;
                    updateFlag(int_val);
                    break;
                case 0x64: //INC Y
                    int_val = Y + 1;
                    Y = (byte)int_val;
                    updateFlag(int_val);
                    break;
                case 0x69: //DEC address
                    address = fetchShort(mem);
                    int_val = readByte(mem, address) - 1;
                    writeByte(mem, address, (byte)int_val);
                    updateFlag(int_val);
                    break;
                case 0x6A: //DEC (address,X)
                    address = fetchShort(mem);
                    int_val = readByte(mem, (ushort)(address+X)) - 1;
                    writeByte(mem, (ushort)(address+X), (byte)int_val);
                    updateFlag(int_val);
                    break;
                case 0x6B: //DEC (address+Y)
                    address = fetchShort(mem);
                    int_val = readByte(mem, (ushort)(address+Y)) - 1;
                    writeByte(mem, (ushort)(address+Y), (byte)int_val);
                    updateFlag(int_val);
                    break;
                case 0x6C: //DEC X
                    int_val = X - 1;
                    X = (byte)int_val;
                    updateFlag(int_val);
                    break;
                case 0x70: //DEC Y
                    int_val = Y - 1;
                    Y = (byte)int_val;
                    updateFlag(int_val);
                    break;
                
                case 0x75: //JMP address
                    address = fetchShort(mem);
                    PC = address;
                    break;
                case 0x79: //JSR address
                    address = fetchShort(mem);
                    PushStackShort(mem, PC);
                    PC = address;
                    break;
                case 0x7C: //RTS
                    PC = PullStackShort(mem);
                    break;
                
                case 0x81: //BCC address
                    address = fetchShort(mem);
                    if(!F_C) PC = address;
                    break;
                case 0x85: //BCS address
                    address = fetchShort(mem);
                    if(F_C) PC = address;
                    break;
                case 0x89: //BNE address
                    address = fetchShort(mem);
                    if(!F_Z) PC = address;
                    break;
                case 0x8D: //BEQ address
                    address = fetchShort(mem);
                    if(F_Z) PC = address;
                    break;
                case 0x90: //DUP
                    // sc.mem = mem;
                    // sc.cpu = cpu;
                    // sc.isUpdateRequest = true;
                    break;

                case 0xff: //HLT
                    HLT = true;
                    break;
                default:
                    print("Invalid Opcode "+Convert.ToString(PC, 16)+ " " +instruction);
                    break;
            }
                
        }
    }


    public CPU cpu;
    public bool isCPUinitiated = false;
    public MEM mem;
    public bool isMemLoaded = false;

    //Clock
    public Scrollbar cpuSpeedNoDelay;
    public TMP_InputField maxSpeed;



    public bool isDelay;
    bool RunCPUbool;
    public float clock_speed; // [value] Hz
    public int clock_step;
    public int frequency;
    int rate = 10;

    //Scene
    public TMP_Text OutputText;

    public void Start() {
        cpu = new CPU();
        // mem = new MEM(mem.getMemory(programFilePath));
        cpu.HLT = true;
    }

    public void loadMem() {
        mem = new MEM(mem.getMemory(programFilePath));
        cpu.reset();
        isMemLoaded = true;
    }


    public void StartCPU() {
        RunCPUbool = false;
        //Get The instruction set from the file and store it in his memory
        clock_step = 0;
        // mem = new MEM(mem.getMemory(programFilePath));
        cpu.reset();

        cpu.HLT = false;
        //Run The loop
        if(isDelay) StartCoroutine(RunCPU());
        else RunCPUbool = true;
    }

    public void run1Tick() {
        //Run One Instruction
        if(!cpu.HLT && toggle.isOn)
        cpu.execute(cpu, mem, screenRenderer);
        clock_step += 8;
        if(cpu.PC == 0x6EFF) {
            cpu.HLT = true;
        }
    }
    [HideInInspector]
    public int index;

    void Update() {
        index = (int)(int.Parse(maxSpeed.text) * cpuSpeedNoDelay.value);
        if(index == 1) index = 0;
        int i = index;
        if(RunCPUbool) {
            if(!cpu.HLT) {
                float oldtime = Time.realtimeSinceStartup;
                while(i != 0) {
                    if(!cpu.HLT && !toggle.isOn) {
                        //Run One Instruction
                        cpu.execute(cpu, mem, screenRenderer);
                        clock_step += 8;
                        if(cpu.PC > 0x6EFF) {
                            cpu.HLT = true;
                        }
                    }
                    i--;
                }
                if(rate == 0) {
                    float time = Time.realtimeSinceStartup-oldtime;
                    if(time == 0) {
                        time = 0.0001f;
                    }
                    frequency = (int)(index*8/time);
                    rate = 10;
                }
                rate--;
            }
        }
    }

    IEnumerator RunCPU() {
        while(true) {
            while(!cpu.HLT && !toggle.isOn) {
                //Run One Instruction
                frequency = (int)clock_speed;
                cpu.execute(cpu, mem, screenRenderer);
                clock_step += 8;
                yield return new WaitForSeconds(1/clock_speed);
                if(cpu.PC == 0x6EFF) {
                    cpu.HLT = true;
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
