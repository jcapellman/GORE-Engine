#pragma once
#include "framework.h"
#include <SDL3_image/SDL_image.h>

class GoreTexture {
public:
    GoreTexture(SDL_Texture * texture);
    ~GoreTexture();

    SDL_Texture* Get() const;

private:
    SDL_Texture* _texture;
};