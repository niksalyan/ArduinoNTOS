#pragma once
#define SOFTWARE_SPI_FOR_SD
#include <SoftSD.h>

class Storage {
private:
  static constexpr uint8_t BUFFER_SIZE = 64;

  inline static char commandBuffer[BUFFER_SIZE];
  inline static uint8_t commandLength = 0;

public:
  static void begin() {
    SD.begin();
    Serial.print("OK");
  }

  static void update() {
    while (Serial.available() > 0) {

      uint8_t c = Serial.read();

      // End of command
      if (c == '\r' || c == '\n') {

        // Ignore empty lines
        if (commandLength == 0)
          continue;

        commandBuffer[commandLength] = '\0';

        Serial.print("[NTOS] COMMAND: <");
        Serial.print(commandBuffer);
        Serial.println(">");



        processCommand(commandBuffer);

        commandLength = 0;

        continue;
      }

      // Store character
      if (commandLength < BUFFER_SIZE - 1) {
        commandBuffer[commandLength++] = c;
      }
    }
  }

  static void listApps(void (*callback)(const char* name)) {
    File root = SD.open("/");

    if (!root)
      return;

    while (true) {
      File entry = root.openNextFile();

      if (!entry)
        break;

      if (entry.isDirectory()) {
        char mainPath[64];

        snprintf(
          mainPath,
          sizeof(mainPath),
          "/%s/main.ntx",
          entry.name());

        if (SD.exists(mainPath))
          callback(entry.name());
      }

      entry.close();
    }

    root.close();
  }

private:
  static void processCommand(const char* command) {
    if (command[0] == 'C' && command[1] == ' ') {
      createApp(command + 2);
      return;
    }

    if (command[0] == 'D' && command[1] == ' ') {
      deleteApp(command + 2);
      return;
    }
  }

  static void createApp(const char* appName) {
    Serial.print("[NTOS] CREATE: ");
    Serial.println(appName);

    if (!isValidName(appName)) {
      Serial.println("[NTOS] ERROR: invalid app name");
      return;
    }

    Serial.println("[NTOS] Name valid");

    if (SD.exists(appName)) {
      Serial.println("[NTOS] ERROR: app already exists");
      return;
    }

    Serial.println("[NTOS] App does not exist");

    if (!SD.mkdir(appName)) {
      Serial.println("[NTOS] ERROR: mkdir failed");
      return;
    }

    Serial.println("[NTOS] Directory created");

    char mainPath[64];

    snprintf(
      mainPath,
      sizeof(mainPath),
      "/%s/main.ntx",
      appName);

    Serial.print("[NTOS] Creating file: ");
    Serial.println(mainPath);

    File file = SD.open(mainPath, FILE_WRITE);

    if (!file) {
      Serial.println("[NTOS] ERROR: main.ntx creation failed");
      return;
    }

    file.close();

    Serial.println("[NTOS] main.ntx created");
    Serial.print("[NTOS] APP CREATED: ");
    Serial.println(appName);
  }

  static bool isValidName(const char* name) {
    if (!name || name[0] == '\0')
      return false;

    for (uint8_t i = 0; name[i] != '\0'; i++) {
      if (name[i] == '/' || name[i] == '\\')
        return false;
    }

    return true;
  }

  static void deleteApp(const char* appName) {
    // We'll implement recursive deletion next.
  }
};
