#define SOFTWARE_SPI_FOR_SD
#include <SoftSD.h>

class Storage {
private:
    static constexpr uint8_t BUFFER_SIZE = 32;

    static char commandBuffer[BUFFER_SIZE];
    static uint8_t commandLength;

public:
    static void begin() {
        SD.begin();
    }

    static void update() {
        while (Serial.available() > 0) {
            uint8_t c = Serial.read();

            if (c == 0x00) {
                commandBuffer[commandLength] = '\0';
                processCommand(commandBuffer);
                commandLength = 0;
                continue;
            }

            if (commandLength < BUFFER_SIZE - 1) {
                commandBuffer[commandLength++] = c;
            }
        }
    }

private:
    static void processCommand(const char* command) {
        // CREATE
        // DELETE
        // UPLOAD
    }
};

char Storage::commandBuffer[Storage::BUFFER_SIZE];
uint8_t Storage::commandLength = 0;