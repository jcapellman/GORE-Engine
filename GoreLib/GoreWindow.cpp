#include "pch.h"
#include "GoreWindow.h"

void GoreWindow::Init(std::string windowTitle, unsigned int width, unsigned int height) {
	SDL_Init(SDL_INIT_VIDEO);

	window = SDL_CreateWindow(windowTitle.c_str(), width, height, 0);
}

void GoreWindow::Close() {
	SDL_DestroyWindow(window);
}