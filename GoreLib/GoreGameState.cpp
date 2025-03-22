#include "pch.h"
#include "GoreGameState.h"

GoreGameState::GoreGameState() {
}

GoreGameState::~GoreGameState() {
	for (auto& pair : _screens) {
		pair.second.~GoreScreen();
	}

	_screens.clear();
}