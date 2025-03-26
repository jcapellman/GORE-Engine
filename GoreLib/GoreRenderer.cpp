#include "pch.h"
#include "GoreRenderer.h"

GoreRenderer::GoreRenderer(GoreWindow& window) {
    _renderer = SDL_CreateRenderer(window.Get(), "opengl");
    if (!_renderer) {
        // Handle error
    }
}

GoreRenderer::~GoreRenderer() {
    if (_renderer) {
        SDL_DestroyRenderer(_renderer);
    }
}

void GoreRenderer::Clear() {
    SDL_RenderClear(_renderer);
}

void GoreRenderer::Present() {
    SDL_RenderPresent(_renderer);
}

void GoreRenderer::RenderTexture(GoreTexture* texture, const SDL_FRect* srcRect, const SDL_FRect* dstRect) {
    if (texture && texture->Get()) {
        SDL_RenderTexture(_renderer, texture->Get(), srcRect, dstRect);
    }
}

SDL_Renderer* GoreRenderer::GetRenderer() const {
    return _renderer;
}
