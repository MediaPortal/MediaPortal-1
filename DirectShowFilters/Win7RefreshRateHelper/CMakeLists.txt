# Win7RefreshRateHelper: translated from Win7RefreshRateHelper.vcxproj.

add_library(Win7RefreshRateHelper SHARED
  source/Win7RefreshRateHelper.cpp
  source/Win7RefreshRateHelper.def
)

mp_target(Win7RefreshRateHelper
  SUFFIX ".dll"
  OUTPUT_NAME "Win7RefreshRateHelper"
  FOLDER "Helpers"
  DEFINITIONS
    WIN7REFRESHRATEHELPER_EXPORTS
    _USRDLL
  LIBS
    winmm
    comsupp
    dxguid
    d3d9
    d3dx9
    mfuuid
    dxva2
    evr
    mf
    strmiids
    mp_dxsdk
)
