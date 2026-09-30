#pragma once

class NTOSUI {
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

  static void dialog(char* title, char* description = "") {
    drawHeader(title, description);
    tft.fastFillRectBlack(0, HEADER_HEIGHT, tft.width(), tft.height() - HEADER_HEIGHT);
  }

  static void drawHeader(char* title, char* description) {

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
    UI::print(
      title,
      12,
      10,
      COLOR_PRIMARY);



    UI::printRight(
      description,
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
    UI::print(
      left,
      10,
      y + 10,
      COLOR_TEXT_SECONDARY);

    UI::printCentered(
      mid,
      tft.width() / 2,
      y + 10,
      COLOR_PRIMARY);

    UI::printRight(
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
};