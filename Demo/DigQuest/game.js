var coinsCount = 0; 
var playerX = 0;
var playerY = 0;
var playerXOld = 0;
var playerYOld = 0;

var enemyX = 0;
var enemyY = 0;
var enemyXOld = 0;
var enemyYOld = 0;
var qTileX = 0;
var qTileY = 0;
var qTileR = NONE;
var flip = 0;

var doorX = 0;
var doorY = 0;

function getTile() {
	qTileR = level[qTileX + qTileY * 15];
}

function drawLevel() {
    cls();
	for(var y = 0; y < 10; y++) {
		for(var x = 0; x < 15; x++){
			var i = 0x00;
			i = level[x + y * 15];
			switch(i) {
				case COIN:
					drawSprite(218, x * 32, y * 32, YELLOW, NONE);
					coinsCount++;
					break;
				case TREE:
					drawSprite(54, x * 32, y * 32, C030, NONE);
					break;
				case PLATFORM:
					drawSprite(19, x * 32, y * 32, C300, NONE);
					break;
				case DOOR:
					//drawSprite(444, x * 32, y * 32, GRAY, NONE);
					doorX = x;
					doorY = y;
					level[x + y * 15] = EMPTY;
					break;
				case ENEMY:
					// drawSprite(126, x * 32, y * 32, RED, NONE);
					enemyX = x;
					enemyY = y;
					level[x + y * 15] = EMPTY;
					break;
				case LADDER:
					drawSprite(70, x * 32, y * 32, DARKGRAY, NONE);
					break;
				case PLAYER:
					// drawSprite(76, x * 32, y * 32, WHITE, NONE);
					playerX = x;
					playerY = y;
					level[x + y * 15] = EMPTY;
					break;
				case GROUND:
					drawSprite(558, x * 32, y * 32, C301, NONE);
					break;
				case WATER:
					fillBox(x * 32, y * 32, 32, 32, BLUE);
					drawSprite(7, x * 32, y * 32, BLACK, NONE);
					break;
			}
		}
	}
}

function updatePlayer() {
	playerXOld = playerX;
	playerYOld = playerY;
	
	if (getKeyPressed('4')) {
		playerX -= 1;
	}

	if (getKeyPressed('6')) {
		playerX += 1;
	}

	if (getKeyPressed('2')) {
		playerY -= 1;
	}

	if (getKeyPressed('8')) {
		playerY += 1;
	}

	qTileX = playerX;
	qTileY = playerY;
	getTile();

	if (qTileR == COIN) {
		coinsCount--;
		if (coinsCount <= 0) {
			drawSprite(444, doorX * 32, doorY * 32, GRAY, NONE);
		}
	}

	if (qTileR == GROUND) {
		playerX = playerXOld;
		playerY = playerYOld;
	}

	if (coinsCount <= 0 && playerX == doorX && playerY == doorY) {
		currentLevel++;
		load("lives");
	}

	fillBox(playerXOld * 32, playerYOld * 32, 32, 32, BLACK);
	drawSprite(76, playerX * 32, playerY * 32, WHITE, NONE);
	level[playerX + playerY * 15] = EMPTY;
}

function checkEnemy() {
	qTileX = enemyX;
	qTileY = enemyY;
	getTile();
	if (qTileR > 0) {
		enemyX = enemyXOld;
		enemyY = enemyYOld;
	}
}

function updateEnemy() {
	enemyXOld = enemyX;
	enemyYOld = enemyY;
	
	if (enemyX > playerX) {
		enemyX -= 1;
		checkEnemy();
	} 

	if (enemyX < playerX) {
		enemyX += 1;
		checkEnemy();
	}

	if (enemyY > playerY) {
		enemyY -= 1;
		checkEnemy();
	}

	if (enemyY < playerY) {
		enemyY += 1;
		checkEnemy();
	}

	fillBox(enemyXOld * 32, enemyYOld * 32, 32, 32, BLACK);
	drawSprite(126, enemyX * 32, enemyY * 32, RED, flip == 1 ? FLIP_X : NONE);
}

drawLevel();
fillBox(150, 6, 180, 20, BLACK);

function loop() {

	fillBox(230, 8, 24, 15, BLACK);
	cursor(230, 8, 2);
	print(coinsCount, YELLOW);

	flip = (flip + 1) % 3;
	updatePlayer();
	if (flip == 0) {
		updateEnemy();
	} else {
		fillBox(enemyX * 32, enemyY * 32, 32, 32, BLACK);
		drawSprite(126, enemyX * 32, enemyY * 32, RED, flip == 1 ? FLIP_X : NONE);
	}

	if (playerX == enemyX && playerY == enemyY) {
		lives--;
		load("lives");
	}
	
	
	sync(100);
}