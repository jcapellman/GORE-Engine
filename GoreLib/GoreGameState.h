#pragma once
#include "framework.h"
#include "GoreScreen.h"

class GoreGameState {
public:
	GoreGameState();

	~GoreGameState();
private:
	std::map<std::string, GoreScreen> _screens;
};