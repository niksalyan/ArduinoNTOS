#pragma once

#include "UI.h"
#include "Storage.h"

class MainView {
private:

  static constexpr uint8_t COLS = 3;
  static constexpr uint8_t ROWS = 2;

  static constexpr uint8_t APPS_PER_PAGE = COLS * ROWS;

  // Maximum number of applications NTOS will keep in memory.
  static constexpr uint8_t MAX_APPS = 32;

  // Maximum application folder name length.
  static constexpr uint8_t MAX_APP_NAME = 20;

  static constexpr int TILE_WIDTH = 130;
  static constexpr int TILE_HEIGHT = 82;

  static constexpr int GRID_X = 25;
  static constexpr int GRID_Y = 48;

  static constexpr int COL_GAP = 20;
  static constexpr int ROW_GAP = 12;

  static char apps[MAX_APPS][MAX_APP_NAME];
  static uint8_t appCount;

  static uint8_t currentPage;
  static uint8_t selectedApp;

  static uint8_t pageCount() {

    if (appCount == 0)
      return 1;

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

  // --------------------------------------------------
  // Load applications from SD
  // --------------------------------------------------

  static void loadApps() {

    appCount = 0;

    Storage::listApps([](const char* name) {

      if (appCount >= MAX_APPS)
        return;

      strncpy(
        apps[appCount],
        name,
        MAX_APP_NAME - 1
      );

      apps[appCount][MAX_APP_NAME - 1] = '\0';

      appCount++;
    });

    selectedApp = constrain(selectedApp, 0, appCount - 1);

    // Make sure current page is still valid.
    if (currentPage >= pageCount())
      currentPage = pageCount() - 1;

    // Make sure selection is still valid.
    if (appCount == 0) {
      selectedApp = 0;
      return;
    }

    if (selectedApp >= appCount)
      selectedApp = appCount - 1;

    // Selection may have moved to another page.
    currentPage = selectedApp / APPS_PER_PAGE;
  }

  // --------------------------------------------------
  // Tile
  // --------------------------------------------------

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
    // Later this can become:
    // Storage::loadIcon(...)
    //
    // or an icon.bmp loaded from the
    // application's directory.

    tft.drawRect(
      x + TILE_WIDTH / 2 - 15,
      y + 10,
      30,
      25,
      selected
        ? TFT_YELLOW
        : TFT_CYAN
    );

    // Application name
    UI::printCentered(
      apps[index],
      x + TILE_WIDTH / 2,
      y + 52,
      selected
        ? TFT_YELLOW
        : TFT_WHITE
    );
  }

public:

  // --------------------------------------------------
  // Open
  // --------------------------------------------------

  static void open() {

    // Discover applications from SD.
    loadApps();

    UI::setHandler(handler);

    draw();
  }

  // --------------------------------------------------
  // Update
  // --------------------------------------------------

  static void update() {

    // For now applications are discovered when
    // the view is opened.
    //
    // Later we can detect SD changes here.
    //
    // IMPORTANT:
    // Do not redraw unless something actually changed.
  }

  // --------------------------------------------------
  // Draw
  // --------------------------------------------------

  static void draw() {

    UI::dialog("NTOS");

    uint8_t start = pageStart();
    uint8_t end = start + APPS_PER_PAGE;

    if (end > appCount)
      end = appCount;

    // Draw applications
    for (uint8_t i = start; i < end; i++) {
      drawTile(i);
    }

    // Empty tiles
    for (
      uint8_t i = end;
      i < start + APPS_PER_PAGE;
      i++
    ) {

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

  // --------------------------------------------------
  // Footer
  // --------------------------------------------------

  static void drawFooter() {

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

  // --------------------------------------------------
  // Input
  // --------------------------------------------------

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

  // --------------------------------------------------
  // Previous application
  // --------------------------------------------------

  static void moveLeft() {

    if (selectedApp == 0)
      return;

    uint8_t oldSelection = selectedApp;

    selectedApp--;

    currentPage =
      selectedApp / APPS_PER_PAGE;

    redrawSelection(oldSelection);
  }

  // --------------------------------------------------
  // Next application
  // --------------------------------------------------

  static void moveRight() {

    if (selectedApp + 1 >= appCount)
      return;

    uint8_t oldSelection = selectedApp;

    selectedApp++;

    currentPage =
      selectedApp / APPS_PER_PAGE;

    redrawSelection(oldSelection);
  }

  // --------------------------------------------------
  // Redraw selection
  // --------------------------------------------------

  static void redrawSelection(
    uint8_t oldSelection
  ) {

    // Page changed
    if (
      oldSelection / APPS_PER_PAGE !=
      selectedApp / APPS_PER_PAGE
    ) {

      draw();

      return;
    }

    // Same page:
    // only redraw the two affected tiles.
    drawTile(oldSelection);
    drawTile(selectedApp);
  }

  // --------------------------------------------------
  // Launch
  // --------------------------------------------------

  static void launchSelected() {

    if (appCount == 0)
      return;

    if (selectedApp >= appCount)
      return;

    Navigation::RunApp(
      apps[selectedApp]
    );
  }
};


// --------------------------------------------------
// State
// --------------------------------------------------

char MainView::apps[
  MainView::MAX_APPS
][
  MainView::MAX_APP_NAME
];

uint8_t MainView::appCount = 0;

uint8_t MainView::currentPage = 0;
uint8_t MainView::selectedApp = 0;