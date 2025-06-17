.val
.index

    LDA #42
    STA val

start:
    LDA #$7000
    STA index
    LDX #68
    LDY #$7288

loop:
    LDA val
    STA [index], 0
    INC val
    CPX val
    BEQ reset
loopC:
    DUP
    INC index
    CPY index
    BEQ start
    JMP loop

reset:
    LDA #42
    STA val
    JMP loopC