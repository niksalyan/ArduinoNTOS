let playerX = 240.0;
let ballX = 240.0;
let ballY = 300.0;
let ballSX = 15.0;
let ballSY = -15.0;

// dialog("Breakout");
cls();

// for (var y = 0; y < 10; y++){
// 	for (var x = 0; x < 15; x++) {
// 		drawSprite((x + y * 15) + 900, x * 32, y * 32, WHITE);
// 	}
//}

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

function loop() {
	updateBall();
    updatePlayer();
    delay(33);
}
