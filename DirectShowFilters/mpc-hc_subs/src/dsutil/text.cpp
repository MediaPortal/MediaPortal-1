/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2014, 2016-2017 see Authors.txt
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
#include "text.h"
#include <memory>

DWORD CharSetToCodePage(DWORD dwCharSet)
{
    if (dwCharSet == CP_UTF8) {
        return CP_UTF8;
    }
    if (dwCharSet == CP_UTF7) {
        return CP_UTF7;
    }
    CHARSETINFO cs;
    ZeroMemory(&cs, sizeof(CHARSETINFO));
    ::TranslateCharsetInfo((DWORD*)(DWORD_PTR)dwCharSet, &cs, TCI_SRCCHARSET);
    return cs.ciACP;
}

std::string ConvertMBCS(std::string str, DWORD SrcCharSet, DWORD DstCharSet)
{
    WCHAR* utf16 = new WCHAR[int(str.size()) + 1];
    ZeroMemory(utf16, (int(str.size()) + 1)*sizeof(WCHAR));

    CHAR* mbcs = new CHAR[int(str.size()) * 6 + 1];
    ZeroMemory(mbcs, int(str.size()) * 6 + 1);

    int len = MultiByteToWideChar(
                  CharSetToCodePage(SrcCharSet),
                  0,
                  str.c_str(),
                  -1, // null terminated string
                  utf16,
                  int(str.size()) + 1);

    len = WideCharToMultiByte(
              CharSetToCodePage(DstCharSet),
              0,
              utf16,
              len,
              mbcs,
              int(str.size()) * 6,
              nullptr,
              nullptr);

    str = mbcs;

    delete [] utf16;
    delete [] mbcs;

    return str;
}

std::string UrlEncode(const std::string& strIn)
{
    std::string strOut;
    DWORD dwLen = DWORD(int(strIn.size())) * 3 + 1;
    const HRESULT hr = UrlEscapeA(strIn.c_str(), StringBuffer(strOut, int(dwLen)), &dwLen, URL_ESCAPE_PERCENT);
    StringRelease(strOut, SUCCEEDED(hr) ? int(dwLen) : 0);
    return strOut;
}

std::string EscapeJSONString(const std::string& str)
{
    std::string escapedString = str;
    // replace all of JSON's reserved characters with their escaped
    // equivalents.
    StringReplace(escapedString, "\"", "\\\"");
    StringReplace(escapedString, "\\", "\\\\");
    StringReplace(escapedString, "/", "\\/");
    StringReplace(escapedString, "\b", "\\b");
    StringReplace(escapedString, "\f", "\\f");
    StringReplace(escapedString, "\n", "\\n");
    StringReplace(escapedString, "\r", "\\r");
    StringReplace(escapedString, "\t", "\\t");
    return escapedString;
}

std::string UrlDecode(const std::string& strIn)
{
    std::string strOut;
    std::string strCopy(strIn);
    DWORD dwLen = DWORD(int(strIn.size())) + 1;
    const HRESULT hr = UrlUnescapeA(StringBuffer(strCopy, 0), StringBuffer(strOut, int(dwLen)), &dwLen, 0);
    StringRelease(strOut, SUCCEEDED(hr) ? int(dwLen) : 0);
    return strOut;
}

std::wstring UrlDecodeWithUTF8(std::wstring in, bool keepEncodedSpecialChar) {
    TCHAR t[100];
    DWORD bufSize = _countof(t);
    tstring tem(in);
    StringReplace(tem, _T("+"), _T(" ")); //UrlUnescape does not deal with '+' properly
    if (keepEncodedSpecialChar) {
        StringReplace(tem, _T("%25"), _T("%2525"));  // %
        StringReplace(tem, _T("%3A"), _T("%253A"));
        StringReplace(tem, _T("%3a"), _T("%253A"));
        StringReplace(tem, _T("%2F"), _T("%252F"));
        StringReplace(tem, _T("%2f"), _T("%252F"));
        StringReplace(tem, _T("%3F"), _T("%253F"));
        StringReplace(tem, _T("%3f"), _T("%253F"));
        StringReplace(tem, _T("%23"), _T("%2523"));
        StringReplace(tem, _T("%5B"), _T("%255B"));
        StringReplace(tem, _T("%5b"), _T("%255B"));
        StringReplace(tem, _T("%5D"), _T("%255D"));
        StringReplace(tem, _T("%5d"), _T("%255D"));
        StringReplace(tem, _T("%40"), _T("%2540"));
        StringReplace(tem, _T("%21"), _T("%2521"));
        StringReplace(tem, _T("%24"), _T("%2524"));
        StringReplace(tem, _T("%26"), _T("%2526"));
        StringReplace(tem, _T("%27"), _T("%2527"));
        StringReplace(tem, _T("%28"), _T("%2528"));
        StringReplace(tem, _T("%29"), _T("%2529"));
        StringReplace(tem, _T("%2A"), _T("%252A"));
        StringReplace(tem, _T("%2a"), _T("%252A"));
        StringReplace(tem, _T("%2B"), _T("%252B"));
        StringReplace(tem, _T("%2b"), _T("%252B"));
        StringReplace(tem, _T("%2C"), _T("%252C"));
        StringReplace(tem, _T("%2c"), _T("%252C"));
        StringReplace(tem, _T("%3B"), _T("%253B"));
        StringReplace(tem, _T("%3b"), _T("%253B"));
        StringReplace(tem, _T("%3D"), _T("%253D"));
        StringReplace(tem, _T("%3d"), _T("%253D"));
        StringReplace(tem, _T("%20"), _T("%2520"));
    }
    HRESULT result = UrlUnescape(StringBuffer(tem, 0), t, &bufSize, URL_ESCAPE_AS_UTF8); //URL_ESCAPE_AS_UTF8 will work as URL_UNESCAPE_AS_UTF8 on windows 8+, otherwise it will just ignore utf-8

    if (result == E_POINTER) {
        std::shared_ptr<TCHAR[]> buffer(new TCHAR[bufSize]);
        if (S_OK == UrlUnescape(StringBuffer(tem, 0), buffer.get(), &bufSize, URL_ESCAPE_AS_UTF8)) {
            tstring urlDecoded(buffer.get());
            return urlDecoded;
        }
    }
    else {
        tstring urlDecoded(t);
        return urlDecoded;
    }
    return in;
}

std::wstring URLGetHostName(const std::wstring in) {
    std::wstring t(in);
    if (StringFind(t, _T("://")) > 1) {
        t = StringMid(t, StringFind(t, _T("://")) + 3);
    }
    if (StringLeft(t, 4) == _T("www.")) {
        t = StringMid(t, 4);
    }
    if (StringFind(t, _T("/")) > 0) {
        t = StringLeft(t, StringFind(t, _T("/")));
    }
    return UrlDecodeWithUTF8(t);
}

std::wstring ShortenURL(const std::wstring url, int targetLength, bool returnHostnameIfTooLong) {
    std::wstring t(url);
    if (StringFind(t, _T("://")) > 1) {
        t = StringMid(t, StringFind(t, _T("://")) + 3);
    }
    if (StringLeft(t, 4) == _T("www.")) {
        t = StringMid(t, 4);
    }
    while (int(t.size()) > targetLength) {
        int position = StringRFind(t, '#');
        if (position > 0) {
            t = StringLeft(t, position);
            continue;
        }
        position = StringRFind(t, '&');
        if (position > 0) {
            t = StringLeft(t, position);
            continue;
        }
        position = StringRFind(t, '?');
        if (position > 0) {
            t = StringLeft(t, position);
            break;
        }
        break;
    }
    t = UrlDecodeWithUTF8(t);
    if (int(t.size()) > targetLength && returnHostnameIfTooLong) return URLGetHostName(url);
    return t;
}

std::string HtmlSpecialChars(std::string str, bool bQuotes /*= false*/)
{
    StringReplace(str, "&", "&amp;");
    StringReplace(str, "\"", "&quot;");
    if (bQuotes) {
        StringReplace(str, "\'", "&#039;");
    }
    StringReplace(str, "<", "&lt;");
    StringReplace(str, ">", "&gt;");

    return str;
}

std::string HtmlSpecialCharsDecode(std::string str)
{
    StringReplace(str, "&amp;", "&");
    StringReplace(str, "&quot;", "\"");
    StringReplace(str, "&#039;", "\'");
    StringReplace(str, "&lt;", "<");
    StringReplace(str, "&gt;", ">");
    StringReplace(str, "&rsquo;", "'");

    return str;
}

std::list<tstring>& MakeLower(std::list<tstring>& sl)
{
    for (tstring& s : sl) {
        StringLower(s);
    }
    return sl;
}

std::list<tstring>& MakeUpper(std::list<tstring>& sl)
{
    for (tstring& s : sl) {
        StringUpper(s);
    }
    return sl;
}

tstring FormatNumber(tstring szNumber, bool bNoFractionalDigits /*= true*/)
{
    tstring ret;

    int nChars = GetNumberFormat(LOCALE_USER_DEFAULT, 0, szNumber.c_str(), nullptr, nullptr, 0);
    GetNumberFormat(LOCALE_USER_DEFAULT, 0, szNumber.c_str(), nullptr, StringBuffer(ret, nChars), nChars);
    StringRelease(ret);

    if (bNoFractionalDigits) {
        TCHAR szNumberFractionalDigits[2] = {0};
        GetLocaleInfo(LOCALE_USER_DEFAULT, LOCALE_IDIGITS, szNumberFractionalDigits, _countof(szNumberFractionalDigits));
        int nNumberFractionalDigits = _tcstol(szNumberFractionalDigits, nullptr, 10);
        if (nNumberFractionalDigits) {
            StringTruncate(ret, int(ret.size()) - nNumberFractionalDigits - 1);
        }
    }

    return ret;
}

void GetLocaleString(LCID lcid, LCTYPE type, tstring& output) {
    int len = GetLocaleInfo(lcid, type, StringBuffer(output, 256), 256);
    StringRelease(output, std::max(len - 1, 0));
}

int LastIndexOfCString(const tstring& text, const tstring& pattern) {
    int found = -1;
    int next_pos = 0;
    while (true) {
        next_pos = StringFind(text, pattern, next_pos);
        if (next_pos > found) {
            found = next_pos;
            next_pos = next_pos + int(pattern.size());
        } else {
            return found;
        }        
    }
}

bool IsNameSimilar(const tstring& title, const tstring& fileName) {
    if (StringFind(fileName, StringLeft(title, 25)) > -1) return true;
    return false;
}
