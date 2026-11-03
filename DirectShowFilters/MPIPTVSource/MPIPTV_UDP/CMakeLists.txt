# MPIPTV_UDP: translated from MPIPTV_UDP.vcxproj.

add_library(MPIPTV_UDP SHARED
  dllmain.cpp
  MPIPTV_UDP.cpp
  stdafx.cpp
  mpiptv_udp.def
)

mp_target(MPIPTV_UDP
  SUFFIX ".dll"
  OUTPUT_NAME "MPIPTV_UDP"
  FOLDER "MPIPTVSource"
  INCLUDES
    ../MPIPTVSource
  DEFINITIONS
    MPIPTV_UDP_EXPORTS
    _USRDLL
  LIBS
    Ws2_32
    wininet
    MPIPTVSource
    BaseClasses
)
