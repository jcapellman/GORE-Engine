#pragma once
#include "framework.h"
#include "GoreWindow.h"
#include "GoreConfig.h"

class GoreMain {
public:
	GoreMain(std::string title) {
		_title = title;
		_gWindow = GoreWindow();
	}

	void Init(const std::string& configFileName = DEFAULT_CONFIG_FILENAME) {
		_gConfig = GoreConfig(configFileName);

		_gWindow.Init(_title, _gConfig.GetIntValue(GoreConfigKeys::R_SCREEN_WIDTH), _gConfig.GetIntValue(GoreConfigKeys::R_SCREEN_WIDTH));
	}

	void Run() {
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

	void Shutdown() {
		_gWindow.Close();
	}

	~GoreMain() {
		Shutdown();
	}
private:
	std::string _title;

	GoreWindow _gWindow;
	GoreConfig _gConfig;
};