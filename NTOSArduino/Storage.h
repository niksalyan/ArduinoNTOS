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

      // --------------------------------------------------
      // Upload mode
      // --------------------------------------------------

      if (isUploading) {
        processUploadByte(c);
        continue;
      }

      // --------------------------------------------------
      // Normal command mode
      // --------------------------------------------------

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

  // ------------------------------------------------------
  // Command processing
  // ------------------------------------------------------

  static void processCommand(const char* command) {

    // Delete app
    if (command[0] == 'D' && command[1] == ' ') {

      deleteApp(command + 2);

      return;
    }

    // Upload file
    if (command[0] == 'U' && command[1] == ' ') {

      startUpload(command + 2);

      return;
    }
  }

  // ------------------------------------------------------
  // Start upload
  //
  // Format:
  //
  // U CalcApp main.ntx 895001FF...
  //
  // Everything after the second space is HEX data.
  // ------------------------------------------------------

  static void startUpload(const char* arguments) {

    // Find first space
    const char* separator1 = strchr(arguments, ' ');

    if (!separator1) {
      Serial.println("[NTOS] ERROR: invalid U command");
      return;
    }

    // Find second space
    const char* separator2 = strchr(separator1 + 1, ' ');

    if (!separator2) {
      Serial.println("[NTOS] ERROR: invalid U command");
      return;
    }

    // --------------------------------------------
    // Extract app name
    // --------------------------------------------

    char appName[9];

    size_t appLength = separator1 - arguments;

    if (appLength == 0 || appLength > 8) {
      Serial.println("[NTOS] ERROR: invalid app name");
      return;
    }

    memcpy(appName, arguments, appLength);
    appName[appLength] = '\0';

    if (!isValidName(appName)) {
      Serial.println("[NTOS] ERROR: invalid app name");
      return;
    }

    // --------------------------------------------
    // Extract filename
    // --------------------------------------------

    char fileName[13];

    size_t fileLength = separator2 - (separator1 + 1);

    if (fileLength == 0 || fileLength >= sizeof(fileName)) {
      Serial.println("[NTOS] ERROR: invalid filename");
      return;
    }

    memcpy(
      fileName,
      separator1 + 1,
      fileLength);

    fileName[fileLength] = '\0';

    // --------------------------------------------
    // Validate filename
    // --------------------------------------------

    if (!isValidFileName(fileName)) {
      Serial.println("[NTOS] ERROR: invalid filename");
      return;
    }

    // --------------------------------------------
    // Create app directory silently
    // --------------------------------------------

    if (!SD.exists(appName)) {

      SD.mkdir(appName);

      // We intentionally don't report this.
    }

    // --------------------------------------------
    // Build destination path
    // --------------------------------------------

    char path[64];

    snprintf(
      path,
      sizeof(path),
      "/%s/%s",
      appName,
      fileName);

    // --------------------------------------------
    // Remove existing file
    //
    // FILE_WRITE behaviour can append depending
    // on the filesystem implementation.
    // --------------------------------------------

    if (SD.exists(path))
      SD.remove(path);

    // --------------------------------------------
    // Open file
    // --------------------------------------------

    uploadFile = SD.open(path, FILE_WRITE);

    if (!uploadFile) {
      Serial.println("[NTOS] ERROR: upload file open failed");
      return;
    }

    // --------------------------------------------
    // Enter upload mode
    // --------------------------------------------

    isUploading = true;

    hasHighNibble = false;
    highNibble = 0;

    // --------------------------------------------
    // Process HEX already present after filename
    //
    // separator2 points to the space immediately
    // before the HEX data.
    // --------------------------------------------

    const char* hexData = separator2 + 1;

    while (*hexData != '\0') {

      processHexCharacter(*hexData);

      hexData++;
    }
  }

  // ------------------------------------------------------
  // Process incoming upload byte
  // ------------------------------------------------------

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

  // ------------------------------------------------------
  // Process one HEX character
  // ------------------------------------------------------

  static void processHexCharacter(char c) {

    int8_t value = hexValue(c);

    // Invalid HEX
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

    // Combine two nibbles into one byte
    uint8_t valueByte =
      (highNibble << 4) | value;

    uploadFile.write(valueByte);

    hasHighNibble = false;
  }

  // ------------------------------------------------------
  // HEX conversion
  // ------------------------------------------------------

  static int8_t hexValue(char c) {

    if (c >= '0' && c <= '9')
      return c - '0';

    if (c >= 'A' && c <= 'F')
      return c - 'A' + 10;

    if (c >= 'a' && c <= 'f')
      return c - 'a' + 10;

    return -1;
  }

  // ------------------------------------------------------
  // Finish upload
  // ------------------------------------------------------

  static void finishUpload() {

    // Odd number of HEX characters
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

  // ------------------------------------------------------
  // Abort upload
  // ------------------------------------------------------

  static void abortUpload() {

    if (uploadFile)
      uploadFile.close();

    isUploading = false;

    hasHighNibble = false;
    highNibble = 0;
  }

  // ------------------------------------------------------
  // Application name validation
  // ------------------------------------------------------

  static bool isValidName(const char* name) {

    if (!name || name[0] == '\0')
      return false;

    uint8_t length = 0;

    for (uint8_t i = 0; name[i] != '\0'; i++) {

      if (name[i] == '/' || name[i] == '\\' || name[i] == ' ')
        return false;

      length++;

      if (length > 8)
        return false;
    }

    return true;
  }

  // ------------------------------------------------------
  // Filename validation
  //
  // Simple FAT-style 8.3 validation.
  // ------------------------------------------------------

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

        // Only one dot
        if (dotPosition != 255)
          return false;

        dotPosition = i;
      }

      length++;

      if (length >= 13)
        return false;
    }

    // Must contain a name
    if (length == 0)
      return false;

    // 8.3 validation
    if (dotPosition == 255) {

      // No extension
      if (length > 8)
        return false;

    } else {

      uint8_t baseLength = dotPosition;
      uint8_t extensionLength = length - dotPosition - 1;

      if (baseLength == 0 || baseLength > 8)
        return false;

      if (extensionLength == 0 || extensionLength > 3)
        return false;
    }

    return true;
  }

  // ------------------------------------------------------
  // Recursive directory deletion
  // ------------------------------------------------------

  static void deleteApp(const char* appName) {
    File dir = SD.open(appName);
    if (!dir) return;

    File file = dir.openNextFile();

    while (file) {
      char path[64];
      snprintf(path, sizeof(path), "%s/%s", appName, file.name());

      file.close();
      SD.remove(path);

      file = dir.openNextFile();
    }

    dir.close();
    SD.rmdir(appName);
  }

  
};