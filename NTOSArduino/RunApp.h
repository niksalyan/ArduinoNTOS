#pragma once
#include "progmem_far.h"
#include "NTOSVM.h"
#include "UI.h"

class RunApp {
private:


public:

  static void open(char* name) {

    UI::setHandler(handler);

    draw();

    NTOSVM::Stop();
    NTOSVM::ResetExecution();
    NTOSVM::ClearMemory();

    File file;

    if (!Storage::openAppFile(name, "main.ntx", file)) {
      return;
    }

    bool loaded = NTOSVM::LoadBytecodeFromFile(file);

    file.close();

    if (!loaded) {
      return;
    }
    NTOSVM::Start();
  }

  static void update() {

    // Eventually:
    //
    // - check SD card
    // - detect new applications
    // - update status information
    //
    // Nothing should be redrawn unless something changed.
  }

  static void draw() {

    UI::dialog("APP");
  }

private:



  static void handler(char key) {

    switch (key) {

      case '*':
        NTOSVM::Stop();
        Navigation::MainView();
        break;

      default:
        if (key != 0) {
          NTOSVM::WriteByteToMemory(0, key);
        }
        break;
    }
  }
};
