#pragma once
#include "framework.h"
#include "GoreResourceManager.h"

class GoreScreen {
public:
    virtual ~GoreScreen() = default;
    virtual void LoadResources() = 0;
    virtual void UnloadResources() = 0;
    virtual void Update() = 0;
    virtual void Render() = 0;

    void SetResourceManager(GoreResourceManager* resourceManager) {
        _resourceManager = resourceManager;
    }
protected:
    GoreResourceManager* _resourceManager = nullptr;
};