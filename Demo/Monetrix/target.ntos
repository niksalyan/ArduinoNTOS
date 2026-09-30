dialog("TRANSFER TO");

cursor(20, 50, 3);
print("FROM: ", WHITE);
print(playerNames[sourceAccount], WHITE);

cursor(20, 90, 3);
print("TO:   ", WHITE);



cursor(20, 290, 2);
print("* = BACK", CYAN);

cursor(460, 290, 2);
printRight("0 = BANK", CYAN);

function loop(){
	var selected = getNumericKey();
	var key = getKey();

	if (key == '*') {
		load("main");
	}

	if (key == 'A') {
		if(editText("PLAYER NAME", addr($playerNames, sourceAccount, 7), 6)) {
			saveStr(addr(1540, sourceAccount, 8), playerNames[sourceAccount], 6);
		}
		load("main");
	}

	if (selected >= 0 && selected <= playersCount) {
		targetAccount = selected;
		load("amount");
	}

    delay(1);
}