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
    while (_isRunning) {
        HandleEvents();
        Update();
        Render();
    }
}

void GoreMain::HandleEvents() {
    SDL_Event e;

    while (SDL_PollEvent(&e)) {
        GoreEvent event = GoreEvent::NONE;

        switch (e.type) {
        case SDL_EVENT_QUIT:
            _isRunning = false;
            break;
        case SDL_EVENT_KEY_DOWN:
            switch (e.key.key) {
            case SDLK_UP:
                event = GoreEvent::KEY_UP;
                break;
            case SDLK_DOWN:
                event = GoreEvent::KEY_DOWN;
                break;
            case SDLK_ESCAPE:
                event = GoreEvent::KEY_ESCAPE;
                break;
            case SDLK_RETURN:
                event = GoreEvent::KEY_ENTER;
                break;
            default:
                break;
            }
            break;
        default:
            break;
        }
    
        if (event == GoreEvent::NONE) {
            return;
        }

        _gGameState.HandleEvents(event);
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