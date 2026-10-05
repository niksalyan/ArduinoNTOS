cls();
scroll(-200); 

function preScroll() {
	for(var i = -200; i <= 0; i+=10) {
		scroll(i);
		delay(20);
	}
	delay(1300);
}


for (var i = 0; i < 480; i += 10) {
	fillBox(i, 80, 5, 1, C730);
	fillBox(i, 240, 5, 1, C730); 
}

if (lives > 0) {

	cursor(240, 100, 2);
	printCentered("LEVEL", WHITE);
	cursor(230, 120, 4);
	print(currentLevel, C730);

	cursor(240, 170, 2);
	printCentered("LIVES", WHITE);
	cursor(230, 190, 4);
	print(lives, C730);
	preScroll();
	load("game");
} else {
	cursor(240, 150, 3);
	printCentered("GAME OVER", C730);
	preScroll();
	load("main");
}
