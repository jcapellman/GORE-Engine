#pragma once
#include "framework.h"
#include "GoreWindow.h"
#include "GoreConfig.h"
#include "GoreResourceManager.h"
#include "GoreRenderer.h"

class GoreMain {
public:
	GoreMain(std::string title);

	void Init(const std::string& configFileName = DEFAULT_CONFIG_FILENAME, const std::string& gameName = DEFAULT_GAME_NAME);

	void Run();

	void Shutdown();

	~GoreMain();
private:
	std::string _title;

	GoreWindow _gWindow;
	GoreConfig _gConfig;

	std::unique_ptr<GoreResourceManager> _gResourceManager;
	std::unique_ptr<GoreRenderer> _gRenderer;
};