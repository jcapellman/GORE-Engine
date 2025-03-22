#include "pch.h"
#include "GoreTexture.h"

GoreTexture::GoreTexture(std::string fileName) {
    _texture = IMG_Load(fileName.c_str());

    if (!_texture) {
        throw std::runtime_error("Failed to load image: " + fileName);
    }
}

SDL_Surface* GoreTexture::Get() {
    return _texture;
}

GoreTexture::~GoreTexture() {
    SDL_DestroySurface(_texture);
}