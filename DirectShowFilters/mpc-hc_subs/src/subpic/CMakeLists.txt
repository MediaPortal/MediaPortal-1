# SubPic: translated from subpic.vcxproj.

add_library(SubPic STATIC
  CoordGeom.cpp
  DX11SubPic.cpp
  DX9SubPic.cpp
  MemSubPic.cpp
  ScreenUtil.cpp
  stdafx.cpp
  SubPicAllocatorPresenterImpl.cpp
  SubPicImpl.cpp
  SubPicProviderImpl.cpp
  SubPicQueueImpl.cpp
  XySubPicProvider.cpp
  XySubPicQueueImpl.cpp
)

mp_target(SubPic
  OUTPUT_NAME "SubPic"
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
)
