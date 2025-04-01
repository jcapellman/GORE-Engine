#pragma once
#include "GoreEngine.h"

class GoreTexture {
public:
    GoreTexture(SDL_Surface * texture);
    ~GoreTexture();

    GLuint Get() const;

private:
    GLuint _texture;
};