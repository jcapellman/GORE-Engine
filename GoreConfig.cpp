#include "pch.h"
#include "GoreConfig.h"

GoreConfig::GoreConfig(const std::string& fileName) {
    std::ifstream file(fileName);

    if (!file.is_open()) {
        LoadDefaults();

        SaveToFile(fileName);

        return;
    }

    std::string line;
    while (std::getline(file, line)) {
        auto delimiterPos = line.find('=');
        if (delimiterPos == std::string::npos) {
            throw std::runtime_error("Invalid format in file: " + line);
        }
        std::string key = line.substr(0, delimiterPos);
        std::string value = line.substr(delimiterPos + 1);
        _mapConfig[key] = value;
    }

    file.close();
}

void GoreConfig::SaveToFile(const std::string & fileName) {
	std::ofstream file(fileName);

    if (!file.is_open()) {
        throw std::runtime_error("Failed to open " + fileName + " to write the config");
    }

	for (const auto& pair : _mapConfig) {
		file << pair.first << "=" << pair.second << std::endl;
	}

    file.close();
}

std::string GoreConfig::GetStringValue(const std::string& key) const {
    auto it = _mapConfig.find(key);
    if (it == _mapConfig.end()) {
        throw std::runtime_error("Key not found: " + key);
    }
    return it->second;
}

int GoreConfig::GetIntValue(const std::string& key) const {
    std::string value = GetStringValue(key);
    return std::stoi(value);
}

std::string GoreConfig::GetValue(GoreConfigKeys configKey) {
    std::string key = enum_to_string(configKey);
    return GetStringValue(key);
}

int GoreConfig::GetIntValue(GoreConfigKeys configKey) {
    std::string key = enum_to_string(configKey);
    return GetIntValue(key);
}

void GoreConfig::LoadDefaults() {
    _mapConfig[enum_to_string(GoreConfigKeys::R_SCREEN_WIDTH)] = DEFAULT_R_SCREEN_WIDTH;
    _mapConfig[enum_to_string(GoreConfigKeys::R_SCREEN_HEIGHT)] = DEFAULT_R_SCREEN_HEIGHT;
    _mapConfig[enum_to_string(GoreConfigKeys::R_BPP)] = DEFAULT_R_BPP;
    _mapConfig[enum_to_string(GoreConfigKeys::S_VOLUME)] = DEFAULT_S_VOLUME;
    _mapConfig[enum_to_string(GoreConfigKeys::S_MUSIC_VOLUME)] = DEFAULT_S_MUSIC_VOLUME;
}

std::string GoreConfig::enum_to_string(GoreConfigKeys configKey) {
    static const std::map<GoreConfigKeys, std::string> enumToStringMap = {
        {R_SCREEN_WIDTH, "R_SCREEN_WIDTH"},
        {R_SCREEN_HEIGHT, "R_SCREEN_HEIGHT"},
        {R_BPP, "R_BPP"},
        {S_VOLUME, "S_VOLUME"},
        {S_MUSIC_VOLUME, "S_MUSIC_VOLUME"}
    };

    auto it = enumToStringMap.find(configKey);
    if (it != enumToStringMap.end()) {
        return it->second;
    }

    throw std::runtime_error("Invalid GoreConfigKey");
}
