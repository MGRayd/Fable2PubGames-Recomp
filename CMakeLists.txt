# Fable2PubGames - ReXGlue Recompiled Project
#
# SDK boilerplate lives in generated/rexglue.cmake.

cmake_minimum_required(VERSION 3.25)
project(Fable2PubGames LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

include(generated/rexglue.cmake)

# Sources
set(FABLE2PUBGAMES_SOURCES
    src/main.cpp
    src/PubGamesAchievementsOverlay.cpp
)

if(WIN32)
    add_executable(Fable2PubGames WIN32 ${FABLE2PUBGAMES_SOURCES})
else()
    add_executable(Fable2PubGames ${FABLE2PUBGAMES_SOURCES})
endif()

rexglue_setup_target(Fable2PubGames GPU_PLUGINS xenos)
