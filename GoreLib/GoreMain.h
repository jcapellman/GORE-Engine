#pragma once
#include "framework.h"
#include "GoreWindow.h"

class GoreMain {
public:
	GoreMain(std::string title) {
		_title = title;
	}

	void Init() {
		gWindow = GoreWindow();

		gWindow.Init(_title, 640, 480);
	}

	void Shutdown() {
		gWindow.Close();
	}
private:
	std::string _title;

	GoreWindow gWindow;
};