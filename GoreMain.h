#pragma once
#include "framework.h"
#include "GoreWindow.h"
#include "GoreConfig.h"
#include "GoreResourceManager.h"
#include "GoreRenderer.h"
#include "GoreGameState.h"
#include "GoreScreen.h"

class GoreMain {
public:
    GoreMain(const std::string& title);

    void Init(const std::string& configFileName = DEFAULT_CONFIG_FILENAME, const std::string& gameName = DEFAULT_GAME_NAME);

    void Run();

    void Shutdown();

    ~GoreMain();

    void AddScreen(const std::string& name, std::unique_ptr<GoreScreen> screen, bool isActiveScreen = false);
private:
    void HandleEvents();
    void Update();
    void Render();

    bool _isRunning = true;

    std::string _title;

    GoreWindow _gWindow;
    GoreConfig _gConfig;
    std::unique_ptr<GoreResourceManager> _gResourceManager;
    std::unique_ptr<GoreRenderer> _gRenderer;
	GoreGameState _gGameState;
};
