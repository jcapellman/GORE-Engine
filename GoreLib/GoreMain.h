#pragma once
#include "framework.h"
#include "GoreWindow.h"
#include "GoreConfig.h"

class GoreMain {
public:
	GoreMain(std::string title);

	void Init(const std::string& configFileName = DEFAULT_CONFIG_FILENAME);

	void Run();

	void Shutdown();

	~GoreMain();
private:
	std::string _title;

	GoreWindow _gWindow;
	GoreConfig _gConfig;
};