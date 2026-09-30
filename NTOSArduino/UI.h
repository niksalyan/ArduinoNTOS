#pragma once
#include "Terminal.h"

class UI {
public:
  inline static const char nameCharset[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";

  static void setHandler(KeyHandler kh) {
    Terminal::setHandlers(kh);
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
    drawPopupMessage(line1, line2, line3, true);
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
    const char* line3,
    bool joinLines = false
    ) {
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
    if (joinLines) {
      print(
      line2,
      100,
      145,
      TFT_WHITE);

    printRight(
      line3,
      tft.width() - 100,
      145,
      TFT_WHITE);
    } else {
      printCentered(
      line2,
      tft.width() / 2,
      145,
      TFT_WHITE);

    printCentered(
      line3,
      tft.width() / 2,
      145,
      TFT_WHITE);
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