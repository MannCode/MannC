.line
.col
.indexStart
.line_length = 36
.val

start:
    LDA #42
    STA val
    LDA #$7000
    STA indexStart

    @add_char #51
    @add_char #42
    @add_char #66
    @add_char #42
    @add_char #55
    @add_char #60
    @add_char #49
    @add_char #0
    @add_char #50
    @add_char #60
    @add_char #0
    @add_char #42
    @add_char #0
    @add_char #47
    @add_char #62
    @add_char #44
    @add_char #52
    @add_char #50
    @add_char #55
    @add_char #48
    @add_char #0
    @add_char #57
    @add_char #50
    @add_char #46
    @add_char #44
    @add_char #46
    @add_char #0
    @add_char #56
    @add_char #47
    @add_char #0
    @add_char #60
    @add_char #49
    @add_char #50
    @add_char #61
    HLT

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
    DUP
    LDA col
    SUB #(line_length)
    BEQ else
    INC col
    JMP end
else:
    JSR new_line
end:
}

new_line:
    LDA #0
    STA col
    INC line
    RTS



~mul 2 {
; A, B
    LDA #0
loop_mul:
    LDX ^2
    BEQ end_mul
    ADD ^1
    DEC ^2
    JMP loop_mul
end_mul:
}