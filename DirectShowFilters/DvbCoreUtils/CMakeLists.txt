# DvbCoreUtils: translated from DvbCoreUtils.vcxproj.

add_library(DvbCoreUtils STATIC
  AdaptionField.cpp
  ../shared/BasePmtParser.cpp
  ../shared/ChannelInfo.cpp
  DvbUtil.cpp
  PacketSync.cpp
  Pcr.cpp
  ../shared/PidTable.cpp
  Section.cpp
  SectionDecoder.cpp
  TsHeader.cpp
)

mp_target(DvbCoreUtils
  OUTPUT_NAME "DvbCoreUtils"
  FOLDER "Libraries"
)
