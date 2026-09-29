#pragma once

#include <Arduino.h>
#include <EEPROM.h>

class EEPROMStorage
{
private:
    static constexpr byte VALID_MARKER = 0xA5;

    static bool IsValid(int address)
    {
        return EEPROM.read(address) == VALID_MARKER;
    }

    static void SetValid(int address)
    {
        EEPROM.update(address, VALID_MARKER);
    }

public:

    // =========================
    // INT
    // =========================

    static void SaveInt(int address, int value)
    {
        EEPROM.put(address + 1, value);

        // Write marker last
        SetValid(address);
    }

    static int LoadInt(int address, int defaultValue)
    {
        if (!IsValid(address))
            return defaultValue;

        int value;
        EEPROM.get(address + 1, value);

        return value;
    }


    // =========================
    // FLOAT
    // =========================

    static void SaveFloat(int address, float value)
    {
        EEPROM.put(address + 1, value);

        // Write marker last
        SetValid(address);
    }

    static float LoadFloat(int address, float defaultValue)
    {
        if (!IsValid(address))
            return defaultValue;

        float value;
        EEPROM.get(address + 1, value);

        return value;
    }


    // =========================
    // STRING
    // =========================

    static void SaveStr(int address, const char* value, int maxSize)
    {
        // Clear the entire string area
        for (int i = 0; i < maxSize; i++)
            EEPROM.update(address + 1 + i, 0);

        // Write string
        int length = strlen(value);

        if (length >= maxSize)
            length = maxSize - 1;

        for (int i = 0; i < length; i++)
            EEPROM.update(address + 1 + i, value[i]);

        // Null terminator
        EEPROM.update(address + 1 + length, '\0');

        // Write marker last
        SetValid(address);
    }

    static void LoadStr(
        int address,
        char* buffer,
        int maxSize,
        const char* defaultValue)
    {
        if (!IsValid(address))
        {
            strncpy(buffer, defaultValue, maxSize - 1);
            buffer[maxSize - 1] = '\0';
            return;
        }

        for (int i = 0; i < maxSize - 1; i++)
        {
            char c = EEPROM.read(address + 1 + i);

            buffer[i] = c;

            if (c == '\0')
                return;
        }

        buffer[maxSize - 1] = '\0';
    }
};