# mpcSubs: translated from mpc-hc_subs.vcxproj.

add_library(mpcSubs SHARED
  IPinHook.cpp
  mpcSubs.cpp
  stdafx.cpp
  SubManager.cpp
  Subresync.cpp
  TextPassThruFilter.cpp
  mpcSubs.rc
)

mp_target(mpcSubs
  SUFFIX ".dll"
  OUTPUT_NAME "mpcSubs"
  FOLDER "MPC-HC"
  CXX_STANDARD 17
  CHARSET UNICODE
  INCLUDES
    ../../../BaseClasses
    ../../include
    ../dsutil
    ../thirdparty
  DEFINITIONS
    ${MP_MPC_DEFINITIONS}
    MPCSUBS_EXPORTS
    _USRDLL
  LIBS
    Winmm
    vfw32
    Version
    DSUtil
    SubPic
    Subtitles
    Kasumi
    VDsystem
    BaseClasses
    Freetype::Freetype
    ZLIB::ZLIB
    unofficial::unrar::unrar
)
