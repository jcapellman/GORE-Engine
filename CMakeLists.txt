cmake_minimum_required(VERSION 4.0)

project("GoreEngine")

# Set CMAKE_PREFIX_PATH to the SDL3 directory
set(CMAKE_PREFIX_PATH "C:/SDL3/VisualC/")

# Find SDL3 package
find_package(SDL3 REQUIRED)

file(GLOB SOURCES "*.cpp" "*.h")
add_executable(GoreEngine WIN32 ${SOURCES})

# Link SDL3 and SDL3_image libraries
target_link_libraries(GoreEngine PRIVATE SDL3::SDL3)

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET GoreEngine PROPERTY CXX_STANDARD 20)
endif()
