


var cursorX = 5;
var cursorY = 5;
var movePosX = -1;
var movePosY = -1; 

var board =
[
    'r', 'n', 'b', 'q', 'k', 'b', 'n', 'r',
    'p', 'p', 'p', 'p', 'p', 'p', 'p', 'p',

    ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
    ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
    ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
    ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',

    'P', 'P', 'P', 'P', 'P', 'P', 'P', 'P',
    'R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R'
];

// NTOS Main executable file
function init() {
    // This function is called when the project is initialized
}

function drawBoard() {
    for(var y = 0; y < 8; y++) {
		for(var x = 0; x < 8; x++) {
			fillBox(1 + x * 40, 1 + y * 40, 38, 38, (x + y) % 2 == 0 ? rgb666 : rgb333);
			var figure = board[x + y * 8];
			switch(figure) {
				case 'p':
					drawSprite(1022, 4 + x * 40, 4 + y * 40, BLACK);
					break;
				case 'P':
					drawSprite(1022, 4 + x * 40, 4 + y * 40, WHITE);
					break;
				case 'r':
					drawSprite(1023, 4 + x * 40, 4 + y * 40, BLACK);
					break;
				case 'R':
					drawSprite(1023, 4 + x * 40, 4 + y * 40, WHITE);
					break;
				case 'n':
					drawSprite(1027, 4 + x * 40, 4 + y * 40, BLACK);
					break;
				case 'N':
					drawSprite(1027, 4 + x * 40, 4 + y * 40, WHITE);
					break;
				case 'b':
					drawSprite(1024, 4 + x * 40, 4 + y * 40, BLACK);
					break;
				case 'B':
					drawSprite(1024, 4 + x * 40, 4 + y * 40, WHITE);
					break;
				case 'q':
					drawSprite(1025, 4 + x * 40, 4 + y * 40, BLACK);
					break;
				case 'Q':
					drawSprite(1025, 4 + x * 40, 4 + y * 40, WHITE);
					break;
				case 'k':
					drawSprite(1026, 4 + x * 40, 4 + y * 40, BLACK);
					break;
				case 'K':
					drawSprite(1026, 4 + x * 40, 4 + y * 40, WHITE);
					break;
			}
			
		}
	}
}

function drawCursor() {
	drawBox(cursorX * 40, cursorY * 40, 40, 40, WHITE);
	if (movePosX >= 0 && movePosY >= 0) {
		drawBox(movePosX * 40, movePosY * 40, 40, 40, YELLOW);
	}
}

function clearCursor() {
	drawBox(cursorX * 40, cursorY * 40, 40, 40, BLACK);
	if (movePosX >= 0 && movePosY >= 0) {
		drawBox(movePosX * 40, movePosY * 40, 40, 40, BLACK);
	}
}

cls();
cursor(330, 10, 2);
print("SHAXMAD", WHITE);
drawBoard();
drawCursor();

function loop() {
    // This function is called every frame
    var key = getKey();
    switch(key) {
		case '2':
			clearCursor();
			cursorY = (cursorY + 7) % 8;
			drawCursor();
			break;
		case '8':
			clearCursor();
			cursorY = (cursorY + 1) % 8;
			drawCursor();
			break;
		case '4':
			clearCursor();
			cursorX = (cursorX + 7) % 8;
			drawCursor();
			break;
		case '6':
			clearCursor();
			cursorX = (cursorX + 1) % 8;
			drawCursor();
			break;
        case '*':
            // Do something when the '*' key is pressed
            break;
		case '5':
        case '#':
			if (movePosX >= 0 && movePosY >= 0) {
				clearCursor();
				var piece = board[movePosX + movePosY * 8];
				board[movePosX + movePosY * 8] = ' ';
				board[cursorX + cursorY * 8] = piece;
				movePosX = -1;
				movePosY = -1;
				drawBoard();
				drawCursor();
			} else {
				movePosX = cursorX;
				movePosY = cursorY;
				drawCursor();
			}
            // Do something when the '#' key is pressed
            break;
    }
    delay(1);
}