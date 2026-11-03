# CMake build of the native filters

CMake counterpart of `Filters.sln`. MSBuild stays the reference build used by CI; the CMake build
exists to build **x86 and x64** and **Unicode and MBCS** from one description, with any generator
that can drive MSVC.

## Prerequisites

| What | Why |
|---|---|
| Visual Studio 2026 (verified) or 2022 (`vs2022-*` presets exist, not verified) with the *Desktop development with C++* workload | MSVC and the Windows SDK. The filters use neither ATL nor MFC, so the component *C++ MFC for latest build tools* is not needed |
| CMake 3.25 or newer (4.2 or newer for the *Visual Studio 18 2026* generator) | Presets |
| vcpkg, environment variable `VCPKG_ROOT` | `zlib`, `freetype`, `libsamplerate`, `soundtouch`, `unrar`, `live555` (ADR-0001) |
| NASM on `PATH` | Assembler sources of `Kasumi` and `system` (MPC-HC engine) |
| NuGet package `DirectX_9_SDK` | `d3dx9`, `dxguid`, `DxErr`, `dsound`: `nuget restore DirectShowFilters\Filters.sln -PackagesDirectory Packages` |
| libbluray headers: `git submodule update --init libbluray` or the NuGet package `MediaPortal.libbluray` | `BDReader` (the DLL is loaded at run time) |

## Build

```bat
cd DirectShowFilters
cmake --list-presets
cmake --preset vs2026-x64-unicode
cmake --build --preset vs2026-x64-unicode-release --parallel
```

Presets are named `<ide>-<arch>-<charset>`: `vs2026` or `vs2022`, `x64` or `x86`, `unicode` or `mbcs`.
Build presets add `-release` or `-debug`. Output: `build/<preset>/bin/<Config>/` (DLLs, `.ax`, `.pdb`) and
`build/<preset>/lib/<Config>/` (static and import libraries). The vcpkg libraries are installed to
`vcpkg_installed/<triplet>`, the same tree MSBuild uses, so the two builds share them.

Without presets (any generator, for example Ninja from a Developer Command Prompt of the right architecture):

```bat
cmake -S . -B build/ninja-x64 -G Ninja -DCMAKE_BUILD_TYPE=Release ^
  -DCMAKE_TOOLCHAIN_FILE=%VCPKG_ROOT%/scripts/buildsystems/vcpkg.cmake ^
  -DVCPKG_TARGET_TRIPLET=x64-windows-mp -DVCPKG_OVERLAY_TRIPLETS=vcpkg-triplets ^
  -DVCPKG_INSTALLED_DIR=%CD%/vcpkg_installed/x64-windows-mp -DMP_CHARSET=UNICODE
cmake --build build/ninja-x64
```

## Options

| Option | Default | Meaning |
|---|---|---|
| `MP_CHARSET` | `UNICODE` | `UNICODE` defines `UNICODE`, `_UNICODE`; `MBCS` defines `_MBCS`. Applies to every MediaPortal target at once |
| `MP_LTCG` | `ON` (`OFF` for x86 with MSVC 14.51) | `/GL` + `/LTCG` for Release, see Known issues |
| `MP_BUILD_MPC_SUBS` | `AUTO` | The MPC-HC subtitle engine: `AUTO` builds it when `MP_CHARSET=UNICODE` |
| `MP_BUILD_BDREADER` | `AUTO` | `AUTO` builds it when the libbluray headers are found |
| `MP_DXSDK_DIR` | auto | DirectX 9 SDK; found through `%DXSDK_DIR%` or the NuGet package |
| `MP_LIBBLURAY_DIR` | auto | libbluray submodule or NuGet package folder |

The MPC-HC engine (`DSUtil`, `SubPic`, `Subtitles`, `mpcSubs`) is upstream code: it is always Unicode, and it is
built as C++17 without MFC and ATL (strings are `std::basic_string`, see `mpc-hc_subs/src/dsutil/StrUtils.h`). The VirtualDub libraries (`system`, `Kasumi`) are built with neither `UNICODE` nor `_MBCS`, as upstream
does. `MP_CHARSET` does not change them.

## MSBuild

The same switch exists for `Filters.sln`: pass `-p:CharacterSet=Unicode` or `-p:CharacterSet=MultiByte`
(Git Bash: use dashes, `/p:` is rewritten into a path). The MPC-HC and VirtualDub projects ignore it
(`TreatAsLocalProperty="CharacterSet"` in their `.vcxproj`).

```bat
msbuild DirectShowFilters\Filters.sln -m -t:Build -p:Configuration=Release -p:Platform=x64 -p:CharacterSet=MultiByte
```

## Source files in the other charset

Both builds compile the same sources, so code must not hard-wire one character set:

* Use `TCHAR`, `_T("...")`, `_tcs*` and the Win32 names without `A`/`W` suffix **only** for text that is meant to follow the
  build character set (file names shown to the user, registry keys, log text).
* Use `wchar_t`/`std::wstring`/`...W()` or `char`/`std::string`/`...A()` explicitly where the data has a fixed encoding
  (DirectShow `IFileSourceFilter::Load` paths are always wide, network protocol text is bytes).
* Convert with `ToAnsi()`, `ToWide()` and `ToTchar()` from `shared/TcharConv.h` (they replace the ATL macros `T2CA`,
  `CT2CW`, `W2CT`). They return `std::string`/`std::wstring`, so pass `ToAnsi(x).c_str()`; the temporary lives until the end of
  the full expression. Do not keep the `c_str()` pointer in a variable.
* Do not pass a `std::string`/`std::wstring` through `...` (`LogDebug("%ws", str)`): use `str.c_str()`.
* `NAME("text")` of the DirectShow base classes is already wrapped in `TEXT()`: do not write `NAME(_T("text"))`.

## Known issues

* **x86 Release with MSVC 14.51 (Visual Studio 2026): internal compiler error** in the link step (`C1001`, `LNK1000`) for
  `MPAudioRenderer` and `MPIPTVSource` when whole program optimization is on. Configure with `-DMP_LTCG=OFF` (MSBuild:
  `-p:WholeProgramOptimization=false`). x64 and Debug builds are not affected.
* The `.vcxproj` and CMake builds choose output names the same way, but the MSBuild tree places binaries in each
  project's `bin\<Config>` (x86 and x64 share it); CMake keeps one tree per preset.
