/**
 *	GlobalFunctions.h
 *	Copyright (C) 2004 Nate
 *
 *	This file is part of DigitalWatch, a free DTV watching and recording
 *	program for the VisionPlus DVB-T.
 *
 *	DigitalWatch is free software; you can redistribute it and/or modify
 *	it under the terms of the GNU General Public License as published by
 *	the Free Software Foundation; either version 2 of the License, or
 *	(at your option) any later version.
 *
 *	DigitalWatch is distributed in the hope that it will be useful,
 *	but WITHOUT ANY WARRANTY; without even the implied warranty of
 *	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *	GNU General Public License for more details.
 *
 *	You should have received a copy of the GNU General Public License
 *	along with DigitalWatch; if not, write to the Free Software
 *	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

#ifndef GLOBALFUNCTIONS_H
#define GLOBALFUNCTIONS_H

#include "stdafx.h"

typedef struct
{
	double left;
	double top;
	double right;
	double bottom;
} dRECT;

void PStringCopy(char** destString, char* srcString);
void PStringCopy(wchar_t** destString, wchar_t* srcString);
void SetdRect(dRECT* rect, double left, double top, double right, double bottom);
void SetdRectEmpty(dRECT* bounds);

void GetCommandPath(LPWSTR pPath);
void GetCommandExe(LPWSTR pExe);

long wcsToColor(LPWSTR string);

__inline BOOL findchr(char character, LPCSTR strCharSet)
{
	int length = strlen(strCharSet);
	for ( int i=0 ; i<length ; i++ )
	{
		if (strCharSet[i] == character)
			return TRUE;
	}
	return FALSE;
}

__inline BOOL isWhitespace(char character)
{
	return ((character == ' ') ||
			(character == '\t'));
}

__inline void skipWhitespaces(LPCSTR &str)
{
	while (isWhitespace(str[0]))
		str++;
}

__inline LPSTR findEndOfTokenName(LPCSTR str)
{
	int length = strlen(str);
	for ( int i=0 ; i<=length ; i++ )
	{
		if (((str[i] < 'A') || (str[i] > 'Z')) &&
			((str[i] < 'a') || (str[i] > 'z')) &&
			((str[i] < '0') || (str[i] > '9')) &&
			(str[i] != '_') &&
			(str[i] != '$')
		   )
		   return (LPSTR)&str[i];
	}
	//Should never get to here because the for loop includes the '\0' character
	return NULL;
}

__inline BOOL findchr(wchar_t character, LPCWSTR strCharSet)
{
	int length = wcslen(strCharSet);
	for ( int i=0 ; i<length ; i++ )
	{
		if (strCharSet[i] == character)
			return TRUE;
	}
	return FALSE;
}

__inline BOOL isWhitespace(wchar_t character)
{
	return ((character == ' ') ||
			(character == '\t'));
}

__inline void skipWhitespaces(LPCWSTR &str)
{
	while (isWhitespace(str[0]))
		str++;
}

__inline LPWSTR findEndOfTokenName(LPCWSTR str)
{
	int length = wcslen(str);
	for ( int i=0 ; i<=length ; i++ )
	{
		if (((str[i] < 'A') || (str[i] > 'Z')) &&
			((str[i] < 'a') || (str[i] > 'z')) &&
			((str[i] < '0') || (str[i] > '9')) &&
			(str[i] != '_') &&
			(str[i] != '$')
		   )
		   return (LPWSTR)&str[i];
	}
	//Should never get to here because the for loop includes the '\0' character
	return NULL;
}

__inline void strCopy(LPSTR &dest, LPCSTR src, long length)
{
	if (dest)
		delete[] dest;
	dest = NULL;
	if (!src)
		return;
	if (length < 0)
		length = strlen(src);
	dest = new char[length + 1];
	memcpy(dest, src, length);
	dest[length] = 0;
}

__inline void strCopy(LPWSTR &dest, LPCWSTR src, long length)
{
	if (dest)
		delete[] dest;
	dest = NULL;
	if (!src)
		return;
	if (length < 0)
		length = wcslen(src);
	dest = new wchar_t[length + 1];
	memcpy(dest, src, length*2);
	dest[length] = 0;
}

__inline void strCopyA2W(LPWSTR &dest, LPCSTR src, long length)
{
	if (dest)
		delete[] dest;
	dest = NULL;
	if (!src)
		return;
	if (length < 0)
		length = strlen(src);
	dest = new wchar_t[length + 1];
	mbstowcs(dest, src, length);
	dest[length] = 0;
}

__inline void strCopyW2A(LPSTR &dest, LPCWSTR src, long length)
{
	if (dest)
		delete[] dest;
	dest = NULL;
	if (!src)
		return;
	if (length < 0)
		length = wcslen(src);
	dest = new char[length + 1];
	wcstombs(dest, src, length);
}

__inline void strCopy(LPSTR &dest, LPCSTR src)
{
	strCopy(dest, src, -1);
}

__inline void strCopy(LPWSTR &dest, LPCWSTR src)
{
	strCopy(dest, src, -1);
}

__inline void strCopyA2W(LPWSTR &dest, LPCSTR src)
{
	strCopyA2W(dest, src, -1);
}

__inline void strCopyW2A(LPSTR &dest, LPCWSTR src)
{
	strCopyW2A(dest, src, -1);
}

__inline void strCopy(LPWSTR &dest, long value)
{
	if (dest)
		delete[] dest;
	BOOL bNegative = (value < 0);
	value = abs(value);
	long length = (long)log10((double)value) + (bNegative ? 2 : 1);
	dest = new wchar_t[length + 1];

	for ( int i=length-1 ; i>=0 ; i-- )
	{
		dest[i] = (wchar_t)('0' + (value % 10));
		value /= 10;
	}
	if (bNegative)
		dest[0] = '-';
	dest[length] = 0;
}

void strCopyHex(LPWSTR &dest, long value);
long StringToLong(LPWSTR pValue);


__inline int strCmp(LPWSTR string1, LPWSTR string2, BOOL ignoreCase = TRUE)
{
	if (!string1 && !string2)
		return 0;
	if (!string1)
		return -1;
	if (!string2)
		return 1;

	if (ignoreCase)
		return _wcsicmp(string1, string2);
	else
		return wcscmp(string1, string2);
}

// returns < 0 if failed, otherwise returns length of findString
__inline long strStartsWith(LPWSTR searchString, LPWSTR findString, BOOL ignoreCase = TRUE)
{
	if (!searchString || !findString)
		return -1;
	long searchLength = wcslen(searchString);
	long findLength = wcslen(findString);

	if (findLength > searchLength)
		return -1;

	wchar_t storeChar = searchString[findLength];
	searchString[findLength] = '\0';

	int cmp;
	if (ignoreCase)
		cmp = _wcsicmp(searchString, findString);
	else
		cmp = wcscmp(searchString, findString);

	searchString[findLength] = storeChar;
	if (cmp)
		return -1;

	return findLength;
}

class AutoDeletingString
{
public:
	AutoDeletingString()
	{
		pStr = NULL;
	}
	virtual ~AutoDeletingString()
	{
		if (pStr)
			delete[] pStr;
	}

	LPWSTR pStr;
};

#endif
