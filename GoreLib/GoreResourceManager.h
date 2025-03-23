#pragma once
#include "framework.h"
#include "GoreTexture.h"

enum RESOURCE_TYPES {
	TEXTURE
};

class GoreResourceManager {
public:
	GoreResourceManager(std::string baseFolderName, GoreRenderer renderer);

	~GoreResourceManager();

	void LoadResource(RESOURCE_TYPES resourceType, std::string fileName, std::string key);

	GoreTexture GetTexture(std::string key);
private:
	std::map<std::string, GoreTexture> _textures;
	std::string _baseFolderName;
	GoreRenderer _renderer;
};