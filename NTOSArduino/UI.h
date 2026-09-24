#pragma once
#include "Terminal.h"

class UI {
public:
  inline static const char nameCharset[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";

  static void setHandler(KeyHandler kh) {
    Terminal::setHandlers(kh);
  }

  static void dialog(const char* text, uint16_t color = TFT_BLUE) {
    
    tft.fastFillRect(0, 0, tft.width(), 25, color);
    printCentered(text, 243, 9, TFT_BLACK, 2);
    printCentered(text, 240, 5, TFT_WHITE, 2);
    tft.setFont();
    
    tft.fastFillRectBlack(0, 25, tft.width(), tft.height() - 44);
  }

  static void fadeOut() {
    for (int y1 = 0; y1 < 8; y1++) {
      for (int x2 = 0; x2 < tft.width(); x2 += 8) {
        tft.fastFillRectBlack(y1 + x2, 0, 1, tft.height());
      }
      for (int y2 = 0; y2 < tft.height(); y2 += 8) {
        tft.fastFillRectBlack(0, y1 + y2, tft.width(), 1);
      }
    }
  }


  static void printMoney(long value, bool paint = false) {
    if (paint) {
      tft.setTextColor(moneyColor(value));
    }
    if (value >= 0) {
      tft.print("$");
      tft.print(value);
    } else {
      tft.print("-$");
      tft.print(-value);
    }
  }

  static uint16_t moneyColor(long value) {
    if (value <= 0) {
      return TFT_RED;
    } else {
      return value >= 200 ? TFT_GREEN : TFT_YELLOW;
    }
  }

  static const char* moneyString(long value) {
    static char buffer[16];

    if (value >= 0) {
      snprintf(buffer, sizeof(buffer), "$%ld", value);
    } else {
      snprintf(buffer, sizeof(buffer), "-$%ld", -value);
    }

    return buffer;
  }

  static char* getAmountText(long amount) {
    static char text[16];

    if (amount < 0) {
      sprintf(text, "-$%ld", -amount);
    } else {
      sprintf(text, "$%ld", amount);
    }

    return text;
  }

  static char* getPlayerNumberText(int player) {
    static char text[3];

    snprintf(text, sizeof(text), "%d", player + 1);

    return text;
  }

  // ----------------------------------------------------------
  // Top-center
  //
  // x = center
  // y = top edge
  // ----------------------------------------------------------

  static void printCentered(
    const char* text,
    int x,
    int y,
    uint16_t color = TFT_WHITE,
    uint8_t textSize = 0,
    uint8_t clear = 0) {
    if (text == nullptr)
      return;

    int16_t bx;
    int16_t by;
    uint16_t w;
    uint16_t h;

    if (textSize > 0) {
      tft.setTextSize(textSize);
    }

    tft.getTextBounds(
      text,
      0,
      0,
      &bx,
      &by,
      &w,
      &h);

    int16_t drawX = x - (w / 2) - bx;

    int16_t drawY = y - by;

    if (clear > 0) {
      tft.fastFillRectBlack(drawX - clear, drawY, w + clear * 2, h);
    }

    tft.setTextColor(color);
    tft.setCursor(drawX, drawY);
    tft.print(text);
  }

  // ==========================================================
  // TEXT
  // ==========================================================

  // ----------------------------------------------------------
  // Top-left
  //
  // x = left edge
  // y = top edge
  // ----------------------------------------------------------

  static void print(
    const char* text,
    int x,
    int y,
    uint16_t color = TFT_WHITE,
    uint8_t textSize = 0,
    uint8_t clear = 0
    ) {
    if (text == nullptr)
      return;

    int16_t bx;
    int16_t by;
    uint16_t w;
    uint16_t h;

    if (textSize > 0) {
      tft.setTextSize(textSize);
    }

    tft.getTextBounds(
      text,
      0,
      0,
      &bx,
      &by,
      &w,
      &h);

    // Adafruit_GFX's y coordinate is the baseline.
    // Adjust it so the actual text bounds start at y.
    int16_t drawX = x - bx;
    int16_t drawY = y - by;

    if (clear > 0) {
      tft.fastFillRectBlack(drawX, drawY, w + clear, h);
    }

    tft.setTextColor(color);
    tft.setCursor(drawX, drawY);
    tft.print(text);
  }


  // ----------------------------------------------------------
  // Top-right
  //
  // x = right edge
  // y = top edge
  // ----------------------------------------------------------

  static void printRight(
    const char* text,
    int x,
    int y,
    uint16_t color = TFT_WHITE,
    uint8_t textSize = 0,
    uint8_t clear = 0) {
    if (text == nullptr)
      return;

    int16_t bx;
    int16_t by;
    uint16_t w;
    uint16_t h;

    if (textSize > 0) {
      tft.setTextSize(textSize);
    }

    tft.getTextBounds(
      text,
      0,
      0,
      &bx,
      &by,
      &w,
      &h);

    int16_t drawX = x - (bx + w);
    int16_t drawY = y - by;

    if (clear > 0) {
      tft.fastFillRectBlack(drawX - clear, drawY, w + clear, h);
    }

    tft.setTextColor(color);
    tft.setCursor(drawX, drawY);
    tft.print(text);
  }

  static void button(char* text, int x, int y, uint16_t color) {
    tft.drawRoundRect(
      x,
      y,
      170,
      55,
      10,
      color);

    tft.setTextSize(2);
    printCentered(text, x + 85, y + 20, color);
  }

  // ---------------------------------------------------------
  // Player colors
  // ---------------------------------------------------------

  static uint16_t playerColor(uint8_t player)
  {
    switch (player)
    {
      
      case 0: return TFT_GREEN;
      case 1: return TFT_YELLOW;
      case 2: return TFT_CYAN;
      case 3: return TFT_MAGENTA;
      case 4: return TFT_ORANGE;
      case 5: return TFT_RED;

      default:
        return TFT_WHITE;
    }
  }

  static void alert(const char* line1, const char* line2) {
    drawPopupMessage(line1, "", line2);
    waitForKey(1000);
  }

  static void waitForKey(int ms) {
    for (int d = 0; d < ms; d++) {
      char key = Terminal::getKey();
      if (key) {
        return;
      }
      delay(1);
    }
  }

  static bool confirm(const char* line1) {
    drawPopupMessage(line1, "", "* = CANCEL    D = CONFIRM");
    while (true) {
      char key = Terminal::getKey();
      switch (key) {
        case 'D':
          return true;
        case '*':
          return false;
      }
    }
  }

  static int yesnocancel(const char* line1) {
    drawPopupMessage(line1, "", "* = CANCEL  # = NO  D = YES");
    while (true) {
      char key = Terminal::getKey();
      switch (key) {
        case 'D':
          return 1;
        case '#':
          return 0;
        case '*':
          return -1;
      }
    }
  }

  static int confirmNumber(const char* line1, const char* line2, const char* line3, int from, int to) {
    drawPopupMessage(line1, line2, line3);
    while (true) {
      char key = Terminal::getKey();
      if (key) {
        switch (key) {
          case '*':
            return -1;
          default:
            int num = key - '0';
            if (num >= from && num <= to) {
              return num;
            }
            break;
        }
      }
    }
  }

  static void drawPopupMessage(
    const char* line1,
    const char* line2,
    const char* line3) {
    tft.fastFillRectBlack(
      30,
      85,
      tft.width() - 60,
      tft.height() - 205);

    tft.drawRoundRect(
      30,
      85,
      tft.width() - 60,
      tft.height() - 205,
      10,
      TFT_YELLOW);

    tft.setTextSize(3);
    printCentered(
      line1,
      tft.width() / 2,
      105,
      TFT_YELLOW);

    tft.setTextSize(2);
    printCentered(
      line2,
      tft.width() / 2,
      145,
      TFT_WHITE);

    printCentered(
      line3,
      tft.width() / 2,
      165,
      TFT_WHITE);
  }

  static bool editText(
    const char* line1,
    char* text,
    int length) {

    if (text == nullptr || length <= 0) {
      return false;
    }

    // ----------------------------------------------------------
    // Temporary editing buffer
    // ----------------------------------------------------------

    char buffer[length + 1];

    for (int i = 0; i < length; i++) {
      buffer[i] = text[i];

      if (buffer[i] == '\0') {
        buffer[i] = ' ';
      }
    }

    buffer[length] = '\0';

    // ----------------------------------------------------------
    // Find charset length
    // ----------------------------------------------------------

    int charsetLength = 0;

    while (nameCharset[charsetLength] != '\0') {
      charsetLength++;
    }

    // ----------------------------------------------------------
    // Editor state
    // ----------------------------------------------------------

    int cursor = 0;

    // ----------------------------------------------------------
    // Layout
    // ----------------------------------------------------------

    const int charWidth = 20;
    const int textY = 140;
    const int totalWidth = length * charWidth;
    const int startX = (tft.width() - totalWidth) / 2;

    // ----------------------------------------------------------
    // Draw editor frame ONCE
    // ----------------------------------------------------------

    tft.fastFillRectBlack(
      30,
      85,
      tft.width() - 60,
      180);

    tft.drawRoundRect(
      30,
      85,
      tft.width() - 60,
      180,
      10,
      TFT_YELLOW);

    // ----------------------------------------------------------
    // Title
    // ----------------------------------------------------------

    tft.setTextSize(3);

    printCentered(
      line1,
      tft.width() / 2,
      105,
      TFT_YELLOW);

    // ----------------------------------------------------------
    // Controls
    // ----------------------------------------------------------

    tft.setTextSize(2);

    printCentered(
      "4 6 =    CURSOR",
      tft.width() / 2,
      185,
      TFT_WHITE);

    printCentered(
      "2 8 = CHARACTER",
      tft.width() / 2,
      205,
      TFT_WHITE);

    printCentered(
      "* = CANCEL    D = CONFIRM",
      tft.width() / 2,
      225,
      TFT_WHITE);

    // ----------------------------------------------------------
    // Draw ONLY the text/cursor area
    // ----------------------------------------------------------

    auto drawText = [&]() {
      // Clear only the text area
      // tft.fastFillRectBlack(
      //   startX - 5,
      //   textY - 5,
      //   totalWidth + 10,
      //   40);

      // tft.setTextSize(3);

      for (int i = 0; i < length; i++) {

        char c[2];

        c[0] = buffer[i];
        c[1] = '\0';

        uint16_t color =
          (i == cursor) ? TFT_CYAN : TFT_WHITE;

        printCentered(
          c,
          startX + i * charWidth + charWidth / 2,
          textY,
          color, 3, true);
      }

      tft.fastFillRectBlack(
        startX - 5,
        textY + 28,
        totalWidth + 10,
        1);

      // Cursor underline
      tft.drawLine(
        startX + cursor * charWidth,
        textY + 28,
        startX + cursor * charWidth + charWidth - 3,
        textY + 28,
        TFT_CYAN);
    };

    // ----------------------------------------------------------
    // Initial draw
    // ----------------------------------------------------------

    drawText();

    // ----------------------------------------------------------
    // Blocking input loop
    // ----------------------------------------------------------

    while (true) {

      char key = Terminal::getKey();

      if (!key) {
        continue;
      }

      switch (key) {

          // ========================================================
          // CANCEL
          // ========================================================

        case '*':
          return false;
          // ========================================================
          // CONFIRM
          // ========================================================

        case 'D':

          for (int i = 0; i < length; i++) {
            text[i] = buffer[i];
          }

          text[length] = '\0';

          return true;

          // ========================================================
          // CURSOR LEFT
          // ========================================================

        case '4':

          if (cursor > 0) {
            cursor--;
          }

          break;

          // ========================================================
          // CURSOR RIGHT
          // ========================================================

        case '6':

          if (cursor < length - 1) {
            cursor++;
          }

          break;

          // ========================================================
          // PREVIOUS CHARACTER
          // ========================================================

        case '2':
          {
            int index = 0;

            while (
              index < charsetLength && nameCharset[index] != buffer[cursor]) {
              index++;
            }

            if (index >= charsetLength) {
              index = 0;
            } else if (index > 0) {
              index--;
            } else {
              index = charsetLength - 1;
            }

            buffer[cursor] = nameCharset[index];
            break;
          }

          // ========================================================
          // NEXT CHARACTER
          // ========================================================

        case '8':
          {
            int index = 0;

            while (
              index < charsetLength && nameCharset[index] != buffer[cursor]) {
              index++;
            }

            if (index >= charsetLength) {
              index = 0;
            } else {
              index++;

              if (index >= charsetLength) {
                index = 0;
              }
            }

            buffer[cursor] = nameCharset[index];
            break;
          }
        case '#':
          buffer[cursor] = ' ';
          if (cursor < length - 1) {
            cursor++;
          }
          break;
        default:
          buffer[cursor] = key;
          break;
      }

      drawText();
    }
  }

  static void setText(
    uint16_t color,
    uint8_t size,
    int16_t x = -1,
    int16_t y = -1,
    const char* text = "") {
    tft.setTextColor(color);
    tft.setTextSize(size);
    if (x >= 0 && y >= 0) {
      tft.setCursor(x, y);
    }
    if (text != "") {
      print(text, x, y, color);
    }
  }
};