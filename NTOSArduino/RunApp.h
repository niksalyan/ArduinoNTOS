#pragma once
#include "progmem_far.h"
#include "NTOSVM.h"
#include "Terminal.h"

class RunApp {
private:


public:

  static void open(char* name) {

    NTOSUI::setHandler(handler);

    tft.fillScreenBlack();
    NTOSUI::scroll(0);
    tft.setTextSize(2);
    tft.setCursor(0, 0);

    if (Storage::drawImage(name, "splash", tft.width() / 2, tft.height() / 2, 2, 2, 0)) {
      delay(1500);
    }

    NTOSVM::Stop();
    NTOSVM::ResetExecution();
    NTOSVM::ClearMemory();

    File file;

    if (!Storage::openAppFile(name, "main", "ntx", file)) {
      return;
    }

    bool loaded = NTOSVM::LoadBytecodeFromFile(file, name);

    file.close();

    if (!loaded) {
      return;
    }
    NTOSVM::initialized = false;
    NTOSVM::Start();
  }

private:


  static void handler(char key) {
    if (key == 27) { // Exit
      NTOSVM::Stop();
      Navigation::MainView();
      return;
    }
    if (key != 0) {
          NTOSVM::currentKey = key;
          NTOSVM::currentNumber = key - '0';
    }
  }
};
