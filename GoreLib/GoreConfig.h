#pragma once

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
        _mapConfig[enum_to_string(GoreConfigKeys::R_SCREEN_WIDTH)] = "720";
        _mapConfig[enum_to_string(GoreConfigKeys::R_SCREEN_HEIGHT)] = "480";
        _mapConfig[enum_to_string(GoreConfigKeys::R_BPP)] = "16";
    }

private:
    std::string enum_to_string(GoreConfigKeys configKey) {
        std::string key;

        switch (configKey) {
        case R_SCREEN_WIDTH:
            key = "R_SCREEN_WIDTH";
            break;
        case R_SCREEN_HEIGHT:
            key = "R_SCREEN_HEIGHT";
            break;
        case R_BPP:
            key = "R_BPP";
            break;
        case S_VOLUME:
            key = "S_VOLUME";
            break;
        case S_MUSIC_VOLUME:
            key = "S_MUSIC_VOLUME";
            break;
        }

        return key;
    }

	std::map<std::string, std::string> _mapConfig;
};