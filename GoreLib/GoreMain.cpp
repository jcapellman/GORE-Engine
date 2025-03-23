#include "pch.h"
#include "GoreMain.h"

GoreMain::GoreMain(const std::string& title) : _title(title), _gWindow() {
}

void GoreMain::Init(const std::string& configFileName, const std::string& gameName) {
    _gConfig = GoreConfig(configFileName);

    _gWindow.Init(_title, _gConfig.GetIntValue(GoreConfigKeys::R_SCREEN_WIDTH), _gConfig.GetIntValue(GoreConfigKeys::R_SCREEN_HEIGHT));

    _gRenderer = std::make_unique<GoreRenderer>(_gWindow);

    _gResourceManager = std::make_unique<GoreResourceManager>(gameName, _gRenderer.get());
}

void GoreMain::Run() {
    SDL_Event e;

    bool quit = false;

    while (!quit) {
        while (SDL_PollEvent(&e)) {
            if (e.type == SDL_EVENT_QUIT) {
                quit = true;
            }
        }

        _gRenderer->Clear();
        // Render your textures here using _gRenderer->GetRenderer()
        _gRenderer->Present();
    }
}

void GoreMain::Shutdown() {
    _gWindow.Close();
}

GoreMain::~GoreMain() {
    Shutdown();
}
