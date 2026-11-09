# AudioRenderer: translated from AudioRenderer.vcxproj.

add_library(AudioRenderer SHARED
  AE_mixer/AEChannelInfo.cpp
  AE_mixer/AERemap.cpp
  source/AC3EncoderFilter.cpp
  source/BaseAudioSink.cpp
  source/BitDepthAdapter.cpp
  source/ChannelMixer.cpp
  source/dllmain.cpp
  source/dllsetup.cpp
  source/FilterApp.cpp
  source/Globals.cpp
  source/Logger.cpp
  source/MediaSeeking.cpp
  source/MpAudioRenderer.cpp
  source/QueuedAudioSink.cpp
  source/SampleCopier.cpp
  source/SampleRateConverterFilter.cpp
  source/Settings.cpp
  source/SettingsProp.cpp
  source/SoundTouchEx.cpp
  source/SyncClock.cpp
  source/TimeSource.cpp
  source/TimeStretchFilter.cpp
  source/VolumeHandler.cpp
  AC3_encoder/ac3enc.cpp
  AC3_encoder/bits.cpp
  source/SynchCorrection.cpp
  source/WASAPIRenderFilter.cpp
  source/MpAudioRenderer.rc
  source/MpAudioRenderer.def
)

mp_target(AudioRenderer
  SUFFIX ".ax"
  OUTPUT_NAME "mpaudiorenderer"
  FOLDER "Filters"
  DEFINITIONS
    FLOAT_SAMPLES
    _USING_V110_SDK71_
    _USRDLL
    $<$<NOT:$<CONFIG:Debug>>:MULTICHANNEL_SUPPORT>
  LIBS
    Winmm
    dsound
    BaseClasses
    SampleRate::samplerate
    SoundTouch::SoundTouch
    mp_dxsdk
)
