# dvblib: translated from dvblib.vcxproj.
add_library(dvblib SHARED
  source/B2C2MPEG2AdapterWin.cpp
  source/dvblib.cpp
  source/stdafx.cpp
  source/dvblib.rc
  source/dvblib.def
)

mp_target(dvblib
  SUFFIX ".dll"
  OUTPUT_NAME "dvblib"
  FOLDER "Helpers"
  CXX_STANDARD 14
  DEFINITIONS
    _USRDLL
  LIBS
    strmiids
    BaseClasses
)
