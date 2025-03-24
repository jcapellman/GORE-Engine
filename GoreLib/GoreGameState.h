#pragma once
#include "framework.h"
#include "GoreScreen.h"
#include "GoreEvent.h"

class GoreGameState {
public:
	GoreGameState();

    ~GoreGameState();
    
    void AddScreen(const std::string& name, std::unique_ptr<GoreScreen> screen);
    void RemoveScreen(const std::string& name);
    void SetActiveScreen(const std::string& name);

    void HandleEvents(GoreEvent event);
    void Update();
    void Render();

private:
    std::map<std::string, std::unique_ptr<GoreScreen>> _screens;
    GoreScreen* _activeScreen;
};