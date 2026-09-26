#pragma once

#define SOFTWARE_SPI_FOR_SD
#include <SoftSD.h>

class Storage {
private:
  static constexpr uint8_t COMMAND_SIZE = 32;
  static constexpr uint8_t WRITE_BUFFER_SIZE = 32;

  inline static char commandBuffer[COMMAND_SIZE];
  inline static uint8_t commandLength = 0;

  inline static char uploadApp[9] = "";
  inline static char uploadFileName[13] = "";

  inline static bool isUploading = false;
  inline static File uploadFile;

  // HEX decoder
  inline static bool hasHighNibble = false;
  inline static uint8_t highNibble = 0;

  // SD write buffer
  inline static uint8_t writeBuffer[WRITE_BUFFER_SIZE];
  inline static uint8_t writeBufferLength = 0;

  static void writeBufferToSD() {
    if (writeBufferLength == 0)
      return;

    if (uploadFile.write(writeBuffer, writeBufferLength)
        != writeBufferLength) {
      Serial.println("[NTOS] SD WRITE ERROR");
      abortUpload();
      return;
    }

    writeBufferLength = 0;
  }

public:

  static void begin() {
    Serial.begin(9600);
    SD.begin();

    Serial.println("[NTOS] Storage ready");
  }

  static void update() {
    while (Serial.available()) {
      char c = Serial.read();

      // Upload mode
      if (isUploading) {

        if (c == '>') {
          finishUpload();
          continue;
        }

        if (c == '\r' || c == '\n')
          continue;

        processHexCharacter(c);
        continue;
      }

      // Start upload
      if (c == '<') {

        if (isUploading) {
          abortUpload();
        }
        if (!beginUpload())
          Serial.println("UP ER");

        continue;
      }

      // Normal command
      if (c == '\r')
        continue;

      if (c == '\n') {
        if (commandLength == 0)
          continue;

        commandBuffer[commandLength] = '\0';
        processCommand(commandBuffer);
        commandLength = 0;
        continue;
      }

      if (commandLength < COMMAND_SIZE - 1) {
        commandBuffer[commandLength++] = c;
      } else {
        Serial.println("[NTOS] COMMAND TOO LONG");
        commandLength = 0;
      }
    }
  }

  static void processCommand(const char* command) {

    if (command[0] == 'D' && command[1] == ' ') {
      deleteApp(command + 2);
      return;
    }

    if (command[0] == 'U' && command[1] == ' ') {
      prepareUpload(command + 2);
      return;
    }

    Serial.println("[NTOS] UNKNOWN COMMAND");
  }

  static bool prepareUpload(const char* arguments) {
    const char* separator = strchr(arguments, ' ');

    if (!separator) {
      Serial.println("[NTOS] INVALID U");
      return false;
    }

    uint8_t appLength = separator - arguments;

    if (appLength == 0 || appLength > 8) {
      Serial.println("[NTOS] INVALID APP");
      return false;
    }

    memcpy(uploadApp, arguments, appLength);
    uploadApp[appLength] = '\0';

    if (!isValidName(uploadApp)) {
      uploadApp[0] = '\0';
      uploadFileName[0] = '\0';
      Serial.println("[NTOS] INVALID APP");
      return false;
    }

    const char* filename = separator + 1;

    if (!isValidFileName(filename)) {
      uploadApp[0] = '\0';
      uploadFileName[0] = '\0';
      Serial.println("[NTOS] INVALID FILE");
      return false;
    }

    strcpy(uploadFileName, filename);

    Serial.print("[NTOS] TARGET /");
    Serial.print(uploadApp);
    Serial.print("/");
    Serial.println(uploadFileName);

    return true;
  }

  static bool beginUpload() {
    if (uploadApp[0] == '\0' || uploadFileName[0] == '\0') {
      return false;
    }

    // Reset upload decoder state
    hasHighNibble = false;
    highNibble = 0;

    // Reset write buffer
    writeBufferLength = 0;

    char path[32];

    snprintf(path, sizeof(path),
             "/%s", uploadApp);

    SD.mkdir(path);

    snprintf(path, sizeof(path),
             "/%s/%s",
             uploadApp,
             uploadFileName);

    SD.remove(path);

    uploadFile = SD.open(path, FILE_WRITE);

    if (!uploadFile) {
      return false;
    }

    isUploading = true;

    Serial.println("[NTOS] UPLOAD START");

    return true;
  }

  static void processHexCharacter(char c) {

    int8_t value = hexValue(c);

    if (value < 0) {
      Serial.println("[NTOS] HEX ERROR");
      abortUpload();
      return;
    }

    if (!hasHighNibble) {
      highNibble = value;
      hasHighNibble = true;
      return;
    }

    uint8_t byteValue =
      (highNibble << 4) | value;

    hasHighNibble = false;

    writeBuffer[writeBufferLength++] = byteValue;

    if (writeBufferLength == WRITE_BUFFER_SIZE)
      writeBufferToSD();
  }


  static void finishUpload() {
    if (!isUploading)
      return;

    if (hasHighNibble) {
      Serial.println("[NTOS] ODD HEX");
      abortUpload();
      return;
    }

    writeBufferToSD();

    if (!isUploading)
      return;

    uploadFile.flush();
    uploadFile.close();

    isUploading = false;

    hasHighNibble = false;
    highNibble = 0;
    writeBufferLength = 0;

    Serial.println("[NTOS] UPLOAD OK");
  }

  static void abortUpload() {

    if (uploadFile)
      uploadFile.close();

    isUploading = false;
    hasHighNibble = false;
    highNibble = 0;
    writeBufferLength = 0;

    char path[32];

    if (uploadApp[0] != '\0' && uploadFileName[0] != '\0') {

      snprintf(
        path,
        sizeof(path),
        "/%s/%s",
        uploadApp,
        uploadFileName);

      SD.remove(path);
    }
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

  static bool isValidName(const char* name) {

    if (!name || name[0] == '\0')
      return false;

    uint8_t length = 0;

    while (name[length] != '\0') {

      char c = name[length];

      if (c == '/' || c == '\\' || c == ' ')
        return false;

      if (++length > 8)
        return false;
    }

    return true;
  }

  static bool isValidFileName(const char* name) {

    if (!name || name[0] == '\0')
      return false;

    uint8_t length = 0;
    int8_t dot = -1;

    while (name[length] != '\0') {

      char c = name[length];

      if (c == '/' || c == '\\' || c == ' ')
        return false;

      if (c == '.') {
        if (dot >= 0)
          return false;

        dot = length;
      }

      if (++length > 12)
        return false;
    }

    if (dot < 0)
      return length <= 8;

    uint8_t baseLength = dot;
    uint8_t extensionLength =
      length - dot - 1;

    return baseLength >= 1 && baseLength <= 8 && extensionLength >= 1 && extensionLength <= 3;
  }

  static void deleteApp(const char* appName) {

    if (!isValidName(appName)) {
      Serial.println("[NTOS] INVALID APP");
      return;
    }

    File dir = SD.open(appName);

    if (!dir) {
      if (strcmp(uploadApp, appName) == 0) {
        uploadApp[0] = '\0';
        uploadFileName[0] = '\0';
      }

      Serial.println("[NTOS] DELETE OK");
      return;
    }

    while (true) {

      File file = dir.openNextFile();

      if (!file)
        break;

      char path[32];

      snprintf(
        path,
        sizeof(path),
        "/%s/%s",
        appName,
        file.name());

      file.close();
      SD.remove(path);
    }

    dir.close();
    SD.rmdir(appName);

    if (strcmp(uploadApp, appName) == 0) {
      uploadApp[0] = '\0';
      uploadFileName[0] = '\0';
    }

    Serial.println("[NTOS] DELETE OK");
  }

  static bool openAppFile(
    const char* appName,
    const char* fileName,
    File& file) {
    char path[32];

    snprintf(
      path,
      sizeof(path),
      "/%s/%s",
      appName,
      fileName);

    file = SD.open(path, FILE_READ);

    return file;
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

        char path[32];

        snprintf(
          path,
          sizeof(path),
          "/%s/main.ntx",
          entry.name());

        if (SD.exists(path))
          callback(entry.name());
      }

      entry.close();
    }

    root.close();
  }
};