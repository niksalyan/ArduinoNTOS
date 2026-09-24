#include "Navigation.h"
#include "MainView.h"
#include "RunApp.h"


void Navigation::MainView()
{
  MainView::open();
}

void Navigation::RunApp(char* name)
{
  RunApp::open(name);
}
