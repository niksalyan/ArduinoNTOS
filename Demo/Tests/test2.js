dialog("MATH TEST");

cursor(10, 50, 2);
print("PI        = ", WHITE);
print(PI, WHITE);

cursor(10, 75, 2);
print("sin(PI/2) = ", WHITE);
print(sin(PI / 2.0), WHITE);

cursor(10, 100, 2);
print("cos(PI/2) = ", WHITE);
print(cos(PI / 2.0), WHITE);

cursor(10, 125, 2);
print("abs(-10)  = ", WHITE);
print(abs(-10), WHITE);

cursor(10, 150, 2);
print("clamp(20, 0, 10) = ", WHITE);
print(clamp(20, 0, 10), WHITE);

cursor(10, 175, 2);
print("sqrt(144) = ", WHITE);
print(sqrt(144.0), WHITE);

cursor(10, 200, 2);
print("pow(2, 8) = ", WHITE);
print(pow(2.0, 8.0), WHITE);

cursor(10, 225, 2);
print("floor(3.7) = ", WHITE);
print(floor(3.7), WHITE);

cursor(10, 250, 2);
print("round(3.6) = ", WHITE);
print(round(3.6), WHITE);

cursor(10, 275, 2);
print("lerp(0, 100, 0.25) = ", WHITE);
print(lerp(0.0, 100.0, 0.25), WHITE);

cursor(10, 300, 2);
print("atan2(1, 1) = ", WHITE);
print(atan2(1.0, 1.0), WHITE);

delay(4000);
load("test3");