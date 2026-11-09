# MPIPTVSource: translated from MPIPTVSource.vcxproj.

add_library(MPIPTVSource SHARED
  Crc32.cpp
  dllmain.cpp
  LinearBuffer.cpp
  Logger.cpp
  Memory.cpp
  MPIPTVSource.cpp
  MPIPTVSourceStream.cpp
  Network.cpp
  Parameter.cpp
  ParameterCollection.cpp
  PatParser.cpp
  PmtParser.cpp
  PmtStreamDescription.cpp
  PmtStreamDescriptionCollection.cpp
  stdafx.cpp
  Strings.cpp
  Utilities.cpp
  mpiptvsource.def
)

mp_target(MPIPTVSource
  SUFFIX ".ax"
  OUTPUT_NAME "MPIPTVSource"
  FOLDER "MPIPTVSource"
  DEFINITIONS
    MPIPTVSOURCE_EXPORTS
    _USRDLL
  LIBS
    winmm
    Shlwapi
    Ws2_32
    Iphlpapi
    BaseClasses
)
