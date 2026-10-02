let playerX = 240.0;
let ballX = 240.0;
let ballY = 300.0;
let ballSX = 10.0;
let ballSY = -10.0;

let level = [
		EMPTY, EMPTY, RED,    RED,    RED,    RED,    EMPTY, EMPTY,
		EMPTY, RED,   RED,    ORANGE, ORANGE, RED,   RED,    EMPTY,
		RED,   RED,   ORANGE, YELLOW, YELLOW, ORANGE, RED,   RED,
		RED,   ORANGE,YELLOW, GREEN,  GREEN,  YELLOW,ORANGE, RED,
		RED,   ORANGE,YELLOW, GREEN,  GREEN,  YELLOW,ORANGE, RED,
		RED,   RED,   ORANGE, YELLOW, YELLOW, ORANGE, RED,   RED,
		EMPTY, RED,   RED,    ORANGE, ORANGE, RED,   RED,    EMPTY,
		EMPTY, EMPTY, RED,    RED,    RED,    RED,    EMPTY, EMPTY,
];


// dialog("Breakout");
cls();

// for (var y = 0; y < 10; y++){
// 	for (var x = 0; x < 15; x++) {
// 		drawSprite((x + y * 15) + 900, x * 32, y * 32, WHITE);
// 	}
//}

function drawLevel() {
	for (var y = 0; y < 8; y++) {
		for (var x = 0; x < 8; x++) {
			var i = x + y * 8;
			fillBox(2 + x * 60, 30 + y * 20, 56, 16, level[i]);
		}
	}
}

function updateBall() {

	fillBox(ballX - 5.0, ballY - 5.0, 11,11, BLACK);
	ballX += ballSX;
	ballY += ballSY;

	if (ballX > 470 && ballSX > 0) {ballSX = -ballSX;}
	if (ballX < 10 && ballSX < 0) {ballSX = -ballSX;}
	
	if (ballY > 310 && ballSY > 0) {ballSY = -ballSY;}
	if (ballY < 10 && ballSY < 0) {ballSY = -ballSY;}

	fillCircle(ballX, ballY, 5.0, YELLOW);
}


function updatePlayer() {

	if (getKeyPressed('4')) {
		playerX -= 10.0;
	}

	if (getKeyPressed('6')) {
		playerX += 10.0;
	}

	if (playerX < 50.0) {
		playerX = 50.0;
	}

	if (playerX > 430.0) {
		playerX = 430.0;
	}
	
	fillBox(playerX - 50, 300, 100, 10, GREEN);
	fillBox(playerX - 60, 300, 10, 10, BLACK);
	fillBox(playerX + 50, 300, 10, 10, BLACK);
}

drawLevel();

function loop() {
	updateBall();
    updatePlayer();
    delay(33);
}
