lives--;
cls();
if (lives > 0) {

	cursor(240, 80, 2);
	printCentered("LEVEL", WHITE);
	cursor(230, 110, 4);
	print(currentLevel, GREEN);

	cursor(240, 150, 2);
	printCentered("LIVES", WHITE);
	cursor(230, 180, 4);
	print(lives, GREEN);
} else {
	cursor(240, 150, 3);
	printCentered("GAME OVER", RED);
}



delay(2000);
if (lives <= 0) {
	load("main");
} else {
	load("game");
}