# Kasumi: translated from Kasumi.vcxproj.

add_library(Kasumi STATIC
  source/alphablt.cpp
  source/blitter.cpp
  source/blt.cpp
  source/blt_reference.cpp
  source/blt_reference_pal.cpp
  source/blt_reference_rgb.cpp
  source/blt_reference_yuv.cpp
  source/blt_reference_yuv2yuv.cpp
  source/blt_reference_yuvrev.cpp
  source/blt_setup.cpp
  source/blt_spanutils.cpp
  source/blt_uberblit.cpp
  source/pixel.cpp
  source/pixmaputils.cpp
  source/region.cpp
  source/resample.cpp
  source/resample_kernels.cpp
  source/resample_stages.cpp
  source/resample_stages_reference.cpp
  source/stdafx.cpp
  source/stretchblt_reference.cpp
  source/tables.cpp
  source/text.cpp
  source/triblt.cpp
  source/uberblit.cpp
  source/uberblit_16f.cpp
  source/uberblit_gen.cpp
  source/uberblit_resample.cpp
  source/uberblit_resample_special.cpp
  source/uberblit_swizzle.cpp
  source/uberblit_v210.cpp
  source/uberblit_ycbcr_generic.cpp
)
if(MP_ARCH STREQUAL "x86")
  target_sources(Kasumi PRIVATE
    source/uberblit_swizzle_x86.cpp
    source/uberblit_ycbcr_x86.cpp
    source/blt_spanutils_x86.cpp
    source/blt_x86.cpp
    source/resample_stages_x86.cpp
    source/uberblit_resample_special_x86.cpp
  )
endif()
if(MP_ARCH STREQUAL "x64")
  target_sources(Kasumi PRIVATE
    source/resample_stages_x64.cpp
  )
endif()

mp_target(Kasumi
  OUTPUT_NAME "Kasumi"
  FOLDER "MPC-HC"
  CHARSET NONE
  INCLUDES
    ../h
    h
  DEFINITIONS
    NOMINMAX
    WIN32_LEAN_AND_MEAN
  LIBS
    VDsystem
)

# YASM sources: assembled with NASM (see MP_ASM_FLAGS in MediaPortalCommon.cmake)
if(MP_ARCH STREQUAL "x86")
  mp_target_asm(Kasumi
    source/a_bltrgb.asm
    source/a_bltrgb2yuv_mmx.asm
    source/a_bltrgb_mmx.asm
    source/a_bltyuv2rgb_sse2.asm
    source/a_resample_mmx.asm
    source/a_resample_sse41.asm
    source/a_spanutils_isse.asm
    source/a_stretchrgb_mmx.asm
    source/a_stretchrgb_point.asm
    source/a_triblt_mmx.asm
    source/a_triblt_scalar.asm
    source/a_triblt_sse2.asm
  )
endif()
if(MP_ARCH STREQUAL "x64")
  mp_target_asm(Kasumi
    source/a64_resample.asm64
  )
endif()
