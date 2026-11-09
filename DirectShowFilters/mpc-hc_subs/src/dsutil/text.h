/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2017 see Authors.txt
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

#include <string>

template<class T, typename SEP>
T Explode(const T& str, std::list<T>& sl, SEP sep, size_t limit = 0)
{
    sl.clear();

    for (int i = 0, j = 0; ; i = j + 1) {
        j = StringFind(str, sep, i);

        if (j < 0 || sl.size() == limit - 1) {
            sl.push_back(StringTrimmed(StringMid(str, i)));
            break;
        } else {
            sl.push_back(StringTrimmed(StringMid(str, i, j - i)));
        }
    }

    return sl.front();
}

template<class T, typename SEP>
T ExplodeNoTrim(const T& str, std::list<T>& sl, SEP sep, size_t limit = 0)
{
    sl.clear();

    for (int i = 0, j = 0; ; i = j + 1) {
        j = StringFind(str, sep, i);

        if (j < 0 || sl.size() == limit - 1) {
            sl.push_back(StringMid(str, i));
            break;
        } else {
            sl.push_back(StringMid(str, i, j - i));
        }
    }

    return sl.front();
}

template<class T, typename SEP>
T ExplodeMin(const T& str, std::list<T>& sl, SEP sep, size_t limit = 0)
{
    Explode(str, sl, sep, limit);
    for (auto it = sl.begin(); it != sl.end();) {
        if (it->IsEmpty()) {
            it = sl.erase(it);
        } else {
            ++it;
        }
    }
    if (sl.empty()) {
        sl.push_back(T());    // eh
    }

    return sl.front();
}

template<class T, typename SEP>
T ExplodeEsc(T str, std::list<T>& sl, SEP sep, size_t limit = 0, SEP esc = _T('\\'))
{
    sl.clear();

    int split = 0;
    for (int i = 0, j = 0; ; i = j + 1) {
        j = str.Find(sep, i);
        if (j < 0) {
            break;
        }

        // Skip this separator if it is escaped
        if (j > 0 && str.GetAt(j - 1) == esc) {
            // Delete the escape character
            str.Delete(j - 1);
            continue;
        }

        if (sl.size() < limit - 1) {
            sl.push_back(str.Mid(split, j - split).Trim());

            // Save new splitting position
            split = j + 1;
        }
    }
    sl.push_back(str.Mid(split).Trim());

    return sl.front();
}

template<class T, typename SEP>
T Implode(const std::list<T>& sl, SEP sep)
{
    T ret;
    for (auto it = sl.begin(); it != sl.end(); ++it) {
        ret += *it;
        if (std::next(it) != sl.end()) {
            ret += sep;
        }
    }
    return ret;
}

template<class T, typename SEP>
T ImplodeEsc(const std::list<T>& sl, SEP sep, SEP esc = _T('\\'))
{
    T ret;
    T escsep = T(esc) + T(sep);
    for (auto it = sl.begin(); it != sl.end(); ++it) {
        T str = *it;
        str.Replace(T(sep), escsep);
        ret += str;
        if (std::next(it) != sl.end()) {
            ret += sep;
        }
    }
    return ret;
}

extern std::string ConvertMBCS(std::string str, DWORD SrcCharSet, DWORD DstCharSet);
extern std::string UrlEncode(const std::string& strIn);
/**
 * @brief Escape the characters that JSON reserves as special.
 * @param str The string that needs escaping.
 * @return The input string with the special characters escaped.
 */
extern std::string EscapeJSONString(const std::string& str);
extern std::string UrlDecode(const std::string& strIn);
extern std::wstring UrlDecodeWithUTF8(const std::wstring in, bool keepEncodedSpecialChar = false);
extern std::wstring URLGetHostName(const std::wstring in);
extern std::wstring ShortenURL(const std::wstring url, int targetLength = 100, bool returnHostnameIfTooLong = false);
extern std::string HtmlSpecialChars(std::string str, bool bQuotes = false);
extern std::string HtmlSpecialCharsDecode(std::string str);
extern DWORD CharSetToCodePage(DWORD dwCharSet);
extern std::list<tstring>& MakeLower(std::list<tstring>& sl);
extern std::list<tstring>& MakeUpper(std::list<tstring>& sl);
extern int LastIndexOfCString(const tstring& text, const tstring& pattern);
extern bool IsNameSimilar(const tstring& title, const tstring& fileName);

tstring FormatNumber(tstring szNumber, bool bNoFractionalDigits = true);
void GetLocaleString(LCID lcid, LCTYPE type, tstring& output);

template<class C>
std::basic_string<C>& FastTrimRight(std::basic_string<C>& str)
{
    // the ellipsis character (133) is not taken for a space
    while (!str.empty() && StrDetail::Chars<C>::isSpace(str.back()) && str.back() != 133) {
        str.pop_back();
    }

    return str;
}

template<class C>
std::basic_string<C>& FastTrim(std::basic_string<C>& str)
{
    return StringTrimLeft(FastTrimRight(str));
}

template<class C>
int FindOneOf(const std::basic_string<C>& str, const typename StrDetail::Id<C>::type* pszCharSet, int iStart)
{
    if (iStart < 0 || iStart >= int(str.size())) {
        return -1;
    }

    return StrDetail::Pos(str.find_first_of(pszCharSet, static_cast<size_t>(iStart)));
}

template<typename T>
tstring NumToCString(T num)
{
    static_assert(std::numeric_limits<T>::is_specialized, "NumToCString can be used only for numeric types.");
    return StringToT(std::to_string(num));
}
