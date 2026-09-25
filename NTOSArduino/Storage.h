#pragma once
#define SOFTWARE_SPI_FOR_SD
#include <SoftSD.h>

class Storage {
private:
  static constexpr uint8_t BUFFER_SIZE = 64;

  inline static char commandBuffer[BUFFER_SIZE];
  inline static uint8_t commandLength = 0;

  // Upload state
  inline static bool isUploading = false;
  inline static File uploadFile;

  inline static bool hasHighNibble = false;
  inline static uint8_t highNibble = 0;

public:
  static void begin() {
    SD.begin();
    Serial.begin(9600);
    Serial.print("OK");
  }

  static void update() {
    while (Serial.available() > 0) {
      uint8_t c = Serial.read();

      // Upload mode
      if (isUploading) {
        processUploadByte(c);
        continue;
      }

      // Detect upload command immediately.
      // We don't wait for the complete line.
      if (commandLength == 0 && c == 'U') {
        commandBuffer[commandLength++] = c;
        continue;
      }

      if (commandLength == 1 && commandBuffer[0] == 'U' && c == ' ') {
        commandBuffer[commandLength++] = c;
        continue;
      }

      // If this is an U command, look for the third space.
      if (commandLength >= 2 &&
          commandBuffer[0] == 'U' &&
          c != '\r' &&
          c != '\n') {

        if (c == ' ') {
          commandBuffer[commandLength] = '\0';

          if (startUpload(commandBuffer + 2)) {
            commandLength = 0;
            continue;
          }

          commandLength = 0;
          continue;
        }

        if (commandLength < BUFFER_SIZE - 1) {
          commandBuffer[commandLength++] = c;
        } else {
          Serial.println("[NTOS] ERROR: command too long");
          commandLength = 0;
        }

        continue;
      }

      // Normal command mode
      if (c == '\r' || c == '\n') {
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

  static bool openAppFile(
    const char* appName,
    const char* fileName,
    File& file) {

    char path[64];

    snprintf(
      path,
      sizeof(path),
      "/%s/%s",
      appName,
      fileName);

    file = SD.open(path, FILE_READ);

    return file;
  }

private:

  static void processCommand(const char* command) {

    // Delete app
    if (command[0] == 'D' && command[1] == ' ') {
      deleteApp(command + 2);
      return;
    }
  }

  // arguments:
  // CalcApp main.ntx
  //
  // The HEX data has NOT been buffered.
  // The next bytes received from Serial are the HEX data.
  static bool startUpload(const char* arguments) {

    const char* separator = strchr(arguments, ' ');

    if (!separator) {
      Serial.println("[NTOS] ERROR: invalid U command");
      return false;
    }

    // App name
    char appName[9];

    size_t appLength = separator - arguments;

    if (appLength == 0 || appLength > 8) {
      Serial.println("[NTOS] ERROR: invalid app name");
      return false;
    }

    memcpy(appName, arguments, appLength);
    appName[appLength] = '\0';

    if (!isValidName(appName)) {
      Serial.println("[NTOS] ERROR: invalid app name");
      return false;
    }

    // Filename
    const char* fileNameStart = separator + 1;

    if (*fileNameStart == '\0') {
      Serial.println("[NTOS] ERROR: invalid filename");
      return false;
    }

    char fileName[13];

    size_t fileLength = strlen(fileNameStart);

    if (fileLength == 0 || fileLength >= sizeof(fileName)) {
      Serial.println("[NTOS] ERROR: invalid filename");
      return false;
    }

    memcpy(fileName, fileNameStart, fileLength + 1);

    if (!isValidFileName(fileName)) {
      Serial.println("[NTOS] ERROR: invalid filename");
      return false;
    }

    // Create app directory silently
    if (!SD.exists(appName))
      SD.mkdir(appName);

    // Build destination path
    char path[64];

    snprintf(
      path,
      sizeof(path),
      "/%s/%s",
      appName,
      fileName);

    // Replace existing file
    if (SD.exists(path))
      SD.remove(path);

    uploadFile = SD.open(path, FILE_WRITE);

    if (!uploadFile) {
      Serial.println("[NTOS] ERROR: upload file open failed");
      return false;
    }

    isUploading = true;
    hasHighNibble = false;
    highNibble = 0;

    return true;
  }

  static void processUploadByte(uint8_t c) {

    // LF terminates upload
    if (c == '\n') {
      finishUpload();
      return;
    }

    // Ignore CR
    if (c == '\r')
      return;

    processHexCharacter((char)c);
  }

  static void processHexCharacter(char c) {

    int8_t value = hexValue(c);

    if (value < 0) {
      Serial.println("[NTOS] ERROR: invalid HEX data");
      abortUpload();
      return;
    }

    if (!hasHighNibble) {
      highNibble = value;
      hasHighNibble = true;
      return;
    }

    uint8_t valueByte =
      (highNibble << 4) | value;

    uploadFile.write(valueByte);

    hasHighNibble = false;
  }

  static int8_t hexValue(char c) {

    if (c >= '0' && c <= '9')
      return c - '0';

    if (c >= 'A' && c <= 'F')
      return c - 'A' + 10;

    if (c >= 'a' && c <= 'f')
      return c - 'a' + 10;

    return -1;
  }

  static void finishUpload() {

    if (hasHighNibble) {
      Serial.println("[NTOS] ERROR: incomplete HEX byte");
      abortUpload();
      return;
    }

    uploadFile.flush();
    uploadFile.close();

    isUploading = false;
    hasHighNibble = false;
    highNibble = 0;

    Serial.println("[NTOS] UPLOAD OK");
  }

  static void abortUpload() {

    if (uploadFile)
      uploadFile.close();

    isUploading = false;
    hasHighNibble = false;
    highNibble = 0;
  }

  static bool isValidName(const char* name) {

    if (!name || name[0] == '\0')
      return false;

    uint8_t length = 0;

    for (uint8_t i = 0; name[i] != '\0'; i++) {

      if (name[i] == '/' ||
          name[i] == '\\' ||
          name[i] == ' ')
        return false;

      length++;

      if (length > 8)
        return false;
    }

    return true;
  }

  static bool isValidFileName(const char* name) {

    if (!name || name[0] == '\0')
      return false;

    uint8_t length = 0;
    uint8_t dotPosition = 255;

    for (uint8_t i = 0; name[i] != '\0'; i++) {

      char c = name[i];

      if (c == '/' || c == '\\' || c == ' ')
        return false;

      if (c == '.') {

        if (dotPosition != 255)
          return false;

        dotPosition = i;
      }

      length++;

      if (length >= 13)
        return false;
    }

    if (length == 0)
      return false;

    if (dotPosition == 255) {

      if (length > 8)
        return false;

    } else {

      uint8_t baseLength = dotPosition;
      uint8_t extensionLength =
        length - dotPosition - 1;

      if (baseLength == 0 || baseLength > 8)
        return false;

      if (extensionLength == 0 || extensionLength > 3)
        return false;
    }

    return true;
  }

  static void deleteApp(const char* appName) {

    File dir = SD.open(appName);

    if (!dir)
      return;

    File file = dir.openNextFile();

    while (file) {

      char path[64];

      snprintf(
        path,
        sizeof(path),
        "%s/%s",
        appName,
        file.name());

      file.close();

      SD.remove(path);

      file = dir.openNextFile();
    }

    dir.close();

    SD.rmdir(appName);
  }
};