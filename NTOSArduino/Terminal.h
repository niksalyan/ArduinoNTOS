#pragma once
#include <Keypad.h>
#include <MCUFRIEND_kbv.h>
#include <Adafruit_GFX.h>
#include <EEPROM.h>



#define SERIAL_ENABLED true

// ============================================================
// TFT
// ============================================================
class MCUFRIEND8B_kbv : public MCUFRIEND_kbv
{
public:
    // ============================================================
    // BEGIN
    // ============================================================

    uint16_t beginFast(uint16_t id = 0x9486)
    {
        MCUFRIEND_kbv::begin(id);

        setRotation(3);

        /*
            Data bus outputs

            PH3 PH4 PH5 PH6
            PE3 PE4 PE5
            PG5
        */

        DDRH |= 0x78;
        DDRE |= 0x38;
        DDRG |= 0x20;


        /*
            Control pins

            PF1 = WR
            PF2 = RS
            PF3 = CS
        */

        DDRF |= 0x0E;


        /*
            Idle state
        */

        PORTF |= 0x0E;
    }


    // ============================================================
    // LOW LEVEL 8-BIT WRITE
    // ============================================================

    static inline void write8(uint8_t value)
    {
        /*
            D0,D1,D6,D7 -> PORT H
        */

        PORTH =
            (PORTH & ~0x78) |
            ((value & 0x03) << 5) |
            ((value & 0xC0) >> 3);


        /*
            D2,D3,D5 -> PORT E
        */

        PORTE =
            (PORTE & ~0x38) |
            ((value & 0x0C) << 2) |
            ((value & 0x20) >> 2);


        /*
            D4 -> PORT G
        */

        PORTG =
            (PORTG & ~0x20) |
            ((value & 0x10) << 1);


        /*
            WR pulse
        */

        PORTF &= ~0x02;
        PORTF |=  0x02;
    }


    // ============================================================
    // COMMAND
    // ============================================================

    static inline void writeCommand(uint8_t command)
    {
        /*
            CS LOW
            RS LOW
        */

        PORTF &= ~0x0C;

        write8(command);

        /*
            RS HIGH
        */

        PORTF |= 0x04;
    }


    // ============================================================
    // DATA
    // ============================================================

    static inline void writeData(uint8_t value)
    {
        write8(value);
    }


    // ============================================================
    // ADDRESS WINDOW
    // ============================================================

    static inline void setWindow(
        uint16_t x0,
        uint16_t y0,
        uint16_t x1,
        uint16_t y1)
    {
        /*
            COLUMN ADDRESS SET
        */

        writeCommand(0x2A);

        writeData(x0 >> 8);
        writeData(x0);
        writeData(x1 >> 8);
        writeData(x1);


        /*
            PAGE ADDRESS SET
        */

        writeCommand(0x2B);

        writeData(y0 >> 8);
        writeData(y0);
        writeData(y1 >> 8);
        writeData(y1);


        /*
            MEMORY WRITE
        */

        writeCommand(0x2C);
    }


    // ============================================================
    // FAST BLACK SCREEN
    // ============================================================

    /*
        This is the special function you asked for.

        Black RGB565:

            0x0000

        Therefore every pixel consists of:

            00 00

        We can completely eliminate the normal
        color/port calculations.

        This is intended specifically for:

            TFT screen clearing
    */

    void fillScreenBlack()
    {
        /*
            CS LOW
        */

        PORTF &= ~0x08;


        /*
            480x320 window
        */

        setWindow(
            0,
            0,
            width() - 1,
            height() - 1
        );


        /*
            The data bus must contain zero.

            Clear only the TFT data bits.

            PH3,PH4,PH5,PH6
            PE3,PE4,PE5
            PG5
        */

        PORTH &= ~0x78;
        PORTE &= ~0x38;
        PORTG &= ~0x20;


        /*
            480 * 320 pixels
        */

        uint32_t pixels =
            (uint32_t)width() * height();


        /*
            Two bytes per RGB565 pixel.

            Since both bytes are 0, we only need
            two WR pulses.

            The data bus remains zero for the
            entire operation.

            This is the hot loop.
        */

        while (pixels--)
        {
            /*
                HIGH BYTE = 0x00

                WR pulse
            */

            PORTF &= ~0x02;
            PORTF |=  0x02;


            /*
                LOW BYTE = 0x00

                WR pulse
            */

            PORTF &= ~0x02;
            PORTF |=  0x02;
        }


        /*
            CS HIGH
        */

        PORTF |= 0x08;
    }


    // ============================================================
    // FAST SOLID RECTANGLE
    // ============================================================

    void fastFillRect(
        uint16_t x,
        uint16_t y,
        uint16_t w,
        uint16_t h,
        uint16_t color)
    {
        if (w == 0 || h == 0)
            return;

        if (x >= width() || y >= height())
            return;

        if ((uint32_t)x + w > width())
            w = width() - x;

        if ((uint32_t)y + h > height())
            h = height() - y;


        /*
            Special-case black.

            This is faster than the generic path.
        */

        if (color == 0x0000)
        {
            fastFillRectBlack(x, y, w, h);
            return;
        }


        PORTF &= ~0x08;

        setWindow(
            x,
            y,
            x + w - 1,
            y + h - 1
        );


        uint8_t hi = color >> 8;
        uint8_t lo = color;


        /*
            Precalculate physical port values.
        */

        const uint8_t hiH =
            ((hi & 0x03) << 5) |
            ((hi & 0xC0) >> 3);

        const uint8_t hiE =
            ((hi & 0x0C) << 2) |
            ((hi & 0x20) >> 2);

        const uint8_t hiG =
            (hi & 0x10) << 1;


        const uint8_t loH =
            ((lo & 0x03) << 5) |
            ((lo & 0xC0) >> 3);

        const uint8_t loE =
            ((lo & 0x0C) << 2) |
            ((lo & 0x20) >> 2);

        const uint8_t loG =
            (lo & 0x10) << 1;


        uint32_t count =
            (uint32_t)w * h;


        while (count--)
        {
            /*
                HIGH BYTE
            */

            PORTH =
                (PORTH & ~0x78) |
                hiH;

            PORTE =
                (PORTE & ~0x38) |
                hiE;

            PORTG =
                (PORTG & ~0x20) |
                hiG;

            PORTF &= ~0x02;
            PORTF |=  0x02;


            /*
                LOW BYTE
            */

            PORTH =
                (PORTH & ~0x78) |
                loH;

            PORTE =
                (PORTE & ~0x38) |
                loE;

            PORTG =
                (PORTG & ~0x20) |
                loG;

            PORTF &= ~0x02;
            PORTF |=  0x02;
        }


        PORTF |= 0x08;
    }


    // ============================================================
    // FAST BLACK RECTANGLE
    // ============================================================

    void fastFillRectBlack(
        uint16_t x,
        uint16_t y,
        uint16_t w,
        uint16_t h)
    {
        if (w == 0 || h == 0)
            return;


        PORTF &= ~0x08;


        setWindow(
            x,
            y,
            x + w - 1,
            y + h - 1
        );


        /*
            RGB565 black = 0x0000.

            Set data bus to zero ONCE.
        */

        PORTH &= ~0x78;
        PORTE &= ~0x38;
        PORTG &= ~0x20;


        uint32_t count =
            (uint32_t)w * h;


        /*
            Two WR pulses per pixel.
        */

        while (count--)
        {
            PORTF &= ~0x02;
            PORTF |=  0x02;

            PORTF &= ~0x02;
            PORTF |=  0x02;
        }


        PORTF |= 0x08;
    }
};



inline MCUFRIEND8B_kbv tft;
#define TFT_DARKESTGREY    0x4208    /* 64, 64, 64 */
#define TFT_BLUE           0x00AA
#define TFT_LIGHTBLUE      0x001F      /*   0,   0, 255 */
#define TFT_CYAN           0x04F6      



// ============================================================
// KEYPAD
// ============================================================

const byte ROWS = 4;
const byte COLS = 4;

inline char keys[ROWS][COLS] = {
  { '1', '2', '3', 'A' },
  { '4', '5', '6', 'B' },
  { '7', '8', '9', 'C' },
  { '*', '0', '#', 'D' }
};

inline byte colPins[COLS] = { 23, 25, 27, 29 };
inline byte rowPins[ROWS] = { 31, 33, 35, 37 };

inline Keypad keypad = Keypad(
  makeKeymap(keys),
  rowPins,
  colPins,
  ROWS,
  COLS);

typedef void (*KeyHandler)(char);
typedef void (*EventHandler)();

class Terminal {
public:

  inline static KeyHandler keyHandler = nullptr;
  inline static KeyHandler serialHandler = nullptr;
  inline static EventHandler drawHandler = nullptr;
  inline static EventHandler updateHandler = nullptr;


  static void begin() {
    if (SERIAL_ENABLED) {
        Serial.begin(115200);
    }
    uint16_t ID = tft.readID();

    if (ID == 0xD3D3) {
      ID = 0x9486;
    }

    tft.beginFast(ID);
    // tft.invertDisplay(true);

    tft.setRotation(3);
    tft.setTextSize(1);
  }

  static void setHandlers(KeyHandler kh, EventHandler dh = nullptr, EventHandler uh = nullptr) {
    keyHandler = kh;
    drawHandler = dh;
    updateHandler = uh;
    if (drawHandler) {
        drawHandler();
    }
  }

  static char getSerialKey() {
    if (!SERIAL_ENABLED) return 0;
    if (Serial.available() > 0)
    {
        char key = Serial.read();
        if (serialHandler) {
            serialHandler(key);
        }
        if (key > 32 && key < 127) {
            return key;
        } 
    }
    return 0;
  }

  static char getKey() {
    char sk = getSerialKey();
    if (sk > 0) {
        return sk;
    }
    return keypad.getKey();
  }


  static void update() {

    char sk = getKey();
    if (sk > 0 && keyHandler) {
        keyHandler(sk);
    }

    if (updateHandler) {
        updateHandler();
    }
  }

  static void sendData(const char* key, const char* val, int index = -1) {
    if (!SERIAL_ENABLED) return;
    Serial.print(key);
    if (index >= 0) {
        Serial.print(index);
    }
    Serial.print('=');
    Serial.print(val);
    Serial.print('\n');
  }

  static void sendData(const char* key, const long val, int index = -1) {
    if (!SERIAL_ENABLED) return;
    Serial.print(key);
    if (index >= 0) {
        Serial.print(index);
    }
    Serial.print('=');
    Serial.print(val);
    Serial.print('\n');
  }
};