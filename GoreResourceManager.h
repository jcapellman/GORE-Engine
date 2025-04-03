#pragma once
#include "GoreEngine.h"
#include "GoreTexture.h"
#include <map>
#include <string>

enum RESOURCE_TYPES {
    TEXTURE
};

class GoreResourceManager {
public:
    GoreResourceManager(SDL_Renderer * renderer, const std::string& baseFolderName);
    ~GoreResourceManager();
    void LoadResource(RESOURCE_TYPES resourceType, const std::string& fileName, const std::string& key);
    GoreTexture* GetTexture(const std::string& key);

private:
    std::string GetResourceFolderFromEnum(RESOURCE_TYPES resourceType);
    std::filesystem::path GetResourceFilePath(const std::string& fileName, RESOURCE_TYPES resourceType, bool usingBaseFolder);

    std::map<std::string, GoreTexture*> _textures;
    std::string _baseFolderName;
    SDL_Renderer* _renderer;
};

