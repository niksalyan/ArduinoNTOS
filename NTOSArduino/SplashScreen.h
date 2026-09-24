#pragma once
#include "Terminal.h"
// #include "UI.h"
#include "Graphics.h"

class SplashScreen {
public:
  static void draw() {
      tft.fillScreenBlack();
      tft.pushCommand(0x28, nullptr, 0);  // TFT OFF
      LOGO_DRAW(0,0,2,2);
      tft.pushCommand(0x29, nullptr, 0);  // TFT ON
      tft.setFont();
      // UI::printCentered("NTOS CREATED BY NIKSALYAN TIGRAN 2026", tft.width() / 2, tft.height() - 20, TFT_WHITE, 1);
      delay(2000);
      // UI::fadeOut();
    
      tft.fillScreenBlack();
  }

};


