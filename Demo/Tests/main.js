function init(){
	var px = 240;
	var py = 160;
}

cls();



// ================================
// SKY
// ================================

dialog("DRAWING TEST");
fillBox(0, 32, 480, 320 - 25, BLUE);


// ================================
// MOON
// ================================

fillCircle(380, 90, 45, YELLOW);
drawCircle(380, 90, 47, WHITE);

// Moon craters
fillCircle(360, 65, 6, WHITE);
fillCircle(395, 55, 5, WHITE);
fillCircle(405, 90, 8, WHITE);
fillCircle(370, 100, 4, WHITE);

// ================================
// DISTANT CITY
// ================================

fillBox(0, 235, 480, 85, GREEN);

// Buildings

var width = 0;
var bx = 0;
for (bx = 5; bx < 480; bx = bx + width + 5) {
	width = 20 + (bx % 15);
    var height = 20 + (bx * 3) % 55;

    fillBox(
        bx,
        235 - height,
        width,
        height,
        GREEN
    );
}

// ================================
// BUILDING WINDOWS
// ================================

bx = 10;
while (bx < 470) {

    var by = 210;

    while (by < 230) {

        if ((bx + by) % 4 == 0) {
            fillBox(
                bx,
                by,
                5,
                5,
                YELLOW
            );
        }

        by = by + 10;
    }

    bx = bx + 30;
}

// ================================
// FIREWORK 1
// ================================

var cx = 120;
var cy = 105;

var i = 0;

drawPixel(10,30, RED);

while (i < 12) {

    var dx = (i * 17) % 35 - 17;
    var dy = (i * 29) % 35 - 17;

    drawLine(
        cx,
        cy,
        cx + dx,
        cy + dy,
        YELLOW
    );

    i = i + 1;
}

// ================================
// FIREWORK 2
// ================================

cx = 250;
cy = 75;

i = 0;

while (i < 16) {

    var dx = (i * 23) % 50 - 25;
    var dy = (i * 31) % 50 - 25;

    drawLine(
        cx,
        cy,
        cx + dx,
        cy + dy,
        WHITE
    );

    i = i + 1;
}

// ================================
// FIREWORK 3
// ================================

cx = 310;
cy = 145;

i = 0;

while (i < 14) {

    var dx = (i * 19) % 40 - 20;
    var dy = (i * 27) % 40 - 20;

    drawLine(
        cx,
        cy,
        cx + dx,
        cy + dy,
        YELLOW
    );

    i = i + 1;
}

// ================================
// WATER
// ================================

fillBox(0, 275, 480, 45, BLUE);

// Water reflection lines
var wy = 280;

while (wy < 320) {

    var wx = (wy * 13) % 40;

    while (wx < 480) {

        drawLine(
            wx,
            wy,
            wx + 12,
            wy,
            WHITE
        );

        wx = wx + 35;
    }

    wy = wy + 7;
}

// ================================
// MOON REFLECTION
// ================================
 
var ry = 280;

while (ry < 320) {

    var width = 40 - (ry - 280);

    drawLine(
        380 - width,
        ry,
        380 + width,
        ry,
        YELLOW
    );

    ry = ry + 5;
}

// ================================
// FOREGROUND
// ================================

fillBox(0, 310, 480, 10, GREEN);

// Grass
var gx = 0;

while (gx < 480) {

    drawLine(
        gx,
        310,
        gx + 3,
        300 - (gx % 8),
        GREEN
    );

    gx = gx + 8;
}

delay(4000);
load("test2");