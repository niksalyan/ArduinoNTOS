using playersCount = 4;
using playerBalance = [0, 2500, 2500, 2500, 2500, 2500, 2500];
using playerNames = ["[BANK]", "P1", "P2", "P3", "P4", "P5", "P6"]; 


function init(){
	
	for(var i = 1; i <= 6; i++) {
		playerBalance[i] = loadInt(1500 + i * 5, 2500);
		loadStr(addr(1540, i, 8), addr($playerNames, i, 7), playerNames[i], 6); 
	}
}


var sourceAccount = -1;
var targetAccount = -1; 

function draw() {
	dialog("MONETRIX BANK"); 
	
	
	for(var y = 0; y <= 1; y++) {
		for(var x = 0; x <= 2; x++) {
			var i = x + y * 3 + 1;
			if (i <= playersCount) {
				cursor(12 + x * 155, 60 + y * 80, 2);
				print(i, GRAY);
				print(":", GRAY);
				print(playerNames[i], WHITE); 

				var color = YELLOW;
				if (playerBalance[i] < 0) {
					color = RED;
				}
				if (playerBalance[i] > 200) {
					color = GREEN;
				}

				cursor(12 + x * 155, 85 + y * 80, 4);
				print("$", color);
				print(playerBalance[i], color);
			}
			
		}
	}

	// drawRoundBox(10,190,460,120,20, GRAY);

	cursor(20, 290, 2);
	print("SELECT PLAYER 1-", CYAN);
	print(playersCount, CYAN);

	cursor(460, 290, 2);
	printRight("0 = BANK", CYAN);

	// editText(addr(&playerNames, 1, 7), "EDIT PLAYER NAME", 6);
}


draw();

function loop(){
    var key = getKey();
	var selected = getNumericKey();

	if (key == 'C') {
		var pc = confirmNumber("PLAYESR COUNT", "2 - 6", 2, 6);
		if (pc >= 0) {playersCount = pc;}
		draw();
	}

	if (key == 'A') {
		load("settings");
	}

	if (selected >= 0 && selected <= playersCount) {
		sourceAccount = selected;
		load("target");
	}

    delay(1);
}
