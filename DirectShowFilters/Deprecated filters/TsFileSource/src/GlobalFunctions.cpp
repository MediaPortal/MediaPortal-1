/**
 *	GlobalFunctions.cpp
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

/* Something to consider using
struct PinInfo : public PIN_INFO
{
   PinInfo() { pFilter = NULL; }
   ~PinInfo() { SAFE_RELEASE( pFilter );}
};


struct FilterInfo : public FILTER_INFO
{
   FilterInfo() { pFilter = NULL; }
   ~FilterInfo() { SAFE_RELEASE( pGraph );}
};
*/

#include "StdAfx.h"
#include "GlobalFunctions.h"

void PStringCopy(char** destString, char* srcString)
{
	if (*destString)
		delete *destString;
	*destString = new char[strlen(srcString)+1];
	strcpy(*destString, srcString);
}

void PStringCopy(wchar_t** destString, wchar_t* srcString)
{
	if (*destString)
		delete *destString;
	*destString = new wchar_t[wcslen(srcString)+1];
	wcscpy(*destString, srcString);
}

void SetdRect(dRECT* rect, double left, double top, double right, double bottom)
{
	rect->top = top;
	rect->left = left;
	rect->right = right;
	rect->bottom = bottom;
}

void SetdRectEmpty(dRECT* rect)
{
	rect->top = 0;
	rect->left = 0;
	rect->right = 0;
	rect->bottom = 0;
}

void GetCommandPath(LPWSTR pPath)
{
	USES_CONVERSION;

	LPWSTR cmdLine = A2W(GetCommandLine());
	LPWSTR pCurr;

	if (cmdLine[0] == '"')
	{
		cmdLine++;
		pCurr = wcschr(cmdLine, '"');	//the +1 is because it starts with a quote
		if (pCurr <= 0) //cannot find closing quote
			return;
		pCurr[0] = '\0';

	}
	pCurr = wcsrchr(cmdLine, '\\');
	if (pCurr <= 0)
	{
		pPath[0] = 0;
		return;
	}
	pCurr[1] = '\0';

	wcscpy(pPath, cmdLine);
}

void GetCommandExe(LPWSTR pExe)
{
	USES_CONVERSION;

	LPWSTR cmdLine = A2W(GetCommandLine());
	LPWSTR pCurr;

	if (cmdLine[0] == '"')
	{
		cmdLine++;
		pCurr = wcschr(cmdLine, '"');	//the +1 is because it starts with a quote
		if (pCurr <= 0) //cannot find closing quote
			return;
		pCurr[0] = '\0';

	}
	wcscpy(pExe, cmdLine);
}

long wcsToColor(LPWSTR str)
{
	long result = 0;
	if (str[0] == '#')
		str++;

	int length = wcslen(str);
	if (length <= 0)
		return 0;
		
	for (int i=length-1 ; i>=0 ; i-- )
	{
		if ((str[i] >= '0') && (str[i] <= '9'))
			result += ((str[i]-'0') << ((length-1-i)*4));
		if ((str[i] >= 'A') && (str[i] <= 'F'))
			result += ((str[i]-'A'+10) << ((length-1-i)*4));
	}

	if (length <= 6)
		result = result | 0xFF000000;
	
	return result;
}

void strCopyHex(LPWSTR &dest, long value)
{
	if (dest)
		delete[] dest;

	unsigned long rem;
	long length = 0;
	for ( rem=value; rem>0; rem = rem>>4 )
	{
		length++;
	}
	length += 2; // 0x

	dest = new wchar_t[length + 1];
	dest[length] = 0;
	dest[0] = '0';
	dest[1] = 'x';

	for ( rem=value; rem>0; rem = rem>>4 )
	{
		length--;
		unsigned short digit = (unsigned short)(rem & 0x000F);
		if (digit < 10)
			dest[length] = '0' + digit;
		else
			dest[length] = 'A' + digit - 10;
	}
}

long StringToLong(LPWSTR pValue)
{
	if (pValue[0] == '\0')
		return 0;
	if ((pValue[0] != '0') || (pValue[1] != 'x'))
		return _wtol(pValue);

	int i=2;
	long result = 0;
	long val;
	while ((val = pValue[i++]) != '\0')
	{
		result *= 16;
		val -= '0';
		if (val < 0)
			return 0;
		if (val > 9)
		{
			val += '0' - 'A' + 10;
			if (val < 10)
				return 0;
			if (val > 16)
			{
				val += 'A' - 'a';
				if ((val < 10) || (val > 16))
					return 0;
			}
		}
		result += val;
	}
	return result;
}

