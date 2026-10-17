# MPIPTV_RTSP: translated from MPIPTV_RTSP.vcxproj.

add_library(MPIPTV_RTSP SHARED
  dllmain.cpp
  MPIPTV_RTSP.cpp
  RtspTaskScheduler.cpp
  stdafx.cpp
  mpiptv_rtsp.def
)

mp_target(MPIPTV_RTSP
  SUFFIX ".dll"
  OUTPUT_NAME "MPIPTV_RTSP"
  FOLDER "MPIPTVSource"
  INCLUDES
    ../../LiveMedia555/MediaPortal
    ../MPIPTVSource
    ../MPIPTV_UDP
  DEFINITIONS
    MPIPTV_RTSP_EXPORTS
    _USRDLL
  LIBS
    wininet
    ws2_32
    MPIPTVSource
    MPIPTV_UDP
    LiveMedia555
    BaseClasses
)
