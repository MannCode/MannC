.whiteColor = $1002 ; 8 bit white color
.offset = $1003 ; 16 bit offset

intialize:
	LDA #$70
	STA offset
	LDA #$00
	STA (offset+1)

	LDA #$01
	STA whiteColor

loop:
	JSR start
	JSR inc
	JMP loop


start:
	LDA whiteColor
	LDX #00
	STA [offset], X
	RTS

inc:
	INC (offset+1)
	BCS incBig
	RTS

incBig:
	INC offset
	BCS reset
	DUP
	RTS

reset:
	LDA #$70
	STA offset
	LDA #$00
	STA (offset+1)

	DEC whiteColor
	BNE intialize
	RTS