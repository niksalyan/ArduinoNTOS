
var currentLevel = 0;
var lives = 5;
var level = byte[64];

// ---------------------------------------- 
// Title
// ----------------------------------------


cls();

// ========================================
//              BREAKOUT
// ========================================

// Outer frame
drawRoundBox(15, 15, 450, 290, 12, GREEN);

// Inner frame
drawRoundBox(22, 22, 436, 276, 8, WHITE);


// ----------------------------------------
// Title
// ----------------------------------------

cursor(240, 80, 5);
printCentered("BREAKOUT", GREEN);

cursor(240, 130, 1);
printCentered("BRICK BREAKER", WHITE);


// ----------------------------------------
// Ball
// ----------------------------------------

fillRoundBox(246, 190, 9, 9, 4, WHITE);

fillRoundBox(232, 193, 6, 6, 3, YELLOW);
fillRoundBox(221, 196, 4, 4, 2, ORANGE);


// ----------------------------------------
// Paddle
// ----------------------------------------

fillRoundBox(185, 220, 110, 10, 5, GREEN);


// ----------------------------------------
// Start button
// ----------------------------------------

//fillRoundBox(115, 250, 250, 45, 10, GREEN);

cursor(240, 258, 2);
printCentered("PRESS ANY KEY", WHITE);

cursor(240, 280, 1);
printCentered("TO START", WHITE);

function loop() {
	

	var key = getKey();
	if (key > 0) {
		lives = 5;
		load("level1");
	}
	delay(1);
}



