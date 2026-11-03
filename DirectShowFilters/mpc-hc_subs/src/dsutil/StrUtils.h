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

#include <windows.h>
#include <oleauto.h>
#include <tchar.h>
#include <algorithm>
#include <climits>
#include <cstdarg>
#include <cstdio>
#include <cstring>
#include <cwchar>
#include <cwctype>
#include <string>
#include <utility>
#include <type_traits>

// Strings are std::basic_string. What MFC had as methods of tstring is here as functions that take the string as the first
// argument. The functions work with std::string and std::wstring (and so with tstring), indices are int and -1 means "not found".

typedef std::basic_string<TCHAR> tstring;

namespace StrDetail
{
    template <class T> struct Id { typedef T type; };

    template <class T> struct Chars;

    template <> struct Chars<char> {
        static int vlength(const char* fmt, va_list args) { return _vscprintf(fmt, args); }
        static int vformat(char* buf, size_t n, const char* fmt, va_list args) { return _vsnprintf(buf, n, fmt, args); }
        static int compare(const char* a, const char* b) { return strcmp(a, b); }
        static int compareNoCase(const char* a, const char* b) { return _stricmp(a, b); }
        static bool isSpace(char c) { return isspace(static_cast<unsigned char>(c)) != 0; }
        static char lower(char c) { return static_cast<char>(tolower(static_cast<unsigned char>(c))); }
        static char upper(char c) { return static_cast<char>(toupper(static_cast<unsigned char>(c))); }
        static size_t length(const char* s) { return strlen(s); }
    };

    template <> struct Chars<wchar_t> {
        static int vlength(const wchar_t* fmt, va_list args) { return _vscwprintf(fmt, args); }
        static int vformat(wchar_t* buf, size_t n, const wchar_t* fmt, va_list args) { return _vsnwprintf(buf, n, fmt, args); }
        static int compare(const wchar_t* a, const wchar_t* b) { return wcscmp(a, b); }
        static int compareNoCase(const wchar_t* a, const wchar_t* b) { return _wcsicmp(a, b); }
        static bool isSpace(wchar_t c) { return iswspace(c) != 0; }
        static wchar_t lower(wchar_t c) { return static_cast<wchar_t>(towlower(c)); }
        static wchar_t upper(wchar_t c) { return static_cast<wchar_t>(towupper(c)); }
        static size_t length(const wchar_t* s) { return wcslen(s); }
    };

    inline int Pos(size_t p) { return p == std::string::npos ? -1 : static_cast<int>(p); }
}

// ---- printf into a string ---------------------------------------------------------------------------------------------------

template <class T>
std::basic_string<T> FormatV(const T* fmt, va_list args)
{
    typedef StrDetail::Chars<T> Tr;
    va_list copy;
    va_copy(copy, args);
    const int n = Tr::vlength(fmt, copy);
    va_end(copy);
    if (n <= 0) {
        return std::basic_string<T>();
    }
    std::basic_string<T> s(static_cast<size_t>(n) + 1, T());
    Tr::vformat(&s[0], static_cast<size_t>(n) + 1, fmt, args);
    s.resize(static_cast<size_t>(n));
    return s;
}

// %s takes a pointer to characters, so a string that is passed by mistake would be copied as a structure: refused at compile time
template <class T, class... Args>
std::basic_string<T> Format(const T* fmt, Args... args)
{
    static_assert((!std::is_class<Args>::value && ...), "Format: pass c_str() of a string, not the string");
    auto format = [](const T* f, ...) {
        va_list list;
        va_start(list, f);
        std::basic_string<T> s = FormatV(f, list);
        va_end(list);
        return s;
    };
    return format(fmt, args...);
}

template <class T, class... Args>
std::basic_string<T> Format(const std::basic_string<T>& fmt, Args... args)
{
    return Format(fmt.c_str(), args...);
}

// ---- Left, Right, Mid -------------------------------------------------------------------------------------------------------

template <class T>
std::basic_string<T> StringLeft(const std::basic_string<T>& s, int n)
{
    return n <= 0 ? std::basic_string<T>() : s.substr(0, static_cast<size_t>(n));
}

template <class T>
std::basic_string<T> StringRight(const std::basic_string<T>& s, int n)
{
    if (n <= 0) {
        return std::basic_string<T>();
    }
    return n >= static_cast<int>(s.size()) ? s : s.substr(s.size() - static_cast<size_t>(n));
}

template <class T>
std::basic_string<T> StringMid(const std::basic_string<T>& s, int first, int count = INT_MAX)
{
    if (first < 0) {
        first = 0;
    }
    if (count < 0) {
        count = 0;
    }
    if (first > static_cast<int>(s.size())) {
        return std::basic_string<T>();
    }
    return s.substr(static_cast<size_t>(first), static_cast<size_t>(count));
}

// ---- find -------------------------------------------------------------------------------------------------------------------

template <class T>
int StringFind(const std::basic_string<T>& s, typename StrDetail::Id<T>::type ch, int start = 0)
{
    return StrDetail::Pos(s.find(ch, static_cast<size_t>(start)));
}

template <class T>
int StringFind(const std::basic_string<T>& s, const typename StrDetail::Id<T>::type* sub, int start = 0)
{
    if (start > static_cast<int>(s.size())) {
        return -1;
    }
    return StrDetail::Pos(s.find(sub, static_cast<size_t>(start)));
}

template <class T>
int StringFind(const std::basic_string<T>& s, const std::basic_string<T>& sub, int start = 0)
{
    return StringFind(s, sub.c_str(), start);
}

template <class T>
int StringRFind(const std::basic_string<T>& s, typename StrDetail::Id<T>::type ch)
{
    return StrDetail::Pos(s.rfind(ch));
}

template <class T>
int StringFindOneOf(const std::basic_string<T>& s, const typename StrDetail::Id<T>::type* set)
{
    return StrDetail::Pos(s.find_first_of(set));
}

// ---- change in place --------------------------------------------------------------------------------------------------------

// The number of replaced pieces
template <class T>
int StringReplace(std::basic_string<T>& s, typename StrDetail::Id<T>::type from, typename StrDetail::Id<T>::type to)
{
    if (from == to) {
        return 0;
    }
    int n = 0;
    for (T& c : s) {
        if (c == from) {
            c = to;
            n++;
        }
    }
    return n;
}

template <class T>
int StringReplace(std::basic_string<T>& s, const typename StrDetail::Id<T>::type* from, const typename StrDetail::Id<T>::type* to)
{
    const size_t fl = StrDetail::Chars<T>::length(from), tl = StrDetail::Chars<T>::length(to);
    if (fl == 0) {
        return 0;
    }
    int n = 0;
    size_t p = 0;
    while ((p = s.find(from, p)) != std::basic_string<T>::npos) {
        s.replace(p, fl, to);
        p += tl;
        n++;
    }
    return n;
}

template <class T>
int StringReplace(std::basic_string<T>& s, const std::basic_string<T>& from, const std::basic_string<T>& to)
{
    return StringReplace(s, from.c_str(), to.c_str());
}

template <class T>
int StringReplace(std::basic_string<T>& s, const std::basic_string<T>& from, const typename StrDetail::Id<T>::type* to)
{
    return StringReplace(s, from.c_str(), to);
}

template <class T>
int StringReplace(std::basic_string<T>& s, const typename StrDetail::Id<T>::type* from, const std::basic_string<T>& to)
{
    return StringReplace(s, from, to.c_str());
}

template <class T>
int StringRemove(std::basic_string<T>& s, typename StrDetail::Id<T>::type ch)
{
    const size_t before = s.size();
    s.erase(std::remove(s.begin(), s.end(), ch), s.end());
    return static_cast<int>(before - s.size());
}

template <class T>
void StringInsert(std::basic_string<T>& s, int i, typename StrDetail::Id<T>::type ch)
{
    s.insert(static_cast<size_t>((std::min)((std::max)(i, 0), static_cast<int>(s.size()))), 1, ch);
}

template <class T>
void StringInsert(std::basic_string<T>& s, int i, const typename StrDetail::Id<T>::type* str)
{
    s.insert(static_cast<size_t>((std::min)((std::max)(i, 0), static_cast<int>(s.size()))), str);
}

template <class T>
void StringInsert(std::basic_string<T>& s, int i, const std::basic_string<T>& str)
{
    StringInsert(s, i, str.c_str());
}

template <class T>
void StringDelete(std::basic_string<T>& s, int i, int count = 1)
{
    i = (std::max)(i, 0);
    if (count > 0 && i < static_cast<int>(s.size())) {
        s.erase(static_cast<size_t>(i), static_cast<size_t>(count));
    }
}

template <class T>
void StringTruncate(std::basic_string<T>& s, int n)
{
    if (n < static_cast<int>(s.size())) {
        s.resize(static_cast<size_t>(n));
    }
}

// ---- trim, case -------------------------------------------------------------------------------------------------------------

template <class T>
std::basic_string<T>& StringTrimLeft(std::basic_string<T>& s)
{
    size_t i = 0;
    while (i < s.size() && StrDetail::Chars<T>::isSpace(s[i])) {
        i++;
    }
    s.erase(0, i);
    return s;
}

template <class T>
std::basic_string<T>& StringTrimLeft(std::basic_string<T>& s, typename StrDetail::Id<T>::type ch)
{
    size_t i = 0;
    while (i < s.size() && s[i] == ch) {
        i++;
    }
    s.erase(0, i);
    return s;
}

template <class T>
std::basic_string<T>& StringTrimLeft(std::basic_string<T>& s, const typename StrDetail::Id<T>::type* set)
{
    const size_t i = s.find_first_not_of(set);
    s.erase(0, i == std::basic_string<T>::npos ? s.size() : i);
    return s;
}

template <class T>
std::basic_string<T>& StringTrimRight(std::basic_string<T>& s)
{
    size_t n = s.size();
    while (n > 0 && StrDetail::Chars<T>::isSpace(s[n - 1])) {
        n--;
    }
    s.resize(n);
    return s;
}

template <class T>
std::basic_string<T>& StringTrimRight(std::basic_string<T>& s, typename StrDetail::Id<T>::type ch)
{
    size_t n = s.size();
    while (n > 0 && s[n - 1] == ch) {
        n--;
    }
    s.resize(n);
    return s;
}

template <class T>
std::basic_string<T>& StringTrimRight(std::basic_string<T>& s, const typename StrDetail::Id<T>::type* set)
{
    const size_t i = s.find_last_not_of(set);
    s.resize(i == std::basic_string<T>::npos ? 0 : i + 1);
    return s;
}

template <class T>
std::basic_string<T>& StringTrim(std::basic_string<T>& s)
{
    return StringTrimLeft(StringTrimRight(s));
}

template <class T>
std::basic_string<T>& StringTrim(std::basic_string<T>& s, typename StrDetail::Id<T>::type ch)
{
    return StringTrimLeft(StringTrimRight(s, ch), ch);
}

template <class T>
std::basic_string<T>& StringTrim(std::basic_string<T>& s, const typename StrDetail::Id<T>::type* set)
{
    return StringTrimLeft(StringTrimRight(s, set), set);
}

// Trimming a temporary gives the trimmed copy
template <class T>
std::basic_string<T> StringTrim(std::basic_string<T>&& s)
{
    return std::move(StringTrim(s));
}

template <class T>
std::basic_string<T> StringTrim(std::basic_string<T>&& s, typename StrDetail::Id<T>::type ch)
{
    return std::move(StringTrim(s, ch));
}

template <class T>
std::basic_string<T> StringTrim(std::basic_string<T>&& s, const typename StrDetail::Id<T>::type* set)
{
    return std::move(StringTrim(s, set));
}

template <class T>
std::basic_string<T> StringTrimLeft(std::basic_string<T>&& s)
{
    return std::move(StringTrimLeft(s));
}

template <class T>
std::basic_string<T> StringTrimLeft(std::basic_string<T>&& s, typename StrDetail::Id<T>::type ch)
{
    return std::move(StringTrimLeft(s, ch));
}

template <class T>
std::basic_string<T> StringTrimLeft(std::basic_string<T>&& s, const typename StrDetail::Id<T>::type* set)
{
    return std::move(StringTrimLeft(s, set));
}

template <class T>
std::basic_string<T> StringTrimRight(std::basic_string<T>&& s)
{
    return std::move(StringTrimRight(s));
}

template <class T>
std::basic_string<T> StringTrimRight(std::basic_string<T>&& s, typename StrDetail::Id<T>::type ch)
{
    return std::move(StringTrimRight(s, ch));
}

template <class T>
std::basic_string<T> StringTrimRight(std::basic_string<T>&& s, const typename StrDetail::Id<T>::type* set)
{
    return std::move(StringTrimRight(s, set));
}

// A trimmed copy
template <class T>
std::basic_string<T> StringTrimmed(std::basic_string<T> s)
{
    return StringTrim(s);
}

template <class T>
std::basic_string<T>& StringLower(std::basic_string<T>& s)
{
    for (T& c : s) {
        c = StrDetail::Chars<T>::lower(c);
    }
    return s;
}

template <class T>
std::basic_string<T>& StringUpper(std::basic_string<T>& s)
{
    for (T& c : s) {
        c = StrDetail::Chars<T>::upper(c);
    }
    return s;
}

// A copy in lower or upper case
template <class T>
std::basic_string<T> StringLowered(std::basic_string<T> s)
{
    return StringLower(s);
}

template <class T>
std::basic_string<T> StringUppered(std::basic_string<T> s)
{
    return StringUpper(s);
}

// ---- compare ----------------------------------------------------------------------------------------------------------------

template <class T>
int StringCompare(const std::basic_string<T>& a, const typename StrDetail::Id<T>::type* b)
{
    return StrDetail::Chars<T>::compare(a.c_str(), b);
}

template <class T>
int StringICompare(const std::basic_string<T>& a, const typename StrDetail::Id<T>::type* b)
{
    return StrDetail::Chars<T>::compareNoCase(a.c_str(), b);
}

template <class T>
int StringICompare(const std::basic_string<T>& a, const std::basic_string<T>& b)
{
    return StrDetail::Chars<T>::compareNoCase(a.c_str(), b.c_str());
}

template <class T>
int StringICompare(const T* a, const std::basic_string<T>& b)
{
    return StrDetail::Chars<T>::compareNoCase(a, b.c_str());
}

template <class T>
bool StringIEquals(const std::basic_string<T>& a, const typename StrDetail::Id<T>::type* b)
{
    return StringICompare(a, b) == 0;
}

template <class T>
bool StringIEquals(const std::basic_string<T>& a, const std::basic_string<T>& b)
{
    return StringICompare(a, b) == 0;
}

// ---- pieces -----------------------------------------------------------------------------------------------------------------

// The next piece of s between the delimiters from pos; pos becomes -1 after the last one
template <class T>
std::basic_string<T> StringTokenize(const std::basic_string<T>& s, const typename StrDetail::Id<T>::type* delims, int& pos)
{
    typedef std::basic_string<T> S;
    if (pos < 0) {
        return S();
    }
    const size_t start = s.find_first_not_of(delims, static_cast<size_t>(pos));
    if (start == S::npos) {
        pos = -1;
        return S();
    }
    const size_t end = s.find_first_of(delims, start);
    if (end == S::npos) {
        pos = -1;
        return s.substr(start);
    }
    pos = static_cast<int>(end + 1);
    return s.substr(start, end - start);
}

template <class T>
std::basic_string<T> StringSpanExcluding(const std::basic_string<T>& s, const typename StrDetail::Id<T>::type* set)
{
    return s.substr(0, s.find_first_of(set));
}

template <class T>
std::basic_string<T> StringSpanIncluding(const std::basic_string<T>& s, const typename StrDetail::Id<T>::type* set)
{
    return s.substr(0, s.find_first_not_of(set));
}

// ---- a buffer for the functions that fill a character array ---------------------------------------------------------------

// The string has at least minLength characters; the pointer is for writing, StringRelease() cuts the string at the end that was written
template <class T>
T* StringBuffer(std::basic_string<T>& s, int minLength = 0)
{
    if (minLength > static_cast<int>(s.size())) {
        s.resize(static_cast<size_t>(minLength), T());
    }
    return &s[0];
}

// The length is found by the terminating 0 when newLength is -1
template <class T>
void StringRelease(std::basic_string<T>& s, int newLength = -1)
{
    if (newLength < 0) {
        newLength = static_cast<int>(StrDetail::Chars<T>::length(s.c_str()));
    }
    s.resize(static_cast<size_t>(newLength));
}

// ---- conversions ------------------------------------------------------------------------------------------------------------

inline std::wstring StringWiden(const char* s, UINT codePage = CP_ACP)
{
    const int n = s ? static_cast<int>(strlen(s)) : 0;
    if (n == 0) {
        return std::wstring();
    }
    const int len = MultiByteToWideChar(codePage, 0, s, n, nullptr, 0);
    std::wstring w(static_cast<size_t>(len > 0 ? len : 0), L'\0');
    if (len > 0) {
        MultiByteToWideChar(codePage, 0, s, n, &w[0], len);
    }
    return w;
}

inline std::string StringNarrow(const wchar_t* s, UINT codePage = CP_ACP)
{
    const int n = s ? static_cast<int>(wcslen(s)) : 0;
    if (n == 0) {
        return std::string();
    }
    const int len = WideCharToMultiByte(codePage, 0, s, n, nullptr, 0, nullptr, nullptr);
    std::string a(static_cast<size_t>(len > 0 ? len : 0), '\0');
    if (len > 0) {
        WideCharToMultiByte(codePage, 0, s, n, &a[0], len, nullptr, nullptr);
    }
    return a;
}

inline std::wstring StringWiden(const wchar_t* s) { return s ? s : L""; }
inline std::string StringNarrow(const char* s) { return s ? s : ""; }

#ifdef _UNICODE
inline tstring StringToT(const std::string& s) { return StringWiden(s.c_str()); }
inline tstring StringToT(const std::wstring& s) { return s; }
#else
inline tstring StringToT(const std::string& s) { return s; }
inline tstring StringToT(const std::wstring& s) { return StringNarrow(s.c_str()); }
#endif

inline std::wstring StringToW(const std::wstring& s) { return s; }
inline std::wstring StringToW(const std::string& s) { return StringWiden(s.c_str()); }
inline std::string StringToA(const std::string& s) { return s; }
inline std::string StringToA(const std::wstring& s) { return StringNarrow(s.c_str()); }

#ifdef _UNICODE
inline tstring StringToT(const char* s, UINT codePage = CP_ACP) { return StringWiden(s, codePage); }
inline tstring StringToT(const wchar_t* s) { return s ? s : L""; }
#else
inline tstring StringToT(const char* s) { return s ? s : ""; }
inline tstring StringToT(const wchar_t* s, UINT codePage = CP_ACP) { return StringNarrow(s, codePage); }
#endif

// The string for the functions of COM; free it with SysFreeString
inline BSTR StringAllocSys(const std::wstring& s)
{
    return SysAllocStringLen(s.c_str(), static_cast<UINT>(s.size()));
}

inline BSTR StringAllocSys(const std::string& s)
{
    return StringAllocSys(StringWiden(s.c_str()));
}
