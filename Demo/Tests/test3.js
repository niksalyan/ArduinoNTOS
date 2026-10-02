// Test 3 - simple shapes in the lower area (Y > 32)
cls();
// Compact boxes across the lower area
var bx = 2;
while (bx < 476) {
    fillBox(bx, 40, 6, 6, RED);
    bx = bx + 10;
}

// Rows of small circles (lower area)
var cx = 6;
while (cx < 474) {
    fillCircle(cx, 50, 6, GREEN);
    cx = cx + 12;
}

// Many short horizontal lines (decorative) below the top band
var y = 40;
while (y < 320) {
    var x = 0;
    while (x < 480) {
        drawLine(x, y, x + 4, y, WHITE);
        x = x + 6;
    }
    y = y + 4;
}

// Some pixels for detail
var px = 3;
while (px < 480) {
    drawPixel(px, 36 + ((px / 7) % 80), YELLOW);
    px = px + 5;
}

delay(2000);
load("test4");
