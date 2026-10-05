
let currentLevel = 0;
let lives = 5;
let level = byte[64];

// ---------------------------------------- 
// Title
// ----------------------------------------

cls();
// ========================================
//              BREAKOUT
// ========================================



// ----------------------------------------
// Title
// ----------------------------------------

cursor(240, 60, 5);


drawSprite(918, 150, 70, C730);
delay(100);
drawSprite(970, 171, 70, C700);
delay(100);
drawSprite(921, 192, 70, C730);
delay(100);
drawSprite(917, 213, 70, C700);
delay(100);
drawSprite(927, 234, 70, C730);
delay(100);
drawSprite(967, 255, 70, C700);
delay(100);
drawSprite(973, 276, 70, C730);
delay(100);
drawSprite(972, 297, 70, C700);
delay(100);


// Outer frame
drawRoundBox(15, 15, 450, 290, 12, C730);

// Inner frame
drawRoundBox(22, 22, 436, 276, 8, WHITE);



cursor(240, 110, 1);
printCentered("BRICK BREAKER", WHITE);


// ----------------------------------------
// Ball
// ----------------------------------------

fillRoundBox(246, 140, 9, 9, 4, WHITE);

fillRoundBox(232, 143, 6, 6, 3, YELLOW);
fillRoundBox(221, 146, 4, 4, 2, ORANGE);


// ----------------------------------------
// Paddle
// ----------------------------------------

fillRoundBox(185, 170, 110, 10, 5, DARKGRAY);


// ----------------------------------------
// Start button
// ----------------------------------------

//fillRoundBox(115, 250, 250, 45, 10, GREEN);

cursor(240, 230, 1);
printCentered("PRESS ANY KEY", WHITE);

cursor(240, 250, 1);
printCentered("TO START", WHITE);

function loop() {
	

	var key = getKey();
	if (key > 0) {
		lives = 5;
		load("level1");
	}
	delay(1);
}



