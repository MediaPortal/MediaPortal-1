// Replacements for the few MFC macros that the filters used through <afx.h>.
//
// TsReader, BDReader and the other filters use neither MFC nor ATL any more (std containers,
// std::unique_ptr, std::wstring, Microsoft::WRL::ComPtr), so afx.h is no longer included.
#pragma once

#include <crtdbg.h>

// Same behaviour as the MFC macro: checks in Debug builds, nothing in Release builds.
// DirectShow's wxdebug.h keeps an ASSERT that is already defined.
#ifndef ASSERT
#define ASSERT(f) _ASSERTE(f)
#endif

#ifndef UNUSED_ALWAYS
#define UNUSED_ALWAYS(x) ((void)(x))
#endif
