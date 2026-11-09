# TsReader: translated from TsReader.vcxproj.

add_library(TsReader SHARED
  source/AudioPin.cpp
  source/Buffer.cpp
  source/ChannelInfo.cpp
  source/DeMultiplexer.cpp
  source/FileReader.cpp
  source/FrameHeaderParser.cpp
  source/GolombBuffer.cpp
  ../shared/HEVC/BitstreamReader.cpp
  ../shared/HEVC/Hevc.cpp
  ../shared/HEVC/HevcNalDecode.cpp
  source/MediaSeeking.cpp
  source/MemoryBuffer.cpp
  source/MemoryReader.cpp
  source/MemorySink.cpp
  source/MpegPesParser.cpp
  source/MultiFileReader.cpp
  source/PatParser.cpp
  source/PcrDecoder.cpp
  source/PidTable.cpp
  source/PmtParser.cpp
  source/RTSPClient.cpp
  source/SubtitlePin.cpp
  source/TsAVRT.cpp
  source/TsDuration.cpp
  source/TsFileSeek.cpp
  source/TsReader.cpp
  source/TSThread.cpp
  source/VideoPin.cpp
  source/WaitEvent.cpp
  source/TsReader.rc
  source/TsReader.def
)

mp_target(TsReader
  SUFFIX ".ax"
  OUTPUT_NAME "TsReader"
  FOLDER "Filters"
  INCLUDES
    ../LiveMedia555/MediaPortal
    ../shared
    ../dxerr
  DEFINITIONS
    $<$<CONFIG:Debug>:INC_OLE2>
    $<$<CONFIG:Debug>:STRICT>
    $<$<NOT:$<CONFIG:Debug>>:_WIN32>
  LIBS
    winmm
    ws2_32
    quartz
    DvbCoreUtils
    LiveMedia555
    BaseClasses
)
