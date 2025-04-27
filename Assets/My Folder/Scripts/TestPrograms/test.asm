.val
.intruptAdd = $3000
.index

Start:
    LDA #42
    STA val
    LDA #$7000
    STA index

loop:
    JMP loop


` #intruptAdd
    LDA val
    STA [index], 0
    INC index
    DUP
    RTS 