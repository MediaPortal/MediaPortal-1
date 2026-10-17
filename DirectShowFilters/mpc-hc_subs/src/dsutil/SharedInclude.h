/*
 * (C) 2009-2013, 2016 see Authors.txt
 *
 * This file is part of MPC-HC.
 *
 * MPC-HC is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * MPC-HC is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

#pragma once

#pragma warning(disable:4995)
#pragma warning(disable:5054)
#pragma warning(disable:4467)

#define WIN32_LEAN_AND_MEAN // Exclude rarely-used stuff from Windows headers
#define VC_EXTRALEAN        // Exclude rarely-used stuff from Windows headers

#ifndef STRICT_TYPED_ITEMIDS
#define STRICT_TYPED_ITEMIDS
#endif

#ifdef _DEBUG
#if 1
#define _CRTDBG_MAP_ALLOC   // include Microsoft memory leak detection procedures
#include <crtdbg.h>
#else
#include "vld.h" //include visual leak detector procedures
#endif
#endif

#include <windows.h>
#include <shellapi.h>
#include <shlobj.h>
#include <shobjidl.h>
#include <stdexcept>
#include <algorithm>
#include <list>
#include <map>
#include <memory>
#include <unordered_map>
#include <vector>
#include <wrl/client.h>
#include "StdHelpers.h"
#include "DebugMacros.h"
#include "RectUtils.h"
#include "StrUtils.h"
#include <comdef.h> // _bstr_t and _variant_t
#include <shlwapi.h>

#pragma comment(lib, "shlwapi.lib")

using Microsoft::WRL::ComPtr;

// A pointer to the interface T of an object, or an empty one when it has none (what CComQIPtr did)
template <typename T>
inline ComPtr<T> ComQuery(IUnknown* unknown)
{
    ComPtr<T> result;
    if (unknown) {
        unknown->QueryInterface(__uuidof(T), reinterpret_cast<void**>(result.GetAddressOf()));
    }
    return result;
}

template <typename T, typename U>
inline ComPtr<T> ComQuery(const ComPtr<U>& source)
{
    return ComQuery<T>(static_cast<IUnknown*>(source.Get()));
}
