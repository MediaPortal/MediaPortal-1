# dxerr9: translated from dxerr9.vcxproj.

add_library(dxerr9 SHARED
  source/dxerr9.cpp
  source/stdafx.cpp
)

mp_target(dxerr9
  SUFFIX ".dll"
  OUTPUT_NAME "dxerr9"
  FOLDER "Helpers"
  DEFINITIONS
    DXERR9_EXPORTS
    _USRDLL
  LIBS
    dxerr
    legacy_stdio_definitions
    mp_dxsdk
    $<IF:$<CONFIG:Debug>,comsuppwd,comsuppw>
)
