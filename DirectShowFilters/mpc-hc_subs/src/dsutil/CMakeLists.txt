# DSUtil: translated from DSUtil.vcxproj.

add_library(DSUtil STATIC
  AudioParser.cpp
  AudioTools.cpp
  deinterlace.cpp
  DSMPropertyBag.cpp
  DSUtil.cpp
  FileHandle.cpp
  FileVersionInfo.cpp
  FontInstaller.cpp
  GolombBuffer.cpp
  H264Nalu.cpp
  HdmvClipInfo.cpp
  ISOLang.cpp
  MediaTypeEx.cpp
  MediaTypes.cpp
  MP4AudioDecoderConfig.cpp
  NullRenderers.cpp
  Packet.cpp
  PathUtils.cpp
  stdafx.cpp
  text.cpp
  Utils.cpp
  vd.cpp
  WinAPIUtils.cpp
)
if(MP_ARCH STREQUAL "x86")
  target_sources(DSUtil PRIVATE
    vd_asm.cpp
  )
endif()

mp_target(DSUtil
  OUTPUT_NAME "DSUtil"
  FOLDER "MPC-HC"
  CXX_STANDARD 17
  CHARSET UNICODE
  INCLUDES
    ../../include
    ../DSUtil
    ../filters/renderer/VideoRenderers
    ../thirdparty
    ../thirdparty/VirtualDub/h
  DEFINITIONS
    ${MP_MPC_DEFINITIONS}
  LIBS
    Kasumi
    BaseClasses
)
