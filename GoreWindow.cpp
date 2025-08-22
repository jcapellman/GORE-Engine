#include "GoreWindow.h"

void GoreWindow::Init(std::string_view windowTitle, unsigned int width, unsigned int height) {
	GoreLogger::getInstance().log(DEBUG, "Initializing SDL...");

	if (!SDL_Init(SDL_INIT_VIDEO)) {
		GoreLogger::getInstance().log(ERR, "Failed to Init SDL");
		GoreLogger::getInstance().log(ERR, SDL_GetError());

		return;
	}

	GoreLogger::getInstance().log(DEBUG, "SDL Initialized");

	_window = SDL_CreateWindow(windowTitle.data(),
		width, height, 0);

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

constexpr SDL_Window* GoreWindow::Get() const {
	return _window;
}

constexpr unsigned int GoreWindow::GetWidth() const {
	return _width;
}

constexpr unsigned int GoreWindow::GetHeight() const {
	return _height;
}