#pragma once
#include "framework.h"

class GoreWindow {
public:
	void Init(std::string windowTitle, unsigned int width, unsigned int height);

	void Close();

	SDL_Window* GetWindow() const;
private:
	SDL_Window* _window;
};