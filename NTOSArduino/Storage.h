#pragma once

#define SOFTWARE_SPI_FOR_SD
#include <SoftSD.h>

class Storage {
private:
  static constexpr uint8_t BUFFER_SIZE = 32;

  inline static char commandBuffer[BUFFER_SIZE];
  inline static uint8_t commandLength = 0;

  // Last U command
  inline static char uploadApp[9] = { 0 };
  inline static char uploadFileName[13] = { 0 };

  // Upload state
  inline static bool isUploading = false;
  inline static File uploadFile;

  inline static bool hasHighNibble = false;
  inline static uint8_t highNibble = 0;

public:

  // ============================================================
  // BEGIN
  // ============================================================

  static void begin() {

    Serial.begin(115200);

    SD.begin();

    Serial.println();
    Serial.println("[NTOS] Storage ready");
  }


  // ============================================================
  // UPDATE
  // ============================================================

  static void update() {

    while (Serial.available()) {

      char c = Serial.read();


      // ========================================================
      // UPLOAD MODE
      // ========================================================

      if (isUploading) {

        // '>' finishes upload
        if (c == '>') {

          finishUpload();

          continue;
        }


        // Ignore line endings inside upload
        if (
          c == '\r' ||
          c == '\n'
        )
          continue;


        processHexCharacter(c);

        continue;
      }


      // ========================================================
      // START UPLOAD
      // ========================================================

      if (c == '<') {

        if (!beginUpload()) {

          Serial.println(
            "[NTOS] ERROR: upload start failed"
          );
        }

        continue;
      }


      // ========================================================
      // NORMAL COMMAND
      // ========================================================

      if (c == '\r')
        continue;


      // ========================================================
      // END COMMAND
      // ========================================================

      if (c == '\n') {

        if (commandLength == 0)
          continue;


        commandBuffer[commandLength] = '\0';

        processCommand(commandBuffer);

        commandLength = 0;

        continue;
      }


      // ========================================================
      // COMMAND BUFFER
      // ========================================================

      if (commandLength < BUFFER_SIZE - 1) {

        commandBuffer[commandLength++] = c;

      } else {

        Serial.println(
          "[NTOS] ERROR: command too long"
        );

        commandLength = 0;
      }
    }
  }


  // ============================================================
  // COMMAND
  // ============================================================

  static void processCommand(const char* command) {

    // ----------------------------------------------------------
    // D MyApp
    // ----------------------------------------------------------

    if (
      command[0] == 'D' &&
      command[1] == ' '
    ) {

      deleteApp(command + 2);

      return;
    }


    // ----------------------------------------------------------
    // U MyApp main.ntx
    // ----------------------------------------------------------

    if (
      command[0] == 'U' &&
      command[1] == ' '
    ) {

      prepareUpload(command + 2);

      return;
    }


    Serial.print(
      "[NTOS] ERROR: unknown command: "
    );

    Serial.println(command);
  }


  // ============================================================
  // PREPARE UPLOAD
  //
  // U MyApp main.ntx
  //
  // Only remembers the destination.
  //
  // ============================================================

  static bool prepareUpload(const char* arguments) {

    // ----------------------------------------------------------
    // IMPORTANT:
    //
    // Clear the previous target FIRST.
    //
    // This prevents a failed U command from accidentally
    // reusing an old target.
    // ----------------------------------------------------------

    uploadApp[0] = '\0';
    uploadFileName[0] = '\0';


    // ----------------------------------------------------------
    // Find separator
    // ----------------------------------------------------------

    const char* separator =
      strchr(arguments, ' ');


    if (!separator) {

      Serial.println(
        "[NTOS] ERROR: invalid U command"
      );

      return false;
    }


    // ----------------------------------------------------------
    // App name
    // ----------------------------------------------------------

    size_t appLength =
      separator - arguments;


    if (
      appLength == 0 ||
      appLength > 8
    ) {

      Serial.println(
        "[NTOS] ERROR: invalid app name"
      );

      return false;
    }


    memcpy(
      uploadApp,
      arguments,
      appLength
    );

    uploadApp[appLength] = '\0';


    if (!isValidName(uploadApp)) {

      uploadApp[0] = '\0';

      Serial.println(
        "[NTOS] ERROR: invalid app name"
      );

      return false;
    }


    // ----------------------------------------------------------
    // Filename
    // ----------------------------------------------------------

    const char* filename =
      separator + 1;


    if (!isValidFileName(filename)) {

      uploadApp[0] = '\0';

      Serial.println(
        "[NTOS] ERROR: invalid filename"
      );

      return false;
    }


    strcpy(
      uploadFileName,
      filename
    );


    // ----------------------------------------------------------
    // Make sure there isn't another space after filename
    //
    // isValidFileName() already rejects spaces, so this is
    // effectively enforced there.
    // ----------------------------------------------------------

    Serial.print("[NTOS] TARGET: /");
    Serial.print(uploadApp);
    Serial.print("/");
    Serial.println(uploadFileName);


    return true;
  }


  // ============================================================
  // BEGIN UPLOAD
  //
  // <
  //
  // Starts writing to the remembered destination.
  // ============================================================

  static bool beginUpload() {

    // ----------------------------------------------------------
    // A U command must have been received first.
    // ----------------------------------------------------------

    if (
      uploadApp[0] == '\0' ||
      uploadFileName[0] == '\0'
    ) {

      Serial.println(
        "[NTOS] ERROR: no upload target"
      );

      return false;
    }


    // ----------------------------------------------------------
    // Create application directory
    // ----------------------------------------------------------

    if (!SD.exists(uploadApp)) {

      if (!SD.mkdir(uploadApp)) {

        Serial.println(
          "[NTOS] ERROR: cannot create app"
        );

        return false;
      }
    }


    // ----------------------------------------------------------
    // Build path
    // ----------------------------------------------------------

    char path[32];

    snprintf(
      path,
      sizeof(path),
      "/%s/%s",
      uploadApp,
      uploadFileName
    );


    Serial.print(
      "[NTOS] UPLOAD: "
    );

    Serial.println(path);


    // ----------------------------------------------------------
    // Remove existing file
    // ----------------------------------------------------------

    if (SD.exists(path)) {

      if (!SD.remove(path)) {

        Serial.println(
          "[NTOS] ERROR: cannot overwrite file"
        );

        return false;
      }
    }


    // ----------------------------------------------------------
    // Open new file
    // ----------------------------------------------------------

    uploadFile =
      SD.open(
        path,
        FILE_WRITE
      );


    if (!uploadFile) {

      Serial.println(
        "[NTOS] ERROR: cannot open file"
      );

      return false;
    }


    // ----------------------------------------------------------
    // Reset HEX decoder
    // ----------------------------------------------------------

    hasHighNibble = false;
    highNibble = 0;

    isUploading = true;


    Serial.println(
      "[NTOS] UPLOAD START"
    );


    return true;
  }


  // ============================================================
  // HEX CHARACTER
  // ============================================================

  static void processHexCharacter(char c) {

    int8_t value =
      hexValue(c);


    // ----------------------------------------------------------
    // Invalid HEX
    // ----------------------------------------------------------

    if (value < 0) {

      Serial.print(
        "[NTOS] ERROR: invalid HEX: "
      );

      Serial.println(c);

      abortUpload();

      return;
    }


    // ----------------------------------------------------------
    // First nibble
    // ----------------------------------------------------------

    if (!hasHighNibble) {

      highNibble = value;

      hasHighNibble = true;

      return;
    }


    // ----------------------------------------------------------
    // Second nibble
    // ----------------------------------------------------------

    uint8_t byteValue =
      (highNibble << 4) | value;


    // ----------------------------------------------------------
    // Write byte
    // ----------------------------------------------------------

    size_t written =
      uploadFile.write(byteValue);


    if (written != 1) {

      Serial.println(
        "[NTOS] ERROR: SD write failed"
      );

      abortUpload();

      return;
    }


    hasHighNibble = false;
  }


  // ============================================================
  // FINISH UPLOAD
  //
  // >
  // ============================================================

  static void finishUpload() {

    // ----------------------------------------------------------
    // Odd number of HEX characters
    // ----------------------------------------------------------

    if (hasHighNibble) {

      Serial.println(
        "[NTOS] ERROR: incomplete HEX byte"
      );

      abortUpload();

      return;
    }


    // ----------------------------------------------------------
    // Flush
    // ----------------------------------------------------------

    uploadFile.flush();


    // ----------------------------------------------------------
    // Close
    // ----------------------------------------------------------

    uploadFile.close();


    isUploading = false;


    Serial.println(
      "[NTOS] UPLOAD OK"
    );
  }


  // ============================================================
  // ABORT UPLOAD
  //
  // IMPORTANT:
  // Remove the partially-written file.
  // ============================================================

  static void abortUpload() {

    if (uploadFile)
      uploadFile.close();


    isUploading = false;

    hasHighNibble = false;
    highNibble = 0;


    // ----------------------------------------------------------
    // Remove partial file.
    // ----------------------------------------------------------

    if (
      uploadApp[0] == '\0' ||
      uploadFileName[0] == '\0'
    )
      return;


    char path[32];

    snprintf(
      path,
      sizeof(path),
      "/%s/%s",
      uploadApp,
      uploadFileName
    );


    SD.remove(path);
  }


  // ============================================================
  // HEX VALUE
  // ============================================================

  static int8_t hexValue(char c) {

    if (
      c >= '0' &&
      c <= '9'
    )
      return c - '0';


    if (
      c >= 'A' &&
      c <= 'F'
    )
      return c - 'A' + 10;


    if (
      c >= 'a' &&
      c <= 'f'
    )
      return c - 'a' + 10;


    return -1;
  }


  // ============================================================
  // VALID APP NAME
  // ============================================================

  static bool isValidName(const char* name) {

    if (
      !name ||
      name[0] == '\0'
    )
      return false;


    uint8_t length = 0;


    while (name[length] != '\0') {

      char c =
        name[length];


      if (
        c == '/' ||
        c == '\\' ||
        c == ' '
      )
        return false;


      length++;


      if (length > 8)
        return false;
    }


    return true;
  }


  // ============================================================
  // VALID 8.3 FILENAME
  // ============================================================

  static bool isValidFileName(
    const char* name
  ) {

    if (
      !name ||
      name[0] == '\0'
    )
      return false;


    uint8_t length = 0;
    int8_t dot = -1;


    while (name[length] != '\0') {

      char c =
        name[length];


      if (
        c == '/' ||
        c == '\\' ||
        c == ' '
      )
        return false;


      if (c == '.') {

        if (dot >= 0)
          return false;

        dot = length;
      }


      length++;


      if (length > 12)
        return false;
    }


    // ----------------------------------------------------------
    // No extension
    // ----------------------------------------------------------

    if (dot < 0)
      return length <= 8;


    // ----------------------------------------------------------
    // 8.3
    // ----------------------------------------------------------

    uint8_t baseLength =
      dot;


    uint8_t extensionLength =
      length - dot - 1;


    return
      baseLength >= 1 &&
      baseLength <= 8 &&
      extensionLength >= 1 &&
      extensionLength <= 3;
  }


  // ============================================================
  // DELETE APP
  //
  // D MyApp
  // ============================================================

  static void deleteApp(
    const char* appName
  ) {

    // ----------------------------------------------------------
    // Validate first
    // ----------------------------------------------------------

    if (!isValidName(appName)) {

      Serial.println(
        "[NTOS] ERROR: invalid app name"
      );

      return;
    }


    // ----------------------------------------------------------
    // Clear remembered target BEFORE doing anything else.
    //
    // This fixes the stale-target bug when the directory does
    // not exist.
    // ----------------------------------------------------------

    if (
      strcmp(
        uploadApp,
        appName
      ) == 0
    ) {

      uploadApp[0] = '\0';
      uploadFileName[0] = '\0';
    }


    // ----------------------------------------------------------
    // Open directory
    // ----------------------------------------------------------

    File dir =
      SD.open(appName);


    if (!dir) {

      Serial.println(
        "[NTOS] DELETE OK"
      );

      return;
    }


    // ----------------------------------------------------------
    // Delete files
    // ----------------------------------------------------------

    while (true) {

      File file =
        dir.openNextFile();


      if (!file)
        break;


      char path[32];


      snprintf(
        path,
        sizeof(path),
        "/%s/%s",
        appName,
        file.name()
      );


      file.close();


      SD.remove(path);
    }


    dir.close();


    // ----------------------------------------------------------
    // Remove directory
    // ----------------------------------------------------------

    SD.rmdir(appName);


    Serial.println(
      "[NTOS] DELETE OK"
    );
  }


public:

  // ============================================================
  // OPEN APP FILE
  // ============================================================

  static bool openAppFile(
    const char* appName,
    const char* fileName,
    File& file
  ) {

    char path[32];


    snprintf(
      path,
      sizeof(path),
      "/%s/%s",
      appName,
      fileName
    );


    file =
      SD.open(
        path,
        FILE_READ
      );


    return file;
  }


  // ============================================================
  // LIST APPS
  // ============================================================

  static void listApps(
    void (*callback)(const char* name)
  ) {

    File root =
      SD.open("/");


    if (!root)
      return;


    while (true) {

      File entry =
        root.openNextFile();


      if (!entry)
        break;


      if (entry.isDirectory()) {

        char path[32];


        snprintf(
          path,
          sizeof(path),
          "/%s/main.ntx",
          entry.name()
        );


        if (SD.exists(path))
          callback(entry.name());
      }


      entry.close();
    }


    root.close();
  }
};