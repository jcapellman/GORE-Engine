#include "GoreResourceManager.h"
#include "GoreLogger.h"
#include <filesystem>

GoreResourceManager::GoreResourceManager(SDL_Renderer * renderer, const std::string& baseFolderName)
    : _baseFolderName(baseFolderName), _renderer(renderer) {
}

GoreResourceManager::~GoreResourceManager() {
    for (auto& pair : _textures) {
        delete pair.second;
    }
    _textures.clear();
}

std::string GoreResourceManager::GetResourceFolderFromEnum(RESOURCE_TYPES resourceType) {
    switch (resourceType) {
    case TEXTURE:
        return "textures";
    default:
        throw std::invalid_argument("Unhandled resource type");
    }
}

std::filesystem::path GoreResourceManager::GetResourceFilePath(const std::string& fileName, RESOURCE_TYPES resourceType, bool usingBaseFolder) {
    std::string resourceFolderName = GetResourceFolderFromEnum(resourceType);

    std::filesystem::path requestedPath = std::filesystem::path(_baseFolderName) / resourceFolderName / fileName;

    if (!std::filesystem::exists(requestedPath)) {
        if (_baseFolderName == DEFAULT_GAME_NAME || usingBaseFolder) {
            throw std::filesystem::filesystem_error("File does not exist: " + requestedPath.string(),
                std::make_error_code(std::errc::no_such_file_or_directory));
        }

        return GetResourceFilePath(fileName, resourceType, true);
    }

    return requestedPath;
}

void GoreResourceManager::LoadResource(RESOURCE_TYPES resourceType, const std::string& fileName, const std::string& key) {
    std::filesystem::path filePath = GetResourceFilePath(fileName, resourceType, false);

    switch (resourceType) {
    case TEXTURE:
        _textures[key] = new GoreTexture(_renderer, filePath.string());
        break;
    }
}

GoreTexture* GoreResourceManager::GetTexture(const std::string& key) {
    return _textures.at(key);
}

