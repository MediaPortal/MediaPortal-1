# CCCP: translated from CCCP.vcxproj.

add_library(CCCP SHARED
  ../parsing/CcParseH264.cpp
  ../parsing/CCParser.cpp
  CCcFilter.cpp
  CCCP.cpp
  CcDataProcessor.cpp
  CCPProps.cpp
  StdAfx.cpp
  CCParser.rc
  CCCP.def
)

mp_target(CCCP
  SUFFIX ".ax"
  OUTPUT_NAME "cccp"
  FOLDER "Filters"
  INCLUDES
    .
    SoundTouch/include
  LIBS
    quartz
    vfw32
    winmm
    version
    comctl32
    BaseClasses
    mp_dxsdk
)
