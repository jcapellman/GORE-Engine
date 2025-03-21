#pragma once

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
	GoreConfig(const std::string& fileName = DEFAULT_CONFIG_FILENAME) {
        std::ifstream file(fileName);

        if (!file.is_open()) {
            LoadDefaults();

            return;
            //throw std::runtime_error("Failed to open file: " + fileName);
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

    std::string GetStringValue(const std::string& key) const {
        auto it = _mapConfig.find(key);

        if (it == _mapConfig.end()) {
            throw std::runtime_error("Key not found: " + key);
        }
        return it->second;
    }

    int GetIntValue(const std::string& key) const {
        std::string value = GetStringValue(key);

        return std::stoi(value);
    }

    std::string GetValue(GoreConfigKeys configKey) {
        std::string key = enum_to_string(configKey);

        return GetStringValue(key);
    }

    int GetIntValue(GoreConfigKeys configKey) {
        std::string key = enum_to_string(configKey);

        return GetIntValue(key);
    }

    void LoadDefaults() {
        _mapConfig[enum_to_string(GoreConfigKeys::R_SCREEN_WIDTH)] = DEFAULT_R_SCREEN_WIDTH;
        _mapConfig[enum_to_string(GoreConfigKeys::R_SCREEN_HEIGHT)] = DEFAULT_R_SCREEN_WIDTH;
        _mapConfig[enum_to_string(GoreConfigKeys::R_BPP)] = DEFAULT_R_BPP;
        _mapConfig[enum_to_string(GoreConfigKeys::S_VOLUME)] = DEFAULT_S_VOLUME;
        _mapConfig[enum_to_string(GoreConfigKeys::S_MUSIC_VOLUME)] = DEFAULT_S_MUSIC_VOLUME;
    }

private:
    std::string enum_to_string(GoreConfigKeys configKey) {
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

	std::map<std::string, std::string> _mapConfig;
};