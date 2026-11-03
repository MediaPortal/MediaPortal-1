# MPIPTV_HTTP: translated from MPIPTV_HTTP.vcxproj.

add_library(MPIPTV_HTTP SHARED
  dllmain.cpp
  MPIPTV_HTTP.cpp
  stdafx.cpp
  mpiptv_http.def
)

mp_target(MPIPTV_HTTP
  SUFFIX ".dll"
  OUTPUT_NAME "MPIPTV_HTTP"
  FOLDER "MPIPTVSource"
  INCLUDES
    ../MPIPTVSource
  DEFINITIONS
    MPIPTV_HTTP_EXPORTS
    _USRDLL
  LIBS
    Ws2_32
    wininet
    Iphlpapi
    MPIPTVSource
    BaseClasses
)
