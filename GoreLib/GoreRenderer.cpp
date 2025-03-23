#include "pch.h"
#include "GoreRenderer.h"

GoreRenderer::GoreRenderer(GoreWindow goreWindow) {
	_renderer = SDL_CreateRenderer(goreWindow.GetWindow(), NULL);

	if (!_renderer) {
		SDL_DestroyWindow(goreWindow.GetWindow());
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