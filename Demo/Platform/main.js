
using oLevel = byte[150];
using level = byte[150];

const COIN = 0x01;
const TREE = 0x02;
const PLATFORM = 0x03;
const DOOR = 0x04;
const ENEMY = 0x05;
const LADDER = 0x06;
const PLAYER = 0x07;
const GROUND = 0x08;
const WATER = 0x09;

level = [
    NONE,    NONE,    NONE,    NONE,    NONE,    NONE,    NONE,    NONE,    NONE,    NONE,    NONE,    NONE,    NONE,    NONE,    NONE,
    NONE,    NONE,    NONE,    NONE,    NONE,    COIN,    NONE,    NONE,    NONE,    NONE,    NONE,    COIN,    NONE,    NONE,    NONE,
    NONE,    NONE,    NONE,    NONE,    NONE,    PLATFORM,PLATFORM,PLATFORM,NONE,    NONE,    TREE,    NONE,    NONE,    DOOR,   NONE,
    NONE,    NONE,    COIN,    TREE,    NONE,    NONE,    NONE,    NONE,    NONE,    PLATFORM,PLATFORM,PLATFORM,LADDER,    PLATFORM,    NONE,
    NONE,    PLATFORM,PLATFORM,PLATFORM,NONE,    NONE,    ENEMY,   NONE,    NONE,    NONE,    NONE,    NONE,    LADDER,  NONE,    NONE,
    PLAYER,  NONE,    NONE,    NONE,    NONE,    NONE,    NONE,    NONE,    NONE,    COIN,    NONE,    NONE,    LADDER,  NONE,    NONE,
    GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,
    GROUND,  GROUND,  WATER,   WATER,   WATER,   GROUND,  GROUND,  GROUND,  WATER,   WATER,   WATER,   GROUND,  GROUND,  GROUND,  GROUND,
    GROUND,  GROUND,  WATER,   WATER,   WATER,   GROUND,  PLATFORM,PLATFORM,PLATFORM,WATER,   WATER,   GROUND,  GROUND,  GROUND,  GROUND,
    GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,  GROUND,
];

// NTOS Main executable file
function init() {
    // This function is called when the project is initialized
}

function draw() {
    cls();
	for(var y = 0; y < 10; y++) {
		for(var x = 0; x < 15; x++){
			var i = 0x00;
			i = level[x + y * 15];
			switch(i) {
				case COIN:
					drawSprite(218, x * 32, y * 32, YELLOW);
					break;
				case TREE:
					drawSprite(54, x * 32, y * 32, C030);
					break;
				case PLATFORM:
					drawSprite(19, x * 32, y * 32, C300);
					break;
				case DOOR:
					drawSprite(444, x * 32, y * 32, GRAY);
					break;
				case ENEMY:
					drawSprite(126, x * 32, y * 32, RED);
					break;
				case LADDER:
					drawSprite(70, x * 32, y * 32, DARKGRAY);
					break;
				case PLAYER:
					drawSprite(76, x * 32, y * 32, WHITE);
					break;
				case GROUND:
					drawSprite(558, x * 32, y * 32, C301);
					break;
				case WATER:
					fillBox(x * 32, y * 32, 32, 32, BLUE);
					drawSprite(7, x * 32, y * 32, BLACK);
					break;
			}
		}
	}
}

draw();

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
