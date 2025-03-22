#pragma once
#include "framework.h"
#include <SDL3_image/SDL_image.h>

class GoreTexture {
public:
	GoreTexture(std::string fileName, SDL_Renderer * renderer);

	~GoreTexture();

	SDL_Texture * Get();
private:
	SDL_Texture * _texture;
};