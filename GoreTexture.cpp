#include "GoreTexture.h"

GoreTexture::GoreTexture(SDL_Renderer* renderer, const std::string& filePath) {
    // Load image as SDL_Surface
    SDL_Surface* surface = SDL_LoadBMP(filePath.c_str());
    if (!surface) {
        throw std::runtime_error("Failed to load texture: " + filePath);
    }

    // Create SDL_Texture from SDL_Surface
    _texture = SDL_CreateTextureFromSurface(renderer, surface);
    if (!_texture) {
        SDL_DestroySurface(surface);
        throw std::runtime_error("Failed to create texture from surface: " + filePath);
    }

    // Free the surface as it is no longer needed
    SDL_DestroySurface(surface);
}

GoreTexture::~GoreTexture() {
    if (_texture) {
        SDL_DestroyTexture(_texture);
    }
}

SDL_Texture* GoreTexture::Get() const {
    return _texture;
}
