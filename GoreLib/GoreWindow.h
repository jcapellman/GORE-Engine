#pragma once
#include "framework.h"

class GoreWindow {
public:
	void Init(std::string windowTitle, unsigned int width, unsigned int height) {
		SDL_Init(SDL_INIT_VIDEO);

		window = SDL_CreateWindow(windowTitle.c_str(), width, height, 0);
	}

	void Close() {
		SDL_DestroyWindow(window);
	}
private:
	SDL_Window* window;
};