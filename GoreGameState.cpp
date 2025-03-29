#include "pch.h"
#include "GoreGameState.h"

GoreGameState::GoreGameState() : _activeScreen(nullptr) {
}

GoreGameState::~GoreGameState() {
	_screens.clear();
}


void GoreGameState::AddScreen(const std::string& name, std::unique_ptr<GoreScreen> screen) {
    _screens[name] = std::move(screen);
}

void GoreGameState::RemoveScreen(const std::string& name) {
    _screens.erase(name);
}

void GoreGameState::SetActiveScreen(const std::string& name) {
    auto it = _screens.find(name);
    if (it != _screens.end()) {
        _activeScreen = it->second.get();
        _activeScreen->LoadResources();
    }
}

void GoreGameState::HandleEvents(GoreEvent event) {
    if (_activeScreen) {
        _activeScreen->HandleEvents(event);
    }
}

void GoreGameState::Update() {
    if (_activeScreen) {
        _activeScreen->Update();
    }
}

void GoreGameState::Render() {
    if (_activeScreen) {
        _activeScreen->Render();
    }
}
