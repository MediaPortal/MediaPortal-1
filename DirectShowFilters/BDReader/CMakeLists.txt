# BDReader: translated from BDReader.vcxproj.

add_library(BDReader SHARED
  source/AudioPin.cpp
  source/BDReader.cpp
  source/Clip.cpp
  source/DeMultiplexer.cpp
  source/FrameHeaderParser.cpp
  source/GolombBuffer.cpp
  source/H264Nalu.cpp
  ../shared/HEVC/BitstreamReader.cpp
  ../shared/HEVC/Hevc.cpp
  ../shared/HEVC/HevcNalDecode.cpp
  source/LibBlurayWrapper.cpp
  source/OverlayRenderer.cpp
  source/Packet.cpp
  source/PcrDecoder.cpp
  source/Playlist.cpp
  source/PlaylistManager.cpp
  source/StreamParser.cpp
  source/utils.cpp
  source/VideoPin.cpp
  source/BDReader.def
)

mp_target(BDReader
  SUFFIX ".ax"
  OUTPUT_NAME "BDReader"
  FOLDER "Filters"
  INCLUDES
    ${MP_LIBBLURAY_INCLUDES}
    ../shared
    ./libbluray
  DEFINITIONS
    $<$<CONFIG:Debug>:INC_OLE2>
    $<$<CONFIG:Debug>:STRICT>
    $<$<NOT:$<CONFIG:Debug>>:_WIN32>
  LIBS
    winmm
    ws2_32
    mfuuid
    D3dx9
    quartz
    DvbCoreUtils
    BaseClasses
    mp_dxsdk
)
