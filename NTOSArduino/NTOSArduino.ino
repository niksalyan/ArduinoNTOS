#include "Terminal.h"

#include "Navigation.h"
#include "SplashScreen.h"


void setup() {
  randomSeed(analogRead(0));
  Terminal::begin();
  SplashScreen::draw();
  Navigation::MainView();
}

void loop() {
  Terminal::update();
}
