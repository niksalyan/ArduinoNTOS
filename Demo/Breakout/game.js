
let playerX = 240.0;
let ballX = 240.0;
let ballY = 300.0;
let ballSX = 1.0;
let ballSY = -15.0;
let bricksLeft = 0;




// dialog("Breakout");


function drawLevel() {
	bricksLeft = 0;
	cls();
	for (var y = 0; y < 8; y++) {
		for (var x = 0; x < 8; x++) {
			var i = x + y * 8;
			if (level[i] != EMPTY) {
				fillBox(2 + x * 60, 30 + y * 20, 56, 16, level[i]);
				bricksLeft++;
			}
		}
	}
}

function updateBall() {

	fillBox(ballX - 5.0, ballY - 5.0, 11,11, BLACK);
	ballX += ballSX;
	ballY += ballSY;
	fillCircle(ballX, ballY, 5.0, YELLOW);

	if (ballX > 470 && ballSX > 0) {ballSX = -ballSX;}
	if (ballX < 10 && ballSX < 0) {ballSX = -ballSX;}
	
	if (ballY > 310 && ballSY > 0) {load("lives");}
	if (ballY < 10 && ballSY < 0) {ballSY = -ballSY;}

	if (collision(ballX, ballY, 9, 9, playerX, 305, 100, 10) && ballSY > 0) {
		ballSY = -ballSY;
		ballSX = (ballX - playerX) / 5.0;
	}

	for (var y = 0; y < 8; y++) {
		for (var x = 0; x < 8; x++) {
			var i = x + y * 8;
			if (level[i] != EMPTY) {
				if (collision(ballX, ballY, 9, 9, 30 + x * 60, 40 + y * 20, 56, 16)) {
					fillBox(2 + x * 60, 30 + y * 20, 56, 16, BLACK);
					level[i] = EMPTY;
					bricksLeft--;
					// ballSX = -ballSX;
					ballSY = -ballSY;
				}
			}
		}
	}

	
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
	
	fillBox(playerX - 40, 300, 80, 10, GRAY);
	fillBox(playerX - 50, 300, 10, 10, DARKRED);
	fillBox(playerX + 40, 300, 10, 10, DARKRED);
	fillBox(playerX - 60, 300, 10, 10, BLACK);
	fillBox(playerX + 50, 300, 10, 10, BLACK);
}

drawLevel();

function loop() {
	updateBall();
    updatePlayer();
	if (bricksLeft <= 0) {
		switch(currentLevel) {
			case 1:
				load("level2");
				break;
			case 2:
				load("level3");
				break;
			case 3:
				load("level4");
				break;
			case 4:
				load("level5");
				break;
			default:
				load("main");
				break;
		}
	}
    sync(30);
}