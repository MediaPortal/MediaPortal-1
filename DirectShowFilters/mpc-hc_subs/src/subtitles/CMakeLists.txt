# Subtitles: translated from subtitles.vcxproj.

add_library(Subtitles STATIC
  Ellipse.cpp
  OpenTypeLangTags.cpp
  RenderingCache.cpp
  CCDecoder.cpp
  CompositionObject.cpp
  DVBSub.cpp
  ColorConvTable.cpp
  SSASub.cpp
  SubtitleHelpers.cpp
  PGSSub.cpp
  Rasterizer.cpp
  RealTextParser.cpp
  RLECodedSubtitle.cpp
  RTS.cpp
  stdafx.cpp
  STS.cpp
  SubtitleInputPin.cpp
  TextFile.cpp
  USFSubtitles.cpp
  VobSubFile.cpp
  VobSubFileRipper.cpp
  VobSubImage.cpp
)

mp_target(Subtitles
  OUTPUT_NAME "Subtitles"
  FOLDER "MPC-HC"
  CXX_STANDARD 17
  CHARSET UNICODE
  INCLUDES
    ../../include
    ../thirdparty
  DEFINITIONS
    ${MP_MPC_DEFINITIONS}
  LIBS
    BaseClasses
    Freetype::Freetype
    ZLIB::ZLIB
    unofficial::unrar::unrar
)
