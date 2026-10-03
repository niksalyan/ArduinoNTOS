lives--;
cls();
if (lives > 0) {
	cursor(240, 100, 2);
	printCentered("LIVES", WHITE);
	cursor(230, 150, 4);
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