/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#pragma once

#ifndef __UTILITIES_DEFINED
#define __UTILITIES_DEFINED

#include "ParameterCollection.h"

#include <WinInet.h>

// get Tv Server folder
// @return : reference to null terminated string with path of Tv Server ended with '\' or NULL if error occured
wchar_t *GetTvServerFolder(void);

// get MediaPortal folder
// @return : reference to null terminated string with path of MediaPortal ended with '\' or NULL if error occured
wchar_t *GetMediaPortalFolder(void);

// get path to file in Tv Server folder
// Tv Server folder always ended with '\'
// @param filePath : the file path in Tv Server folder
// @return : reference to null terminated string with path of file or NULL if error occured
wchar_t *GetTvServerFilePath(const wchar_t *filePath);

// get path to file in MediaPortal folder
// MediaPortal folder always ended with '\'
// @param filePath : the file path in MediaPortal folder
// @return : reference to null terminated string with path of file or NULL if error occured
wchar_t *GetMediaPortalFilePath(const wchar_t *filePath);

// clear url components
// @param url : url components to clear
void ZeroURL(URL_COMPONENTS *url);

// gets version info
// caller is responsible for freeing memory
// @return : version info string or NULL if error
wchar_t *GetVersionInfo(const wchar_t *commit, const wchar_t *date);

// gets store file path based on configuration
// creates folder structure if not created
// @param folder : the folder name under LOCAL_APPDATA folder (used only in case if not specified PARAMETER_NAME_CACHE_FOLDER in configuration collection)
// @param configuration : the configuration
// @return : store file path or NULL if error
wchar_t *GetStoreFilePath(const wchar_t *folder, CParameterCollection *configuration);

// sets thread name
// @param dwThreadID : the thread ID to set name (-1 for caller thread)
// @param szThreadName : the thread name to set
void SetThreadName(DWORD dwThreadID, LPCSTR szThreadName);

// computes greatest common divisor by binary GCD algorithm
// http://en.wikipedia.org/wiki/Binary_GCD_algorithm
// @param u : first number
// @param v : second number
// @return : greatest common divisor
unsigned int GreatestCommonDivisor(unsigned int u, unsigned int v);

// computes greatest common divisor by binary GCD algorithm
// http://en.wikipedia.org/wiki/Binary_GCD_algorithm
// @param u : first number
// @param v : second number
// @return : greatest common divisor
int64_t GreatestCommonDivisor(int64_t u, int64_t v);

// computes greatest common divisor by binary GCD algorithm
// http://en.wikipedia.org/wiki/Binary_GCD_algorithm
// @param u : first number
// @param v : second number
// @return : greatest common divisor
uint64_t GreatestCommonDivisor(uint64_t u, uint64_t v);

#endif
