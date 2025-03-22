#include "pch.h"
#include "GoreResourceManager.h"
#include <SDL3_image/SDL_image.h>

GoreResourceManager::GoreResourceManager(std::string baseFolderName, SDL_Renderer * renderer) {
    _baseFolderName = baseFolderName;
	_renderer = renderer;
}

GoreResourceManager::~GoreResourceManager() {
    for (auto& pair : _textures) {
		pair.second.~GoreTexture();
    }

    _textures.clear();
}

void GoreResourceManager::LoadResource(RESOURCE_TYPES resourceType, std::string fileName, std::string key) {
    std::filesystem::path filePath = std::filesystem::path(_baseFolderName) / fileName;

    switch (resourceType) {
    case TEXTURE:
        GoreTexture texture = GoreTexture(filePath.string(), _renderer);
        _textures.insert(std::make_pair(key, texture));
        break;
    }
}

GoreTexture GoreResourceManager::GetTexture(std::string key) {
	return _textures.at(key);
}