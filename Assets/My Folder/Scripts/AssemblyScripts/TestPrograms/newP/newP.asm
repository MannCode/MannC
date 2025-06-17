#include "myMacros.asm"

.maxX = 256
.maxY = 144


    @drawRect{#100, #100, #20, #50}
    ;@drawRect{#200, #100, #10, #50}
    HLT

~drawRect %xstart %ystart %xsize %ysize {
    .startPix
    .White = 0001
    .offset

        LDX %ystart
    addY:
        BEQ addX
        LDA maxX
        ADD startPix
        STA startPix
        DEX
        JMP addY
    addX:
        LDA startPix
        ADD %xstart
        STA startPix ; at this point we have our start pixel

    setOffset:
        LDA #$7000
        ADD startPix
        STA offset
    
        LDX %xsize
    loopX:
        JSR draw
        INC offset
        DEX
        BEQ end
        JMP loopX

    draw:
        LDA White
        STA [offset], 0
        RTS

    end:
        DUP
}

~drawPoint %xpoint %ypoint {
    LDA #7000
    ADD %xpoint
    STA 
}