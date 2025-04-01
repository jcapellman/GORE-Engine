#pragma once
#include "GoreEngine.h"
#include "GoreLogger.h"

class GoreWindow {
public:
	void Init(std::string windowTitle, unsigned int width, unsigned int height);

	void Close();

	unsigned int GetWidth() const;

	unsigned int GetHeight() const;

	SDL_Window* Get() const;
private:
	SDL_Window* _window;

	unsigned int _width;

	unsigned int _height;
};