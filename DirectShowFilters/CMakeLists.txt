# MediaPortal native DirectShow filters and helper libraries.
#
# CMake counterpart of Filters.sln (MSBuild stays the reference build, see CLAUDE.md).
# See cmake/README.md for the presets and options (x86/x64, Unicode/MBCS).
cmake_minimum_required(VERSION 3.25)

project(MediaPortalNativeFilters VERSION 1.39 LANGUAGES C CXX)

if(NOT MSVC)
  message(FATAL_ERROR
    "The filters need MSVC: DirectShow, Direct3D 9 and the exploit mitigations of ADR-0002. "
    "Use a Visual Studio generator or run Ninja from a Developer Command Prompt.")
endif()

list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake")
include(MediaPortalCommon)

# The Visual Studio generators write the .vcxproj files below DirectShowFilters/build, where MSBuild would
# pick up Directory.Build.props/.targets of the MSBuild build (vcpkg auto-linking, crypt32.lib, ...).
# Keep the two builds independent: this build sets the same options itself and takes vcpkg from the toolchain.
set(CMAKE_VS_GLOBALS "ImportDirectoryBuildProps=false" "ImportDirectoryBuildTargets=false")

# Whole program optimization (/GL + /LTCG) as in the MSBuild Release configurations.
# MSVC 14.51 (Visual Studio 2026) crashes with C1001/LNK1000 in the x86 link step of MPAudioRenderer
# and MPIPTVSource when it is on, so it defaults to OFF for x86 with that compiler.
set(_mp_ltcg_default ON)
if(MP_ARCH STREQUAL "x86" AND MSVC_VERSION GREATER_EQUAL 1951)
  set(_mp_ltcg_default OFF)
endif()
option(MP_LTCG "Whole program optimization (/GL, /LTCG) for Release builds" ${_mp_ltcg_default})
if(MP_LTCG)
  set(CMAKE_INTERPROCEDURAL_OPTIMIZATION_RELEASE ON)
endif()
message(STATUS "Whole program optimization (MP_LTCG): ${MP_LTCG}")

# ---------------------------------------------------------------------------
# Third-party libraries from vcpkg (manifest mode, vcpkg.json, triplets *-windows-mp)
# ---------------------------------------------------------------------------
find_package(ZLIB REQUIRED)
find_package(Freetype REQUIRED)
find_package(SampleRate CONFIG REQUIRED)
find_package(SoundTouch CONFIG REQUIRED)
find_package(unofficial-live555 CONFIG REQUIRED)
find_package(unofficial-unrar CONFIG REQUIRED)

# ---------------------------------------------------------------------------
# Optional parts
# ---------------------------------------------------------------------------
# MPC-HC subtitle engine (mpcSubs.dll) and what depends on it. Upstream code: Unicode only; no MFC and no ATL.
set(MP_MPC_SUBS_ENABLED OFF)
if(MP_BUILD_MPC_SUBS STREQUAL "ON")
  set(MP_MPC_SUBS_ENABLED ON)
elseif(MP_BUILD_MPC_SUBS STREQUAL "AUTO")
  if(MP_CHARSET STREQUAL "UNICODE")
    set(MP_MPC_SUBS_ENABLED ON)
  endif()
endif()
if(MP_MPC_SUBS_ENABLED)
  enable_language(ASM_NASM)   # Kasumi and system have x86/x64 assembler sources
  message(STATUS "MPC-HC subtitle engine: enabled")
else()
  message(STATUS "MPC-HC subtitle engine: skipped (needs MP_CHARSET=UNICODE, or -DMP_BUILD_MPC_SUBS=ON)")
endif()

# BDReader needs the libbluray headers only.
set(MP_BDREADER_ENABLED OFF)
if(MP_BUILD_BDREADER STREQUAL "ON" OR (MP_BUILD_BDREADER STREQUAL "AUTO" AND MP_LIBBLURAY_INCLUDES))
  set(MP_BDREADER_ENABLED ON)
endif()
if(NOT MP_BDREADER_ENABLED)
  message(STATUS "BDReader: skipped (libbluray sources not found: init the submodule or restore the NuGet packages)")
endif()

# ---------------------------------------------------------------------------
# Libraries
# ---------------------------------------------------------------------------
add_subdirectory(baseclasses)
add_subdirectory(DvbCoreUtils)
add_subdirectory(LiveMedia555)

# ---------------------------------------------------------------------------
# DirectShow filters
# ---------------------------------------------------------------------------
add_subdirectory(TsWriter)
add_subdirectory(TsReader)
add_subdirectory(TsMuxer)
add_subdirectory(MPWriter)
add_subdirectory(StreamingServer)
add_subdirectory(DVBSubtitle3)
add_subdirectory(Core-CC-Parser/ax)
add_subdirectory(MPAudioRenderer)
if(MP_BDREADER_ENABLED)
  add_subdirectory(BDReader)
endif()

# ---------------------------------------------------------------------------
# IPTV source and its protocol plugins
# ---------------------------------------------------------------------------
add_subdirectory(MPIPTVSource/MPIPTVSource)
add_subdirectory(MPIPTVSource/MPIPTV_UDP)
add_subdirectory(MPIPTVSource/MPIPTV_FILE)
add_subdirectory(MPIPTVSource/MPIPTV_HTTP)
add_subdirectory(MPIPTVSource/MPIPTV_RTP)
add_subdirectory(MPIPTVSource/MPIPTV_RTSP)

# ---------------------------------------------------------------------------
# Helper DLLs called from the client (P/Invoke)
# ---------------------------------------------------------------------------
add_subdirectory(DXUtil)
add_subdirectory(DXErr9)
add_subdirectory(fontEngine)
add_subdirectory(Win7RefreshRateHelper)
add_subdirectory(DirectShowHelper)
add_subdirectory(dvblib)

# ---------------------------------------------------------------------------
# MPC-HC subtitle engine and the filter that needs its DSUtil
# ---------------------------------------------------------------------------
if(MP_MPC_SUBS_ENABLED)
  add_subdirectory(mpc-hc_subs/src/thirdparty/VirtualDub/system)
  add_subdirectory(mpc-hc_subs/src/thirdparty/VirtualDub/Kasumi)
  add_subdirectory(mpc-hc_subs/src/dsutil)
  add_subdirectory(mpc-hc_subs/src/SubPic)
  add_subdirectory(mpc-hc_subs/src/subtitles)
  add_subdirectory(mpc-hc_subs/src/mpc-hc_subs)
  add_subdirectory(MPAudioswitcher)
endif()

# ---------------------------------------------------------------------------
# Tests (GoogleTest from vcpkg): cmake ... -DMP_BUILD_TESTS=ON -DVCPKG_MANIFEST_FEATURES=tests
# ---------------------------------------------------------------------------
option(MP_BUILD_TESTS "Build the unit and integration tests (needs the vcpkg feature 'tests')" OFF)
if(MP_BUILD_TESTS)
  enable_testing()
  add_subdirectory(tests)
endif()
