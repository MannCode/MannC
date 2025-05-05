.line
.col
.indexStart
.line_length = 36
.val

start:
    LDA #20
    STA val
    LDA #$7000
    STA indexStart

loop:
    @add_char val
    LDX line
    CPX #18
    BEQ reset_line
    JMP loop

reset_line:
    LDA #0
    STA line
    INC val
    LDA val
    CMP #41
    BEQ start
    CLS
    JMP loop

~add_char 1 {
.index
    LDA ^1
    PHA
    @mul #(line_length) line
    ADD col
    ADD indexStart
    STA index
    PLA
    STA [index], 0
    LDA col
    SUB #(line_length)
    BEQ else
    INC col
    DUP
    JMP end
else:
    JSR new_line
end:
}

new_line:
    INC line
    LDA #0
    STA col
    RTS

~mul 2 {
; A, B
    LDA #0
loop_mul:
    ADD ^1
    LDX ^2
    BEQ end_mul
    DEC ^2
    JMP loop_mul
end_mul:
}

intrupt:
    PHA
    LDA $1FCF
    PHA
    @add_char #3
    PLA
    STA $1FCF
    PLA
    RTS

` $3000
    JMP intrupt