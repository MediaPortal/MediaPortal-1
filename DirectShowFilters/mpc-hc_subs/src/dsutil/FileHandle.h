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

#pragma once

std::wstring GetFileOnly(LPCWSTR Path);
std::wstring GetFolderOnly(LPCWSTR Path);
std::wstring AddSlash(LPCWSTR Path);
std::wstring RemoveSlash(LPCWSTR Path);
std::wstring GetFileExt(LPCWSTR Path);
std::wstring RenameFileExt(LPCWSTR Path, LPCWSTR Ext);
std::wstring RemoveFileExt(LPCWSTR Path);
BOOL     GetTemporaryFilePath(std::wstring strExtension, std::wstring& strFileName);
std::wstring CompactPath(LPCWSTR Path, UINT cchMax);

// Get path of specified module
std::wstring GetModulePath(HMODULE hModule);
// Get path of the executable file of the current process
std::wstring GetProgramPath();
// Get programm directory with slash
std::wstring GetProgramDir();

// wFunc can be FO_MOVE or FO_COPY.
// To move a folder, add "\" to the end of the source path.
// To copy a folder, add "\*" to the end of the source path.
int FileOperation(const std::wstring& source, const std::wstring& target, const UINT wFunc);
