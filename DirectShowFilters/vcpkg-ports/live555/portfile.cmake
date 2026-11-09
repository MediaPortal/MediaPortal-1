vcpkg_check_linkage(ONLY_STATIC_LIBRARY)

string(REPLACE "-" "." format_version "${VERSION}")
# download.live555.com keeps only the latest release; the mirror holds unmodified copies (same SHA512)
vcpkg_download_distfile(ARCHIVE
    URLS
        "https://github.com/yartat/mediaportal-native-deps/releases/download/live555-${format_version}/live.${format_version}.tar.gz"
        "https://download.live555.com/live.${format_version}.tar.gz"
    FILENAME "live.${format_version}.tar.gz"
    SHA512 c5ee0ffa89a08e01a09367e3ae414fe2933e173abf060c8c9a39fde0e555df59f4f81f31e14d014bc6227569f464f736325b05e270fd061d472e3d1cc5136986
)

vcpkg_extract_source_archive(
    SOURCE_PATH
    ARCHIVE "${ARCHIVE}"
)

file(COPY "${CMAKE_CURRENT_LIST_DIR}/CMakeLists.txt" DESTINATION "${SOURCE_PATH}")

vcpkg_cmake_configure(
    SOURCE_PATH "${SOURCE_PATH}"
)

vcpkg_cmake_install()
vcpkg_copy_pdbs()

vcpkg_cmake_config_fixup(PACKAGE_NAME unofficial-live555)

file(GLOB HEADERS
    "${SOURCE_PATH}/BasicUsageEnvironment/include/*.h*"
    "${SOURCE_PATH}/groupsock/include/*.h*"
    "${SOURCE_PATH}/liveMedia/include/*.h*"
    "${SOURCE_PATH}/UsageEnvironment/include/*.h*"
)

file(COPY ${HEADERS} DESTINATION "${CURRENT_PACKAGES_DIR}/include")

vcpkg_install_copyright(FILE_LIST
    "${SOURCE_PATH}/COPYING"
    "${SOURCE_PATH}/COPYING.LESSER"
)
