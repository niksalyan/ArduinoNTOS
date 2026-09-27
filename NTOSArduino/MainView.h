
#pragma once

#include "UI.h"
#include "Storage.h"

class MainView {
private:

  // ==================================================
  // Layout
  // ==================================================

  static constexpr uint8_t COLS = 3;
  static constexpr uint8_t ROWS = 2;

  static constexpr uint8_t APPS_PER_PAGE =
    COLS * ROWS;

  static constexpr uint8_t MAX_APPS = 32;
  static constexpr uint8_t MAX_APP_NAME = 11;

  // Screen
  static constexpr int HEADER_HEIGHT = 32;
  static constexpr int FOOTER_HEIGHT = 32;

  // Application cards
  static constexpr int TILE_WIDTH = 135;
  static constexpr int TILE_HEIGHT = 88;

  static constexpr int GRID_X = 20;
  static constexpr int GRID_Y = 42;

  static constexpr int COL_GAP = 12;
  static constexpr int ROW_GAP = 10;

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

  // ==================================================
  // State
  // ==================================================

  static char apps[MAX_APPS][MAX_APP_NAME];

  static uint8_t appCount;
  static uint8_t currentPage;
  static uint8_t selectedApp;

  // ==================================================
  // Page helpers
  // ==================================================

  static uint8_t pageCount() {

    if (appCount == 0)
      return 1;

    return
      (appCount + APPS_PER_PAGE - 1)
      / APPS_PER_PAGE;
  }

  static uint8_t pageStart() {

    return currentPage * APPS_PER_PAGE;
  }

  // ==================================================
  // Layout helpers
  // ==================================================

  static int tileX(uint8_t column) {

    return GRID_X +
           column * (TILE_WIDTH + COL_GAP);
  }

  static int tileY(uint8_t row) {

    return GRID_Y +
           row * (TILE_HEIGHT + ROW_GAP);
  }

  // ==================================================
  // Load applications
  // ==================================================

  static void loadApps() {

    appCount = 0;

    Storage::listApps([](const char* name) {

      if (appCount >= MAX_APPS)
        return;

      strncpy(
        apps[appCount],
        name,
        MAX_APP_NAME - 1);

      apps[appCount][MAX_APP_NAME - 1] = '\0';

      appCount++;
    });

    // No applications
    if (appCount == 0) {

      selectedApp = 0;
      currentPage = 0;

      return;
    }

    // Clamp selection
    if (selectedApp >= appCount)
      selectedApp = appCount - 1;

    // Make sure selection is visible
    currentPage =
      selectedApp / APPS_PER_PAGE;

    // Clamp page
    if (currentPage >= pageCount())
      currentPage = pageCount() - 1;
  }

  // ==================================================
  // Format application name
  // ==================================================

  static void getDisplayName(
    uint8_t index,
    char* buffer,
    uint8_t bufferSize) {

    if (bufferSize == 0)
      return;

    strncpy(
      buffer,
      apps[index],
      bufferSize - 1);

    buffer[bufferSize - 1] = '\0';

    // Convert underscores to spaces
    for (
      uint8_t i = 0;
      buffer[i] != '\0';
      i++) {

      if (buffer[i] == '_')
        buffer[i] = ' ';
    }
  }

  static void getAppName(
    uint8_t index,
    char* buffer,
    uint8_t bufferSize) {

    if (bufferSize == 0)
      return;

    strncpy(
      buffer,
      apps[index],
      bufferSize - 1);

    buffer[bufferSize - 1] = '\0';
  }

  // ==================================================
  // Draw application icon
  // ==================================================

  static void drawIcon(
    int x,
    int y,
    bool selected) {



    uint16_t color =
      selected
        ? COLOR_TEXT
        : TFT_CYAN;

    // Simple modern "window" icon.
    //
    // This can later be replaced with:
    //
    // Storage::loadIcon(...)
    //

    int iconX = x + TILE_WIDTH / 2 - 14;
    int iconY = y + 15;

    tft.drawRect(
      iconX,
      iconY,
      28,
      24,
      color);

    tft.drawFastHLine(
      iconX,
      iconY + 6,
      28,
      color);

    // Window buttons
    tft.fillCircle(
      iconX + 4,
      iconY + 3,
      1,
      color);

    tft.fillCircle(
      iconX + 8,
      iconY + 3,
      1,
      color);

    // Simple content symbol
    tft.drawRect(
      iconX + 6,
      iconY + 11,
      16,
      8,
      color);
  }

  // ==================================================
  // Draw application tile
  // ==================================================

  static void drawTile(uint8_t index) {

    uint8_t start = pageStart();

    if (
      index < start ||
      index >= start + APPS_PER_PAGE) {

      return;
    }

    uint8_t localIndex =
      index - start;

    uint8_t column =
      localIndex % COLS;

    uint8_t row =
      localIndex / COLS;

    int x = tileX(column);
    int y = tileY(row);

    bool selected =
      index == selectedApp;

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
      TILE_WIDTH,
      TILE_HEIGHT,
      background);

    tft.drawRect(
      x,
      y,
      TILE_WIDTH,
      TILE_HEIGHT,
      border);

    // ------------------------------------------------
    // Selection indicator
    // ------------------------------------------------

    if (selected) {

      // Small accent bar on the left.
      tft.fastFillRect(
        x,
        y,
        3,
        TILE_HEIGHT,
        COLOR_PRIMARY);
    }

    // ------------------------------------------------
    // Icon
    // ------------------------------------------------

    

    // ------------------------------------------------
    // Name
    // ------------------------------------------------

    char displayName[MAX_APP_NAME];

    getAppName(
      index,
      displayName,
      MAX_APP_NAME);


    if(!Storage::drawImage(displayName, "icon", x + TILE_WIDTH / 2, y + TILE_HEIGHT / 3, 2, 2, 0)) {
      drawIcon(
            x,
            y,
            true);
    }

    getDisplayName(
      index,
      displayName,
      MAX_APP_NAME);

    UI::printCentered(
      displayName,
      x + TILE_WIDTH / 2,
      y + 58,
      COLOR_TEXT);
  }

  // ==================================================
  // Draw empty tile
  // ==================================================

  static void drawEmptyTile(
    uint8_t localIndex) {

    uint8_t column =
      localIndex % COLS;

    uint8_t row =
      localIndex / COLS;

    int x = tileX(column);
    int y = tileY(row);

    tft.fastFillRect(
      x,
      y,
      TILE_WIDTH,
      TILE_HEIGHT,
      COLOR_BACKGROUND);
  }

  // ==================================================
  // Header
  // ==================================================

  static void drawHeader() {

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
      "NTOS",
      12,
      10,
      COLOR_PRIMARY);

    // Page indicator
    char pageText[20];

    sprintf(
      pageText,
      "%d / %d",
      currentPage + 1,
      pageCount());

    UI::printRight(
      pageText,
      tft.width() - 12,
      10,
      COLOR_TEXT_SECONDARY);
  }

  // ==================================================
  // Footer
  // ==================================================

  static void drawFooter() {

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
      "4/6",
      10,
      y + 7,
      COLOR_TEXT_SECONDARY);

    UI::printCentered(
      "0 OPEN",
      tft.width() / 2,
      y + 7,
      COLOR_PRIMARY);

    UI::printRight(
      "2/8",
      tft.width() - 10,
      y + 7,
      COLOR_TEXT_SECONDARY);
  }

  // ==================================================
  // Empty state
  // ==================================================

  static void drawEmptyState() {

    UI::printCentered(
      "NO APPS",
      tft.width() / 2,
      130,
      COLOR_TEXT);

    UI::printCentered(
      "APPLICATIONS NOT FOUND",
      tft.width() / 2,
      150,
      COLOR_TEXT_SECONDARY);
  }

  // ==================================================
  // Draw
  // ==================================================

public:

  static void open() {

    loadApps();

    UI::setHandler(handler);

    draw();
  }

  static void update() {

    // Reserved for future SD monitoring.
  }

  static void draw() {

    // ------------------------------------------------
    // Clear desktop
    // ------------------------------------------------

    tft.fillScreenBlack();

    tft.setTextSize(2);

    // ------------------------------------------------
    // Header
    // ------------------------------------------------

    drawHeader();

    // ------------------------------------------------
    // Applications
    // ------------------------------------------------

    if (appCount == 0) {

      drawEmptyState();

    } else {

      uint8_t start =
        pageStart();

      uint8_t end =
        start + APPS_PER_PAGE;

      if (end > appCount)
        end = appCount;

      // Existing applications
      for (
        uint8_t i = start;
        i < end;
        i++) {

        drawTile(i);
      }

      // Empty slots
      for (
        uint8_t i = end;
        i < start + APPS_PER_PAGE;
        i++) {

        drawEmptyTile(
          i - start);
      }
    }

    // ------------------------------------------------
    // Footer
    // ------------------------------------------------

    drawFooter();
  }

private:

  // ==================================================
  // Input
  // ==================================================

  static void handler(char key) {

    switch (key) {

      // Left
      case '4':
        moveLeft();
        break;

      // Right
      case '6':
        moveRight();
        break;

      // Up
      case '2':
        moveUp();
        break;

      // Down
      case '8':
        moveDown();
        break;

      // Open
      case '0':
        launchSelected();
        break;

      // Previous page
      case '*':
        previousPage();
        break;

      // Next page
      case '#':
        nextPage();
        break;

      default:
        break;
    }
  }

  // ==================================================
  // Move Left
  // ==================================================

  static void moveLeft() {

    if (appCount == 0)
      return;

    uint8_t oldSelection =
      selectedApp;

    uint8_t column =
      selectedApp % COLS;

    if (column == 0)
      return;

    selectedApp--;

    currentPage =
      selectedApp / APPS_PER_PAGE;

    redrawSelection(
      oldSelection);
  }

  // ==================================================
  // Move Right
  // ==================================================

  static void moveRight() {

    if (appCount == 0)
      return;

    uint8_t oldSelection =
      selectedApp;

    uint8_t column =
      selectedApp % COLS;

    if (column >= COLS - 1)
      return;

    if (selectedApp + 1 >= appCount)
      return;

    selectedApp++;

    currentPage =
      selectedApp / APPS_PER_PAGE;

    redrawSelection(
      oldSelection);
  }

  // ==================================================
  // Move Up
  // ==================================================

  static void moveUp() {

    if (appCount == 0)
      return;

    uint8_t oldSelection =
      selectedApp;

    if (selectedApp < COLS)
      return;

    selectedApp -= COLS;

    currentPage =
      selectedApp / APPS_PER_PAGE;

    redrawSelection(
      oldSelection);
  }

  // ==================================================
  // Move Down
  // ==================================================

  static void moveDown() {

    if (appCount == 0)
      return;

    uint8_t oldSelection =
      selectedApp;

    uint8_t next =
      selectedApp + COLS;

    if (next >= appCount)
      return;

    selectedApp =
      next;

    currentPage =
      selectedApp / APPS_PER_PAGE;

    redrawSelection(
      oldSelection);
  }

  // ==================================================
  // Previous page
  // ==================================================

  static void previousPage() {

    if (pageCount() <= 1)
      return;

    if (currentPage == 0)
      return;

    currentPage--;

    selectedApp =
      currentPage * APPS_PER_PAGE;

    if (selectedApp >= appCount)
      selectedApp = appCount - 1;

    draw();
  }

  // ==================================================
  // Next page
  // ==================================================

  static void nextPage() {

    if (pageCount() <= 1)
      return;

    if (currentPage + 1 >= pageCount())
      return;

    currentPage++;

    selectedApp =
      currentPage * APPS_PER_PAGE;

    if (selectedApp >= appCount)
      selectedApp = appCount - 1;

    draw();
  }

  // ==================================================
  // Selection redraw
  // ==================================================

  static void redrawSelection(
    uint8_t oldSelection) {

    // Selection crossed page
    if (
      oldSelection / APPS_PER_PAGE !=
      selectedApp / APPS_PER_PAGE) {

      draw();

      return;
    }

    // Only redraw affected tiles
    drawTile(oldSelection);
    drawTile(selectedApp);
  }

  // ==================================================
  // Launch application
  // ==================================================

  static void launchSelected() {

    if (appCount == 0)
      return;

    if (selectedApp >= appCount)
      return;

    Navigation::RunApp(
      apps[selectedApp]);
  }
};


// ======================================================
// Static state
// ======================================================

char MainView::apps[
  MainView::MAX_APPS
][
  MainView::MAX_APP_NAME
];

uint8_t MainView::appCount = 0;

uint8_t MainView::currentPage = 0;

uint8_t MainView::selectedApp = 0;
