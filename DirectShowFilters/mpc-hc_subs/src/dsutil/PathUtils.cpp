/*
 * (C) 2013-2015, 2017 see Authors.txt
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

#include "stdafx.h"
#include <filesystem>
#include "PathUtils.h"
#include <memory>
#include "text.h"

namespace PathUtils
{
    // The path functions of shlwapi change the string in place
    template<typename Edit>
    tstring EditPath(LPCTSTR path, Edit edit)
    {
        tstring ret(path);
        LPTSTR buff = StringBuffer(ret, (std::max)(int(ret.size()) + 1, int(MAX_PATH)));
        edit(buff);
        StringRelease(ret);
        return ret;
    }

    tstring BaseName(LPCTSTR path)
    {
        return EditPath(path, [](LPTSTR p) {
            PathRemoveBackslash(p);
            PathStripPath(p);
        });
    }

    tstring DirName(LPCTSTR path)
    {
        return EditPath(path, [](LPTSTR p) {
            PathRemoveBackslash(p);
            PathRemoveFileSpec(p);
        });
    }

    tstring FileName(LPCTSTR path)
    {
        return EditPath(path, [](LPTSTR p) {
            PathStripPath(p);
            PathRemoveExtension(p);
            PathRemoveBackslash(p);
        });
    }

    tstring FileExt(LPCTSTR path)
    {
        return tstring(PathFindExtension(path));
    }

    tstring GetModulePath(HMODULE hModule)
    {
        tstring ret;
        int pos, len = MAX_PATH - 1;
        for (;;) {
            pos = GetModuleFileName(hModule, StringBuffer(ret, len), len);
            if (pos == len) {
                // buffer was too small, enlarge it and try again
                len *= 2;
                StringRelease(ret, 0);
                continue;
            }
            StringRelease(ret, pos);
            break;
        }
        ASSERT(!ret.empty());
        return ret;
    }

    tstring GetAfxModulePath(bool bWithModuleName/* = false*/)
    {
        tstring ret = GetModulePath(ThisModule());
        if (!bWithModuleName) {
            ret = DirName(ret.c_str());
        }
        return ret;
    }

    tstring GetProgramPath(bool bWithExeName/* = false*/)
    {
        tstring ret = GetModulePath(nullptr);
        if (!bWithExeName) {
            ret = DirName(ret.c_str());
        }
        return ret;
    }

    tstring CombinePaths(LPCTSTR dir, LPCTSTR path)
    {
        TCHAR combined[MAX_PATH] = {};
        return PathCombine(combined, dir, path) ? tstring(combined) : tstring();
    }

    tstring FilterInvalidCharsFromFileName(LPCTSTR fn, TCHAR replacementChar /*= _T('_')*/)
    {
        tstring ret = fn;
        int iLength = int(ret.size());
        LPTSTR buff = StringBuffer(ret, 0);

        for (int i = 0; i < iLength; i++) {
            switch (buff[i]) {
                case _T('<'):
                case _T('>'):
                case _T(':'):
                case _T('"'):
                case _T('/'):
                case _T('\\'):
                case _T('|'):
                case _T('?'):
                case _T('*'):
                    buff[i] = replacementChar;
                    break;
                default:
                    // Do nothing
                    break;
            }
        }

        StringRelease(ret);

        return ret;
    }

    tstring Unquote(LPCTSTR path)
    {
        tstring unquoted(path);
        return StringTrim(unquoted, _T("\""));
    }

    tstring StripPathOrUrl(LPCTSTR path)
    {
        // Replacement for CPath::StripPath which works fine also for URLs
        tstring p = path;
        bool isURL = StringFind(p, _T("://")) > 1;
        StringReplace(p, '\\', '/');
        StringTrimRight(p, '/');
        p = StringMid(p, StringRFind(p, '/') + 1);
        if (p.empty()) {
            return tstring(path);
        } else if (isURL) {
            return UrlDecodeWithUTF8(p);
        }
        return p;
    }

    bool IsInDir(LPCTSTR path, LPCTSTR dir)
    {
        return !!PathIsPrefix(dir, path);
    }

    tstring ToRelative(LPCTSTR dir, const LPCTSTR path, bool* pbRelative/* = nullptr*/)
    {
        TCHAR relative[MAX_PATH] = {};
        BOOL rel = PathRelativePathTo(relative, dir, FILE_ATTRIBUTE_DIRECTORY, path, 0);
        if (pbRelative) {
            *pbRelative = !!rel;
        }
        return rel ? tstring(relative) : tstring();
    }

    bool IsRelative(LPCTSTR path)
    {
        return !!PathIsRelative(path);
    }

    bool Exists(LPCTSTR path)
    {
        return GetFileAttributes(path) != INVALID_FILE_ATTRIBUTES;
    }

    bool IsFile(LPCTSTR path)
    {
        DWORD attr = GetFileAttributes(path);
        return (attr != INVALID_FILE_ATTRIBUTES) && !(attr & FILE_ATTRIBUTE_DIRECTORY);
    }

    bool IsDir(LPCTSTR path)
    {
        DWORD attr = GetFileAttributes(path);
        return (attr != INVALID_FILE_ATTRIBUTES) && (attr & FILE_ATTRIBUTE_DIRECTORY);
    }

    bool IsLinkFile(LPCTSTR path)
    {
        return !StringICompare(FileExt(path), _T(".lnk"));
    }

    bool CreateDirRecursive(LPCTSTR path)
    {
        bool ret = IsDir(path) || CreateDirectory(path, nullptr);
        if (!ret) {
            ret = CreateDirRecursive(DirName(path).c_str()) && CreateDirectory(path, nullptr);
        }
        return ret;
    }

    tstring ResolveLinkFile(LPCTSTR path)
    {
        TCHAR buff[MAX_PATH];
        ComPtr<IShellLink> pSL;
        CoCreateInstance(CLSID_ShellLink, nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&pSL));
        auto pPF = ComQuery<IPersistFile>(pSL);

        if (pSL && pPF
                && SUCCEEDED(pPF->Load(path, STGM_READ))
                && SUCCEEDED(pSL->Resolve(nullptr, SLR_ANY_MATCH | SLR_NO_UI))
                && SUCCEEDED(pSL->GetPath(buff, _countof(buff), nullptr, 0))) {
            return buff;
        }

        return _T("");
    }

    void RecurseAddDir(LPCTSTR path, std::list<tstring>& paths)
    {
        std::error_code ec;
        for (const auto& entry : std::filesystem::directory_iterator(std::filesystem::path(path), ec)) {
            std::error_code isDirError;
            if (entry.is_directory(isDirError)) {
                tstring folderPath = entry.path().c_str();
                paths.push_back(folderPath);
                RecurseAddDir(folderPath.c_str(), paths);
            }
        }
    }

    void ParseDirs(std::list<tstring>& paths)
    {
        for (auto pathIt = paths.begin(); pathIt != paths.end();) {
            const auto nextIt = std::next(pathIt);
            tstring fn = *pathIt;
            pathIt = nextIt;
            // Try to follow link files that point to a directory
            if (IsLinkFile(fn.c_str())) {
                fn = ResolveLinkFile(fn.c_str());
            }

            if (IsDir(fn.c_str())) {
                std::list<tstring> subDirs;
                RecurseAddDir(fn.c_str(), subDirs);
                // Add the subdirectories just after their parent
                // so that the tree is not parsed multiple times
                // (they are put in front of the next element, so the loop does not visit them again)
                paths.splice(nextIt, subDirs);
            }
        }
    }

    bool IsURL(tstring& fn)
    {
        return StringFind(fn, _T("://")) > 1;
    }

    bool IsFullFilePath(tstring& fn)
    {
        return (StringFind(fn, _T(":")) > 0) && !IsURL(fn) || (StringFind(fn, _T("\\\\")) == 0);
    }
}
