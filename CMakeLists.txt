cmake_minimum_required(VERSION 3.10)
project(GoreProject VERSION 2025.3.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED True)

find_package(SDL2 REQUIRED)
find_package(SDL2_image REQUIRED)

file(GLOB_RECURSE SOURCES
    "${PROJECT_SOURCE_DIR}/*.cpp"
    "${PROJECT_SOURCE_DIR}/*.h"
)

add_library(sge SHARED ${HEADERS} ${SOURCES})

target_include_directories(
    GoreProject
    PUBLIC
        ${SDL2_INCLUDE_DIRS}
        ${SDL2_IMAGE_INCLUDE_DIRS}
)

target_link_libraries(
    sge
    ${SDL2_LIBRARIES}
    ${SDL2_IMAGE_LIBRARIES}
)

target_compile_definitions(sge PUBLIC SDL_MAIN_HANDLED)