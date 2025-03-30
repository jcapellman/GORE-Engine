#include "GoreWindow.h"

void GoreWindow::Init(std::string windowTitle, unsigned int width, unsigned int height) {
	GoreLogger::getInstance().log(DEBUG, "Initializing SDL...");

	if (!SDL_Init(SDL_INIT_VIDEO)) {
		GoreLogger::getInstance().log(ERR, "Failed to Init SDL");
		GoreLogger::getInstance().log(ERR, SDL_GetError());

		return;
	}

	GoreLogger::getInstance().log(DEBUG, "SDL Initialized");

	GoreLogger::getInstance().log(DEBUG, "Initializing SDL Window with OpenGL...");

	_window = SDL_CreateWindow(windowTitle.c_str(), width, height, SDL_WINDOW_OPENGL);

	if (!_window) {
		GoreLogger::getInstance().log(ERR, "Failed to create Window");
		GoreLogger::getInstance().log(ERR, SDL_GetError());
	}

	_width = width;
	_height = height;

	GoreLogger::getInstance().log(DEBUG, "SDL Window Initialized");
}

void GoreWindow::Close() {
	SDL_DestroyWindow(_window);
}

SDL_Window* GoreWindow::Get() const {
	return _window;
}

unsigned int GoreWindow::GetWidth() const {
	return _width;
}

unsigned int GoreWindow::GetHeight() const {
	return _height;
}