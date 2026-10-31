# Common settings for the MediaPortal native filters.
#
# Mirrors DirectShowFilters/Directory.Build.props (MSBuild): same security
# hardening and CRT, plus the character set switch MP_CHARSET.

# ---------------------------------------------------------------------------
# Options
# ---------------------------------------------------------------------------
set(MP_CHARSET "UNICODE" CACHE STRING
  "Character set of the MediaPortal targets: UNICODE (UNICODE, _UNICODE) or MBCS (_MBCS)")
set_property(CACHE MP_CHARSET PROPERTY STRINGS UNICODE MBCS)
string(TOUPPER "${MP_CHARSET}" MP_CHARSET)
if(NOT MP_CHARSET MATCHES "^(UNICODE|MBCS)$")
  message(FATAL_ERROR "MP_CHARSET must be UNICODE or MBCS, got '${MP_CHARSET}'")
endif()

set(MP_DXSDK_DIR "" CACHE PATH
  "DirectX 9 SDK (June 2010) root or the 'build' folder of the NuGet package DirectX_9_SDK. Default: %DXSDK_DIR%, then <repo>/Packages/DirectX_9_SDK.*/build")

set(MP_BUILD_MPC_SUBS "AUTO" CACHE STRING
  "Build the MPC-HC subtitle engine (mpcSubs.dll) and MPAudioSwitcher: AUTO, ON or OFF. AUTO needs UNICODE")
set_property(CACHE MP_BUILD_MPC_SUBS PROPERTY STRINGS AUTO ON OFF)

set(MP_BUILD_BDREADER "AUTO" CACHE STRING
  "Build BDReader: AUTO, ON or OFF. AUTO needs the libbluray headers (submodule or NuGet package)")
set_property(CACHE MP_BUILD_BDREADER PROPERTY STRINGS AUTO ON OFF)

# ---------------------------------------------------------------------------
# Architecture
# ---------------------------------------------------------------------------
if(CMAKE_SIZEOF_VOID_P EQUAL 8)
  set(MP_ARCH "x64")
else()
  set(MP_ARCH "x86")
endif()
message(STATUS "MediaPortal native filters: ${MP_ARCH}, ${MP_CHARSET}")

# ---------------------------------------------------------------------------
# DirectX 9 SDK (d3dx9, dxguid, DxErr, dsound, dinput8)
# ---------------------------------------------------------------------------
if(NOT MP_DXSDK_DIR AND DEFINED ENV{DXSDK_DIR})
  set(MP_DXSDK_DIR "$ENV{DXSDK_DIR}")
endif()
if(NOT MP_DXSDK_DIR)
  file(GLOB _mp_nuget_dx "${CMAKE_CURRENT_LIST_DIR}/../../Packages/DirectX_9_SDK.*/build")
  list(SORT _mp_nuget_dx)
  list(REVERSE _mp_nuget_dx)
  if(_mp_nuget_dx)
    list(GET _mp_nuget_dx 0 MP_DXSDK_DIR)
  endif()
endif()
if(MP_DXSDK_DIR AND EXISTS "${MP_DXSDK_DIR}/Include/d3dx9.h")
  cmake_path(NORMAL_PATH MP_DXSDK_DIR)
  set(MP_DXSDK_INCLUDE "${MP_DXSDK_DIR}/Include")
  if(MP_ARCH STREQUAL "x64")
    set(MP_DXSDK_LIB "${MP_DXSDK_DIR}/Lib/x64")
  else()
    set(MP_DXSDK_LIB "${MP_DXSDK_DIR}/Lib/x86")
  endif()
  message(STATUS "DirectX 9 SDK: ${MP_DXSDK_DIR}")
else()
  message(FATAL_ERROR
    "DirectX 9 SDK not found. Restore the NuGet packages (nuget restore DirectShowFilters\\Filters.sln "
    "-PackagesDirectory Packages), install the June 2010 SDK, or pass -DMP_DXSDK_DIR=<path>")
endif()

# Interface target: the DirectX SDK headers and libraries.
add_library(mp_dxsdk INTERFACE)
target_include_directories(mp_dxsdk SYSTEM INTERFACE "${MP_DXSDK_INCLUDE}")
target_link_directories(mp_dxsdk INTERFACE "${MP_DXSDK_LIB}")

# ---------------------------------------------------------------------------
# Definitions of mpc-hc_subs/src/common.props, for the targets that build the MPC-HC code
set(MP_MPC_DEFINITIONS WINDOWS WINVER=0x0601 _WIN32_WINNT=0x0601 _WIN32_IE=0x0800 PSAPI_VERSION=1 _USE_MATH_DEFINES
    __STDC_CONSTANT_MACROS NOMINMAX)

# Neither MFC nor ATL: the MPC-HC code (DSUtil, SubPic, Subtitles, mpcSubs) and MPAudioSwitcher use std and the helpers of
# mpc-hc_subs/src/dsutil (StrUtils.h, StdHelpers.h, RectUtils.h, SharedInclude.h)
# ---------------------------------------------------------------------------

# ---------------------------------------------------------------------------
# Compiler and linker defaults (same as Directory.Build.props)
# ---------------------------------------------------------------------------
# /MD, /MDd: the vcpkg triplets (vcpkg-triplets/*-mp.cmake) use the dynamic CRT as well.
set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>DLL")
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Flat output tree: <build>/bin/<config>/ (DLLs, .ax, pdb) and <build>/lib/<config>/ (static and import libraries).
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/$<CONFIG>")
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/$<CONFIG>")
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib/$<CONFIG>")
set(CMAKE_PDB_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/$<CONFIG>")

# Everything except the character set.
add_library(mp_base INTERFACE)
target_compile_features(mp_base INTERFACE cxx_std_17)
target_compile_definitions(mp_base INTERFACE
  WIN32 _WINDOWS _CRT_SECURE_NO_WARNINGS _CRT_NONSTDC_NO_WARNINGS
  # C++17 adds std::byte, which clashes with the Windows 'byte' typedef under 'using namespace std'
  _HAS_STD_BYTE=0
  "$<$<CONFIG:Debug>:_DEBUG>" "$<$<NOT:$<CONFIG:Debug>>:NDEBUG>")
# Only for the C and C++ compiler: the NASM sources of the VirtualDub libraries get the options of mp_target_asm.
target_compile_options(mp_base INTERFACE
  "$<$<COMPILE_LANGUAGE:C,CXX>:/MP>" "$<$<COMPILE_LANGUAGE:C,CXX>:/GS>"
  "$<$<AND:$<COMPILE_LANGUAGE:C,CXX>,$<CONFIG:Release>>:/guard:cf>"
  "$<$<AND:$<COMPILE_LANGUAGE:C,CXX>,$<CONFIG:RelWithDebInfo>>:/guard:cf>")
# ASLR, DEP, CFG (checked by Build/Check-NativeHardening.ps1); do not weaken (ADR-0002).
target_link_options(mp_base INTERFACE
  /DYNAMICBASE /NXCOMPAT
  "$<$<CONFIG:Release>:/guard:cf>" "$<$<CONFIG:RelWithDebInfo>:/guard:cf>"
  "$<$<STREQUAL:${MP_ARCH},x64>:/HIGHENTROPYVA>"
  "$<$<STREQUAL:${MP_ARCH},x86>:/SAFESEH>")

add_library(mp_charset_unicode INTERFACE)
target_compile_definitions(mp_charset_unicode INTERFACE UNICODE _UNICODE)
add_library(mp_charset_mbcs INTERFACE)
target_compile_definitions(mp_charset_mbcs INTERFACE _MBCS)

# What a MediaPortal target gets by default: mp_base plus the character set of MP_CHARSET.
add_library(mp_common INTERFACE)
target_link_libraries(mp_common INTERFACE mp_base)
if(MP_CHARSET STREQUAL "UNICODE")
  target_link_libraries(mp_common INTERFACE mp_charset_unicode)
else()
  target_link_libraries(mp_common INTERFACE mp_charset_mbcs)
endif()

# ---------------------------------------------------------------------------
# mp_target: the one place that applies the settings every MediaPortal target needs.
#
#   mp_target(<name>
#     [SUFFIX <.ax|.dll>] [OUTPUT_NAME <name>] [FOLDER <ide folder>]
#     [CHARSET UNICODE|MBCS|NONE]         # pin the character set (default: MP_CHARSET)
#     [EH ASYNC] [CXX_STANDARD <n>]
#     [INCLUDES <dirs>] [PUBLIC_INCLUDES <dirs>] [DEFINITIONS <defs>] [LIBS <libs>] [PUBLIC_LIBS <libs>])
#
# The target must already exist (add_library). CHARSET UNICODE is for the MPC-HC code and
# dshowhelper (Unicode only), CHARSET MBCS for dvblib (ANSI only), CHARSET NONE for the
# VirtualDub libraries (neither UNICODE nor _MBCS).
# ---------------------------------------------------------------------------
function(mp_target name)
  cmake_parse_arguments(T "" "SUFFIX;OUTPUT_NAME;FOLDER;CHARSET;EH;CXX_STANDARD"
    "INCLUDES;PUBLIC_INCLUDES;DEFINITIONS;LIBS;PUBLIC_LIBS" ${ARGN})

  if(T_CHARSET STREQUAL "UNICODE")
    target_link_libraries(${name} PRIVATE mp_base mp_charset_unicode)
  elseif(T_CHARSET STREQUAL "MBCS")
    target_link_libraries(${name} PRIVATE mp_base mp_charset_mbcs)
  elseif(T_CHARSET STREQUAL "NONE")
    target_link_libraries(${name} PRIVATE mp_base)
  elseif(T_CHARSET)
    message(FATAL_ERROR "mp_target(${name}): CHARSET must be UNICODE, MBCS or NONE")
  else()
    target_link_libraries(${name} PRIVATE mp_common)
  endif()

  if(T_SUFFIX)
    set_target_properties(${name} PROPERTIES SUFFIX "${T_SUFFIX}")
  endif()
  if(T_OUTPUT_NAME)
    set_target_properties(${name} PROPERTIES OUTPUT_NAME "${T_OUTPUT_NAME}")
  endif()
  if(T_FOLDER)
    set_target_properties(${name} PROPERTIES FOLDER "${T_FOLDER}")
  endif()
  if(T_CXX_STANDARD)
    set_target_properties(${name} PROPERTIES CXX_STANDARD ${T_CXX_STANDARD})
  endif()
  if(T_INCLUDES)
    target_include_directories(${name} PRIVATE ${T_INCLUDES})
  endif()
  if(T_PUBLIC_INCLUDES)
    target_include_directories(${name} PUBLIC ${T_PUBLIC_INCLUDES})
  endif()
  if(T_DEFINITIONS)
    target_compile_definitions(${name} PRIVATE ${T_DEFINITIONS})
  endif()
  if(T_LIBS)
    target_link_libraries(${name} PRIVATE ${T_LIBS})
  endif()
  if(T_PUBLIC_LIBS)
    target_link_libraries(${name} PUBLIC ${T_PUBLIC_LIBS})
  endif()
  if(T_EH STREQUAL "ASYNC")
    # /EHa: catch(...) also catches structured exceptions (access violations). See audit SEC-20.
    target_compile_options(${name} PRIVATE "$<$<COMPILE_LANGUAGE:C,CXX>:/EHa>")
  else()
    target_compile_options(${name} PRIVATE "$<$<COMPILE_LANGUAGE:C,CXX>:/EHsc>")
  endif()
endfunction()

# ---------------------------------------------------------------------------
# Helper: YASM sources of the VirtualDub libraries, assembled with NASM.
#
# The MSBuild projects use vsyasm (-X vc -f win32|win64). YASM is no longer maintained and
# the sources only use the syntax common to YASM and NASM. The .asm64 extension is not known
# to CMake, so the language is set per source.
#
#   mp_target_asm(<target> <sources...>)
# ---------------------------------------------------------------------------
function(mp_target_asm target)
  set(_sources "")
  foreach(src IN LISTS ARGN)
    get_filename_component(_dir "${src}" DIRECTORY)
    get_filename_component(_abs "${src}" ABSOLUTE)
    file(READ "${_abs}" _text)
    # The x64 sources describe the stack frame with the YASM directives proc_frame, [savereg ...] and so on. NASM has no
    # such directives: the functions are assembled without unwind information (they call nothing that throws).
    if(_text MATCHES "proc_frame")
      get_filename_component(_name "${src}" NAME_WE)
      set(_gen "${CMAKE_CURRENT_BINARY_DIR}/nasm/${_name}_nasm.asm")
      set(NL "\n")
      string(REPLACE "\r\n" "${NL}" _text "${_text}")
      string(REGEX REPLACE "(${NL})proc_frame[ \t]+([A-Za-z0-9_]+)" "\\1global \\2${NL}\\2:" _text "${_text}")
      string(REGEX REPLACE "(${NL})(endproc_frame|end_prolog)" "\\1" _text "${_text}")
      string(REGEX REPLACE "\\[(savereg|savexmm128|pushreg|allocstack|setframe|pushframe|endprolog)[^]]*\\]" "" _text "${_text}")
      # a local label cannot be an expression with a register in NASM: a macro does it
      string(REGEX REPLACE "\\.parms[ \t]+equ[ \t]+([^\n]*)" "%define VDPARMS \\1" _text "${_text}")
      string(REPLACE ".parms" "VDPARMS" _text "${_text}")
      # [esp+8] is a 32 bit address in 64 bit mode (the stack is above 4 GB): it means the stack pointer
      string(REPLACE "[esp+" "[rsp+" _text "${_text}")
      file(WRITE "${_gen}" "${_text}")
      set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS "${_abs}")
      set(_use "${_gen}")
    else()
      set(_use "${src}")
    endif()
    set_source_files_properties("${_use}" PROPERTIES
      LANGUAGE ASM_NASM
      COMPILE_OPTIONS "-Xvc;-I${CMAKE_CURRENT_SOURCE_DIR}/${_dir}/")
    list(APPEND _sources "${_use}")
  endforeach()
  target_sources(${target} PRIVATE ${_sources})
endfunction()

# ---------------------------------------------------------------------------
# libbluray: BDReader needs only the headers (libbluray.dll is loaded at run time).
# Either the git submodule <repo>/libbluray or the NuGet package MediaPortal.libbluray.
# ---------------------------------------------------------------------------
set(MP_LIBBLURAY_DIR "" CACHE PATH
  "libbluray sources: the git submodule <repo>/libbluray, or <repo>/Packages/MediaPortal.libbluray.*/build/lib")
set(MP_LIBBLURAY_INCLUDES "")
if(NOT MP_LIBBLURAY_DIR)
  if(EXISTS "${CMAKE_CURRENT_LIST_DIR}/../../libbluray/src/libbluray/bluray.h")
    set(MP_LIBBLURAY_DIR "${CMAKE_CURRENT_LIST_DIR}/../../libbluray")
  else()
    file(GLOB _mp_nuget_bd "${CMAKE_CURRENT_LIST_DIR}/../../Packages/MediaPortal.libbluray.*/build/lib")
    list(SORT _mp_nuget_bd)
    list(REVERSE _mp_nuget_bd)
    if(_mp_nuget_bd)
      list(GET _mp_nuget_bd 0 MP_LIBBLURAY_DIR)
    endif()
  endif()
endif()
if(MP_LIBBLURAY_DIR)
  cmake_path(NORMAL_PATH MP_LIBBLURAY_DIR)
  if(EXISTS "${MP_LIBBLURAY_DIR}/src/libbluray/bluray.h")        # git submodule layout
    set(MP_LIBBLURAY_INCLUDES
      "${MP_LIBBLURAY_DIR}/includes" "${MP_LIBBLURAY_DIR}/src/libbluray"
      "${MP_LIBBLURAY_DIR}/src/libbluray/decoders")
  elseif(EXISTS "${MP_LIBBLURAY_DIR}/libbluray/bluray.h")        # NuGet package layout
    set(MP_LIBBLURAY_INCLUDES
      "${MP_LIBBLURAY_DIR}/includes" "${MP_LIBBLURAY_DIR}/libbluray"
      "${MP_LIBBLURAY_DIR}/libbluray/decoders")
  endif()
endif()
