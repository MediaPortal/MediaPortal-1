# MPAudioSwitcher: translated from MPAudioSwitcher.vcxproj.

add_library(MPAudioSwitcher SHARED
  source/Audio.cpp
  source/AudioSwitcher.cpp
  source/FilterApp.cpp
  source/stdafx.cpp
  source/StreamSwitcher.cpp
  source/MPAudioSwitcher.rc
  source/AudioSwitcher.def
)

mp_target(MPAudioSwitcher
  SUFFIX ".ax"
  OUTPUT_NAME "MPAudioSwitcher"
  FOLDER "Filters"
  INCLUDES
    ../mpc-hc_subs/src/dsutil
  DEFINITIONS
    REGISTER_FILTER
    _CRT_SECURE_NO_DEPRECATE
    $<$<NOT:$<CONFIG:Debug>>:_USRDLL>
  LIBS
    Vfw32
    DSUtil
    VDsystem
    BaseClasses
)
