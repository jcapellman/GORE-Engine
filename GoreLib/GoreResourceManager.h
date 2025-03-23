#pragma once
#include "framework.h"
#include "GoreTexture.h"
#include "GoreRenderer.h"
#include <map>
#include <string>

enum RESOURCE_TYPES {
    TEXTURE
};

class GoreResourceManager {
public:
    GoreResourceManager(const std::string& baseFolderName, GoreRenderer* renderer);
    ~GoreResourceManager();

    void LoadResource(RESOURCE_TYPES resourceType, const std::string& fileName, const std::string& key);
    GoreTexture* GetTexture(const std::string& key);

private:
    std::map<std::string, GoreTexture*> _textures;
    std::string _baseFolderName;
    GoreRenderer* _renderer;
};
