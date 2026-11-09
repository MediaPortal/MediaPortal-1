# DXUtil: translated from DXUtil.vcxproj.

add_library(DXUtil SHARED
  source/DXUtil.cpp
  source/stdafx.cpp
)

mp_target(DXUtil
  SUFFIX ".dll"
  OUTPUT_NAME "dxutil"
  FOLDER "Helpers"
  DEFINITIONS
    DXUTIL_EXPORTS
    _USRDLL
  LIBS
    mp_dxsdk
)
