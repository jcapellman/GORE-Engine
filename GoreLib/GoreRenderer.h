#pragma once
#include "GoreWindow.h"

class GoreRenderer {
public:
    GoreRenderer(GoreWindow& window);
    ~GoreRenderer();

    void Clear();
    void Present();
    SDL_Renderer* GetRenderer() const;

private:
    SDL_Renderer* _renderer;
};
