#include "pch.h"
#include "GoreResourceManager.h"
#include <filesystem>

GoreResourceManager::GoreResourceManager(const std::string& baseFolderName, GoreRenderer* renderer)
    : _baseFolderName(baseFolderName), _renderer(renderer) {
}

GoreResourceManager::~GoreResourceManager() {
    for (auto& pair : _textures) {
        delete pair.second;
    }
    _textures.clear();
}

void GoreResourceManager::LoadResource(RESOURCE_TYPES resourceType, const std::string& fileName, const std::string& key) {
    std::filesystem::path filePath = std::filesystem::path(_baseFolderName) / fileName;

    switch (resourceType) {
    case TEXTURE:
        _textures[key] = new GoreTexture(filePath.string(), _renderer);
        break;
    }
}

GoreTexture* GoreResourceManager::GetTexture(const std::string& key) {
    return _textures.at(key);
}
