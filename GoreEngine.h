#pragma once

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

#include <SDL3/SDL.h>

#include <iostream>
#include <fstream>
#include <string>
#include <map>
#include <memory>
#include <stdexcept>
#include <filesystem>
#include <mutex>
#include <iomanip>
#include <chrono>
#include <sstream>

#pragma comment(lib, "SDL3.lib")

const std::string DEFAULT_CONFIG_FILENAME = "config.cfg";
const std::string DEFAULT_GAME_NAME = "base";