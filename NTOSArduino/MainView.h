
#pragma once

#include "NTOSUI.h"
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


  // Application cards
  static constexpr int TILE_WIDTH = 135;
  static constexpr int TILE_HEIGHT = 88;

  static constexpr int GRID_X = 20;
  static constexpr int GRID_Y = 42;

  static constexpr int COL_GAP = 12;
  static constexpr int ROW_GAP = 10;

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

    return (appCount + APPS_PER_PAGE - 1)
           / APPS_PER_PAGE;
  }

  static uint8_t pageStart() {

    return currentPage * APPS_PER_PAGE;
  }

  // ==================================================
  // Layout helpers
  // ==================================================

  static int tileX(uint8_t column) {

    return GRID_X + column * (TILE_WIDTH + COL_GAP);
  }

  static int tileY(uint8_t row) {

    return GRID_Y + row * (TILE_HEIGHT + ROW_GAP);
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
  // Draw application tile
  // ==================================================

  static void drawTile(uint8_t index) {

    uint8_t start = pageStart();

    if (
      index < start || index >= start + APPS_PER_PAGE) {

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

    NTOSUI::drawWindow(x, y, TILE_WIDTH, TILE_HEIGHT, selected);

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


    if (!Storage::drawImage(displayName, "icon", x + TILE_WIDTH / 2, y + TILE_HEIGHT / 3, 2, 2, 0)) {
      NTOSUI::drawIcon(x + TILE_WIDTH / 2 - 14, y + 18);
    }

    getDisplayName(
      index,
      displayName,
      MAX_APP_NAME);

    NTOSUI::printCentered(
      displayName,
      x + TILE_WIDTH / 2,
      y + 58,
      NTOSUI::COLOR_TEXT);
  }

  // ==================================================
  // Empty state
  // ==================================================

  static void drawEmptyState() {

    NTOSUI::printCentered(
      "NO APPS",
      tft.width() / 2,
      130,
      NTOSUI::COLOR_TEXT);

    NTOSUI::printCentered(
      "APPLICATIONS NOT FOUND",
      tft.width() / 2,
      150,
      NTOSUI::COLOR_TEXT_SECONDARY);
  }

  // ==================================================
  // Draw
  // ==================================================

public:

  static void open() {

    loadApps();

    NTOSUI::setHandler(handler);

    tft.fillScreenBlack();
    NTOSUI::scroll(0);
    draw();
  }

  static void update() {

    // Reserved for future SD monitoring.
  }

  static void draw() {

    // ------------------------------------------------
    // Clear desktop
    // ------------------------------------------------


    tft.setTextSize(2);

    // ------------------------------------------------
    // Header
    // ------------------------------------------------

    // Page indicator
    char pageText[10];

    sprintf(
      pageText,
      "%d / %d",
      currentPage + 1,
      pageCount());
    NTOSUI::dialog("NTOS", pageText);

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

        // drawEmptyTile( i - start);
      }
    }

    // ------------------------------------------------
    // Footer
    // ------------------------------------------------

    NTOSUI::drawFooter("4/6", "0 OPEN", "2/8");
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

    uint8_t start = pageStart();
    uint8_t localIndex = selectedApp - start;
    uint8_t column = localIndex % COLS;
    uint8_t row = localIndex / COLS;

    // If at first column, move to previous page and keep same row/column position
    if (column == 0) {
      if (currentPage == 0)
        return;

      uint8_t prevPage = currentPage - 1;
      uint8_t newLocal = row * COLS + (COLS - 1);
      uint8_t newIndex = prevPage * APPS_PER_PAGE + newLocal;

      if (newIndex >= appCount) {
        // Clamp to last available app on previous page
        uint8_t prevStart = prevPage * APPS_PER_PAGE;
        uint8_t prevEnd = prevStart + APPS_PER_PAGE;
        if (prevEnd > appCount) prevEnd = appCount;
        if (prevEnd == prevStart) return;
        selectedApp = prevEnd - 1;
      } else {
        selectedApp = newIndex;
      }

      currentPage = prevPage;
      draw();
      return;
    }

    // Normal move within page
    selectedApp--;
    currentPage = selectedApp / APPS_PER_PAGE;
    redrawSelection(oldSelection);
  }

  // ==================================================
  // Move Right
  // ==================================================

  static void moveRight() {

    if (appCount == 0)
      return;

    uint8_t oldSelection =
      selectedApp;

    uint8_t start = pageStart();
    uint8_t localIndex = selectedApp - start;
    uint8_t column = localIndex % COLS;
    uint8_t row = localIndex / COLS;

    // If at last column, move to next page keeping same row
    if (column >= COLS - 1) {
      if (currentPage + 1 >= pageCount())
        return;

      uint8_t nextPage = currentPage + 1;
      uint8_t newLocal = row * COLS; // first column on next page
      uint8_t newIndex = nextPage * APPS_PER_PAGE + newLocal;

      if (newIndex >= appCount) {
        // Clamp to last available app on next page
        uint8_t nextStart = nextPage * APPS_PER_PAGE;
        if (nextStart >= appCount) return;
        selectedApp = appCount - 1;
      } else {
        selectedApp = newIndex;
      }

      currentPage = nextPage;
      draw();
      return;
    }

    // Normal move within page
    if (selectedApp + 1 >= appCount)
      return;

    selectedApp++;
    currentPage = selectedApp / APPS_PER_PAGE;
    redrawSelection(oldSelection);
  }

  // ==================================================
  // Move Up
  // ==================================================

  static void moveUp() {

    if (appCount == 0)
      return;

    uint8_t oldSelection =
      selectedApp;

    uint8_t start = pageStart();
    uint8_t localIndex = selectedApp - start;
    uint8_t column = localIndex % COLS;
    uint8_t row = localIndex / COLS;

    // If in top row, move to previous page same column
    if (row == 0) {
      if (currentPage == 0)
        return;

      uint8_t prevPage = currentPage - 1;
      uint8_t newLocal = (ROWS - 1) * COLS + column; // bottom row, same column
      uint8_t newIndex = prevPage * APPS_PER_PAGE + newLocal;

      if (newIndex >= appCount) {
        // Clamp to last app on previous page
        uint8_t prevStart = prevPage * APPS_PER_PAGE;
        uint8_t prevEnd = prevStart + APPS_PER_PAGE;
        if (prevEnd > appCount) prevEnd = appCount;
        if (prevEnd == prevStart) return;
        selectedApp = prevEnd - 1;
      } else {
        selectedApp = newIndex;
      }

      currentPage = prevPage;
      draw();
      return;
    }

    // Normal move up within page
    selectedApp -= COLS;
    currentPage = selectedApp / APPS_PER_PAGE;
    redrawSelection(oldSelection);
  }

  // ==================================================
  // Move Down
  // ==================================================

  static void moveDown() {

    if (appCount == 0)
      return;

    uint8_t oldSelection =
      selectedApp;

    uint8_t start = pageStart();
    uint8_t localIndex = selectedApp - start;
    uint8_t column = localIndex % COLS;
    uint8_t row = localIndex / COLS;

    uint8_t next = selectedApp + COLS;

    // If in bottom row, move to next page same column
    if (row + 1 >= ROWS) {
      if (currentPage + 1 >= pageCount())
        return;

      uint8_t nextPage = currentPage + 1;
      uint8_t newLocal = column; // top row, same column on next page
      uint8_t newIndex = nextPage * APPS_PER_PAGE + newLocal;

      if (newIndex >= appCount) {
        // Clamp to last app on next page
        uint8_t nextStart = nextPage * APPS_PER_PAGE;
        if (nextStart >= appCount) return;
        selectedApp = appCount - 1;
      } else {
        selectedApp = newIndex;
      }

      currentPage = nextPage;
      draw();
      return;
    }

    if (next >= appCount)
      return;

    selectedApp = next;
    currentPage = selectedApp / APPS_PER_PAGE;
    redrawSelection(oldSelection);
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
      currentPage * APPS_PER_PAGE + (APPS_PER_PAGE - 1);

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
      oldSelection / APPS_PER_PAGE != selectedApp / APPS_PER_PAGE) {

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

char MainView::apps[MainView::MAX_APPS][MainView::MAX_APP_NAME];

uint8_t MainView::appCount = 0;

uint8_t MainView::currentPage = 0;

uint8_t MainView::selectedApp = 0;
