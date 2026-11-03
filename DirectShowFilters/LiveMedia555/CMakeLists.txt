# LiveMedia555: translated from LiveMedia555.vcxproj.

add_library(LiveMedia555 STATIC
  MediaPortal/MPMPEG2TransportStreamFromPESSource.cpp
  MediaPortal/MPRTSPClient.cpp
  MediaPortal/MPRTSPServer.cpp
  MediaPortal/MPTaskScheduler.cpp
)

mp_target(LiveMedia555
  OUTPUT_NAME "LiveMedia555"
  FOLDER "Libraries"
  CXX_STANDARD 20
  INCLUDES
    ./MediaPortal
  DEFINITIONS
    _WIN32
    $<$<CONFIG:Debug>:WINNT>
  LIBS
    legacy_stdio_definitions
    mp_dxsdk
  PUBLIC_LIBS
    unofficial::liveMedia
    unofficial::groupsock
    unofficial::BasicUsageEnvironment
    unofficial::UsageEnvironment
    legacy_stdio_definitions
    crypt32
    ws2_32
)
