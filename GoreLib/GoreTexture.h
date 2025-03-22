#pragma once
#include "framework.h"
#include <SDL3_image/SDL_image.h>

class GoreTexture {
public:
	GoreTexture(std::string fileName);

	~GoreTexture();

	SDL_Surface* Get();
private:
	SDL_Surface* _texture;
};