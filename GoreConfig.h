#pragma once

#include "GoreEngine.h"

constexpr auto DEFAULT_R_SCREEN_WIDTH = "720";
constexpr auto DEFAULT_R_SCREEN_HEIGHT = "480";
constexpr auto DEFAULT_R_BPP = "16";
constexpr auto DEFAULT_S_VOLUME = "100";
constexpr auto DEFAULT_S_MUSIC_VOLUME = "100";

enum GoreConfigKeys {
    R_SCREEN_WIDTH,  // Resolution width
    R_SCREEN_HEIGHT, // Resolution height
    R_BPP,           // Bits per pixel
    S_VOLUME,        // Sound volume
    S_MUSIC_VOLUME,  // Music volume
};

class GoreConfig {
public:
    GoreConfig(const std::string& fileName = DEFAULT_CONFIG_FILENAME);

    std::string GetStringValue(const std::string& key) const;

    int GetIntValue(const std::string& key) const;

    std::string GetValue(GoreConfigKeys configKey);

    int GetIntValue(GoreConfigKeys configKey);

    void LoadDefaults();
private:
    std::string enum_to_string(GoreConfigKeys configKey);

    void SaveToFile(const std::string& fileName);

    std::map<std::string, std::string> _mapConfig;
};