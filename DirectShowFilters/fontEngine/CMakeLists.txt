# fontEngine: translated from fontEngine.vcxproj.

add_library(fontEngine SHARED
  source/EffectStateManager.cpp
  source/fontEngine.cpp
  source/stdafx.cpp
  source/fontEngine.def
)

mp_target(fontEngine
  SUFFIX ".dll"
  OUTPUT_NAME "fontEngine"
  FOLDER "Helpers"
  EH ASYNC
  DEFINITIONS
    FONTENGINE_EXPORTS
    _USRDLL
  LIBS
    dxguid
    d3d9
    d3dx9
    shell32
    mp_dxsdk
    Freetype::Freetype
)
