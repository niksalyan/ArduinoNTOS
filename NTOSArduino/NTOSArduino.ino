#include "Terminal.h"
#include "Storage.h"
#include "NTOSVM.h"
#include "Navigation.h"
#include "SplashScreen.h"


void setup() {
  randomSeed(analogRead(0));
  Terminal::begin();
  Storage::begin();
  SplashScreen::draw();
  Navigation::MainView();
}

void loop() {
  NTOSVM::Update();
  Terminal::update();

  Storage::update();

  // delay(10);
}
