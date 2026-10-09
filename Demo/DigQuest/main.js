
using level = byte[150];

const COIN = 0x01;
const TREE = 0x02;
const PLATFORM = 0x03;
const DOOR = 0x04;
const ENEMY = 0x05;
const LADDER = 0x06;
const PLAYER = 0x07;
const GROUND = 0x08;
const WATER = 0x09;

let lives = 5;
let currentLevel = 1;

cls();

// ========================================
//              DIG QUEST
// ========================================

// ----------------------------------------
// Outer frame
// ----------------------------------------

drawRoundBox(15, 15, 450, 290, 12, C730);
drawRoundBox(22, 22, 436, 276, 8, WHITE);

// ----------------------------------------
// Title: DIG QUEST
// ----------------------------------------

cursor(240, 65, 5);

drawSprite(54, 191, 55, GREEN, NONE);
delay(100);
drawSprite(218, 219, 55, YELLOW, NONE);
delay(100);
drawSprite(54, 247, 55, GREEN, NONE);
delay(100);

cursor(240, 105, 3);
printCentered("DIG QUEST", YELLOW);

// ----------------------------------------
// Underground layers
// ----------------------------------------

fillBox(40, 35, 400, 3, C730);
fillBox(40, 138, 400, 3, C300);
fillBox(40, 141, 400, 3, C700);

// Dirt blocks
fillBox(65, 165, 60, 28, C300);
fillBox(135, 165, 60, 28, C730);
fillBox(205, 165, 60, 28, C300);
fillBox(275, 165, 60, 28, C730);
fillBox(345, 165, 60, 28, C300);

// ----------------------------------------
// Coins and treasure
// ----------------------------------------

drawSprite(218, 76, 151, YELLOW, NONE);
drawSprite(218, 146, 151, YELLOW, NONE);
drawSprite(218, 216, 151, YELLOW, NONE);
drawSprite(218, 286, 151, YELLOW, NONE);
drawSprite(218, 356, 151, YELLOW, NONE);

// Treasure chest
drawRoundBox(207, 205, 66, 35, 5, C700);
fillRoundBox(207, 205, 66, 12, 3, YELLOW);
fillBox(235, 205, 7, 35, C300);
fillRoundBox(232, 215, 13, 12, 3, WHITE);



// ----------------------------------------
// Start prompt
// ----------------------------------------

cursor(240, 260, 1);
printCentered("PRESS ANY KEY", WHITE);

cursor(240, 280, 1);
printCentered("TO START", YELLOW);


function loop() {
    // This function is called every frame
    var key = getKey();
    if (key > 0) {
		currentLevel = 1;
		lives = 5;
		load("lives");
	}
    delay(1);
}
