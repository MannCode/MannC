.videoPage
.currentPage
.x
.y
.xdir
.ydir
.screenWidth = $F5
.screenHieght = $8F
.ballsize = 4

.leftTileY
.rightTileY

.PatternY

start:
	LDA #$70
	STA videoPage
	LDA #$30
	STA leftTileY
	LDA #$70
	STA rightTileY
	LDA #$7F
	STA x
	LDA #$47
	STA y

loop:
	DUP
	CLS
	; checkPatterMiddle
	JSR printCheckPattern

	; tiles
	JSR printTiles

	JSR changeRightTileY

	; ball
	JSR printball
	JSR changePos
	JMP loop

printCheckPattern:
	LDA PatternY
	CMP #$8F
	BCS printCheckPatternEnd
	@drawPoint #$7F PatternY
	ADD #2
	STA PatternY
	JMP printCheckPattern
printCheckPatternEnd:
	LDA #0
	STA PatternY
	RTS


	

printTiles:
	; left tile
	@drawRect #$5 leftTileY #6 #30
	
	; right tile
	@drawRect #$F5 rightTileY #6 #30
	RTS

printball:
	@drawRect x y #ballsize #ballsize
	RTS

changeRightTileY:
	; check bottom
	LDA y
	SUB #9
	ADD #30
	ADD #($FFFF-$8F)
	BCS edgeExceded

	; check top
	LDA y
	SUB #9
	BCS edgeExceded

	LDA y
	SUB #9
	STA rightTileY
edgeExceded:
	RTS



changePos:
	; check for start and end
	LDA x
	SUB #$B
	BEQ xdirPositive
	CMP #(screenWidth-ballsize-$B)
	BEQ xdirNegative
xdirCheckEnd:
	LDA y
	BEQ ydirPositive
	CMP #(screenHieght-ballsize)
	BEQ ydirNegative
ydirCheckEnd:
	LDA xdir
	BEQ decX
	JMP incX
changeXEnd:
	LDA ydir
	BEQ decY
	JMP incY
changeYEnd:
	RTS

xdirPositive:
	PHA
	LDA #01
	STA xdir
	PLA
	JMP xdirCheckEnd
xdirNegative:
	PHA
	LDA #00
	STA xdir
	PLA
	JMP xdirCheckEnd
ydirPositive:
	PHA
	LDA #01
	STA ydir
	PLA
	JMP ydirCheckEnd
ydirNegative:
	PHA
	LDA #00
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
	DEC y
	JMP changeYEnd


~drawRect 4 {
    .startPix

        LDA ^2
		SLA #8
		ADD ^1
		ADD #$7000
		STA startPix
    
		LDY ^4
	loadX:
        LDX ^3
    loopX:
        JSR draw
        INC startPix
        DEX
        BEQ loopY
        JMP loopX

	loopY:
		LDA startPix
		ADD #$100
		SUB ^3
		STA startPix
		DEY
		BEQ end
		JMP loadX

    draw:
        LDA #01
        STA [startPix], 0
        RTS

    end:
}

~drawPoint 2 {
		PHA
		LDA ^2
		ADD videoPage
		SLA #8
		ADD ^1
		STA currentPage
		LDA #1
		STA [currentPage], 0
		PLA
}