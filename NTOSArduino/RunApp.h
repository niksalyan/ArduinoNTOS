#pragma once
#include "progmem_far.h"
#include "NTOSVM.h"
#include "UI.h"
#include "Terminal.h"

class RunApp {
private:


public:

  static void open(char* name) {

    UI::setHandler(handler);
    Terminal::updateHandler = update;

    UI::dialog(name);

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

  static void update() {
    if (Terminal::isPressed('*')  && Terminal::isPressed('#')) {
      NTOSVM::Stop();
      Navigation::MainView();
    }
  }

  static void handler(char key) {
    if (key != 0) {
          NTOSVM::WriteByteToMemory(0, key);
    }
  }
};
