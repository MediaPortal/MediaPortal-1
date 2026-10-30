# Tests of the native filters (GoogleTest from vcpkg, feature "tests").
#
#   cmake --preset vs2026-x64-unicode -DMP_BUILD_TESTS=ON -DVCPKG_MANIFEST_FEATURES=tests
#   cmake --build --preset vs2026-x64-unicode-debug --target mp_unit_tests mp_integration_tests
#   ctest --preset ...   (or run bin\<Config>\mp_unit_tests.exe directly)
#
# mp_unit_tests        small pure code: TS header/PCR/adaptation field/section parsing, packet sync, string helpers
# mp_integration_tests the real .ax/.dll files: load them, create the filters, put them in a DirectShow graph
# mp_tsmuxer_tests, mp_tsreader_tests, mp_tswriter_tests
#                      the code of these filters, built from their sources (no DLL, no graph)
# mp_local_tests       samples, a graphics or audio device of the developer machine (MP_BUILD_LOCAL_TESTS)
#
# All are built in the character set of MP_CHARSET, like the filters.

find_package(GTest CONFIG REQUIRED)
include(GoogleTest)

option(MP_TEST_ASAN "Build the tests with AddressSanitizer (MSVC /fsanitize=address)" OFF)

# ---------------------------------------------------------------------------
# Unit tests
# ---------------------------------------------------------------------------
add_executable(mp_unit_tests
  unit/TestSupport.cpp
  unit/TcharConvTests.cpp
  unit/WStringUtilTests.cpp
  unit/TsHeaderTests.cpp
  unit/PcrTests.cpp
  unit/AdaptionFieldTests.cpp
  unit/PacketSyncTests.cpp
  unit/PacketSyncAlgorithmTests.cpp
  unit/SectionTests.cpp
  unit/SectionDecoderTests.cpp
  unit/PmtParserTests.cpp
  unit/DirectShowHelperTests.cpp
  unit/TransformMatrixTests.cpp
  ../DirectShowHelper/source/TimeSource.cpp
  ../DirectShowHelper/source/threads/SystemClock.cpp
  unit/Crc32Tests.cpp
  unit/BitstreamReaderTests.cpp
  unit/BdPacketTests.cpp
  unit/StrUtilsTests.cpp
  unit/RectUtilsTests.cpp
  unit/ResamplerTests.cpp
  ../shared/HEVC/BitstreamReader.cpp
  ../BDReader/source/Packet.cpp
  ../MPAudioswitcher/source/Audio.cpp
)
# The helpers of the MPC-HC code (RectUtils.h, StrUtils.h) and the files of MPAudioSwitcher that include SharedInclude.h
set_source_files_properties(unit/RectUtilsTests.cpp unit/StrUtilsTests.cpp ../MPAudioswitcher/source/Audio.cpp PROPERTIES
  INCLUDE_DIRECTORIES "${CMAKE_CURRENT_LIST_DIR}/../mpc-hc_subs/src/dsutil")
# DirectShowHelper: TimeSource.cpp (and the tests of its headers) include StdAfx.h and the helpers by their own relative names
set_source_files_properties(unit/DirectShowHelperTests.cpp ../DirectShowHelper/source/TimeSource.cpp
  ../DirectShowHelper/source/threads/SystemClock.cpp PROPERTIES
  INCLUDE_DIRECTORIES "${CMAKE_CURRENT_LIST_DIR}/../DirectShowHelper/source")
target_include_directories(mp_unit_tests PRIVATE ../shared ../BDReader/source)
target_link_libraries(mp_unit_tests PRIVATE mp_common DvbCoreUtils BaseClasses winmm strmiids GTest::gtest GTest::gtest_main)

# ---------------------------------------------------------------------------
# Integration tests
# ---------------------------------------------------------------------------
add_executable(mp_integration_tests
  unit/TestSupport.cpp
  integration/FilterHost.cpp
  integration/FilterLoadTests.cpp
  integration/TsReaderGraphTests.cpp
  integration/Win7RefreshRateTests.cpp
  integration/LiveMediaRtspTests.cpp
  integration/DvbLibTests.cpp
  integration/HelperLibraryTests.cpp
  local/TestSinkFilter.cpp
)
target_include_directories(mp_integration_tests PRIVATE ../shared ../LiveMedia555/MediaPortal ../dvblib/Source/Include local)
target_link_libraries(mp_integration_tests PRIVATE mp_common DvbCoreUtils LiveMedia555 BaseClasses winmm strmiids ole32 oleaut32 uuid GTest::gtest GTest::gtest_main)

# The filters the integration tests load; they have to be built first.
set(MP_INTEGRATION_FILTERS TsReader TsWriter AudioRenderer DVBSubtitle3 MPFileWriter MPIPTVSource TsMuxer CCCP Win7RefreshRateHelper dvblib DirectShowHelper fontEngine)
foreach(_f ${MP_INTEGRATION_FILTERS})
  if(TARGET ${_f})
    add_dependencies(mp_integration_tests ${_f})
  endif()
endforeach()
# Folder with the built filters (the tests look for them next to the executable first).
target_compile_definitions(mp_integration_tests PRIVATE
  "MP_FILTER_DIR=L\"$<TARGET_FILE_DIR:mp_integration_tests>\"")

# ---------------------------------------------------------------------------
# Tests of the filters TsMuxer, TsReader and TsWriter: their own programs, built from the sources of the filters
# (mp_tsmuxer_tests, mp_tsreader_tests, mp_tswriter_tests)
# ---------------------------------------------------------------------------
add_executable(mp_tsmuxer_tests
  unit/TestSupport.cpp
  tsmuxer/TsMuxerTests.cpp
  ../TsMuxer/source/Hamming.cpp
  ../TsMuxer/source/MemoryBuffer.cpp
  ../TsMuxer/source/PacketSync.cpp
  ../TsMuxer/source/TeletextGrabber.cpp
  ../TsMuxer/source/ChannelScan.cpp
  ../TsMuxer/source/WaitEvent.cpp
)
target_include_directories(mp_tsmuxer_tests PRIVATE ../TsMuxer/source ../shared)
target_link_libraries(mp_tsmuxer_tests PRIVATE mp_common BaseClasses winmm ws2_32 strmiids uuid ole32 oleaut32 GTest::gtest GTest::gtest_main)
set_target_properties(mp_tsmuxer_tests PROPERTIES FOLDER "Tests")
gtest_discover_tests(mp_tsmuxer_tests DISCOVERY_MODE PRE_TEST PROPERTIES LABELS tsmuxer)

add_executable(mp_tswriter_tests
  unit/TestSupport.cpp
  tswriter/TextUtilTests.cpp
  tswriter/ParserTests.cpp
  tswriter/FileWriterTests.cpp
  ../TsWriter/source/FileWriter.cpp
  ../TsWriter/source/MultiFileWriter.cpp
  ../TsWriter/source/CDiskBuff.cpp
  ../TsReader/source/FileReader.cpp
  ../TsReader/source/MultiFileReader.cpp
  ../TsWriter/source/TextUtil.cpp
  ../TsWriter/source/RegistryUtil.cpp
  ../TsWriter/source/PcrDecoder.cpp
  ../TsWriter/source/SdtParser.cpp
  ../TsWriter/source/PmtParser.cpp
  ../TsWriter/source/PesDecoder.cpp
  ../TsWriter/source/PesPacket.cpp
)
target_include_directories(mp_tswriter_tests PRIVATE ../TsWriter/source ../shared unit)
target_link_libraries(mp_tswriter_tests PRIVATE mp_common DvbCoreUtils BaseClasses winmm strmiids uuid ole32 oleaut32 GTest::gtest GTest::gtest_main)
set_target_properties(mp_tswriter_tests PROPERTIES FOLDER "Tests")
gtest_discover_tests(mp_tswriter_tests DISCOVERY_MODE PRE_TEST PROPERTIES LABELS tswriter)

add_executable(mp_tsreader_tests
  unit/TestSupport.cpp
  tsreader/GolombBufferTests.cpp
  tsreader/FrameHeaderParserTests.cpp
  tsreader/FileReaderTests.cpp
  tsreader/PsiParserTests.cpp
  ../TsReader/source/PatParser.cpp
  ../TsReader/source/PmtParser.cpp
  ../TsReader/source/PidTable.cpp
  ../TsReader/source/ChannelInfo.cpp
  ../TsReader/source/FileReader.cpp
  ../TsReader/source/MultiFileReader.cpp
  ../TsReader/source/GolombBuffer.cpp
  ../TsReader/source/FrameHeaderParser.cpp
  ../TsReader/source/MpegPesParser.cpp
  ../shared/HEVC/BitstreamReader.cpp
  ../shared/HEVC/Hevc.cpp
  ../shared/HEVC/HevcNalDecode.cpp
)
target_include_directories(mp_tsreader_tests PRIVATE ../TsReader/source ../shared unit)
target_link_libraries(mp_tsreader_tests PRIVATE mp_common DvbCoreUtils BaseClasses winmm strmiids wmcodecdspuuid uuid ole32 oleaut32 ws2_32 GTest::gtest GTest::gtest_main)
set_target_properties(mp_tsreader_tests PROPERTIES FOLDER "Tests")
gtest_discover_tests(mp_tsreader_tests DISCOVERY_MODE PRE_TEST PROPERTIES LABELS tsreader)

# ---------------------------------------------------------------------------
# Local tests: media samples and a graphics device of the developer machine; never part of CI
#
#   cmake ... -DMP_BUILD_TESTS=ON -DMP_BUILD_LOCAL_TESTS=ON [-DMP_SAMPLES_DIR=D:/path/to/MP-MediaInfo.Samples]
#   cmake --build ... --target mp_local_tests
#   ctest -L local        (or run mp_local_tests.exe; tests whose sample or DLL is missing are skipped)
# ---------------------------------------------------------------------------
option(MP_BUILD_LOCAL_TESTS "Build mp_local_tests (mpcSubs.dll and DVBSub3 with real samples; needs a GPU, never run in CI)" OFF)
set(MP_SAMPLES_DIR "" CACHE PATH "Folder with the media samples used by mp_local_tests (also read from the environment variable MP_SAMPLES_DIR)")
if(MP_BUILD_LOCAL_TESTS)
  get_filename_component(_mp_sibling_samples "${CMAKE_CURRENT_LIST_DIR}/../../../MP-MediaInfo.Samples" ABSOLUTE)
  if(NOT MP_SAMPLES_DIR AND EXISTS "${_mp_sibling_samples}")
    set(MP_SAMPLES_DIR "${_mp_sibling_samples}" CACHE PATH "" FORCE)
  endif()

  add_executable(mp_local_tests
    unit/TestSupport.cpp
    integration/FilterHost.cpp
    local/LocalSupport.cpp
    local/TsSubtitleExtractor.cpp
    local/TestSourceFilter.cpp
    local/TestSinkFilter.cpp
    local/DvbSubtitleTests.cpp
    local/MpcSubsTests.cpp
    local/MpAudioRendererTests.cpp
    local/BdReaderTests.cpp
    local/LiveMediaPesTests.cpp
    local/FontEngineTests.cpp
  )
  target_include_directories(mp_local_tests PRIVATE ../shared ../DVBSubtitle3/Source ../LiveMedia555/MediaPortal integration local)
  target_link_libraries(mp_local_tests PRIVATE mp_common DvbCoreUtils LiveMedia555 BaseClasses winmm strmiids ole32 oleaut32 uuid d3d9 shell32 GTest::gtest GTest::gtest_main)
  target_compile_definitions(mp_local_tests PRIVATE
    "MP_FILTER_DIR=L\"$<TARGET_FILE_DIR:mp_local_tests>\""
    "MP_SAMPLES_DIR=L\"${MP_SAMPLES_DIR}\"")
  foreach(_f DVBSubtitle3 mpcSubs AudioRenderer MPAudioSwitcher BDReader fontEngine)
    if(TARGET ${_f})
      add_dependencies(mp_local_tests ${_f})
    endif()
  endforeach()
  # BDReader loads libbluray as bluray.dll from its own folder; the NuGet package MediaPortal.libbluray has both builds.
  # The BD-J jars go next to it (libbluray finds libbluray-awt-j2se-<version>.jar beside libbluray-j2se-<version>.jar).
  if(TARGET BDReader)
    if(CMAKE_SIZEOF_VOID_P EQUAL 8)
      set(_mp_bluray_arch x64)
    else()
      set(_mp_bluray_arch x86)
    endif()
    file(GLOB _mp_bluray_dll "${CMAKE_CURRENT_LIST_DIR}/../../Packages/MediaPortal.libbluray.*/references/runtimes/${_mp_bluray_arch}/bluray.dll")
    if(_mp_bluray_dll)
      list(SORT _mp_bluray_dll)
      list(GET _mp_bluray_dll -1 _mp_bluray_dll)
      get_filename_component(_mp_bluray_dir "${_mp_bluray_dll}" DIRECTORY)
      get_filename_component(_mp_bluray_dir "${_mp_bluray_dir}/.." ABSOLUTE)
      file(GLOB _mp_bluray_jars "${_mp_bluray_dir}/libbluray-*.jar")
      add_custom_command(TARGET mp_local_tests POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_if_different "${_mp_bluray_dll}" "$<TARGET_FILE_DIR:mp_local_tests>/bluray.dll"
        COMMAND ${CMAKE_COMMAND} -E copy_if_different ${_mp_bluray_jars} "$<TARGET_FILE_DIR:mp_local_tests>")
    endif()
  endif()
  set_target_properties(mp_local_tests PROPERTIES FOLDER "Tests")
  gtest_discover_tests(mp_local_tests DISCOVERY_MODE PRE_TEST PROPERTIES LABELS local)
endif()

if(MP_TEST_ASAN)
  foreach(_t mp_unit_tests mp_integration_tests mp_local_tests)
    if(TARGET ${_t})
      target_compile_options(${_t} PRIVATE /fsanitize=address)
      # gtest from vcpkg is not instrumented: the STL container annotations must be off to link with it
      target_compile_definitions(${_t} PRIVATE _DISABLE_VECTOR_ANNOTATION _DISABLE_STRING_ANNOTATION _DISABLE_OPTIONAL_ANNOTATION)
    endif()
  endforeach()
endif()

set_target_properties(mp_unit_tests mp_integration_tests PROPERTIES FOLDER "Tests")

gtest_discover_tests(mp_unit_tests DISCOVERY_MODE PRE_TEST)
gtest_discover_tests(mp_integration_tests DISCOVERY_MODE PRE_TEST PROPERTIES LABELS integration)
