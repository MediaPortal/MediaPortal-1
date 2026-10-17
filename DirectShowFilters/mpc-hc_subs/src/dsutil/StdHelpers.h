/*
 * (C) 2024 see Authors.txt
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

#include <cstdio>
#include <algorithm>
#include <cstring>
#include <memory>
#include <vector>
#include <share.h>
#include "StrUtils.h"

// Looks a key up in a std::map or std::unordered_map: the value is only set when the key is there
template <class Map>
inline bool Lookup(const Map& map, const typename Map::key_type& key, typename Map::mapped_type& value)
{
    const auto it = map.find(key);
    if (it == map.end()) {
        return false;
    }
    value = it->second;
    return true;
}

// A C stream that is closed when the pointer goes
struct FileCloser {
    void operator()(FILE* file) const {
        if (file) {
            fclose(file);
        }
    }
};
using FilePtr = std::unique_ptr<FILE, FileCloser>;

// mode as for _wfsopen ("rb", "wb", "rt", "wt"...), share as _SH_DENYNO, _SH_DENYWR...
inline FilePtr OpenFile(const wchar_t* path, const wchar_t* mode, int share = _SH_DENYNO)
{
    return FilePtr(_wfsopen(path, mode, share));
}

// A growing buffer in memory that is read and written like a file
class CMemBuffer
{
    std::vector<BYTE> m_data;
    size_t m_pos = 0;

public:
    ULONGLONG GetLength() const { return m_data.size(); }
    ULONGLONG GetPosition() const { return m_pos; }
    void SetLength(ULONGLONG len) {
        m_data.resize(static_cast<size_t>(len));
        m_pos = (std::min)(m_pos, m_data.size());
    }
    void SeekToBegin() { m_pos = 0; }
    ULONGLONG Seek(LONGLONG off, UINT from = SEEK_SET) {
        const LONGLONG base = from == SEEK_CUR ? static_cast<LONGLONG>(m_pos) : from == SEEK_END ? static_cast<LONGLONG>(m_data.size()) : 0;
        m_pos = static_cast<size_t>((std::max)(base + off, 0LL));
        return m_pos;
    }
    UINT Read(void* buf, UINT count) {
        if (m_pos >= m_data.size()) {
            return 0;
        }
        const UINT n = static_cast<UINT>((std::min<size_t>)(count, m_data.size() - m_pos));
        memcpy(buf, m_data.data() + m_pos, n);
        m_pos += n;
        return n;
    }
    void Write(const void* buf, UINT count) {
        if (m_pos + count > m_data.size()) {
            m_data.resize(m_pos + count);
        }
        memcpy(m_data.data() + m_pos, buf, count);
        m_pos += count;
    }
};

inline FilePtr OpenFile(const std::wstring& path, const wchar_t* mode, int share = _SH_DENYNO)
{
    return OpenFile(path.c_str(), mode, share);
}

inline FilePtr OpenFile(const char* path, const wchar_t* mode, int share = _SH_DENYNO)
{
    return OpenFile(StringWiden(path).c_str(), mode, share);
}

inline FilePtr OpenFile(const std::string& path, const wchar_t* mode, int share = _SH_DENYNO)
{
    return OpenFile(path.c_str(), mode, share);
}

// The module that contains this code: the DLL
inline HMODULE ThisModule()
{
    HMODULE module = nullptr;
    GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
                      reinterpret_cast<LPCTSTR>(&ThisModule), &module);
    return module;
}

inline ULONGLONG FileLength(FILE* file)
{
    const long long position = _ftelli64(file);
    _fseeki64(file, 0, SEEK_END);
    const long long length = _ftelli64(file);
    _fseeki64(file, position, SEEK_SET);
    return length < 0 ? 0 : static_cast<ULONGLONG>(length);
}
