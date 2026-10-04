// ========================================
// NTOS CALCULATOR
//
// Keypad:
//
//   1 2 3 A
//   4 5 6 B
//   7 8 9 C
//   * 0 # D
//
// A = +
// B = -
// C = *
// D = /
// # = =
// * = Clear
// ========================================

function init() {
    var current = 0;
    var left = 0;
    var operator = 0;
    var newInput = 1;
}


// ========================================
// DRAW CALCULATOR
// ========================================
dialog("CALCULATOR");

function update() {
	// ====================================
    // DISPLAY
    // ====================================

    fillBox(20, 45, 440, 40, DARKGREEN);
    drawBox(20, 45, 440, 40, WHITE);

    cursor(40, 55,3);
    print(current, WHITE);
}


function draw() {
    // Background
    fillBox(0, 33, 480, 285, rgb023);


    update();


    // ====================================
    // BUTTONS
    // ====================================

    // Row 1
    drawBox(20, 95, 100, 45, WHITE);
    drawBox(130, 95, 100, 45, WHITE);
    drawBox(240, 95, 100, 45, WHITE);
    drawBox(350, 95, 110, 45, YELLOW);

    cursor(60, 110, 2);
    print("1", WHITE);

    cursor(170, 110, 2);
    print("2", WHITE);

    cursor(280, 110, 2);
    print("3", WHITE);

    cursor(395, 110, 2);
    print("+", WHITE);


    // Row 2
    drawBox(20, 150, 100, 45, WHITE);
    drawBox(130, 150, 100, 45, WHITE);
    drawBox(240, 150, 100, 45, WHITE);
    drawBox(350, 150, 110, 45, YELLOW);

    cursor(60, 165, 2);
    print("4", WHITE);

    cursor(170, 165, 2);
    print("5", WHITE);

    cursor(280, 165, 2);
    print("6", WHITE);

    cursor(395, 165, 2);
    print("-", WHITE);


    // Row 3
    drawBox(20, 205, 100, 45, WHITE);
    drawBox(130, 205, 100, 45, WHITE);
    drawBox(240, 205, 100, 45, WHITE);
    drawBox(350, 205, 110, 45, YELLOW);

    cursor(60, 220, 2);
    print("7", WHITE);

    cursor(170, 220, 2);
    print("8", WHITE);

    cursor(280, 220, 2);
    print("9", WHITE);

    cursor(395, 220, 2);
    print("*", WHITE);


    // Row 4
    drawBox(20, 260, 100, 45, YELLOW);
    drawBox(130, 260, 100, 45, WHITE);
    drawBox(240, 260, 100, 45, GREEN);
    drawBox(350, 260, 110, 45, YELLOW);

    cursor(60, 275, 2);
    print("C", WHITE);

    cursor(170, 275, 2);
    print("0", WHITE);

    cursor(280, 275, 2);
    print("=", WHITE);

    cursor(395, 275, 2);
    print("/", WHITE);
}

draw();

// Draw the initial calculator
//draw();


// ========================================
// KEYPAD LOOP
// ========================================

function loop() {

    var key = getKey();
	var num = getNumericKey();
    switch(key) {
        // =================================
        // ADD
        // A = +
        // =================================

        case 'A':

            left = current;
            operator = 1;
            newInput = 1;

            update();
			break; 


        // =================================
        // SUBTRACT
        // B = -
        // =================================

        case 'B':

            left = current;
            operator = 2;
            newInput = 1;

            update();
			break; 


        // =================================
        // MULTIPLY
        // C = *
        // =================================

        case 'C':

            left = current;
            operator = 3;
            newInput = 1;

            update();
			break; 


        // =================================
        // DIVIDE
        // D = /
        // =================================

        case 'D':

            left = current;
            operator = 4;
            newInput = 1;

            update();
			break; 


        // =================================
        // CLEAR
        // * = C
        // =================================

        case '*':

            current = 0;
            left = 0;
            operator = 0;
            newInput = 1;

            update();
			break; 


        // =================================
        // EQUALS
        // # = =
        // =================================

        case '#':

            if (operator == 1) {
                current = left + current;
            }

            if (operator == 2) {
                current = left - current;
            }

            if (operator == 3) {
                current = left * current;
            }

            if (operator == 4) {
                if (current != 0) {
                    current = left / current;
                }
            }

            left = current;
            operator = 0;
            newInput = 1;

            update();
			break; 
		default:
			if (num >= 0) {
				if (newInput == 1) {
					current = 0;
					newInput = 0;
				}
				current = current * 10 + num;
				update();
			}
			break;
    }


    delay(1);
}