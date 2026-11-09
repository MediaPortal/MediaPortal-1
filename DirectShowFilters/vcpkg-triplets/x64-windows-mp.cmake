# MediaPortal native filters, 64 bit. See x86-windows-mp.cmake.
set(VCPKG_TARGET_ARCHITECTURE x64)
set(VCPKG_CRT_LINKAGE dynamic)
set(VCPKG_LIBRARY_LINKAGE static)
if(PORT STREQUAL "unrar")
    set(VCPKG_LIBRARY_LINKAGE dynamic)
endif()

set(VCPKG_C_FLAGS "/guard:cf")
set(VCPKG_CXX_FLAGS "/guard:cf")
set(VCPKG_LINKER_FLAGS "/guard:cf /DYNAMICBASE /NXCOMPAT /HIGHENTROPYVA")
