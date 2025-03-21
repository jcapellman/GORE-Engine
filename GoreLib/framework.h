#pragma once

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

#include <SDL3/SDL.h>

#include <iostream>
#include <fstream>
#include <string>
#include <map>
#include <stdexcept>

const std::string DEFAULT_CONFIG_FILENAME = "config.cfg";