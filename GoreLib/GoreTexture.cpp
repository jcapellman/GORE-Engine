#include "pch.h"
#include "GoreTexture.h"

GoreTexture::GoreTexture(std::string fileName, GoreRenderer * renderer) {
    SDL_Surface * surface = IMG_Load(fileName.c_str());

    if (!surface) {
        throw std::runtime_error("Failed to load image: " + fileName);
    }

    _texture = SDL_CreateTextureFromSurface(renderer->GetRenderer(), surface);

	SDL_DestroySurface(surface);

	if (!_texture) {
		throw std::runtime_error("Failed to create texture from surface: " + fileName);
	}
}

SDL_Texture * GoreTexture::Get() {
    return _texture;
}

GoreTexture::~GoreTexture() {
    SDL_DestroyTexture(_texture);
}