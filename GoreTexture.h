#pragma once
#include "GoreEngine.h"

class GoreTexture {
public:
    GoreTexture(SDL_Renderer* renderer, const std::string& filePath);
    ~GoreTexture();
    SDL_Texture* Get() const;

private:
    SDL_Texture* _texture;
};