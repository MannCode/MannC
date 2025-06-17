.whiteColor
.offset

intialize:
	LDA #$7240
	STA offset

	LDA #$0001
	STA whiteColor

loop: ; 8
	JSR start
	JSR inc
	JMP loop


start: ; 14
	LDA whiteColor
	STA [offset], 0
	RTS

inc: ; 19
	INC offset
	BCS reset
	RTS

reset: ; 24
	LDA #$7000
	STA offset
	DUP
	DEC whiteColor
	BNE intialize
	RTS