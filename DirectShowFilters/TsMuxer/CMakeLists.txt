# TsMuxer: translated from TsMuxer.vcxproj.

add_library(TsMuxer SHARED
  source/ChannelScan.cpp
  source/ElementaryToTransportStream.cpp
  source/Hamming.cpp
  source/MemoryBuffer.cpp
  source/MemoryStreamSink.cpp
  source/MemoryStreamSource.cpp
  source/PacketSync.cpp
  source/ProgramToTransportStream.cpp
  source/TeletextGrabber.cpp
  source/TsMuxer.cpp
  source/TSThread.cpp
  source/WaitEvent.cpp
  source/AudioInputPin.cpp
  source/MPEGInputPin.cpp
  source/TeletextInputPin.cpp
  source/TsInputPin.cpp
  source/TsOutputPin.cpp
  source/VideoInputPin.cpp
  source/TsMuxer.def
)

mp_target(TsMuxer
  SUFFIX ".ax"
  OUTPUT_NAME "TsMuxer"
  FOLDER "Filters"
  INCLUDES
    ../LiveMedia555/MediaPortal
  DEFINITIONS
    _WIN32
    $<$<CONFIG:Debug>:INC_OLE2>
    $<$<CONFIG:Debug>:STRICT>
    $<$<CONFIG:Debug>:WINVER=0x0400>
    $<$<CONFIG:Debug>:_WIN32_WINNT=0x0400>
    $<$<NOT:$<CONFIG:Debug>>:NDEBUGT>
  LIBS
    winmm
    ws2_32
    LiveMedia555
    BaseClasses
    mp_dxsdk
)
