#include "Terminal.h"
#include "NTOSVM.h"
#include "Navigation.h"
#include "SplashScreen.h"


void setup() {
  randomSeed(analogRead(0));
  Terminal::begin();
  SplashScreen::draw();
  Navigation::MainView();
}

void loop() {
  NTOSVM::Update();
  Terminal::update();

  // delay(10);
}
