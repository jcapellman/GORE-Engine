# CMakeList.txt : CMake project for GoreEngine, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

# Enable Hot Reload for MSVC compilers if supported.
if (POLICY CMP0141)
  cmake_policy(SET CMP0141 NEW)
  set(CMAKE_MSVC_DEBUG_INFORMATION_FORMAT "$<IF:$<AND:$<C_COMPILER_ID:MSVC>,$<CXX_COMPILER_ID:MSVC>>,$<$<CONFIG:Debug,RelWithDebInfo>:EditAndContinue>,$<$<CONFIG:Debug,RelWithDebInfo>:ProgramDatabase>>")
endif()

project ("GoreEngine")
# Find SDL2 package
find_package(SDL2 REQUIRED)
find_package(SDL2_image REQUIRED)

file(GLOB SOURCES "*.cpp" "*.h")
add_executable (GoreEngine ${SOURCES})

target_link_libraries(GoreEngine PRIVATE ${SDL2_LIBRARIES} ${SDL2_IMAGE_LIBRARIES})
include_directories(${SDL2_INCLUDE_DIRS} ${SDL2_IMAGE_INCLUDE_DIRS})

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET GoreEngine PROPERTY CXX_STANDARD 20)
endif()
