#include <Arduino.h>

// ============================================================
// TFT SHIELD SD PINS ON MEGA
// ============================================================

#define SD_CS    10
#define SD_MOSI  11
#define SD_MISO  12
#define SD_SCK   13


// ============================================================
// FAST GPIO
// ============================================================

static inline void fastWrite(uint8_t pin, uint8_t value)
{
    if (pin == 10)
    {
        if (value) PORTB |= _BV(4);
        else       PORTB &= ~_BV(4);
    }
    else if (pin == 11)
    {
        if (value) PORTB |= _BV(5);
        else       PORTB &= ~_BV(5);
    }
    else if (pin == 13)
    {
        if (value) PORTB |= _BV(7);
        else       PORTB &= ~_BV(7);
    }
}

static inline uint8_t fastRead(uint8_t pin)
{
    if (pin == 12)
        return (PINB & _BV(6)) ? HIGH : LOW;

    return LOW;
}


// ============================================================
// SOFTWARE SPI
// ============================================================

static uint8_t spiRec()
{
    uint8_t data = 0;

    for (uint8_t i = 0; i < 8; i++)
    {
        fastWrite(SD_SCK, HIGH);

        __asm__ __volatile__("nop\n\t");
        __asm__ __volatile__("nop\n\t");

        data <<= 1;

        if (fastRead(SD_MISO))
            data |= 1;

        fastWrite(SD_SCK, LOW);
    }

    return data;
}

static void spiSend(uint8_t data)
{
    for (uint8_t i = 0; i < 8; i++)
    {
        if (data & 0x80)
            fastWrite(SD_MOSI, HIGH);
        else
            fastWrite(SD_MOSI, LOW);

        fastWrite(SD_SCK, HIGH);

        __asm__ __volatile__("nop\n\t");
        __asm__ __volatile__("nop\n\t");

        fastWrite(SD_SCK, LOW);

        data <<= 1;
    }
}


// ============================================================
// SD CARD SELECT
// ============================================================

static void selectCard()
{
    fastWrite(SD_CS, LOW);
}

static void deselectCard()
{
    fastWrite(SD_CS, HIGH);

    // Extra clock after deselect
    spiRec();
}


// ============================================================
// SEND COMMAND
// ============================================================

static uint8_t sendCommand(uint8_t cmd, uint32_t arg, uint8_t crc)
{
    selectCard();

    spiSend(0x40 | cmd);

    spiSend((uint8_t)(arg >> 24));
    spiSend((uint8_t)(arg >> 16));
    spiSend((uint8_t)(arg >> 8));
    spiSend((uint8_t)arg);

    spiSend(crc);

    // Wait for response
    for (uint16_t i = 0; i < 1000; i++)
    {
        uint8_t response = spiRec();

        if ((response & 0x80) == 0)
            return response;
    }

    return 0xFF;
}


// ============================================================
// SEND APP COMMAND
// CMD55 + ACMDxx
// ============================================================

static uint8_t sendAppCommand(uint8_t cmd, uint32_t arg, uint8_t crc)
{
    uint8_t response;

    response = sendCommand(55, 0, 0x65);

    if (response > 1)
    {
        deselectCard();
        return response;
    }

    // IMPORTANT:
    // sendCommand() already has CS LOW.
    // We need to issue ACMD while card remains selected.

    spiSend(0x40 | cmd);

    spiSend((uint8_t)(arg >> 24));
    spiSend((uint8_t)(arg >> 16));
    spiSend((uint8_t)(arg >> 8));
    spiSend((uint8_t)arg);

    spiSend(crc);

    for (uint16_t i = 0; i < 1000; i++)
    {
        response = spiRec();

        if ((response & 0x80) == 0)
            return response;
    }

    return 0xFF;
}


// ============================================================
// SETUP
// ============================================================

void setup()
{
    Serial.begin(115200);

    delay(1000);

    Serial.println();
    Serial.println(F("================================"));
    Serial.println(F("NTOS SD RAW SOFTWARE SPI TEST"));
    Serial.println(F("================================"));

    Serial.println(F("Pins:"));
    Serial.println(F("CS   = D10"));
    Serial.println(F("MOSI = D11"));
    Serial.println(F("MISO = D12"));
    Serial.println(F("SCK  = D13"));
    Serial.println();


    // --------------------------------------------------------
    // PIN SETUP
    // --------------------------------------------------------

    pinMode(SD_CS, OUTPUT);
    pinMode(SD_MOSI, OUTPUT);
    pinMode(SD_MISO, INPUT);
    pinMode(SD_SCK, OUTPUT);

    fastWrite(SD_CS, HIGH);
    fastWrite(SD_MOSI, HIGH);
    fastWrite(SD_SCK, LOW);


    // --------------------------------------------------------
    // SD POWER-UP CLOCKS
    // --------------------------------------------------------

    Serial.println(F("Sending 80 clocks..."));

    for (uint8_t i = 0; i < 10; i++)
        spiSend(0xFF);

    Serial.println(F("Done."));
    Serial.println();


    // --------------------------------------------------------
    // CMD0
    // --------------------------------------------------------

    Serial.println(F("CMD0..."));

    uint8_t response = sendCommand(
        0,
        0,
        0x95
    );

    Serial.print(F("CMD0 response: 0x"));
    Serial.println(response, HEX);

    deselectCard();


    // --------------------------------------------------------
    // CMD8
    // --------------------------------------------------------

    Serial.println(F("CMD8..."));

    response = sendCommand(
        8,
        0x000001AA,
        0x87
    );

    Serial.print(F("CMD8 response: 0x"));
    Serial.println(response, HEX);

    if (response == 0x01)
    {
        Serial.println(F("R7:"));

        for (uint8_t i = 0; i < 4; i++)
        {
            uint8_t b = spiRec();

            Serial.print(F("0x"));

            if (b < 0x10)
                Serial.print('0');

            Serial.println(b, HEX);
        }
    }

    deselectCard();


    // --------------------------------------------------------
    // ACMD41
    // --------------------------------------------------------

    Serial.println();
    Serial.println(F("ACMD41..."));

    bool ready = false;

    for (uint16_t attempt = 0; attempt < 100; attempt++)
    {
        // CMD55
        response = sendCommand(
            55,
            0,
            0x65
        );

        Serial.print(F("CMD55 = 0x"));
        Serial.println(response, HEX);

        deselectCard();

        if (response > 1)
            break;


        // ACMD41
        response = sendCommand(
            41,
            0x40000000UL,
            0x77
        );

        Serial.print(F("ACMD41 = 0x"));
        Serial.println(response, HEX);

        deselectCard();

        if (response == 0x00)
        {
            ready = true;
            break;
        }

        delay(10);
    }


    // --------------------------------------------------------
    // RESULT
    // --------------------------------------------------------

    Serial.println();

    if (ready)
        Serial.println(F("SD CARD READY!"));
    else
        Serial.println(F("SD CARD DID NOT BECOME READY."));


    // --------------------------------------------------------
    // CMD58
    // --------------------------------------------------------

    if (ready)
    {
        Serial.println();
        Serial.println(F("CMD58..."));

        response = sendCommand(
            58,
            0,
            0xFD
        );

        Serial.print(F("CMD58 response: 0x"));
        Serial.println(response, HEX);

        if (response == 0x00)
        {
            Serial.println(F("OCR:"));

            uint32_t ocr = 0;

            for (uint8_t i = 0; i < 4; i++)
            {
                uint8_t b = spiRec();

                Serial.print(F("0x"));

                if (b < 0x10)
                    Serial.print('0');

                Serial.println(b, HEX);

                ocr = (ocr << 8) | b;
            }

            Serial.print(F("OCR = 0x"));
            Serial.println(ocr, HEX);

            if (ocr & 0x40000000UL)
                Serial.println(F("CCS = 1 -> SDHC/SDXC"));
            else
                Serial.println(F("CCS = 0 -> SDSC"));
        }

        deselectCard();
    }

    Serial.println();
    Serial.println(F("================================"));
    Serial.println(F("TEST COMPLETE"));
    Serial.println(F("================================"));
}

void loop()
{
}