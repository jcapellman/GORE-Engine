#include "pch.h"
#include "GoreTexture.h"

GoreTexture::GoreTexture(SDL_Texture * texture)
    : _texture(texture) {
}

GoreTexture::~GoreTexture() {
    if (_texture) {
        SDL_DestroyTexture(_texture);
    }
}

SDL_Texture* GoreTexture::Get() const {
    return _texture;
}
