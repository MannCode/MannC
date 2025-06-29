.line
.col
.index
.indexStart = $7000
.actual_index
.line_length = 36
.pix_length = 648
.val

.cursor_val = 4
.show_cursor
.cursor_wait

.return_key = $FE
.backspace_key = $FF

start:
    LDA #0
    STA val
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
    LDA index
    ADD #(indexStart)
    STA actual_index
    LDA ^1
    STA [actual_index], 0
    DUP
}

new_line:
.mod_val
    PHA
    @mod index #(line_length)
    STA mod_val
    LDA #(line_length)
    SUB mod_val
    ADD index
    STA index
    CMP #(pix_length)
    BNE check_pix_end
    LDA #0
    STA index
    CLS
check_pix_end:
    PLA
    RTS

~mod 2 {
; A, B
    LDA ^1
mod_loop:
    CMP ^2
    BCC break_mod
    SUB ^2
    JMP mod_loop
break_mod:
}

return:
    @set_char #0
    JSR new_line
    JMP intrupt_end

backspace:
    @set_char #0
    DEC index
    @set_char #0
    JMP intrupt_end

intrupt:
    LDA $6F00
    STA val
    TXA
    PHA
    LDA val
    CMP #(return_key)      ; #FE
    BEQ return
    CMP #(backspace_key)   ; #FF
    BEQ backspace
    @set_char val
    INC index
intrupt_end:
    PLA
    TAX
    RTS

` $6DF0
    JMP intrupt