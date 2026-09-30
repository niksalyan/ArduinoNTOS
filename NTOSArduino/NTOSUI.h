#pragma once
#include "Terminal.h"

class NTOSUI {
private:
  inline static const char nameCharset[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";  
public:

  // Screen
  static constexpr int HEADER_HEIGHT = 32;
  static constexpr int FOOTER_HEIGHT = 32;

  // ==================================================
  // Colors
  // ==================================================

  static constexpr uint16_t COLOR_BACKGROUND = TFT_BLACK;

  static constexpr uint16_t COLOR_HEADER =
    0x18E3;

  static constexpr uint16_t COLOR_CARD =
    0x1082;

  static constexpr uint16_t COLOR_CARD_SELECTED =
    0x2945;

  static constexpr uint16_t COLOR_BORDER =
    0x39C7;

  static constexpr uint16_t COLOR_BORDER_SELECTED =
    TFT_CYAN;

  static constexpr uint16_t COLOR_PRIMARY =
    TFT_CYAN;

  static constexpr uint16_t COLOR_TEXT =
    TFT_WHITE;

  static constexpr uint16_t COLOR_TEXT_SECONDARY =
    0x8410;

  static void setHandler(KeyHandler kh) {
    Terminal::setHandlers(kh);
  }

  static void dialog(char* title, char* message = "") {
    drawHeader(title, message);
    tft.fastFillRectBlack(0, HEADER_HEIGHT, tft.width(), tft.height() - HEADER_HEIGHT);
  }

  static void alert(char* title, char* message) {
    drawPopupMessage(title, message);
    waitForKey(1000);
  }

  
  static bool confirm(const char* title) {
    drawPopupMessage(title, "* = CANCEL    D = CONFIRM");
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

  static int confirmNumber(const char* title, const char* message, int from, int to) {
    drawPopupMessage(title, "", "* CANCEL", message);
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


  static void drawPopupMessage(char* title, char* message, char* left = "", char* right = "") {

    tft.fastFillRectBlack(35,
               90,
               tft.width() - 60,
               tft.height() - 205);
    drawWindow(30,
               85,
               tft.width() - 60,
               tft.height() - 205, true);

    tft.setTextSize(3);
    printCentered(
      title,
      tft.width() / 2,
      115,
      COLOR_TEXT);

    tft.setTextSize(2);

    print(
      message,
      100,
      155,
      COLOR_TEXT);


    print(
      left,
      100,
      155,
      COLOR_TEXT);

    printRight(
      right,
      tft.width() - 100,
      155,
      COLOR_TEXT);
  }

  static void drawHeader(char* title, char* message) {

    // Header background
    tft.fastFillRect(
      0,
      0,
      tft.width(),
      HEADER_HEIGHT,
      COLOR_HEADER);

    // Bottom separator
    tft.drawFastHLine(
      0,
      HEADER_HEIGHT - 1,
      tft.width(),
      COLOR_BORDER);

    // NTOS title
    print(
      title,
      12,
      10,
      COLOR_PRIMARY);



    printRight(
      message,
      tft.width() - 12,
      10,
      COLOR_TEXT_SECONDARY);
  }


  // ==================================================
  // Footer
  // ==================================================
  static void drawFooter(char* left, char* mid, char* right) {

    int y =
      tft.height() - FOOTER_HEIGHT;

    // Background
    tft.fastFillRect(
      0,
      y,
      tft.width(),
      FOOTER_HEIGHT,
      COLOR_HEADER);

    // Top separator
    tft.drawFastHLine(
      0,
      y,
      tft.width(),
      COLOR_BORDER);

    // Navigation
    print(
      left,
      10,
      y + 10,
      COLOR_TEXT_SECONDARY);

    printCentered(
      mid,
      tft.width() / 2,
      y + 10,
      COLOR_PRIMARY);

    printRight(
      right,
      tft.width() - 10,
      y + 10,
      COLOR_TEXT_SECONDARY);
  }

  // ==================================================
  // Draw application icon
  // ==================================================

  static void drawIcon(
    uint16_t x,
    uint16_t y) {



    uint16_t color = COLOR_TEXT;

    // Simple modern "window" icon.
    //
    // This can later be replaced with:
    //
    // Storage::loadIcon(...)
    //

    tft.drawRect(
      x,
      y,
      28,
      24,
      color);

    tft.drawFastHLine(
      x,
      y + 6,
      28,
      color);

    // Window buttons
    tft.fillCircle(
      x + 4,
      y + 3,
      1,
      color);

    tft.fillCircle(
      x + 8,
      y + 3,
      1,
      color);

    // Simple content symbol
    tft.drawRect(
      x + 6,
      y + 11,
      16,
      8,
      color);
  }

  static void drawWindow(
    uint16_t x,
    uint16_t y,
    uint16_t w,
    uint16_t h,
    bool selected) {
    uint16_t background =
      selected
        ? COLOR_CARD_SELECTED
        : COLOR_CARD;

    uint16_t border =
      selected
        ? COLOR_BORDER_SELECTED
        : COLOR_BORDER;

    // ------------------------------------------------
    // Card
    // ------------------------------------------------

    tft.fastFillRect(
      x,
      y,
      w,
      h,
      background);

    tft.drawRect(
      x,
      y,
      w,
      h,
      border);

    // ------------------------------------------------
    // Selection indicator
    // ------------------------------------------------

    if (selected) {

      // Small accent bar on the left.
      tft.fastFillRect(
        x,
        y,
        5,
        h,
        COLOR_PRIMARY);
    }
  }

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


  static void waitForKey(int ms) {
    for (int d = 0; d < ms; d++) {
      char key = Terminal::getKey();
      if (key) {
        return;
      }
      delay(1);
    }
  }
};