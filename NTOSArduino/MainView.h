#pragma once

#include "UI.h"

class MainView {
private:

  static constexpr uint8_t COLS = 3;
  static constexpr uint8_t ROWS = 2;

  static constexpr uint8_t APPS_PER_PAGE = COLS * ROWS;
  static constexpr uint8_t appCount = 10;

  static constexpr int TILE_WIDTH = 130;
  static constexpr int TILE_HEIGHT = 82;

  static constexpr int GRID_X = 25;
  static constexpr int GRID_Y = 48;

  static constexpr int COL_GAP = 20;
  static constexpr int ROW_GAP = 12;

  static const char* apps[];

  static uint8_t currentPage;
  static uint8_t selectedApp;

  static uint8_t pageCount() {
    return (appCount + APPS_PER_PAGE - 1) / APPS_PER_PAGE;
  }

  static uint8_t pageStart() {
    return currentPage * APPS_PER_PAGE;
  }

  static int tileX(uint8_t column) {
    return GRID_X + column * (TILE_WIDTH + COL_GAP);
  }

  static int tileY(uint8_t row) {
    return GRID_Y + row * (TILE_HEIGHT + ROW_GAP);
  }

  static void drawTile(uint8_t index) {

    uint8_t start = pageStart();

    if (index < start || index >= start + APPS_PER_PAGE)
      return;

    uint8_t localIndex = index - start;

    uint8_t column = localIndex % COLS;
    uint8_t row = localIndex / COLS;

    int x = tileX(column);
    int y = tileY(row);

    bool selected = index == selectedApp;

    uint16_t background = selected
      ? TFT_DARKGREY
      : TFT_BLACK;

    uint16_t border = selected
      ? TFT_YELLOW
      : TFT_DARKGREY;

    // Tile background
    tft.fillRect(
      x,
      y,
      TILE_WIDTH,
      TILE_HEIGHT,
      background
    );

    // Tile border
    tft.drawRect(
      x,
      y,
      TILE_WIDTH,
      TILE_HEIGHT,
      border
    );

    // Placeholder icon
    //
    // Later this becomes an actual icon loaded
    // from the app metadata / SD card.
    tft.drawRect(
      x + TILE_WIDTH / 2 - 15,
      y + 10,
      30,
      25,
      selected ? TFT_YELLOW : TFT_CYAN
    );

    // Application name
    UI::printCentered(
      apps[index],
      x + TILE_WIDTH / 2,
      y + 52,
      selected ? TFT_YELLOW : TFT_WHITE
    );
  }

public:

  static void open() {

    currentPage = 0;
    selectedApp = 0;

    UI::setHandler(handler);

    draw();
  }

  static void update() {

    // Eventually:
    //
    // - check SD card
    // - detect new applications
    // - update status information
    //
    // Nothing should be redrawn unless something changed.
  }

  static void draw() {

    UI::dialog("NTOS");

    uint8_t start = pageStart();
    uint8_t end = start + APPS_PER_PAGE;

    if (end > appCount)
      end = appCount;

    // Draw all tiles
    for (uint8_t i = start; i < end; i++) {
      drawTile(i);
    }

    // Empty tiles
    for (uint8_t i = end; i < start + APPS_PER_PAGE; i++) {

      uint8_t localIndex = i - start;

      uint8_t column = localIndex % COLS;
      uint8_t row = localIndex / COLS;

      int x = tileX(column);
      int y = tileY(row);

      tft.fillRect(
        x,
        y,
        TILE_WIDTH,
        TILE_HEIGHT,
        TFT_BLACK
      );
    }

    drawFooter();
  }

private:

  static void drawFooter() {

    // Clear footer
    tft.fillRect(
      0,
      250,
      tft.width(),
      70,
      TFT_BLACK
    );

    char pageText[20];

    sprintf(
      pageText,
      "%d / %d",
      currentPage + 1,
      pageCount()
    );

    UI::print(
      "* PREV",
      25,
      275,
      TFT_CYAN
    );

    UI::printCentered(
      pageText,
      tft.width() / 2,
      275,
      TFT_CYAN
    );

    UI::printRight(
      "# NEXT",
      455,
      275,
      TFT_CYAN
    );

    UI::printCentered(
      "0 = OPEN",
      tft.width() / 2,
      300,
      TFT_WHITE
    );
  }

  static void handler(char key) {

    switch (key) {

      case '*':
        moveLeft();
        break;

      case '#':
        moveRight();
        break;

      case '0':
        launchSelected();
        break;

      default:
        break;
    }
  }

  static void moveLeft() {

    if (selectedApp == 0)
      return;

    uint8_t oldSelection = selectedApp;

    selectedApp--;

    currentPage = selectedApp / APPS_PER_PAGE;

    redrawSelection(oldSelection);
  }

  static void moveRight() {

    if (selectedApp + 1 >= appCount)
      return;

    uint8_t oldSelection = selectedApp;

    selectedApp++;

    currentPage = selectedApp / APPS_PER_PAGE;

    redrawSelection(oldSelection);
  }

  static void redrawSelection(uint8_t oldSelection) {

    // Page changed
    if (oldSelection / APPS_PER_PAGE !=
        selectedApp / APPS_PER_PAGE) {

      draw();
      return;
    }

    // Same page:
    // only redraw the two affected tiles.
    drawTile(oldSelection);
    drawTile(selectedApp);
  }

  static void launchSelected() {
    Navigation::RunApp("mama");
    // Later:
    //
    // AppManager::launch(apps[selectedApp]);
  }
};


// --------------------------------------------------
// Mock applications
// --------------------------------------------------

const char* MainView::apps[] = {

  "Monetrix",
  "Calculator",
  "Terminal",
  "File Manager",
  "Settings",
  "System Info",
  "Games",
  "Paint",
  "Notes",
  "Diagnostics"
};


// --------------------------------------------------
// State
// --------------------------------------------------

uint8_t MainView::currentPage = 0;
uint8_t MainView::selectedApp = 0;