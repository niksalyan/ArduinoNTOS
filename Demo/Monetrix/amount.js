var amount = 0;

dialog("INPUT AMOUNT");

cursor(20, 50, 3);
print("FROM: ", WHITE);
print(playerNames[sourceAccount], WHITE);

cursor(20, 90, 3);
print("TO:   ", WHITE);
print(playerNames[targetAccount], WHITE);

function update() {
	fillBox(100, 200, 250, 30, BLACK);
	cursor(100, 200, 4);
	print("$", WHITE); 
	print(amount, WHITE);
}

update();
cursor(20, 290, 2);
print("* = BACK", CYAN);

cursor(460, 290, 2);
printRight("# = NEXT", CYAN);

function loop(){
	var num = getNumericKey();
	var key = getKey();

	if (key == '*') {
		if (amount > 0) {
			amount = amount / 10;
			update();
		} else {
			load("main");
		}
	}

	if (key == '#') {
		if (confirm("CONFIRM TRANSFER")) {
			playerBalance[sourceAccount] = playerBalance[sourceAccount] - amount;
			playerBalance[targetAccount] = playerBalance[targetAccount] + amount;
			saveInt(1500 + sourceAccount * 5, playerBalance[sourceAccount]);
			saveInt(1500 + targetAccount * 5, playerBalance[targetAccount]);
			alert("TRANSFER OK", "Money transfer completed!");
		}
		load("main");
	}


	if (num >= 0) {
		amount = amount * 10 + num;
		update();
	}

    delay(1);
}