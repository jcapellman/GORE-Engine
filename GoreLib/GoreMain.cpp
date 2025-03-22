#include "pch.h"
#include "GoreMain.h"

GoreMain::GoreMain(std::string title) {
	_title = title;
	_gWindow = GoreWindow();
}

void GoreMain::Init(const std::string& configFileName) {
	_gConfig = GoreConfig(configFileName);

	_gWindow.Init(_title, _gConfig.GetIntValue(GoreConfigKeys::R_SCREEN_WIDTH), _gConfig.GetIntValue(GoreConfigKeys::R_SCREEN_WIDTH));
}

void GoreMain::Run() {
	SDL_Event e;

	bool quit = false;

	while (!quit) {
		while (SDL_PollEvent(&e)) {
			if (e.type == SDL_EVENT_QUIT) {
				quit = true;
			}
		}
	}
}

void GoreMain::Shutdown() {
	_gWindow.Close();
}

GoreMain::~GoreMain() {
	Shutdown();
}