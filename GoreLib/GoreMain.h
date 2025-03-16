#pragma once
#include "framework.h"
#include "GoreWindow.h"

class GoreMain {
public:
	GoreMain(std::string title) {
		_title = title;
		_gWindow = GoreWindow();
	}

	void Init() {
		_gWindow.Init(_title, 640, 480);
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
private:
	std::string _title;

	GoreWindow _gWindow;
};