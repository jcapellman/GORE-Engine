#include "GoreMain.h"

GoreMain::GoreMain(const std::string& title) : _title(title), _gWindow() {
}

void GoreMain::Init(const std::string& configFileName, const std::string& gameName, LOGLEVEL logLevel) {
    GoreLogger::getInstance().setGameRootFolder(gameName);

    GoreLogger::getInstance().setLogLevel(logLevel);

	GoreLogger::getInstance().log(INFO, "Initializing GoreMain with config file: " + configFileName + " and game name: " + gameName);

    _gConfig = GoreConfig(configFileName);

	GoreLogger::getInstance().log(INFO, "Config values: Screen Width: " + std::to_string(_gConfig.GetIntValue(GoreConfigKeys::R_SCREEN_WIDTH)) +
		", Screen Height: " + std::to_string(_gConfig.GetIntValue(GoreConfigKeys::R_SCREEN_HEIGHT)));

    _gWindow.Init(_title, _gConfig.GetIntValue(GoreConfigKeys::R_SCREEN_WIDTH), _gConfig.GetIntValue(GoreConfigKeys::R_SCREEN_HEIGHT));

    _renderer = SDL_CreateRenderer(_gWindow.Get(), gameName.c_str());

    if (!_renderer) {
        GoreLogger::getInstance().log(ERR, "Failed to create SDL Renderer");
        GoreLogger::getInstance().log(ERR, SDL_GetError());
        return;
    }

    _gResourceManager = std::make_unique<GoreResourceManager>(_renderer, gameName);
}

void GoreMain::Run() {
    bool running = true;
    SDL_Event event;

    while (running) {
        while (SDL_PollEvent(&event)) {
            if (event.type == SDL_EVENT_QUIT) {
                running = false;
            }
        }

        // Clear the screen with a color
        SDL_SetRenderDrawColor(_renderer, 26, 26, 26, 255); // Set clear color (0.1f, 0.1f, 0.1f, 1.0f)
        SDL_RenderClear(_renderer);                         // Clear the screen

        // Render game state
        Render();

        // Present the back buffer
        SDL_RenderPresent(_renderer);
    }
}


void GoreMain::Shutdown() {
    if (_renderer) {
        SDL_DestroyRenderer(_renderer);
        _renderer = nullptr;
    }
    _gWindow.Close();
}

GoreMain::~GoreMain() {
    Shutdown();
}

void GoreMain::Update() {
    _gGameState.Update();
}

void GoreMain::Render() {
    _gGameState.Render();
}

void GoreMain::AddScreen(const std::string& name, std::unique_ptr<GoreScreen> screen, bool isActiveScreen) {
    screen->SetResourceManager(_gResourceManager.get());

    _gGameState.AddScreen(name, std::move(screen));

    if (isActiveScreen) {
        _gGameState.SetActiveScreen(name);
    }
}