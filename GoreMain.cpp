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

    _gResourceManager = std::make_unique<GoreResourceManager>(gameName);
}

void GoreMain::Run() {
    bool running = true;
    SDL_Event event;

    while (running) {
        while (SDL_PollEvent(&event)) {
            if (event.type == SDL_QUIT) {
                running = false;
            }
        }

        // Clear the screen with a color
        glClearColor(0.1f, 0.1f, 0.1f, 1.0f); // Set clear color
        glClear(GL_COLOR_BUFFER_BIT);         // Clear the screen

        // Swap buffers (equivalent to double-buffering in Quake 3)
        SDL_GL_SwapWindow(_gWindow.Get());
    }
}

void GoreMain::Shutdown() {
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