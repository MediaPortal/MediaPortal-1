# DVBSubtitle3: translated from DVBSubtitle3.vcxproj.

add_library(DVBSubtitle3 SHARED
  source/dllmain.cpp
  source/DVBSub.cpp
  source/dvbsubs/DVBSubDecoder.cpp
  source/dvbsubs/subtitle.cpp
  source/SubtitleInputPin.cpp
  ../DvbCoreUtils/PacketSync.cpp
  Source/PesDecoder/PesDecoder.cpp
  Source/PesDecoder/TsHeader.cpp
  source/DVBSub3.def
)

mp_target(DVBSubtitle3
  SUFFIX ".ax"
  OUTPUT_NAME "DVBSub3"
  FOLDER "Filters"
  DEFINITIONS
    _WIN32
    _WIN32_WINNT=0x0501
    WINVER=0x0501
    NTDDI_VERSION=NTDDI_WINXPSP2
    $<$<CONFIG:Debug>:INC_OLE2>
    $<$<CONFIG:Debug>:STRICT>
  LIBS
    winmm
    ws2_32
    gdiplus
    BaseClasses
)
