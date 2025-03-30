#include "GoreTexture.h"

GoreTexture::GoreTexture(SDL_Surface * surface) {
    glGenTextures(1, &_texture); // Generate a texture ID
    glBindTexture(GL_TEXTURE_2D, _texture); // Bind the texture

    // Specify the texture parameters
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

    // Upload the SDL_Surface's pixel data to OpenGL
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, surface->w, surface->h, 0,
        GL_RGBA, GL_UNSIGNED_BYTE, surface->pixels);
}

GoreTexture::~GoreTexture() {
	glDeleteTextures(1, &_texture);
}

GLuint GoreTexture::Get() const {
    return _texture;
}
