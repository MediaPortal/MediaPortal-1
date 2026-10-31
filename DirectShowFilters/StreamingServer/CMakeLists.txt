# StreamingServer: translated from StreamingServer.vcxproj.

add_library(StreamingServer SHARED
  Source/main.cpp
  Source/FileReader.cpp
  Source/MultiFileReader.cpp
  Source/TsMPEG2TransportFileServerMediaSubsession.cpp
  Source/TsMPEG2TransportStreamFramer.cpp
  Source/TsStreamFileSource.cpp
  Source/TSBuffer.cpp
  Source/TsDuration.cpp
  Source/TsFileSeek.cpp
  Source/TSThread.cpp
  Source/StreamingServer.def
)

mp_target(StreamingServer
  SUFFIX ".dll"
  OUTPUT_NAME "StreamingServer"
  FOLDER "Filters"
  INCLUDES
    ../LiveMedia555/MediaPortal
  DEFINITIONS
    $<$<CONFIG:Debug>:_CONSOLE>
    $<$<NOT:$<CONFIG:Debug>>:_WIN32>
  LIBS
    winmm
    WS2_32
    DvbCoreUtils
    LiveMedia555
    BaseClasses
)
