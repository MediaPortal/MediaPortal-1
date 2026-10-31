/*
 * (C) 2011-2021 see Authors.txt
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
#include "FileHandle.h"

//
// Returns the file portion from a path
//
std::wstring GetFileOnly(LPCWSTR Path)
{
	std::wstring cs = Path;
	::PathStripPathW(StringBuffer(cs, 0));
	StringRelease(cs, -1);
	return cs;
}

//
// Returns the folder portion from a path
//
std::wstring GetFolderOnly(LPCWSTR Path)
{
	std::wstring cs = Path; // Force std::wstring to make a copy
	::PathRemoveFileSpecW(StringBuffer(cs, 0));
	StringRelease(cs, -1);
	return cs;
}

//
// Adds a backslash to the end of a path if it is needed
//
std::wstring AddSlash(LPCWSTR Path)
{
	std::wstring cs = Path;
	::PathAddBackslashW(StringBuffer(cs, MAX_PATH));
	StringRelease(cs, -1);
	if(cs.empty()) {
		cs = L"\\";
	}
	return cs;
}

//
// Removes a backslash from the end of a path if it is there
//
std::wstring RemoveSlash(LPCWSTR Path)
{
	tstring cs = Path;
	::PathRemoveBackslashW(StringBuffer(cs, MAX_PATH));
	StringRelease(cs, -1);
	return cs;
}

//
// Returns just the .ext part of the file path
//
std::wstring GetFileExt(LPCWSTR Path)
{
    if (::PathIsURLW(Path)) {
        auto q = wcschr(Path, '?');
        if (q) {
            std::wstring ext = ::PathFindExtensionW(std::wstring(Path, q - Path).c_str());
            return ext;
        }
    }
    std::wstring ext = ::PathFindExtensionW(Path);
    return ext;
}

//
// Exchanges one file extension for another and returns the new file path
//
std::wstring RenameFileExt(LPCWSTR Path, LPCWSTR Ext)
{
	std::wstring cs = Path;
	::PathRenameExtensionW(StringBuffer(cs, MAX_PATH), Ext);
	return cs;
}

//
// Removes the file name extension from a path, if one is present
//
std::wstring RemoveFileExt(LPCWSTR Path)
{
	std::wstring cs = Path;
	::PathRemoveExtensionW(StringBuffer(cs, MAX_PATH));
	return cs;
}

//
// Generate temporary files with any extension
//
BOOL GetTemporaryFilePath(std::wstring strExtension, std::wstring& strFileName)
{
	WCHAR lpszTempPath[MAX_PATH] = { 0 };
	if (!GetTempPathW(MAX_PATH, lpszTempPath)) {
		return FALSE;
	}

	WCHAR lpszFilePath[MAX_PATH] = { 0 };
	do {
		if (!GetTempFileNameW(lpszTempPath, L"mpc", 0, lpszFilePath)) {
			return FALSE;
		}

		DeleteFile(lpszFilePath);

		strFileName = lpszFilePath;
		StringReplace(strFileName, L".tmp", strExtension);

		DeleteFile(strFileName.c_str());
	} while (_waccess(strFileName.c_str(), 00) != -1);

	return TRUE;
}

//
// Compact Path
//
std::wstring CompactPath(LPCWSTR Path, UINT cchMax)
{
	std::wstring cs = Path;
	WCHAR pathbuf[MAX_PATH] = { 0 };
	if (::PathCompactPathExW(pathbuf, cs.c_str(), cchMax, 0)) {
		cs = pathbuf;
	}

	return cs;
}

//
// Get path of specified module
//
std::wstring GetModulePath(HMODULE hModule)
{
	std::wstring path;
	DWORD bufsize = MAX_PATH;
	DWORD len = 0;
	while (1) {
		len = GetModuleFileNameW(hModule, StringBuffer(path, bufsize), bufsize);
		if (len < bufsize) {
			break;
		}
		bufsize *= 2;
	}
	StringRelease(path, len);

	ASSERT(int(path.size()));
	return path;
}

//
// Get path of the executable file of the current process.
//
std::wstring GetProgramPath()
{
	return GetModulePath(nullptr);
}

//
// Get program directory with backslash
//
std::wstring GetProgramDir()
{
	std::wstring path = GetModulePath(nullptr);
	StringTruncate(path, StringRFind(path, '\\') + 1); // do not use this method for random paths

	return path;
}

// wFunc can be FO_MOVE or FO_COPY.
// To move a folder, add "\" to the end of the source path.
// To copy a folder, add "\*" to the end of the source path.
int FileOperation(const std::wstring& source, const std::wstring& target, const UINT wFunc)
{
	auto from_str = std::make_unique<WCHAR[]>(int(source.size()) + 2);
	wcscpy_s(from_str.get(), int(source.size())+1, source.c_str());

	auto to_str = std::make_unique<WCHAR[]>(int(target.size()) + 2);
	wcscpy_s(to_str.get(), int(target.size())+1, target.c_str());

	// set double null-terminated string
	from_str[wcslen(from_str.get())+1] = 0;
	to_str[wcslen(to_str.get())+1] = 0;

	SHFILEOPSTRUCTW FileOp = { 0 };
	FileOp.wFunc  = wFunc;
	FileOp.pFrom  = from_str.get();
	FileOp.pTo    = to_str.get();
	FileOp.fFlags = FOF_NO_UI;

	return SHFileOperationW(&FileOp);
}
