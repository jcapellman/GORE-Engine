#pragma once
#include "framework.h"
#include "GoreWindow.h"

class GoreRenderer {
public:
	GoreRenderer(GoreWindow goreWindow);
	~GoreRenderer();

	void Clear();

	void Present();

	SDL_Renderer* GetRenderer() const;
private:
	SDL_Renderer* _renderer;
};