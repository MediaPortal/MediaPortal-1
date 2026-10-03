# MediaPortal native filters, 32 bit.
# Static libraries against the dynamic CRT (/MD), like the filters themselves, so the
# libraries are linked into the filter DLLs and nothing extra has to be deployed.
# Exception: the unrar port only supports a DLL (shipped next to mpcSubs.dll).
set(VCPKG_TARGET_ARCHITECTURE x86)
set(VCPKG_CRT_LINKAGE dynamic)
set(VCPKG_LIBRARY_LINKAGE static)
if(PORT STREQUAL "unrar")
    set(VCPKG_LIBRARY_LINKAGE dynamic)
endif()

# Same exploit mitigations as Directory.Build.props
set(VCPKG_C_FLAGS "/guard:cf")
set(VCPKG_CXX_FLAGS "/guard:cf")
set(VCPKG_LINKER_FLAGS "/guard:cf /DYNAMICBASE /NXCOMPAT")
