#pragma once
#include "framework.h"
#include <SDL3_image/SDL_image.h>
#include "GoreRenderer.h"

class GoreTexture {
public:
	GoreTexture(std::string fileName, GoreRenderer * renderer);

	~GoreTexture();

	SDL_Texture * Get();
private:
	SDL_Texture * _texture;
};