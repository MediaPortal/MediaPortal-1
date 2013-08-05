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

#ifndef __STRINGS_DEFINED
#define __STRINGS_DEFINED

// converts GUID to MBCS string
// @param guid : GUID to convert
// @return : reference to null terminated string or NULL if error occured
MPIPTVSOURCE_API char *ConvertGuidToStringA(const GUID guid);

// converts GUID to Unicode string
// @param guid : GUID to convert
// @return : reference to null terminated string or NULL if error occured
MPIPTVSOURCE_API wchar_t *ConvertGuidToStringW(const GUID guid);

// converts GUID to string
// @param guid : GUID to convert
// @return : reference to null terminated string or NULL if error occured
#ifdef _MBCS
#define ConvertGuidToString ConvertGuidToStringA
#else
#define ConvertGuidToString ConvertGuidToStringW
#endif

// converts string to mutli byte string
// @param string : string to convert
// @return : reference to null terminated string or NULL if error occured
MPIPTVSOURCE_API char *ConvertToMultiByteA(const char *string);

// converts string to mutli byte string
// @param string : string to convert
// @return : reference to null terminated string or NULL if error occured
MPIPTVSOURCE_API char *ConvertToMultiByteW(const wchar_t *string);

// converts string to Unicode string
// @param string : string to convert
// @return : reference to null terminated string or NULL if error occured
MPIPTVSOURCE_API wchar_t *ConvertToUnicodeA(const char *string);

// converts string to Unicode string
// @param string : string to convert
// @return : reference to null terminated string or NULL if error occured
MPIPTVSOURCE_API wchar_t *ConvertToUnicodeW(const wchar_t *string);

// converts string to mutli byte string
// @param string : string to convert
// @return : reference to null terminated string or NULL if error occured
#ifdef _MBCS
#define ConvertToMultiByte ConvertToMultiByteA
#else
#define ConvertToMultiByte ConvertToMultiByteW
#endif

// converts string to Unicode string
// @param string : string to convert
// @return : reference to null terminated string or NULL if error occured
#ifdef _MBCS
#define ConvertToUnicode ConvertToUnicodeA
#else
#define ConvertToUnicode ConvertToUnicodeW
#endif

// duplicate mutli byte string
// @param string : string to duplicate
// @return : reference to null terminated string or NULL if error occured
MPIPTVSOURCE_API char *DuplicateA(const char *string);

// duplicate Unicode string
// @param string : string to duplicate
// @return : reference to null terminated string or NULL if error occured
MPIPTVSOURCE_API wchar_t *DuplicateW(const wchar_t *string);

// duplicate string
// @param string : string to duplicate
// @return : reference to null terminated string or NULL if error occured
#ifdef _MBCS
#define Duplicate DuplicateA
#else
#define Duplicate DuplicateW
#endif

// tests if mutli byte string is null or empty
// @param string : string to test
// @return : true if null or empty, otherwise false
MPIPTVSOURCE_API BOOL IsNullOrEmptyA(const char *string);

// tests if Unicode string is null or empty
// @param string : string to test
// @return : true if null or empty, otherwise false
MPIPTVSOURCE_API BOOL IsNullOrEmptyW(const wchar_t *string);

#ifdef _MBCS
#define IsNullOrEmpty IsNullOrEmptyA
#else
#define IsNullOrEmpty IsNullOrEmptyW
#endif

// formats string using format string and parameters
// @param format : format string
// @return : result string or NULL if error
MPIPTVSOURCE_API char *FormatStringA(const char *format, ...);

// formats string using format string and parameters
// @param format : format string
// @return : result string or NULL if error
MPIPTVSOURCE_API wchar_t *FormatStringW(const wchar_t *format, ...);

#ifdef _MBCS
#define FormatString FormatStringA
#else
#define FormatString FormatStringW
#endif

MPIPTVSOURCE_API char *ReplaceStringA(const char *string, const char *searchString, const char *replaceString);

MPIPTVSOURCE_API wchar_t *ReplaceStringW(const wchar_t *string, const wchar_t *searchString, const wchar_t *replaceString);

#ifdef _MBCS
#define ReplaceString ReplaceStringA
#else
#define ReplaceString ReplaceStringW
#endif

#endif
