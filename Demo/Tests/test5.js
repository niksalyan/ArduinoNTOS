// Test 5 - simple animation in the lower area (leave top band untouched)
cls();
// make many frames (at least 50) by stepping in small increments
var x = 0;
while (x < 480) {
    // clear only the lower area (preserve top band)
    fillBox(0, 33, 480, 320 - 33, BLACK);

    // moving circle (Y > 32)
    drawCircle(x, 60, 8, BLUE);

    // trailing dots
    var t = 0;
    while (t < 5) {
        var tx = x - (t * 6);
        if (tx >= 0) {
            drawPixel(tx, 60, WHITE);
        }
        t = t + 1;
    }

    // small decorative dashed line below the top band
    var xx = 0;
    while (xx < 480) {
        drawLine(xx, 36, xx + 2, 36, YELLOW);
        xx = xx + 12;
    }

    delay(50);
    x = x + 5;
}

// return to main
load("main");
