
dialog("ANIMATION");


var ang = 0.0;
while(ang < PI * 4.0) {

	fillCircle(240.0 + cos(ang) * 100.0, 160.0 + sin(ang) * 100.0, 10, BLACK);
	ang += 0.1;
	fillCircle(240.0 + cos(ang) * 100.0, 160.0 + sin(ang) * 100.0, 10, WHITE);
	delay(10);
	
}


load("test4");
