# VDsystem: translated from system.vcxproj.

add_library(VDsystem STATIC
  ../h/vd2/system/source/registrymemory.cpp
  source/bitmath.cpp
  source/cache.cpp
  source/cmdline.cpp
  source/cpuaccel.cpp
  source/debug.cpp
  source/debugx86.cpp
  source/Error.cpp
  source/error_win32.cpp
  source/event.cpp
  source/fileasync.cpp
  source/Fraction.cpp
  source/halffloat.cpp
  source/hash.cpp
  source/int128.cpp
  source/linearalloc.cpp
  source/list.cpp
  source/math.cpp
  source/memory.cpp
  source/progress.cpp
  source/protscope.cpp
  source/refcount.cpp
  source/registry.cpp
  source/strutil.cpp
  source/text.cpp
  source/thread.cpp
  source/thunk.cpp
  source/tls.cpp
  source/vdalloc.cpp
  source/VDNamespace.cpp
  source/VDScheduler.cpp
  source/vdstl.cpp
  source/vdstl_hash.cpp
  source/vdstl_hashtable.cpp
  source/VDString.cpp
  source/vectors.cpp
  source/w32assist.cpp
  source/stdaccel.cpp
  source/stdafx.cpp
)

mp_target(VDsystem
  OUTPUT_NAME "system"
  FOLDER "MPC-HC"
  CHARSET NONE
  INCLUDES
    ../h
    h
  DEFINITIONS
    NOMINMAX
    WIN32_LEAN_AND_MEAN
    _SCL_SECURE_NO_WARNINGS
  LIBS
    Winmm
)

# YASM sources: assembled with NASM (see MP_ASM_FLAGS in MediaPortalCommon.cmake)
if(MP_ARCH STREQUAL "x86")
  mp_target_asm(VDsystem
    source/a_memory.asm
    source/a_thunk.asm
  )
endif()
if(MP_ARCH STREQUAL "x64")
  mp_target_asm(VDsystem
    source/a64_cpuaccel.asm
    source/a64_fraction.asm
    source/a64_int128.asm
    source/a64_thunk.asm
  )
endif()
