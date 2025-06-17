.line
.col
.indexStart
.line_length = 36
.val

.cursor_val = 4
.show_cursor
.cursor_wait

.return_key = 72
.backspace_key = 73

start:
    LDA #0
    STA val
    LDA #$7000
    STA indexStart
    LDA #10000
    STA cursor_wait

loop:
    LDX cursor_wait
cursor_loop:
    DEX
    BNE cursor_loop     ; loop until cursor wait is 0

    LDA show_cursor
    BEQ set_cursor_one
    DEC show_cursor
    @set_char #0
    JMP set_cursor_end
set_cursor_one:
    INC show_cursor
    @set_char #4
set_cursor_end:
    JMP loop

~set_char 1 {  ; Set char on the current position of cursor
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
}

inc_cursor:    ; Increament the cursor
    LDA col
    SUB #(line_length)
    BEQ else
    INC col  
    JMP end
else:
    JSR new_line
end:
    RTS

new_line:
    INC line
    LDA #0
    STA col
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

return:
    @set_char #0
    JSR new_line
    JMP intrupt_end
backspace:
    @set_char #0
    DEC col
    @set_char #0
    JMP intrupt_end

intrupt:
    PLA
    STA val
    TXA
    PHA
    LDA val
    CMP #(return_key)      ; #72
    BEQ return
    CMP #(backspace_key)   ; #73
    BEQ backspace
    @set_char val
    JSR inc_cursor
intrupt_end:
    PLA
    TAX
    RTS

` $6DF0
    JMP intrupt