#pragma once
#include "framework.h"
#include "GoreWindow.h"
#include "GoreTexture.h"
#include "GoreLogger.h"

class GoreRenderer {
public:
    GoreRenderer(GoreWindow& window);
    ~GoreRenderer();

    void Clear();
    void Present();
    void RenderTexture(GoreTexture* texture, const SDL_FRect* srcRect = nullptr, const SDL_FRect* dstRect = nullptr);

    SDL_GLContext GetRenderer() const;

private:
    void InitOpenGL();
    GoreWindow& _window;

    SDL_GLContext _glContext;
};
