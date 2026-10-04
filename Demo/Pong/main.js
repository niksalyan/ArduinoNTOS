
let playerX = 240.0;
let aiX = 240.0;

let ballX = 240.0;
let ballY = 160.0;

let ballSX = 3.0;
let ballSY = 3.0;

let playerScore = 0;
let aiScore = 0;

const PADDLE_W = 80;
const PADDLE_H = 10;

const PLAYER_Y = 300;
const AI_Y = 10;

const BALL_SIZE = 10;

const LEFT = 10;
const RIGHT = 470;


// ------------------------------------------------------------
// Draw paddles
// ------------------------------------------------------------

function drawPlayer() {
	fillBox(
		playerX - PADDLE_W / 2,
		PLAYER_Y,
		PADDLE_W,
		PADDLE_H,
		WHITE
	);
}

function drawAI() {
	fillBox(
		aiX - PADDLE_W / 2,
		AI_Y,
		PADDLE_W,
		PADDLE_H,
		WHITE
	);
}


// ------------------------------------------------------------
// Player
// ------------------------------------------------------------

function updatePlayer() {

	let oldX = playerX;

	if (getKeyPressed('4')) {
		playerX -= 8.0;
	}

	if (getKeyPressed('6')) {
		playerX += 8.0;
	}

	if (playerX < LEFT + PADDLE_W / 2) {
		playerX = LEFT + PADDLE_W / 2;
	}

	if (playerX > RIGHT - PADDLE_W / 2) {
		playerX = RIGHT - PADDLE_W / 2;
	}

	if (oldX != playerX) {

		// Erase old paddle
		fillBox(
			oldX - PADDLE_W / 2,
			PLAYER_Y,
			PADDLE_W,
			PADDLE_H,
			BLACK
		);

		drawPlayer();
	}
}


// ------------------------------------------------------------
// AI
// ------------------------------------------------------------

function updateAI() {

	let oldX = aiX;

	// AI only follows the ball while it is travelling upward.
	if (ballSY < 0) {

		let difference = ballX - aiX;

		// Dead zone.
		if (difference > 12.0) {
			aiX += 4.0;
		}

		if (difference < -12.0) {
			aiX -= 4.0;
		}
	}

	if (aiX < LEFT + PADDLE_W / 2) {
		aiX = LEFT + PADDLE_W / 2;
	}

	if (aiX > RIGHT - PADDLE_W / 2) {
		aiX = RIGHT - PADDLE_W / 2;
	}

	if (oldX != aiX) {

		fillBox(
			oldX - PADDLE_W / 2,
			AI_Y,
			PADDLE_W,
			PADDLE_H,
			BLACK
		);

		drawAI();
	}
}


// ------------------------------------------------------------
// Reset ball
// ------------------------------------------------------------

function resetBall() {

	fillBox(
		ballX - BALL_SIZE / 2,
		ballY - BALL_SIZE / 2,
		BALL_SIZE,
		BALL_SIZE,
		BLACK
	);

	ballX = 240.0;
	ballY = 160.0;

	if (ballSY > 0) {
		ballSY = -3.0;
	} else {
		ballSY = 3.0;
	}

	ballSX = 3.0;

	fillBox(
		ballX - BALL_SIZE / 2,
		ballY - BALL_SIZE / 2,
		BALL_SIZE,
		BALL_SIZE,
		WHITE
	);
}


// ------------------------------------------------------------
// Ball
// ------------------------------------------------------------

function updateBall() {

	let oldX = ballX;
	let oldY = ballY;

	ballX += ballSX;
	ballY += ballSY;


	// --------------------------------------------------------
	// Left / right walls
	// --------------------------------------------------------

	if (
		ballX <= LEFT + BALL_SIZE / 2 &&
		ballSX < 0
	) {
		ballX = LEFT + BALL_SIZE / 2;
		ballSX = -ballSX;
	}

	if (
		ballX >= RIGHT - BALL_SIZE / 2 &&
		ballSX > 0
	) {
		ballX = RIGHT - BALL_SIZE / 2;
		ballSX = -ballSX;
	}


	// --------------------------------------------------------
	// Player paddle
	// --------------------------------------------------------

	if (
		ballSY > 0 &&
		collision(
			ballX,
			ballY,
			BALL_SIZE,
			BALL_SIZE,
			playerX,
			PLAYER_Y + PADDLE_H / 2,
			PADDLE_W,
			PADDLE_H
		)
	) {

		ballY = PLAYER_Y - BALL_SIZE / 2;

		ballSY = -ballSY;

		// Change horizontal direction based on
		// where the ball hits the paddle.
		ballSX = (ballX - playerX) / 8.0;
	}


	// --------------------------------------------------------
	// AI paddle
	// --------------------------------------------------------

	if (
		ballSY < 0 &&
		collision(
			ballX,
			ballY,
			BALL_SIZE,
			BALL_SIZE,
			aiX,
			AI_Y,
			PADDLE_W + PADDLE_H / 2,
			PADDLE_H
		)
	) {

		ballY = AI_Y + PADDLE_H + BALL_SIZE / 2;

		ballSY = -ballSY;

		ballSX = (ballX - aiX) / 8.0;
	}


	// --------------------------------------------------------
	// Player missed
	// --------------------------------------------------------

	if (ballY > 320) {

		aiScore++;

		resetBall();

		return;
	}


	// --------------------------------------------------------
	// AI missed
	// --------------------------------------------------------

	if (ballY < -BALL_SIZE) {

		playerScore++;

		resetBall();

		return;
	}


	// --------------------------------------------------------
	// Erase old ball
	// --------------------------------------------------------

	fillBox(
		oldX - BALL_SIZE / 2,
		oldY - BALL_SIZE / 2,
		BALL_SIZE,
		BALL_SIZE,
		BLACK
	);


	// Draw new ball
	fillBox(
		ballX - BALL_SIZE / 2,
		ballY - BALL_SIZE / 2,
		BALL_SIZE,
		BALL_SIZE,
		WHITE
	);
}


// ------------------------------------------------------------
// Initial screen
// ------------------------------------------------------------

cls();

drawPlayer();
drawAI();

fillBox(
	ballX - BALL_SIZE / 2,
	ballY - BALL_SIZE / 2,
	BALL_SIZE,
	BALL_SIZE,
	WHITE
);


// ------------------------------------------------------------
// Game loop
// ------------------------------------------------------------

function loop() {

	updateBall();
	updatePlayer();
	updateAI();

	sync(15);
}
