#include "pch.h"
#include "GoreRenderer.h"

GoreRenderer::GoreRenderer(GoreWindow& window) {
    _renderer = SDL_CreateRenderer(window.Get(), NULL);

    if (!_renderer) {
        throw std::runtime_error("Failed to create renderer: " + std::string(SDL_GetError()));
    }
}

GoreRenderer::~GoreRenderer() {
    SDL_DestroyRenderer(_renderer);
}

void GoreRenderer::Clear() {
    SDL_RenderClear(_renderer);
}

void GoreRenderer::Present() {
    SDL_RenderPresent(_renderer);
}

SDL_Renderer* GoreRenderer::GetRenderer() const {
    return _renderer;
}
