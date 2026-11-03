# MPIPTV_RTP: translated from MPIPTV_RTP.vcxproj.

add_library(MPIPTV_RTP SHARED
  dllmain.cpp
  MPIPTV_RTP.cpp
  RtpPacket.cpp
  RtpSource.cpp
  stdafx.cpp
  mpiptv_rdp.def
)

mp_target(MPIPTV_RTP
  SUFFIX ".dll"
  OUTPUT_NAME "MPIPTV_RTP"
  FOLDER "MPIPTVSource"
  INCLUDES
    ../MPIPTVSource
    ../MPIPTV_UDP
  DEFINITIONS
    MPIPTV_RTP_EXPORTS
    _USRDLL
  LIBS
    Ws2_32
    wininet
    MPIPTVSource
    MPIPTV_UDP
    BaseClasses
)
