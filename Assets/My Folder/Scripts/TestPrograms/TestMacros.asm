.x
.y
.z

loop:
    LDA 53
    ; @testmacro x y z
    LDA x
    STA $1000
    LDA y
    STA $1001
    LDA z
    STA $1002
    JSR testmacro







; ~testmacro 3 {
testmacro:
    .this
    .that
    .those
    
    Start:
        LDA $1000
        STA this
        LDA $1001
        STA that
        LDA $1002
        STA those
    RTS
}