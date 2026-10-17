# MPIPTV_FILE: translated from MPIPTV_FILE.vcxproj.

add_library(MPIPTV_FILE SHARED
  dllmain.cpp
  MPIPTV_FILE.cpp
  stdafx.cpp
  mpiptv_file.def
)

mp_target(MPIPTV_FILE
  SUFFIX ".dll"
  OUTPUT_NAME "MPIPTV_FILE"
  FOLDER "MPIPTVSource"
  INCLUDES
    ../MPIPTVSource
  DEFINITIONS
    MPIPTV_FILE_EXPORTS
    _USRDLL
  LIBS
    wininet
    urlmon
    MPIPTVSource
    BaseClasses
)
