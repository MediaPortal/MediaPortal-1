# TsWriter: translated from MPTSWriter.vcxproj.

add_library(TsWriter SHARED
  source/AtscNitParser.cpp
  source/BaseScteParser.cpp
  source/CaGrabber.cpp
  source/CDiskBuff.cpp
  source/ChannelLinkageScanner.cpp
  source/ChannelScan.cpp
  source/DiskRecorder.cpp
  source/EpgScanner.cpp
  source/FileWriterThreaded.cpp
  source/LvctParser.cpp
  source/NttParser.cpp
  source/PmtGrabber.cpp
  source/RegistryUtil.cpp
  source/SvctParser.cpp
  source/TeletextGrabber.cpp
  source/TextUtil.cpp
  source/TsChannel.cpp
  source/TsWriter.cpp
  source/VideoAnalyzer.cpp
  source/ChannelLinkageParser.cpp
  source/epgDecoder.cpp
  source/EpgParser.cpp
  source/MHWDecoder.cpp
  source/MhwParser.cpp
  source/NITDecoder.cpp
  source/PatParser.cpp
  source/PmtParser.cpp
  source/SdtParser.cpp
  source/PcrDecoder.cpp
  source/VideoAudioScrambledAnalyzer.cpp
  source/FileWriter.cpp
  source/MultiFileWriter.cpp
  source/autoString.cpp
  source/CriticalSection.cpp
  source/EnterCriticalSection.cpp
  source/TsWriter.rc
  source/TsWriter.def
)

mp_target(TsWriter
  SUFFIX ".ax"
  OUTPUT_NAME "TsWriter"
  FOLDER "Filters"
  DEFINITIONS
    _WIN32
    $<$<CONFIG:Debug>:INC_OLE2>
    $<$<CONFIG:Debug>:STRICT>
    $<$<CONFIG:Debug>:_WIN32_WINNT=0x0400>
    $<$<NOT:$<CONFIG:Debug>>:WINVER=0x0600>
  LIBS
    winmm
    DvbCoreUtils
    BaseClasses
    mp_dxsdk
)
