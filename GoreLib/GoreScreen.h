#pragma once
#include "framework.h"

class GoreScreen {
public:
    virtual ~GoreScreen() = default;
    virtual void LoadResources() = 0;
    virtual void UnloadResources() = 0;
    virtual void Update() = 0;
    virtual void Render() = 0;
};