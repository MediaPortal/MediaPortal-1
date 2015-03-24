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

#ifndef __CONVERSIONS_DEFINED
#define __CONVERSIONS_DEFINED

#include <stdint.h>

// converts string to unsigned int
// @param input : string to convert
// @param defaultValue : default value
// @return : converted string value or default value if error
unsigned int GetValueUnsignedIntA(const char *input, unsigned int defaultValue);

// converts string to unsigned int
// @param input : string to convert
// @param defaultValue : default value
// @return : converted string value or default value if error
unsigned int GetValueUnsignedIntW(const wchar_t *input, unsigned int defaultValue);

#ifdef _MBCS
#define GetValueUint GetValueUnsignedIntA
#else
#define GetValueUint GetValueUnsignedIntW
#endif

// converts hex string to unsigned int
// @param input : hex string to convert
// @param defaultValue : default value
// @return : converted string value or default value if error
unsigned int GetHexValueUnsignedIntA(const char *input, unsigned int defaultValue);

// converts hex string to unsigned int
// @param input : hex string to convert
// @param defaultValue : default value
// @return : converted string value or default value if error
unsigned int GetHexValueUnsignedIntW(const wchar_t *input, unsigned int defaultValue);

#ifdef _MBCS
#define GetHexValueUint GetHexValueUnsignedIntA
#else
#define GetHexValueUint GetHexValueUnsignedIntW
#endif

// converts string to unsigned int64
// @param input : string to convert
// @param defaultValue : default value
// @return : converted string value or default value if error
uint64_t GetValueUnsignedInt64A(const char *input, uint64_t defaultValue);

// converts string to unsigned int64
// @param input : string to convert
// @param defaultValue : default value
// @return : converted string value or default value if error
uint64_t GetValueUnsignedInt64W(const wchar_t *input, uint64_t defaultValue);

#ifdef _MBCS
#define GetValueUint64 GetValueUnsignedInt64A
#else
#define GetValueUint64 GetValueUnsignedInt64W
#endif

uint8_t HexToDecA(const char c);

uint8_t HexToDecW(const wchar_t c);

uint8_t *HexToDecA(const char *input);

uint8_t *HexToDecW(const wchar_t *input);

#ifdef _MBCS
#define HexToDec HexToDecA
#else
#define HexToDec HexToDecW
#endif

// converts string to double
// @param input : string to convert
// @param defaultValue : default value
// @return : converted string value or default value if error
double GetValueDoubleA(const char *input, double defaultValue);

// converts string to double
// @param input : string to convert
// @param defaultValue : default value
// @return : converted string value or default value if error
double GetValueDoubleW(const wchar_t *input, double defaultValue);

#ifdef _MBCS
#define GetValueDouble GetValueDoubleA
#else
#define GetValueDouble GetValueDoubleW
#endif

#endif