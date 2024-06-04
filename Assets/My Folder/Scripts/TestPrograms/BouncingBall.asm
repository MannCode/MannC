.videopage = 1000 ;8 bit
.currentPage = 1001 ;16 bit
.x = 1003 ; 8 bit
.y = 1004 ; 8 bit
.xdir = 1005 ; 8 bit
.ydir = 1006 8 bit
.screenWidth = 1007;
.screenHieght = 1008;
	

start:
	LDA #$70
	STA videopage
	LDA #$00
	STA [currentpage+1]
	LDA #$FF
	STA screenWidth
	LDA #$8F
	STA screenHieght
	
loop:
	JSR changePos
	JSR print
	JMP loop

print:
	LDA videopage
	ADD y
	STA currentpage
	LDY #01
	LDX x
	STY (currentpage), X
	DUP
	RTS
	
changePos:
	LDA x
	CMP screenWidth
	BEQ XdirNegative
	CMP #00
	BEQ XdirPostive
.xdirCheckEnd
	LDA y
	CMP screenHieght
	BEQ YdirNegative
	CMP #00
	BEQ YdirPostive

	LDA xdir
	CMP #01
	BEQ incX
	CMP #02
	BEQ decX
.changeXEnd
	LDA ydir
	CMP #01
	BEQ incY
	CMP #02
	BEQ decY
.changeYEnd
	RTS

xdirNegative:
	PHA
	LDA #02
	STA xdir
	PLA
	JMP xdirCheckEnd
xdirPositive:
	PHA
	LDA #01
	STA xdir
	PLA
	JMP xdirCheckEnd
YdirNegative:
	PHA
	LDA #02
	STA ydir
	PLA
	JMP ydirCheckEnd
YdirPositive:
	PHA
	LDA #01
	STA ydir
	PLA
	JMP ydirCheckEnd
incX:
	INC x
	JMP changeXEnd
decX:
	DEC x
	JMP changeXEnd
incY:
	INC y
	JMP changeYEnd
decY:
	INC Y
	JMP changeYEnd