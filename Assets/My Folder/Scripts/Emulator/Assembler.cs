using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using Unity.VisualScripting;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine.Assertions.Must;

public class Assembler : MonoBehaviour
{

    struct Macro
    {
        public string name;
        public string[] arguments;
        public string body;
    }

    public Emulator emulator;
    public string ALProgramFilePath;

    public string Finalcode;

    // Properties of instructions
    Dictionary<string, ushort> instructions = new Dictionary<string, ushort> {
        {"NOP", 0x0000},
        {"LDA", 0x0004},
        {"LDX", 0x0008},
        {"LDY", 0x000C},
        {"STA", 0x0010},
        {"SYX", 0x0014},
        {"STY", 0x0018},
        {"TAX", 0x001C},
        {"TAY", 0x0020},
        {"TXA", 0x0024},
        {"TYA", 0x0028},
        {"TSX", 0x002C},
        {"TXS", 0x0030},
        {"PHA", 0x0034},
        {"PLA", 0x0038},
        {"AND", 0x003C},
        {"ORA", 0x0040},
        {"XOR", 0x0044},
        {"ADD", 0x0048},
        {"SUB", 0x004C},
        {"CMP", 0x0050},
        {"CPX", 0x0054},
        {"CPY", 0x0058},
        {"INC", 0x005C},
        {"INX", 0x0060},
        {"INY", 0x0064},
        {"DEC", 0x0068},
        {"DEX", 0x006C},
        {"DEY", 0x0070},
        {"JMP", 0x0074},
        {"JSR", 0x0078},
        {"RTS", 0x007C},
        {"BCC", 0x0080},
        {"BCS", 0x0084},
        {"BNE", 0x0088},
        {"BEQ", 0x008C},
        {"DUP", 0xFFFE},
        {"HLT", 0xFFFF}
    };

    public List<string> ParseCode = new List<string>();

    public List<ushort> code = new List<ushort>();
    List<int> uShortPerLine = new List<int>();

    //Variables
    public Dictionary<string, ushort> variables = new Dictionary<string, ushort>();
    public Dictionary<string, ushort> lables = new Dictionary<string, ushort>();
    void Start()
    {
        //store each line of the file in an array
        ParseCode = readFile(ALProgramFilePath);

        //Replace @macros with the macros
        ReplaceMacros();

        // Extract variable, lables with wrong address (line number)
        ExtractVariablesAndLables();

        //GetuShortsPerLine
        getuShortsPerLine();

        //parse the code
        parseCode();

        //sabe the code in a file
        saveCode();
    }

    List<string> readFile(string file) {
        StreamReader stm = new StreamReader(file);
        List<string> code = new List<string>();

        while(!stm.EndOfStream) {
            //read the lines and store it in parseCode
            string ln = stm.ReadLine();

            //check for comments
            string ln_temp = "";
            foreach(char _char in ln) {
                if(_char == ';') break;
                ln_temp += _char;
            }
            ln = ln_temp;

            //remove Blank spaces
            ln = ln.Trim();

            //check for blank lines
            if(ln == "") {
                continue;
            }

            code.Add(ln);
        }
        return code;
    }

    void ReplaceMacros() {
        //get all #include if available
        List<string> includedFiles = new List<string>();
        includedFiles = GetIncludedFile();
        

        //get all the macros assigned from current file and other files also
        // { "macro_name", "all_argument_seperated_with_space \n macro_body"}

        List<Macro> macros = new List<Macro>();
        macros = GetAllMacrosInCode(ParseCode);
        //remove the macros from the parseCode
        List<int> macroIndices = new List<int>();
        for(int i=0; i < ParseCode.Count; i++) {
            if(ParseCode[i][0] == '~') {
                macroIndices.Add(i);
                while(!ParseCode[i].Contains("}")) {
                    i++;
                    macroIndices.Add(i);
                }
            }
        }
        ParseCode = removeIndicesFromArray(ParseCode, macroIndices);
        
        //get macros from other files also
        foreach(string includedFile in includedFiles) {
            // print(includedFile);
            List<string> includedFileCode = readFile(includedFile);
            List<Macro> includedFileMacros = GetAllMacrosInCode(includedFileCode);
            foreach(var macro in includedFileMacros) {
                macros.Add(macro);
            }
        }

        //replace the macros - @macro_name{argument1, argument2}
        for(int i=0; i < ParseCode.Count; i++) {
            string ln = ParseCode[i];
            if(ln[0] == '@') {
                // ParseCode.RemoveAt(i);
                string macroName = ln[1..].Split('{')[0];
                if(macros.Exists(x => x.name == macroName)) {
                    string[] arguments = ln[1..].Split('{')[1].Split('}')[0].Split(',');
                    string macroBody = macros.Find(x => x.name == macroName).body;
                    string newMacroBody = macroBody;
                    for(int j=0; j < arguments.Length; j++) {
                        newMacroBody = newMacroBody.Replace(macros.Find(x => x.name == macroName).arguments[j], arguments[j].Trim());
                    }
                    string[] newMacroBodyLines = newMacroBody.Split('\n');
                    ParseCode.InsertRange(i, newMacroBodyLines);
                    ParseCode.RemoveAt(i + newMacroBodyLines.Length);
                }
            }
        }


        update_finalCode();
    }

    List<Macro> GetAllMacrosInCode(List<string> code) {
        List<Macro> macros = new List<Macro>();
        for(int i=0; i < code.Count; i++) {
            string ln = code[i];
            string[] words = ln.Split(' ');
            if(words[0][0] == '~') {
                string macroName = words[0][1..];
                string macroBody = "";

                string[] macroArguments = words[1..^1];

                while (!code[i+1].Contains("}")) {
                    i++;
                    if(code[i+1].Contains("}")) {
                        macroBody += code[i];
                        break;
                    }
                    macroBody += code[i] + "\n";
                }
                macros.Add(new Macro {
                    name = macroName,
                    arguments = macroArguments,
                    body = macroBody
                });
            }
        }
        
        return macros;
    }

    List<string> GetIncludedFile() {
        List<string> includedFiles = new List<string>();
        List<int> include_lines = new List<int>();
        for(int i=0; i < ParseCode.Count; i++) {
            string ln = ParseCode[i];
            string[] words = ln.Split(' ');
            if(words[0] == "#include") {
                string[] next = words[1..];
                string includeFile = next[0][1..^1];
                for(int j=1; j < next.Length; j++) {
                    if(next[j][next[j].Length - 1] == '"') {
                        includeFile += " " + next[j][0..^1];
                        break;
                    }
                    includeFile += " " + next[j];
                }
                //remove the file name from alprogramFilepath
                string newFilePath = ALProgramFilePath[0..^ALProgramFilePath.Split('/')[ALProgramFilePath.Split('/').Length - 1].Length];
                includedFiles.Add(newFilePath + includeFile);
                include_lines.Add(i);
            }
        }
        //remove the include lines from the parseCode
        ParseCode = removeIndicesFromArray(ParseCode, include_lines);
        return includedFiles;
    }

    void ExtractVariablesAndLables() {
        var varIndices = new List<int>();
        var lableIndices = new List<int>();
        int instructionIndex = 0;
        for(int i = 0; i < ParseCode.Count; i++) {
            string ln = ParseCode[i];

            //check for variables
            if(ln[0].ToString() == ".") {
                string[] temp = ln.Split(' ');
                string varName = temp[0][1..];
                ushort varValue = getDecimalVal(temp[2]);

                variables.Add(varName, varValue);
                varIndices.Add(i);
            }
            //check for lables
            else if(ln[ln.Length - 1].ToString() == ":") {
                string lableName = ln[0..^1];
                lables.Add(lableName, (ushort)instructionIndex);
                lableIndices.Add(i);
            }
            else {
                instructionIndex++;
            }
        }

        //remove the variables and labels from the parseCode
        varIndices = varIndices.Concat(lableIndices).ToList();
        varIndices.Sort();
        ParseCode = removeIndicesFromArray(ParseCode, varIndices);
    }

    void getuShortsPerLine() {
        ushort addIndex = 0;
        var newLables = new Dictionary<string, ushort>();
        for(int i = 0; i < ParseCode.Count; i++) {
            foreach(var lable in lables) {
                if(lable.Value == i) {
                    newLables[lable.Key] = addIndex;
                    break;
                }
            }

            string[] temp = ParseCode[i].Split(' ');
            if(temp.Length == 1) {
                uShortPerLine.Add(1);
            }
            else if(temp.Length > 1) {
                uShortPerLine.Add(2);
            }
            addIndex += (ushort)uShortPerLine[i];
        }

        lables = newLables;
    }

    void parseCode() {
        for(int i=0; i < ParseCode.Count; i++) {
            string ln = ParseCode[i];
            string[] temp = Regex.Split(ln, @" |,");
            string instruction = temp[0];
            if(temp.Length > 1) {
                if(temp[1][0].ToString() == "#") {
                    // if operand is like - #value
                    code.Add(instructions[instruction]);
                    code.Add(getDecimalVal(temp[1][1..]));
                }
                else {
                    ushort opCode = 0;
                    ushort CalAddress = 0;
                    if(temp.Length > 3) {
                        //operand include [] and registors
                        if(temp[3][temp[3].Length - 1] == ']') {
                            //operand is like - [address, X] or [address, Y]
                            if((new char[] {'X', 'x'}).Contains(temp[3].Trim()[0])) {
                                //operand is like - [address, X]
                                opCode = (ushort)(instructions[instruction] + 2);
                            }
                            else if((new char[] {'Y', 'y'}).Contains(temp[3].Trim()[0])) {
                                //operand is like - [address, Y]
                                opCode = (ushort)(instructions[instruction] + 3);
                            }
                            CalAddress = evaluate(temp[1][1..].Trim());
                        }
                        else {
                            //operand is like - [address], X
                            char registor = temp[3].Trim()[0];
                            if(instruction == "STA") {
                                if(registor == '0') opCode = 0x94;
                                else if(registor == 'X' || registor == 'x') opCode = 0x95;
                                else if(registor == 'Y' || registor == 'y') opCode = 0x96;
                                else error("Invalid Registor");
                            }
                            else if (instruction == "STX") {
                                if(registor == '0') opCode = 0x98;
                                else if(registor == 'Y' || registor == 'y') opCode = 0x99;
                                else error("Invalid Registor");
                            }
                            else if (instruction == "STY") {
                                if(registor == '0') opCode = 0x9C;
                                else if(registor == 'X' || registor == 'x') opCode = 0x9D;
                                else error("Invalid Registor");
                            }
                            else error("Invalid Instruction for indirect command");

                            CalAddress = evaluate(temp[1].Trim()[1..^1]);
                        }
                    }
                    else {
                        //operand is like - address
                        opCode = (ushort)(instructions[instruction] + 1);
                        CalAddress = evaluate(temp[1].Trim());
                    }
                    code.Add(opCode);
                    
                    if(uShortPerLine[i] < 3) {
                        code.Add(CalAddress);
                        // print(CalAddress);
                    }
                    else {
                        code.Add((ushort)(CalAddress >> 8));
                        code.Add((ushort)(CalAddress & 0xFF));
                    }
                }
            }
            else {
                code.Add(instructions[instruction]);
            }
        }
    }

    ushort evaluate(string expression) {
        /*  1. While there are still tokens to be read in,
                1.1 Get the next token.
                1.2 If the token is:
                    1.2.1 A number: push it onto the value stack.
                    1.2.2 A variable: get its value, and push onto the value stack.
                    1.2.3 A left parenthesis: push it onto the operator stack.
                    1.2.4 A right parenthesis:
                        1 While the thing on top of the operator stack is not a 
                        left parenthesis,
                            1 Pop the operator from the operator stack.
                            2 Pop the value stack twice, getting two operands.
                            3 Apply the operator to the operands, in the correct order.
                            4 Push the result onto the value stack.
                        2 Pop the left parenthesis from the operator stack, and discard it.
                    1.2.5 An operator (call it thisOp):
                        1 While the operator stack is not empty, and the top thing on the
                        operator stack has the same or greater precedence as thisOp,
                            1 Pop the operator from the operator stack.
                            2 Pop the value stack twice, getting two operands.
                            3 Apply the operator to the operands, in the correct order.
                            4 Push the result onto the value stack.
                        2 Push thisOp onto the operator stack.
            2. While the operator stack is not empty,
                1 Pop the operator from the operator stack.
                2 Pop the value stack twice, getting two operands.
                3 Apply the operator to the operands, in the correct order.
                4 Push the result onto the value stack.
            3. At this point the operator stack should be empty, and the value
            stack should have only one value in it, which is the final result. */

        // Split the expression into tockens
        string[] tokens = Regex.Split(expression, @"([+\-*/\(\)])").Where(x => x != "" && x != " ").ToArray();
        List<string> valueStack = new List<string>();
        List<string> operatorStack = new List<string>();
        foreach(string token in tokens) {
            string _token = token.Trim();
            // print(token);
            if(int.TryParse(_token, out _) || _token[0].ToString() == "$" || _token[0].ToString() == "%") {
                valueStack.Add(_token);
            }
            else if(_token == "(") {
                operatorStack.Add(_token);
            }
            else if(_token == ")") {
                while(operatorStack[operatorStack.Count - 1] != "(") {
                    valueStack.Add(addValueToStack(operatorStack, valueStack));
                }
                operatorStack.RemoveAt(operatorStack.Count - 1);
            }
            else if(@"\+\-*/".Contains(_token)) {
                while(operatorStack.Count > 0 && precedence(_token) <= precedence(operatorStack[operatorStack.Count - 1])) {
                    valueStack.Add(addValueToStack(operatorStack, valueStack));
                }
                operatorStack.Add(_token);
            }
            else {
                //it is a variable or a lable
                if(variables.ContainsKey(_token)) {
                    valueStack.Add(variables[_token].ToString());
                }
                else if(lables.ContainsKey(_token)) {
                    valueStack.Add(lables[_token].ToString());
                }
                else {
                    error("Invalid token: " + _token);
                }
            }
        }

        while(operatorStack.Count > 0) {
            valueStack.Add(addValueToStack(operatorStack, valueStack));
        }
        return getDecimalVal(valueStack[valueStack.Count - 1]);
    }

    string addValueToStack(List<string> operatorStack, List<string> valueStack) {
        string _operator = operatorStack[operatorStack.Count - 1];
        operatorStack.RemoveAt(operatorStack.Count - 1);
        ushort operand2 = getDecimalVal(valueStack[valueStack.Count - 1]);
        valueStack.RemoveAt(valueStack.Count - 1);

        ushort operand1 = getDecimalVal(valueStack[valueStack.Count - 1]);
        valueStack.RemoveAt(valueStack.Count - 1);
        
        if(_operator == "+") {
           return (Convert.ToInt32(operand1) + Convert.ToInt32(operand2)).ToString();
        }
        else if(_operator == "-") {
            return (Convert.ToInt32(operand1) - Convert.ToInt32(operand2)).ToString();
        }
        else if(_operator == "*") {
            return (Convert.ToInt32(operand1) * Convert.ToInt32(operand2)).ToString();
        }
        else if(_operator == "/") {
            return (Convert.ToInt32(operand1) / Convert.ToInt32(operand2)).ToString();
        }
        return "0";
    }

    void saveCode() {
        StreamWriter stm = new StreamWriter(ALProgramFilePath[0..^3] + "txt");
        string ln = "";
        for(int i=0; i < code.Count; i++) {
            // print(code[i]);
            string _byte = code[i].ToString("X4") + " ";
            ln += _byte;
            if((i+1) % 8 == 0 || i == code.Count - 1) {
                stm.WriteLine(ln.Trim());
                ln = "";
            }
        }
        stm.Close();

        //save the code in a binary file
        string BinaryFilePath = ALProgramFilePath[0..^3] + "bin";
        BinaryWriter bwm = new BinaryWriter(File.Open(BinaryFilePath, FileMode.Create), Encoding.UTF8);
        for(int i=0; i < code.Count; i++) {
            bwm.Write(code[i]);
        }
        bwm.Close();


        emulator.loadMem(BinaryFilePath);

    }

    int precedence(string op){
        if(op == "+" ||op == "-") return 1;
        if(op == "*" ||op == "/") return 2;
        return 0;
    }

    ushort getDecimalVal(string operand) {
        if(operand[0].ToString() == "$") {
            return Convert.ToUInt16(operand[1..], 16);
        }
        else if(operand[0].ToString() == "%") {
            return Convert.ToUInt16(operand[1..], 2);
        }
        else {
            return Convert.ToUInt16(operand);
        }
    }

    List<string> removeIndicesFromArray(List<string> ParseCode, List<int> indices) {
        foreach(int index in indices.OrderByDescending(x => x)) {
            ParseCode.RemoveAt(index);
        }
        return ParseCode;
    }

    void update_finalCode() {
        Finalcode = "";
        foreach(string ln in ParseCode) {
            Finalcode += ln + "\n";
        }
    }

    void error(string message) {
        print("Error: " + message);
    }
}
