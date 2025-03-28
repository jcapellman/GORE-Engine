#pragma once
#include "GoreWindow.h"
#include "GoreTexture.h"
#include "framework.h"

class GoreRenderer {
public:
    GoreRenderer(GoreWindow& window);
    ~GoreRenderer();

    void Clear();
    void Present();
    void RenderTexture(GoreTexture* texture, const SDL_FRect* srcRect = nullptr, const SDL_FRect* dstRect = nullptr);

    SDL_Renderer* GetRenderer() const;

private:
    SDL_Renderer* _renderer;
};
