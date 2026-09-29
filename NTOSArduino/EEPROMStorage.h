#pragma once

#include <Arduino.h>
#include <EEPROM.h>

class EEPROMStorage {
private:
  static constexpr uint8_t VALID_MARKER = 0xA5;

  static bool IsValid(uint16_t address) {
    return EEPROM.read(address) == VALID_MARKER;
  }

  static void SetValid(uint16_t address) {
    EEPROM.update(address, VALID_MARKER);
  }

public:

  // =========================================================
  // UINT32
  // =========================================================

  static void SaveInt(
    uint16_t address,
    uint32_t value) {
    EEPROM.put(address + 1, value);

    // Valid marker is written last.
    SetValid(address);
  }

  static uint32_t LoadInt(
    uint16_t address,
    uint32_t defaultValue) {
    if (!IsValid(address))
      return defaultValue;

    uint32_t value;

    EEPROM.get(address + 1, value);

    return value;
  }


  // =========================================================
  // FLOAT
  // =========================================================

  static void SaveFloat(
    uint16_t address,
    float value) {
    EEPROM.put(address + 1, value);

    SetValid(address);
  }

  static float LoadFloat(
    uint16_t address,
    float defaultValue) {
    if (!IsValid(address))
      return defaultValue;

    float value;

    EEPROM.get(address + 1, value);

    return value;
  }


  // =========================================================
  // STRING
  // =========================================================

  // =========================================================
  // STRING
  // =========================================================

  static void SaveStr(
    uint16_t address,
    const char* value,
    uint16_t maxSize) {
    if (value == nullptr || maxSize == 0)
      return;

    // Keep one byte for '\0'
    uint16_t length = strlen(value);

    if (length >= maxSize)
      length = maxSize - 1;

    // Write string
    for (uint16_t i = 0; i < length; i++) {
      EEPROM.update(
        address + 1 + i,
        value[i]);
    }

    // Null terminator
    EEPROM.update(
      address + 1 + length,
      '\0');

    // Clear the rest of the allocated slot.
    // This prevents an old longer value from surviving
    // after saving a shorter value.
    for (uint16_t i = length + 1; i < maxSize; i++) {
      EEPROM.update(
        address + 1 + i,
        '\0');
    }

    // Mark valid LAST.
    SetValid(address);
  }

  static void LoadStr(
    uint16_t address,
    char* buffer,
    uint16_t maxSize,
    const char* defaultValue) {
    if (buffer == nullptr || maxSize == 0)
      return;

    // No valid value stored.
    if (!IsValid(address)) {
      if (defaultValue == nullptr) {
        buffer[0] = '\0';
        return;
      }

      strncpy(
        buffer,
        defaultValue,
        maxSize - 1);

      buffer[maxSize - 1] = '\0';
      return;
    }

    // Read stored string.
    for (uint16_t i = 0; i < maxSize - 1; i++) {
      char value = EEPROM.read(
        address + 1 + i);

      buffer[i] = value;

      if (value == '\0')
        return;
    }

    // Guarantee termination even if EEPROM
    // contains a corrupted/non-terminated string.
    buffer[maxSize - 1] = '\0';
  }
};