#include "GoreRenderer.h"

GoreRenderer::GoreRenderer(GoreWindow& window) : _window(window) {
    InitOpenGL();
}

GoreRenderer::~GoreRenderer() {
    if (_glContext) {
        SDL_GL_DeleteContext(_glContext);
    }
}

void GoreRenderer::Clear() {
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
}

void GoreRenderer::Present() {
    SDL_GL_SwapWindow(_window.Get());
}

void GoreRenderer::RenderTexture(GoreTexture* texture, const SDL_FRect* srcRect, const SDL_FRect* dstRect) {
    if (texture && texture->Get()) {
        
    }
}

SDL_GLContext GoreRenderer::GetRenderer() const {
    return _glContext;
}

void GoreRenderer::InitOpenGL() {
    // Create OpenGL context
    GoreLogger::getInstance().log(DEBUG, "Initializing OpenGL Renderer...");

    

    if (!_window.Get()) {
        GoreLogger::getInstance().log(ERR, "Could not get the SDL Window");

        return;
    }

    _glContext = SDL_GL_CreateContext(_window.Get());

    if (!_glContext) {
        GoreLogger::getInstance().log(ERR, "Failed to initialize OpenGL Context");
        GoreLogger::getInstance().log(ERR, SDL_GetError());

        return;
    }

    const GLubyte* version = glGetString(GL_VERSION);
    const GLubyte* vendor = glGetString(GL_VENDOR);
    const GLubyte* renderer = glGetString(GL_RENDERER);

    GoreLogger::getInstance().log(INFO, "OpenGL Renderer:");
    GoreLogger::getInstance().log(INFO, std::string(reinterpret_cast<const char*>(version)));

    // Set up OpenGL state (e.g., viewport, clear color)
    glViewport(0, 0, _window.GetWidth(), _window.GetHeight());
    glClearColor(0.0f, 0.0f, 1.0f, 1.0f);

    GoreLogger::getInstance().log(DEBUG, "OpenGL Renderer Initialized");
}