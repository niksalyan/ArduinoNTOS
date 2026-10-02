
let playerX = 240.0;
let oldPlayerX = 240.0;

let ballX = 240.0;
let ballY = 300.0;
let oldBallX = 240.0;
let oldBallY = 300.0;

let ballSX = 5.0;
let ballSY = -5.0;

let score = 0;
let lives = 3;
let bricksLeft = 0;

let gameOver = false;
let levelComplete = false;

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


// ------------------------------------------------------------
// Level
// ------------------------------------------------------------

function drawLevel() {

	bricksLeft = 0;

	for (var y = 0; y < 8; y++) {

		for (var x = 0; x < 8; x++) {

			var index = x + y * 8;
			var color = level[index];

			if (color != EMPTY) {

				fillBox(
					2 + x * 60,
					30 + y * 20,
					56,
					16,
					color
				);

				bricksLeft++;
			}
		}
	}
}


// ------------------------------------------------------------
// HUD
// ------------------------------------------------------------

function drawScore() {

	// Small fixed area containing the score.
	// Only this area is erased and redrawn.

	fillBox(0, 0, 180, 25, BLACK);

	cursor(10, 4, 2);
	print("Score: ", WHITE);
	print(score, WHITE);
}


function drawLives() {

	fillBox(350, 0, 130, 25, BLACK);

	cursor(355, 4, 2);
	print("Lives: ", WHITE);
	print(lives, WHITE);
}


function drawHud() {

	drawScore();
	drawLives();
}


// ------------------------------------------------------------
// Paddle
// ------------------------------------------------------------

function drawPlayer() {

	fillBox(
		playerX - 50,
		300,
		100,
		10,
		GREEN
	);
}


function updatePlayer() {

	var previousX = playerX;


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


	if (playerX == previousX) {
		return;
	}


	// Only erase the strip that disappeared.
	if (playerX > previousX) {

		fillBox(
			previousX - 50,
			300,
			playerX - previousX,
			10,
			BLACK
		);

		// Draw only the new strip.
		fillBox(
			previousX + 50,
			300,
			playerX - previousX,
			10,
			GREEN
		);

	}
	else {

		fillBox(
			playerX + 50,
			300,
			previousX - playerX,
			10,
			BLACK
		);

		fillBox(
			playerX - 50,
			300,
			previousX - playerX,
			10,
			GREEN
		);
	}
}


// ------------------------------------------------------------
// Ball
// ------------------------------------------------------------

function eraseBall() {

	fillBox(
		oldBallX - 6,
		oldBallY - 6,
		13,
		13,
		BLACK
	);
}


function drawBall() {

	fillCircle(
		ballX,
		ballY,
		5.0,
		YELLOW
	);
}


// ------------------------------------------------------------
// Brick collision
// ------------------------------------------------------------

function checkBrickCollision() {

	var ballLeft = ballX - 5.0;
	var ballRight = ballX + 5.0;

	var ballTop = ballY - 5.0;
	var ballBottom = ballY + 5.0;


	for (var y = 0; y < 8; y++) {

		for (var x = 0; x < 8; x++) {

			var index = x + y * 8;

			if (level[index] == EMPTY) {
				continue;
			}


			var brickLeft = 2 + x * 60;
			var brickRight = brickLeft + 56;

			var brickTop = 30 + y * 20;
			var brickBottom = brickTop + 16;


			if (
				ballRight >= brickLeft &&
				ballLeft <= brickRight &&
				ballBottom >= brickTop &&
				ballTop <= brickBottom
			) {

				// Remove brick from game state.
				level[index] = EMPTY;

				bricksLeft--;
				score += 10;


				// Remove only this brick from the display.
				fillBox(
					brickLeft,
					brickTop,
					56,
					16,
					BLACK
				);


				// Determine whether we hit the
				// horizontal or vertical side.

				var overlapLeft =
					ballRight - brickLeft;

				var overlapRight =
					brickRight - ballLeft;

				var overlapTop =
					ballBottom - brickTop;

				var overlapBottom =
					brickBottom - ballTop;


				var minX = overlapLeft;

				if (overlapRight < minX) {
					minX = overlapRight;
				}


				var minY = overlapTop;

				if (overlapBottom < minY) {
					minY = overlapBottom;
				}


				if (minX < minY) {
					ballSX = -ballSX;
				}
				else {
					ballSY = -ballSY;
				}


				drawScore();


				if (bricksLeft <= 0) {
					levelComplete = true;
				}

				return;
			}
		}
	}
}


// ------------------------------------------------------------
// Paddle collision
// ------------------------------------------------------------

function checkPaddleCollision() {

	if (ballSY <= 0) {
		return;
	}


	var paddleLeft = playerX - 50.0;
	var paddleRight = playerX + 50.0;

	var paddleTop = 300.0;


	if (
		ballX + 5.0 >= paddleLeft &&
		ballX - 5.0 <= paddleRight &&
		ballY + 5.0 >= paddleTop &&
		ballY - 5.0 <= paddleTop + 10.0
	) {

		ballY = paddleTop - 6.0;

		ballSY = -ballSY;


		// Deflect according to where the ball
		// hit the paddle.

		var hit = ballX - playerX;


		if (hit < -30.0) {
			ballSX = -5.0;
		}
		else if (hit < -10.0) {
			ballSX = -3.0;
		}
		else if (hit > 30.0) {
			ballSX = 5.0;
		}
		else if (hit > 10.0) {
			ballSX = 3.0;
		}
		else {
			ballSX = 0.0;
		}
	}
}


// ------------------------------------------------------------
// Ball update
// ------------------------------------------------------------

function updateBall() {

	eraseBall();


	oldBallX = ballX;
	oldBallY = ballY;


	ballX += ballSX;
	ballY += ballSY;


	// Left wall

	if (ballX < 7.0 && ballSX < 0) {

		ballX = 7.0;
		ballSX = -ballSX;
	}


	// Right wall

	if (ballX > 473.0 && ballSX > 0) {

		ballX = 473.0;
		ballSX = -ballSX;
	}


	// Top wall

	if (ballY < 27.0 && ballSY < 0) {

		ballY = 27.0;
		ballSY = -ballSY;
	}


	checkBrickCollision();
	checkPaddleCollision();


	// Ball missed the paddle.

	if (ballY > 325.0) {

		lives--;

		drawLives();


		if (lives <= 0) {

			gameOver = true;

			return;
		}


		// Reset ball without redrawing
		// anything else.

		ballX = playerX;
		ballY = 285.0;

		ballSX = 5.0;
		ballSY = -5.0;

		oldBallX = ballX;
		oldBallY = ballY;

		return;
	}


	drawBall();
}


// ------------------------------------------------------------
// Initialization
// ------------------------------------------------------------

cls();

drawLevel();

drawPlayer();

drawHud();

drawBall();


// ------------------------------------------------------------
// Main loop
// ------------------------------------------------------------

function loop() {

	if (gameOver || levelComplete) {

		delay(33);

		return;
	}


	updatePlayer();

	updateBall();

	delay(33);
}
