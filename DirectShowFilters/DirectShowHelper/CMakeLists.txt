# DirectShowHelper: translated from dshowhelper.vcxproj.
add_library(DirectShowHelper SHARED
  source/DeviceState.cpp
  source/dllmodul.cpp
  source/dshowhelper.cpp
  source/DX9AllocatorPresenter.cpp
  source/EVRCustomPresenter.cpp
  source/GrabBitmaps.cpp
  source/MadPresenter.cpp
  source/OuterEVR.cpp
  source/Scheduler.cpp
  source/StatsRenderer.cpp
  source/threads/SystemClock.cpp
  source/TimeSource.cpp
  source/dshowhelper.rc
  source/dshowhelper.def
)

mp_target(DirectShowHelper
  SUFFIX ".dll"
  OUTPUT_NAME "dshowhelper"
  FOLDER "Helpers"
  EH ASYNC
  CXX_STANDARD 14
  DEFINITIONS
    _HAS_AUTO_PTR_ETC=1  # StdString.h uses std::binary_function; mp_base asks for C++17, which removes it
  LIBS
    gdiplus
    winmm
    comsupp
    dxguid
    d3d9
    d3dx9
    mfuuid
    dxva2
    evr
    mf
    strmiids
    mp_dxsdk
    BaseClasses
)
