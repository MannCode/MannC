.val
.index
.intruptAdd = $3000


    LDA #68
    STA val

start:
    LDA #$7000
    STA index

loop:
    @PrintChar #49
    @PrintChar #46
    @PrintChar #53
    @PrintChar #53
    @PrintChar #56
    @PrintChar #0
    @PrintChar #64
    @PrintChar #56
    @PrintChar #59
    @PrintChar #53
    @PrintChar #45
    HLT

~PrintChar 1 {
    LDA ^1
    STA [index], 0
    INC index
    DUP
}