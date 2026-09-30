function draw() {
	dialog("SETTINGS");

	cursor(50, 180, 2);
	print("9 RESET GAME", WHITE);


	cursor(50, 200, 2);
	print("* BACK", WHITE);
}

draw();

function loop(){
	var key = getKey();

	switch(key) {
		case '9':
			if (confirm("RESET GAME")) {
				for (var i = 1; i <= 6; i++) {
					playerBalance[i] = 2500;
				}
				load("main");
			} else {
				draw();
			}
			break;
		case '*':
			load("main");
			break;
	}

    delay(1);
}