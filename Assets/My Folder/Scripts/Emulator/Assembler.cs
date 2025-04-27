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
using UnityEngine.UIElements;

public class Assembler : MonoBehaviour
{

    struct Macro
    {
        public string name;
        public int num_arguments;
        // public string body;
    }


    public struct Variable {
        public int line;
        public string name;
        public string value;

        public Variable(int Line, string Name, string Value) {
            line = Line;
            name = Name;
            value = Value;
        }

        public void setValue(string _value) {
            value = _value;
        }

        public string getValue() {
            return value;
        }
    }

    public Emulator emulator;
    public string ALProgramFilePath;

    string binaryFilePath;

    public string Finalcode;

    // Properties of instructions
    Dictionary<string, ushort> instructions = new Dictionary<string, ushort> {
        {"NOP", 0x0000},
        {"LDA", 0x0004},
        {"LDX", 0x0008},
        {"LDY", 0x000C},
        {"STA", 0x0010},
        {"STX", 0x0014},
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
        {"SLA", 0x00A0},
        {"SRA", 0x00A4},
        {"CLS", 0xFFFD},
        {"DUP", 0xFFFE},
        {"HLT", 0xFFFF}
    };

    public List<string> ParseCode = new List<string>();

    public List<ushort> code = new List<ushort>();
    List<int> uShortPerLine = new List<int>();

    //Variables
    public List<Variable> variables = new List<Variable>();
    public Dictionary<string, ushort> lables = new Dictionary<string, ushort>();

    List<Macro> macros = new List<Macro>();
    void Start()
    {
        //store each line of the file in an array
        ParseCode = readFile(ALProgramFilePath);

        
        //Replace ~macros like actual code
        ParseCode = ReplaceMacrosFunctions(ParseCode);
        ReplaceMacrosDecleration();

        exportParseCode();


        // Extract variable, lables with wrong address (line number)
        ExtractVariablesAndLables();

        //GetuShortsPerLine
        getuShortsPerLine();
    
        update_finalCode();
        // //parse the code
        parseCode();

        // //sabe the code in a file
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

    List<string> ReplaceMacrosFunctions(List<string> code) {
        List<string> newCode = code;
        // bool ifMacroFoundInMiddle = false;
        for(int i=0; i < code.Count; i++) {
            string ln = code[i];
            string[] words = ln.Split(' ');
            if(words[0][0] == '~') {
                //its a macro function
                string macroName = words[0][1..];
                int macroArguments = int.Parse(words[1]);

                macros.Add(new Macro {
                    name = macroName,
                    num_arguments = macroArguments,
                });

                //manuplate the code
                newCode[i] = macroName + ":";

                ushort startArgumentAddress = 0x1FCE; // 0x2000 - 50 (for max of 50 arguments)
                // int temp_i = i;
                while (!code[i+1].Contains("}")) {
                    i++;
                    ln = code[i];
                    words = ln.Split(' ');
                    if(ln.Contains("^")) {
                        newCode[i] = words[0] + " $" + (startArgumentAddress + int.Parse(words[1][1..]) - 1).ToString("X4");
                    }
                }

                i++;
                newCode[i] = "RTS";
            }
        }
        return newCode;
    }

    void ReplaceMacrosDecleration() {
        // List<string> newcode = ParseCode;
        for(int i=0; i < ParseCode.Count; i++) {
            string ln = ParseCode[i];
            string[] words = ln.Split(' ');
            
            //check for macros
            if(ln[0] == '@') {
                bool isMacroFound = false;
                foreach (Macro macro in macros)
                {
                    if(macro.name == words[0][1..]) {
                        if(macro.num_arguments == words.Length - 1) {
                            ParseCode.RemoveAt(i);
                            string[] arguments = words[1..];

                            for(int j=0; j < arguments.Length; j++) {
                                ParseCode.Insert(i, "LDA " + arguments[j]);
                                ParseCode.Insert(i+1, "STA $" + (0x1FCE + j).ToString("X4"));
                                i+=2;
                            }


                            isMacroFound = true;
                            break;
                        }
                    }   
                }
                if(!isMacroFound) {
                    error("Macro not found: " + words[0][1..]);
                    error("There could be a problem with the number of arguments");
                }
                else {
                    ParseCode.Insert(i, "JSR " + words[0][1..]);
                }
            }
        }
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
        var lableIndices = new List<int>();
        int instructionIndex = 0;
        var variableCount = 0;
        var variableStartindex = 8192;
        for(int i = 0; i < ParseCode.Count; i++) {
            string ln = ParseCode[i];
            //check for variables
            if(ln[0].ToString() == ".") {
                string[] temp = ln.Split(' ');
                string varName = temp[0][1..];
                string varValue = "";
                if(temp.Count() > 1) {
                    varValue = getDecimalVal(temp[2]).ToString();
                }
                else {
                    varValue = (variableCount + variableStartindex).ToString();
                    variableCount++;
                }
                Variable var = new Variable(i, varName, varValue);
                variables.Add(var);
                instructionIndex++;
                // ParseCode.RemoveAt(i);
                // i--;
            }
            //check for lables
            else if(ln[ln.Length - 1].ToString() == ":") {
                string lableName = ln[0..^1];

                // print(lableName + " " + instructionIndex);
                lables.Add(lableName, (ushort)instructionIndex);
                lableIndices.Add(i);
            }
            else {
                instructionIndex++;
            }
        }
        
        //remove the variables and labels from the parseCode
        lableIndices.Sort();
        ParseCode = removeIndicesFromArray(ParseCode, lableIndices);
    }

    void getuShortsPerLine() {
        ushort addIndex = 0;
        var newLables = new Dictionary<string, ushort>();
        // var index = 0;
        for(int i = 0; i < ParseCode.Count; i++) {
            foreach(var lable in lables) {
                if(lable.Value == i) {
                    newLables[lable.Key] = addIndex;
                    break;
                }
            }

            string[] temp = ParseCode[i].Split(' ');
            if(temp[0][0] == '.') {
                uShortPerLine.Add(0);
            }
            else if(temp.Length == 1) {
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
            if(temp[0][0] == '.') {
                    continue;
            }

            if(temp[0][0] == '`') {
                ushort address = 0;
                if(temp[1][0].ToString() == "#") {
                    address = evaluate(temp[1][1..], i).Item1;
                } else {
                    address = evaluate(temp[1], i).Item1;
                }
                int currentCodeIndex = code.Count;
                for(int j=0; j < address-currentCodeIndex; j++) {
                    code.Add(0);
                }

                continue;
            }

            if(temp.Length > 1) {
                if(temp[1][0].ToString() == "#") {
                    // if operand is like - #value
                    code.Add(instructions[instruction]);
                    // code.Add(getDecimalVal(temp[1][1..]));
                    code.Add(evaluate(temp[1][1..], i).Item1);
                }
                else {
                    ushort opCode = 0;
                    ushort CalAddress;
                    if (temp.Length > 3) {
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
                            CalAddress = evaluate(temp[1][1..].Trim(), i).Item1;
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

                            CalAddress = evaluate(temp[1].Trim()[1..^1], i).Item1;
                        }
                    }
                    else {
                        //operand is like - address or a single variable
                        Tuple<ushort, bool> result = evaluate(temp[1].Trim(), i);
                        CalAddress = result.Item1;
                        if(result.Item2) {
                            //if the variable is a constant
                            opCode = instructions[instruction];
                        }
                        else {
                            opCode = (ushort)(instructions[instruction] + 1);
                        }
                    }
                    code.Add(opCode);
                    
                    if(uShortPerLine[i] < 3) {
                        code.Add(CalAddress);
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

    Tuple<ushort, bool> evaluate(string expression, int i) {

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
        bool isConstantVariable = false;
        List<string> operatorStack = new List<string>();
        foreach(string token in tokens) {
            string _token = token.Trim();
            if(int.TryParse(_token, out _) || _token[0].ToString() == "$" || _token[0].ToString() == "^") {
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
                string nearestValue = "";
                int currentDist = i;
                foreach(var variable in variables) {
                    if(variable.name == _token) {
                        if(i - variable.line <= currentDist) {
                            
                            nearestValue = variable.value;
                            currentDist = i - variable.line;
                        }
                    }
                }
                // print(_token);
                // print(matchedVar);
                if(nearestValue != "") {
                    if(nearestValue[0] == '#') {
                        isConstantVariable = true;
                    }
                    valueStack.Add(nearestValue);
                }
                else if(lables.ContainsKey(_token)) {
                    // print("Lable: " + _token);
                    // print("Value: " + lables[_token]);
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
        string value = valueStack[valueStack.Count - 1];
        if(value[0].ToString() == "#") {
            return new Tuple<ushort, bool>(getDecimalVal(value[1..]), isConstantVariable);
        }
        else {
            return new Tuple<ushort, bool>(getDecimalVal(value), isConstantVariable);
        }
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
            // string _byte = "";
            // if(i < 512) _byte = "0000 ";
            // else _byte = code[i-512].ToString("X4") + " ";
            string _byte = code[i].ToString("X4") + " ";
            
            ln += _byte;
            if((i+1) % 8 == 0 || i == code.Count - 1) {
                stm.WriteLine(ln.Trim());
                ln = "";
            }
        }
        stm.Close();

        //save the code in a binary file
        binaryFilePath = ALProgramFilePath[0..^3] + "bin";
        BinaryWriter bwm = new BinaryWriter(File.Open(binaryFilePath, FileMode.Create), Encoding.UTF8);
        for(int i=0; i < code.Count; i++) {
            bwm.Write(code[i]);
        }
        bwm.Close();

        emulator.loadMem(binaryFilePath);

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
        else if(operand[0].ToString() == "^") {
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

    void exportParseCode() {
        StreamWriter stm = new StreamWriter("Assets/My Folder/Scripts/Emulator/ParseCode.txt");
        stm.AutoFlush = true;
        ushort addIndex = 0;
        ushort varindex = 0x2000;
        foreach(string ln in ParseCode) {
            if(ln[0] == '.' || ln[ln.Length-1] == ':') {
                if(ln[0] == '.' && !ln.Contains("=")) {
                    string varName = ln[1..];
                    stm.WriteLine(ln + "   " + varindex.ToString("X4"));
                    varindex++;
                }
                else {
                    stm.WriteLine(ln);
                }
            }
            else {
                stm.WriteLine("    " + addIndex.ToString("X4") + "  " + ln);
                if(ln.Split(' ').Length == 1) addIndex++;
                else addIndex += 2;
            }
        }
    }
}
