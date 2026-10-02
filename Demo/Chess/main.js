
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
			fillBox(x * 40, y * 40, 40, 40, (x + y) % 2 == 0 ? rgb666 : rgb333);
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

cls();
drawBoard();

function loop() {
    // This function is called every frame
    var key = getKey();
    switch(key) {
        case '*':
            // Do something when the '*' key is pressed
            break;
        case '#':
            // Do something when the '#' key is pressed
            break;
    }
    delay(1);
}