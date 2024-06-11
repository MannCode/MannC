.whiteColor = $1002
.offset = $1003

intialize:
	LDA #$7000
	STA offset

	LDA #$0001
	STA whiteColor

loop:
	JSR start
	JSR inc
	JMP loop


start:
	LDA whiteColor
	STA [offset], 0
	RTS

inc:
	INC offset
	BCS reset
	RTS

reset:
	LDA #$7000
	STA offset
	DUP
	DEC whiteColor
	BNE intialize
	RTS