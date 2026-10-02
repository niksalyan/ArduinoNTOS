// Test 4 - decorative patterns in lower area (leave top band untouched)
cls();
// small diagonal ticks and pixels starting below the top band
var x = 0;
while (x < 480) {
    // small diagonal ticks
    drawLine(x, 40 + (x % 6), x + 3, 40 + ((x + 3) % 6), GREEN);
    drawPixel(x + 1, 46 + ((x / 5) % 10), WHITE);
    x = x + 4;
}

// small repeating pattern further down
var sx = 2;
while (sx < 480) {
    fillBox(sx, 60, 3, 3, BLUE);
    sx = sx + 7;
}

// horizontal dashed lines in lower area
var y = 40;
while (y < 200) {
    var xx = 0;
    while (xx < 480) {
        drawLine(xx, y, xx + 2, y, YELLOW);
        xx = xx + 6;
    }
    y = y + 6;
}

delay(2000);
load("test5");
