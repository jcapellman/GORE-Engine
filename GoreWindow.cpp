#include "GoreWindow.h"

void GoreWindow::InitOpenGL() {
	// Create OpenGL context
	GoreLogger::getInstance().log(DEBUG, "Initializing OpenGL Renderer...");

	_glContext = SDL_GL_CreateContext(_window);

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
	glViewport(0, 0, _width, _height);
	glClearColor(0.0f, 0.0f, 1.0f, 1.0f);

	GoreLogger::getInstance().log(DEBUG, "OpenGL Renderer Initialized");
}

void GoreWindow::Init(std::string windowTitle, unsigned int width, unsigned int height) {
	GoreLogger::getInstance().log(DEBUG, "Initializing SDL...");

	if (!SDL_Init(SDL_INIT_EVERYTHING)) {
		GoreLogger::getInstance().log(ERR, "Failed to Init SDL");
		GoreLogger::getInstance().log(ERR, SDL_GetError());

		return;
	}

	GoreLogger::getInstance().log(DEBUG, "SDL Initialized");

	GoreLogger::getInstance().log(DEBUG, "Initializing SDL Window with OpenGL...");

	if (SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 1) != 0) {
		GoreLogger::getInstance().log(ERR, "Error setting GL Context Major Version");
		GoreLogger::getInstance().log(ERR, SDL_GetError());

		return;
	}

	if (SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 1) != 0) {
		GoreLogger::getInstance().log(ERR, "Error setting GL Context Minor Version");
		GoreLogger::getInstance().log(ERR, SDL_GetError());

		return;
	}

	_window = SDL_CreateWindow(windowTitle.c_str(),
		SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED,
		width, height, SDL_WINDOW_OPENGL);

	if (!_window) {
		GoreLogger::getInstance().log(ERR, "Failed to create Window");
		GoreLogger::getInstance().log(ERR, SDL_GetError());
	}

	_width = width;
	_height = height;

	InitOpenGL();

	GoreLogger::getInstance().log(DEBUG, "SDL Window Initialized");
}

void GoreWindow::Close() {
	if (_glContext) {
		SDL_GL_DeleteContext(_glContext);
	}

	SDL_DestroyWindow(_window);
}

SDL_Window* GoreWindow::Get() const {
	return _window;
}

unsigned int GoreWindow::GetWidth() const {
	return _width;
}

unsigned int GoreWindow::GetHeight() const {
	return _height;
}