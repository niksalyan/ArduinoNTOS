#include "Terminal.h"

#include "NTOSVM.h"
#include "Navigation.h"
#include "SplashScreen.h"
#include "Storage.h"

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
  delay(10);
}
