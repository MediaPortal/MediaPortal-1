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

#include "stdafx.h"
#include "STS.h"
#include <algorithm>

#include "RealTextParser.h"
#include <fstream>
#include "USFSubtitles.h"

#include "../DSUtil/PathUtils.h"
#include "../DSUtil/DSMPropertyBag.h"
#include "../DSUtil/DSUtil.h"
#include  <comutil.h>
#include <regex>
#include "SSASub.h"
#include "../dsutil/RegexUtil.h"
#include "../dsutil/SubtitlesProvidersUtils.h"
#include "../DSUtil/ISOLang.h"


struct htmlcolor {
    LPCTSTR name;
    DWORD  color;
}

static constexpr htmlcolors[] = {
    {_T("white"), 0xffffff},
    {_T("whitesmoke"), 0xf5f5f5},
    {_T("ghostwhite"), 0xf8f8ff},
    {_T("snow"), 0xfffafa},
    {_T("gainsboro"), 0xdcdcdc},
    {_T("lightgrey"), 0xd3d3d3},
    {_T("silver"), 0xc0c0c0},
    {_T("darkgray"), 0xa9a9a9},
    {_T("gray"), 0x808080},
    {_T("dimgray"), 0x696969},
    {_T("lightslategray"), 0x778899},
    {_T("slategray"), 0x708090},
    {_T("darkslategray"), 0x2f4f4f},
    {_T("black"), 0x000000},

    {_T("azure"), 0xf0ffff},
    {_T("aliceblue"), 0xf0f8ff},
    {_T("mintcream"), 0xf5fffa},
    {_T("honeydew"), 0xf0fff0},
    {_T("lightcyan"), 0xe0ffff},
    {_T("paleturqoise"), 0xafeeee},
    {_T("powderblue"), 0xb0e0e6},
    {_T("lightblue"), 0xadd8ed},
    {_T("lightsteelblue"), 0xb0c4de},
    {_T("skyblue"), 0x87ceeb},
    {_T("lightskyblue"), 0x87cefa},
    {_T("cyan"), 0x00ffff},
    {_T("aqua"), 0x00ff80},
    {_T("deepskyblue"), 0x00bfff},
    {_T("aquamarine"), 0x7fffd4},
    {_T("turquoise"), 0x40e0d0},
    {_T("darkturquoise"), 0x00ced1},
    {_T("lightseagreen"), 0x20b2aa},
    {_T("mediumturquoise"), 0x40e0dd},
    {_T("mediumaquamarine"), 0x66cdaa},
    {_T("cadetblue"), 0x5f9ea0},
    {_T("teal"), 0x008080},
    {_T("darkcyan"), 0x008b8b},
    {_T("comflowerblue"), 0x6495ed},
    {_T("dodgerblue"), 0x1e90ff},
    {_T("steelblue"), 0x4682b4},
    {_T("royalblue"), 0x4169e1},
    {_T("blue"), 0x0000ff},
    {_T("mediumblue"), 0x0000cd},
    {_T("mediumslateblue"), 0x7b68ee},
    {_T("slateblue"), 0x6a5acd},
    {_T("darkslateblue"), 0x483d8b},
    {_T("darkblue"), 0x00008b},
    {_T("midnightblue"), 0x191970},
    {_T("navy"), 0x000080},

    {_T("palegreen"), 0x98fb98},
    {_T("lightgreen"), 0x90ee90},
    {_T("mediumspringgreen"), 0x00fa9a},
    {_T("springgreen"), 0x00ff7f},
    {_T("chartreuse"), 0x7fff00},
    {_T("lawngreen"), 0x7cfc00},
    {_T("lime"), 0x00ff00},
    {_T("limegreen"), 0x32cd32},
    {_T("greenyellow"), 0xadff2f},
    {_T("yellowgreen"), 0x9acd32},
    {_T("darkseagreen"), 0x8fbc8f},
    {_T("mediumseagreen"), 0x3cb371},
    {_T("seagreen"), 0x2e8b57},
    {_T("olivedrab"), 0x6b8e23},
    {_T("forestgreen"), 0x228b22},
    {_T("green"), 0x008000},
    {_T("darkkhaki"), 0xbdb76b},
    {_T("olive"), 0x808000},
    {_T("darkolivegreen"), 0x556b2f},
    {_T("darkgreen"), 0x006400},

    {_T("floralwhite"), 0xfffaf0},
    {_T("seashell"), 0xfff5ee},
    {_T("ivory"), 0xfffff0},
    {_T("beige"), 0xf5f5dc},
    {_T("cornsilk"), 0xfff8dc},
    {_T("lemonchiffon"), 0xfffacd},
    {_T("lightyellow"), 0xffffe0},
    {_T("lightgoldenrodyellow"), 0xfafad2},
    {_T("papayawhip"), 0xffefd5},
    {_T("blanchedalmond"), 0xffedcd},
    {_T("palegoldenrod"), 0xeee8aa},
    {_T("khaki"), 0xf0eb8c},
    {_T("bisque"), 0xffe4c4},
    {_T("moccasin"), 0xffe4b5},
    {_T("navajowhite"), 0xffdead},
    {_T("peachpuff"), 0xffdab9},
    {_T("yellow"), 0xffff00},
    {_T("gold"), 0xffd700},
    {_T("wheat"), 0xf5deb3},
    {_T("orange"), 0xffa500},
    {_T("darkorange"), 0xff8c00},

    {_T("oldlace"), 0xfdf5e6},
    {_T("linen"), 0xfaf0e6},
    {_T("antiquewhite"), 0xfaebd7},
    {_T("lightsalmon"), 0xffa07a},
    {_T("darksalmon"), 0xe9967a},
    {_T("salmon"), 0xfa8072},
    {_T("lightcoral"), 0xf08080},
    {_T("indianred"), 0xcd5c5c},
    {_T("coral"), 0xff7f50},
    {_T("tomato"), 0xff6347},
    {_T("orangered"), 0xff4500},
    {_T("red"), 0xff0000},
    {_T("crimson"), 0xdc143c},
    {_T("firebrick"), 0xb22222},
    {_T("maroon"), 0x800000},
    {_T("darkred"), 0x8b0000},

    {_T("lavender"), 0xe6e6fe},
    {_T("lavenderblush"), 0xfff0f5},
    {_T("mistyrose"), 0xffe4e1},
    {_T("thistle"), 0xd8bfd8},
    {_T("pink"), 0xffc0cb},
    {_T("lightpink"), 0xffb6c1},
    {_T("palevioletred"), 0xdb7093},
    {_T("hotpink"), 0xff69b4},
    {_T("fuchsia"), 0xff00ee},
    {_T("magenta"), 0xff00ff},
    {_T("mediumvioletred"), 0xc71585},
    {_T("deeppink"), 0xff1493},
    {_T("plum"), 0xdda0dd},
    {_T("violet"), 0xee82ee},
    {_T("orchid"), 0xda70d6},
    {_T("mediumorchid"), 0xba55d3},
    {_T("mediumpurple"), 0x9370db},
    {_T("purple"), 0x9370db},
    {_T("blueviolet"), 0x8a2be2},
    {_T("darkviolet"), 0x9400d3},
    {_T("darkorchid"), 0x9932cc},

    {_T("tan"), 0xd2b48c},
    {_T("burlywood"), 0xdeb887},
    {_T("sandybrown"), 0xf4a460},
    {_T("peru"), 0xcd853f},
    {_T("goldenrod"), 0xdaa520},
    {_T("darkgoldenrod"), 0xb8860b},
    {_T("chocolate"), 0xd2691e},
    {_T("rosybrown"), 0xbc8f8f},
    {_T("sienna"), 0xa0522d},
    {_T("saddlebrown"), 0x8b4513},
    {_T("brown"), 0xa52a2a},
};

CHtmlColorMap::CHtmlColorMap()
{
    for (size_t i = 0; i < _countof(htmlcolors); i++) {
        (*this)[htmlcolors[i].name] = htmlcolors[i].color;
    }
}

const CHtmlColorMap g_colors;

static std::wstring SSAColorTag(std::wstring arg, std::wstring ctag = L"c") {
    DWORD val, color;
    if (Lookup(g_colors, tstring(arg), val)) {
        color = (DWORD)val;
    } else if ((color = wcstol(arg.c_str(), nullptr, 16)) == 0) {
        color = 0x00ffffff;    // default is white
    }
    std::wstring tmp;
    tmp = ::Format(L"%02x%02x%02x", color & 0xff, (color >> 8) & 0xff, (color >> 16) & 0xff);
    return std::wstring(L"{\\" + ctag + L"&H") + tmp + L"&}";
}

static std::wstring SSAColorTagCS(std::wstring arg, std::wstring ctag = L"c") {
    std::wstring _arg(arg.c_str());
    return SSAColorTag(_arg, ctag).c_str();
}

//

const BYTE CharSetList[] = {
    ANSI_CHARSET,
    DEFAULT_CHARSET,
    SYMBOL_CHARSET,
    SHIFTJIS_CHARSET,
    HANGEUL_CHARSET,
    HANGUL_CHARSET,
    GB2312_CHARSET,
    CHINESEBIG5_CHARSET,
    OEM_CHARSET,
    JOHAB_CHARSET,
    HEBREW_CHARSET,
    ARABIC_CHARSET,
    GREEK_CHARSET,
    TURKISH_CHARSET,
    VIETNAMESE_CHARSET,
    THAI_CHARSET,
    EASTEUROPE_CHARSET,
    RUSSIAN_CHARSET,
    MAC_CHARSET,
    BALTIC_CHARSET
};

const TCHAR* CharSetNames[] = {
    _T("ANSI"),
    _T("DEFAULT"),
    _T("SYMBOL"),
    _T("SHIFTJIS"),
    _T("HANGEUL"),
    _T("HANGUL"),
    _T("GB2312"),
    _T("CHINESEBIG5"),
    _T("OEM"),
    _T("JOHAB"),
    _T("HEBREW"),
    _T("ARABIC"),
    _T("GREEK"),
    _T("TURKISH"),
    _T("VIETNAMESE"),
    _T("THAI"),
    _T("EASTEUROPE"),
    _T("RUSSIAN"),
    _T("MAC"),
    _T("BALTIC"),
};

const int CharSetLen = _countof(CharSetList);

//

static size_t CountLines(CTextFile* f, ULONGLONG from, ULONGLONG to, tstring s = _T("")) {
    size_t n = 0;
    f->Seek(from, CTextFile::begin);
    while (f->ReadString(s) && f->GetPosition() < to) {
        n++;
    }
    return n;
}

static int FindChar(std::wstring str, WCHAR c, int pos, bool fUnicode, int CharSet)
{
    if (fUnicode) {
        return StringFind(str, c, pos);
    }

    int fStyleMod = 0;

    DWORD cp = CharSetToCodePage(CharSet);
    int OrgCharSet = CharSet;

    for (int i = 0, j = int(str.size()), k; i < j; i++) {
        WCHAR c2 = str[i];

        if (IsDBCSLeadByteEx(cp, (BYTE)c2)) {
            i++;
        } else if (i >= pos) {
            if (c2 == c) {
                return i;
            }
        }

        if (c2 == '{') {
            fStyleMod++;
        } else if (fStyleMod > 0) {
            if (c2 == '}') {
                fStyleMod--;
            } else if (c2 == 'e' && i >= 3 && i < j - 1 && StringMid(str, i - 2, 3) == L"\\fe") {
                CharSet = 0;
                for (k = i + 1; _istdigit(str[k]); k++) {
                    CharSet = CharSet * 10 + (str[k] - '0');
                }
                if (k == i + 1) {
                    CharSet = OrgCharSet;
                }

                cp = CharSetToCodePage(CharSet);
            }
        }
    }

    return -1;
}

static std::wstring ToMBCS(std::wstring str, DWORD CharSet)
{
    std::wstring ret;

    DWORD cp = CharSetToCodePage(CharSet);

    for (int i = 0, j = int(str.size()); i < j; i++) {
        WCHAR wc = str[i];
        char c[8];

        int len;
        if ((len = WideCharToMultiByte(cp, 0, &wc, 1, c, 8, nullptr, nullptr)) > 0) {
            for (ptrdiff_t k = 0; k < len; k++) {
                ret += (WCHAR)(BYTE)c[k];
            }
        } else {
            ret += L'?';
        }
    }

    return ret;
}

static std::wstring UnicodeSSAToMBCS(std::wstring str, DWORD CharSet)
{
    std::wstring ret;
    int OrgCharSet = CharSet;

    for (int j = 0; j < int(str.size());) {
        j = StringFind(str, '{', j);
        if (j >= 0) {
            ret += ToMBCS(StringLeft(str, j), CharSet);
            str = StringMid(str, j);

            j = StringFind(str, '}');
            if (j < 0) {
                ret += ToMBCS(str, CharSet);
                break;
            } else {
                int k = StringFind(str, L"\\fe");
                if (k >= 0 && k < j) {
                    CharSet = 0;
                    int l = k + 3;
                    for (; _istdigit(str[l]); l++) {
                        CharSet = CharSet * 10 + (str[l] - '0');
                    }
                    if (l == k + 3) {
                        CharSet = OrgCharSet;
                    }
                }

                j++;

                ret += ToMBCS(StringLeft(str, j), OrgCharSet);
                str = StringMid(str, j);
                j = 0;
            }
        } else {
            ret += ToMBCS(str, CharSet);
            break;
        }
    }

    return ret;
}

static std::wstring ToUnicode(std::wstring str, DWORD CharSet)
{
    std::wstring ret;
    DWORD cp = CharSetToCodePage(CharSet);

    for (int i = 0, j = int(str.size()); i < j; i++) {
        WCHAR wc = str[i];
        char c = wc & 0xff;

        if (IsDBCSLeadByteEx(cp, (BYTE)wc)) {
            i++;

            if (i < j) {
                char cc[2];
                cc[0] = c;
                cc[1] = (char)str[i];

                MultiByteToWideChar(cp, 0, cc, 2, &wc, 1);
            }
        } else {
            MultiByteToWideChar(cp, 0, &c, 1, &wc, 1);
        }

        ret += wc;
    }

    return ret;
}

static std::wstring MBCSSSAToUnicode(std::wstring str, int CharSet)
{
    std::wstring ret;
    int OrgCharSet = CharSet;

    for (int j = 0; j < int(str.size());) {
        j = FindChar(str, '{', 0, false, CharSet);

        if (j >= 0) {
            ret += ToUnicode(StringLeft(str, j), CharSet);
            str = StringMid(str, j);

            j = FindChar(str, '}', 0, false, CharSet);

            if (j < 0) {
                ret += ToUnicode(str, CharSet);
                break;
            } else {
                int k = StringFind(str, L"\\fe");
                if (k >= 0 && k < j) {
                    CharSet = 0;
                    int l = k + 3;
                    for (; _istdigit(str[l]); l++) {
                        CharSet = CharSet * 10 + (str[l] - '0');
                    }
                    if (l == k + 3) {
                        CharSet = OrgCharSet;
                    }
                }

                j++;

                ret += ToUnicode(StringLeft(str, j), OrgCharSet);
                str = StringMid(str, j);
                j = 0;
            }
        } else {
            ret += ToUnicode(str, CharSet);
            break;
        }
    }

    return ret;
}

static std::wstring RemoveSSATags(std::wstring str, bool fUnicode, int CharSet)
{
    StringReplace(str, L"{\\i1}", L"<i>");
    StringReplace(str, L"{\\i}", L"</i>");

    for (int i = 0, j; i < int(str.size());) {
        if ((i = FindChar(str, '{', i, fUnicode, CharSet)) < 0) {
            break;
        }
        if ((j = FindChar(str, '}', i, fUnicode, CharSet)) < 0) {
            break;
        }
        StringDelete(str, i, j - i + 1);
    }

    StringReplace(str, L"\\N", L"\n");
    StringReplace(str, L"\\n", L"\n");
    StringReplace(str, L"\\h", L" ");

    return str;
}

//

static std::wstring SubRipper2SSA(std::wstring str)
{
    if (StringFind(str, L'<') >= 0) {
        StringReplace(str, L"<i>", L"{\\i1}");
        StringReplace(str, L"</i>", L"{\\i}");
        StringReplace(str, L"<b>", L"{\\b1}");
        StringReplace(str, L"</b>", L"{\\b}");
        StringReplace(str, L"<u>", L"{\\u1}");
        StringReplace(str, L"</u>", L"{\\u}");
    }
    return str;
}

static void WebVTTCueStrip(std::wstring& str)
{
    int p = StringFind(str, L'\n');
    if (p > 0) {
        if (StringLeft(str, 6) == _T("align:") || StringLeft(str, 9) == _T("position:") || StringLeft(str, 9) == _T("vertical:") || StringLeft(str, 5) == _T("line:") || StringLeft(str, 5) == _T("size:")) {
            StringDelete(str, 0, p);
            StringTrimLeft(str);
        }
    }
}

using WebVTTcolorData = struct _WebVTTcolorData { std::wstring color; std::wstring bg; bool applied = false; };
using WebVTTcolorMap = std::map<std::wstring, WebVTTcolorData>;

static void WebVTT2SSA(std::wstring& str, std::wstring& cueTags, WebVTTcolorMap clrMap)
{

    std::vector<WebVTTcolorData> styleStack;
    auto applyStyle = [&styleStack, &str](std::wstring clr, std::wstring bg, int endTag, bool restoring=false) {
        std::wstring tags = L"";
        WebVTTcolorData previous;
        bool applied = false;
        if (styleStack.size() > 0 && !restoring) {
            auto tmp = styleStack.back();
            if (tmp.applied) {
                previous = tmp;
            }
        }
        if (clr != L"" && clr != previous.color) {
            tags += SSAColorTagCS(clr);
        }
        if (bg != L"" && bg != previous.bg) {
            tags += SSAColorTagCS(bg, L"3c");
        }
        if (tags.length() > 0) {
            if (-1 == endTag) {
                str = tags.c_str() + str;
                applied = true;
            } else if (StringMid(str, endTag + 1, 1) != L"<") { //if we are about to open or close a tag, don't set the style yet, as it may change before formattable text arrives
                str = StringLeft(str, endTag + 1) + tags.c_str() + StringMid(str, endTag + 1);
                applied = true;
            }
        }
        if (!restoring) {
            styleStack.push_back({ clr, bg, applied }); //push current colors for restoring
        }
    };

    std::wstring clr = L"", bg = L"";
    if (clrMap.count(L"::cue")) { //default cue style
        WebVTTcolorData colorData = clrMap[L"::cue"];
        clr = colorData.color;
        bg = colorData.bg;
        applyStyle(clr, bg, -1);
    }

    int tagPos = StringFind(str, L"<");
    while (tagPos != std::wstring::npos) {
        int endTag = StringFind(str, L">", tagPos);
        if (endTag == std::wstring::npos) break;
        std::wstring inner = StringMid(str, tagPos + 1, endTag - tagPos - 1);
        if (StringFind(inner, L"/") == 0) { //close tag
            if (styleStack.size()>0) {//should always be true, unless poorly matched close tags in source
                styleStack.pop_back();
            }
            if (styleStack.size() > 0) {
                auto restoreStyle = styleStack[styleStack.size() - 1];
                clr = restoreStyle.color;
                bg = restoreStyle.bg;
                applyStyle(clr, bg, endTag, true);
            } else { //reset default style
                if (endTag + 1 != int(str.size())) {
                    str = StringLeft(str, endTag + 1) + L"{\\r}" + StringMid(str, endTag + 1);
                }
                clr = L"";
                bg = L"";
            }
            tagPos = StringFind(str, L"<", endTag);
            continue;
        }

        int dotPos = StringFind(inner, L".");
        if (dotPos == std::wstring::npos) {//it's a simple tag, so we can apply a single style to it, if it exists
            if (clrMap.count(inner.c_str())) {
                WebVTTcolorData colorData = clrMap[inner.c_str()];
                clr = colorData.color;
                bg = colorData.bg;
            }
        } else { //could find multiple classes 
            RegexUtil::wregexResults results;
            std::wregex clsPattern(LR"((\.?[^\.]+))");
            RegexUtil::wstringMatch(clsPattern, inner.c_str(), results);
            if (results.size() > 1) {
                std::wstring type = results[0][0];

                for (auto iter = results.begin()+1; iter != results.end(); ++iter) { //loop through all classes--whichever is last gets precedence
                    std::wstring cls = (*iter)[0];
                    WebVTTcolorData colorData;
                    if (clrMap.count(type + cls)) {
                        colorData = clrMap[type + cls];
                    } else if (clrMap.count(cls)) {
                        colorData = clrMap[cls];
                    }
                    if (colorData.color != L"") {
                        clr = colorData.color;
                    }
                    if (colorData.bg != L"") {
                        bg = colorData.bg;
                    }
                }
            }
        }

        applyStyle(clr, bg, endTag);
        tagPos = StringFind(str, L"<", endTag);
    }

    if (StringFind(str, L'<') >= 0) {
        StringReplace(str, L"<i>", L"{\\i1}");
        StringReplace(str, L"</i>", L"{\\i}");
        StringReplace(str, L"<b>", L"{\\b1}");
        StringReplace(str, L"</b>", L"{\\b}");
        StringReplace(str, L"<u>", L"{\\u1}");
        StringReplace(str, L"</u>", L"{\\u}");
    }

    if (StringFind(str, L'<') >= 0) {
        std::wstring stdTmp(str);

        // remove tags we don't support
        stdTmp = std::regex_replace(stdTmp, std::wregex(L"<c[.\\w\\d]*>"), L"");
        stdTmp = std::regex_replace(stdTmp, std::wregex(L"</c[.\\w\\d]*>"), L"");
        stdTmp = std::regex_replace(stdTmp, std::wregex(L"<\\d\\d:\\d\\d:\\d\\d.\\d\\d\\d>"), L"");
        stdTmp = std::regex_replace(stdTmp, std::wregex(L"<v[ .][^>]*>"), L"");
        stdTmp = std::regex_replace(stdTmp, std::wregex(L"</v>"), L"");
        stdTmp = std::regex_replace(stdTmp, std::wregex(L"<lang[^>]*>"), L"");
        stdTmp = std::regex_replace(stdTmp, std::wregex(L"</lang>"), L"");
        str = stdTmp.c_str();
    }
    if (StringFind(str, L'&') >= 0) {
        StringReplace(str, L"&lt;", L"<");
        StringReplace(str, L"&gt;", L">");
        StringReplace(str, L"&nbsp;", L"\\h");
        StringReplace(str, L"&lrm;", L"");
        StringReplace(str, L"&rlm;", L"");
        StringReplace(str, L"&amp;", L"&");
    }

    if (!cueTags.empty()) {
        std::wstring stdTmp(cueTags);
        std::wregex alignRegex(L"align:(start|left|center|middle|end|right)");
        std::wsmatch match;

        if (std::regex_search(stdTmp, match, alignRegex)) {
            if (match[1] == L"start" || match[1] == L"left") {
                str = L"{\\an1}" + str;
            } else if (match[1] == L"center" || match[1] == L"middle") {
                str = L"{\\an2}" + str;
            } else {
                str = L"{\\an3}" + str;
            }
        }
    }
}

static void WebVTT2SSA(std::wstring& str) {
    std::wstring discard;
    WebVTTcolorMap discardMap;
    WebVTT2SSA(str, discard, discardMap);
}

static bool OpenVTT(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet) {
    std::wstring buff;
    file->ReadString(buff);
    if (StringCompare(StringLeft(buff, 6), L"WEBVTT") != 0) {
        return false;
    }

    auto readTimeCode = [](LPCWSTR str, int& hh, int& mm, int& ss, int& ms) {
        WCHAR sep;
        int c = swscanf_s(str, L"%d%c%d%c%d%c%d",
            &hh, &sep, 1, &mm, &sep, 1, &ss, &sep, 1, &ms);
        if (c == 5) {
            // Hours value is absent, shift read values
            ms = ss;
            ss = mm;
            mm = hh;
            hh = 0;
        }
        return (c == 5 || c == 7);
    };

    //default cue color classes: https://w3c.github.io/webvtt/#default-text-color
    WebVTTcolorMap cueColors = {
        {L".white", WebVTTcolorData({L"ffffff", L""})},
        {L".lime", WebVTTcolorData({L"00ff00", L""})},
        {L".cyan", WebVTTcolorData({L"00ffff", L""})},
        {L".red", WebVTTcolorData({L"ff0000", L""})},
        {L".yellow", WebVTTcolorData({L"ffff00", L""})},
        {L".magenta", WebVTTcolorData({L"ff00ff", L""})},
        {L".blue", WebVTTcolorData({L"0000ff", L""})},
        {L".black", WebVTTcolorData({L"000000", L""})},
        {L".bg_white", WebVTTcolorData({L"", L"ffffff"})},
        {L".bg_lime", WebVTTcolorData({L"", L"00ff00"})},
        {L".bg_cyan", WebVTTcolorData({L"", L"00ffff"})},
        {L".bg_red", WebVTTcolorData({L"", L"ff0000"})},
        {L".bg_yellow", WebVTTcolorData({L"", L"ffff00"})},
        {L".bg_magenta", WebVTTcolorData({L"", L"ff00ff"})},
        {L".bg_blue", WebVTTcolorData({L"", L"0000ff"})},
        {L".bg_black", WebVTTcolorData({L"", L"000000"})},
    };

    std::wstring start, end, cueTags;

    auto parseStyle = [&file,&cueColors](std::wstring& buff) {
        std::wstring styleStr = L"";
        while (file->ReadString(buff)) {
            if (StringFind(buff, L"-->") != -1) { //not allowed in style block, so we drop out to cue parsing below
                FastTrimRight(buff);
                break;
            }
            if (buff.empty()) { //empty line not allowed in style block, drop out
                break;
            }
            styleStr += L" "+buff;
        }

        int startComment = StringFind(styleStr, L"/*");
        while (startComment != -1) { //remove comments
            int endComment = StringFind(styleStr, L"*/", startComment + 2);
            if (endComment == -1) {
                endComment = int(styleStr.size())-1;
            }
            StringDelete(styleStr, startComment, endComment - startComment + 1);
            startComment = StringFind(styleStr, L"/*");
        }

        if (!styleStr.empty()) {
            auto parseColor = [](std::wstring styles, std::wstring attr = L"color") {
                //we only support color styles for now
                std::wregex clrPat(LR"(^\s*)" + attr + LR"(\s*:\s*#?([a-zA-Z0-9]*)\s*;)"); //e.g., 0xffffff or white
                std::wregex rgbPat(LR"(^\s*)" + attr + LR"(\s*:\s*rgb\s*\(\s*([0-9]+)\s*,\s*([0-9]+)\s*,\s*([0-9]+)\s*\)\s*;)");
                std::wsmatch match;
                std::wstring clrStr = L"";
                if (std::regex_search(styles, match, clrPat)) {
                    clrStr = match[1];
                } else if (std::regex_search(styles, match, rgbPat)) {
                    int r = stoi(match[1]) & 0xff;
                    int g = stoi(match[2]) & 0xff;
                    int b = stoi(match[3]) & 0xff;
                    DWORD clr = (r << 16) + (g << 8) + b;
                    std::wstringstream hexClr;
                    hexClr << std::hex << clr;
                    clrStr = hexClr.str();
                }
                return clrStr;
            };

            RegexUtil::wregexResults results;
            std::wregex cueDefPattern(LR"(::cue\s*\{([^}]*)\})"); //default cue style
            RegexUtil::wstringMatch(cueDefPattern, styleStr.c_str(), results);
            if (results.size() > 0) {
                auto iter = results[results.size() - 1];
                std::wstring clr, bgClr;
                clr = parseColor(iter[0]);
                bgClr = parseColor(iter[0], L"background-color");
                if (bgClr == L"") {
                    bgClr = parseColor(iter[0], L"background");
                }
                if (clr != L"" || bgClr != L"") {
                    cueColors[L"::cue"] = WebVTTcolorData({ clr, bgClr });
                }
            }

            std::wregex cuePattern(LR"(::cue\(([^)]+)\)\s*\{([^}]*)\})");
            RegexUtil::wstringMatch(cuePattern, styleStr.c_str(), results);
            for (const auto& iter : results) {
                std::wstring clr, bgClr;
                clr=parseColor(iter[1]);
                bgClr=parseColor(iter[1], L"background-color");
                if (bgClr == L"") {
                    bgClr = parseColor(iter[1], L"background");
                }
                if (clr != L"" || bgClr != L"") {
                    cueColors[iter[0]] = WebVTTcolorData({ clr, bgClr });
                }
            }
        }
    };

    std::wstring lastStr, lastBuff;
    bool foundFirstCue = false;
    while (file->ReadString(buff)) {
        FastTrimRight(buff);
        if (!foundFirstCue && !buff.empty()) { //STYLE blocks cannot show up after cues begin
            if (buff == L"STYLE" || buff==L"Style:" /*have seen webvtt with incorrect format using 'Style:' instead of 'STYLE'*/ ) {
                parseStyle(buff); //note that buff will contain next line when done, so we can still use it below
            }
        }
        if (buff.empty()) {
            continue;
        }

        int len = int(buff.size());
        cueTags = L"";
        int c = swscanf_s(buff.c_str(), L"%s --> %s %[^\n]s", StringBuffer(start, len), len, StringBuffer(end, len), len, StringBuffer(cueTags, len), len);
        StringRelease(start);
        StringRelease(end);
        StringRelease(cueTags);

        int hh1, mm1, ss1, ms1, hh2, mm2, ss2, ms2;

        if ((c == 2 || c == 3) //either start/end or start/end/cuetags
            && readTimeCode(start.c_str(), hh1, mm1, ss1, ms1)
            && readTimeCode(end.c_str(), hh2, mm2, ss2, ms2)) {
            foundFirstCue = true;

            std::wstring str, tmp;

            while (file->ReadString(tmp)) {
                FastTrimRight(tmp);
                if (tmp.empty()) {
                    break;
                }
                WebVTT2SSA(tmp, cueTags, cueColors);
                str += tmp + L'\n';
            }

            if (lastStr != str || lastBuff != buff) { //discard repeated subs
                ret.Add(str,
                    file->IsUnicode(),
                    MS2RT((((hh1 * 60i64 + mm1) * 60i64) + ss1) * 1000i64 + ms1),
                    MS2RT((((hh2 * 60i64 + mm2) * 60i64) + ss2) * 1000i64 + ms2));
            }

            lastStr = str;
            lastBuff = buff;
        } else {
            continue;
        }
    }

    // in case of embedded data, we initially might only get the header, so always return true
    return true;
}


static bool OpenSubRipper(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet)
{
    std::wstring buff, start, end;
    while (file->ReadString(buff)) {
        FastTrimRight(buff);
        if (buff.empty()) {
            continue;
        }

        int num = 0; // This one isn't really used just assigned a new value

        WCHAR wc;
        int c = swscanf_s(buff.c_str(), L"%d%c", &num, &wc, 1);

        if (c == 2) { // c == 1 would be numbering, c == 2 might be timecodes
            int len = int(buff.size());
            c = swscanf_s(buff.c_str(), L"%s --> %s", StringBuffer(start, len), len, StringBuffer(end, len), len);
            StringRelease(start);
            StringRelease(end);

            auto readTimeCode = [](LPCWSTR str, int& hh, int& mm, int& ss, int& ms) {
                WCHAR sep;
                int c = swscanf_s(str, L"%d%c%d%c%d%c%d",
                                  &hh, &sep, 1, &mm, &sep, 1, &ss, &sep, 1, &ms);
                // Check if ms was present
                if (c == 5) {
                    ms = 0;
                }
                return (c == 5 || c == 7);
            };

            int hh1, mm1, ss1, ms1, hh2, mm2, ss2, ms2;

            if (c == 2
                    && readTimeCode(start.c_str(), hh1, mm1, ss1, ms1)
                    && readTimeCode(end.c_str(), hh2, mm2, ss2, ms2)) {
                std::wstring str, tmp;

                bool bFoundEmpty = false;

                while (file->ReadString(tmp)) {
                    FastTrimRight(tmp);
                    if (tmp.empty()) {
                        bFoundEmpty = true;
                    }

                    int num2;
                    if (swscanf_s(tmp.c_str(), L"%d%c", &num2, &wc, 1) == 1 && bFoundEmpty) {
                        num = num2;
                        break;
                    }

                    str += tmp + L'\n';
                }

                ret.Add(SubRipper2SSA(str),
                        file->IsUnicode(),
                        MS2RT((((hh1 * 60i64 + mm1) * 60i64) + ss1) * 1000i64 + ms1),
                        MS2RT((((hh2 * 60i64 + mm2) * 60i64) + ss2) * 1000i64 + ms2));
            } else {
                return false;
            }
        } else if (c != 1) { // might be another format
            return false;
        }
    }

    return !ret.empty();
}

static bool OpenOldSubRipper(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet)
{
    std::wstring buff;
    while (file->ReadString(buff)) {
        FastTrim(buff);
        if (buff.empty()) {
            continue;
        }

        for (int i = 0; i < int(buff.size()); i++) {
            if ((i = FindChar(buff, '|', i, file->IsUnicode(), CharSet)) < 0) {
                break;
            }
            buff[i] = '\n';
        }

        int hh1, mm1, ss1, hh2, mm2, ss2;
        int c = swscanf_s(buff.c_str(), L"{%d:%d:%d}{%d:%d:%d}", &hh1, &mm1, &ss1, &hh2, &mm2, &ss2);

        if (c == 6) {
            ret.Add(
                StringMid(buff, StringFind(buff, '}', StringFind(buff, '}') + 1) + 1),
                file->IsUnicode(),
                MS2RT((((hh1 * 60i64 + mm1) * 60i64) + ss1) * 1000i64),
                MS2RT((((hh2 * 60i64 + mm2) * 60i64) + ss2) * 1000i64));
        } else if (c != EOF) { // might be another format
            return false;
        }
    }

    return !ret.empty();
}

static bool OpenSubViewer(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet)
{
    STSStyle def;
    std::wstring font, color, size;
    bool fBold = false;
    bool fItalic = false;
    bool fStriked = false;
    bool fUnderline = false;
    std::wstring buff;

    while (file->ReadString(buff)) {
        FastTrim(buff);
        if (buff.empty()) {
            continue;
        }

        if (buff[0] == '[') {
            for (int i = 0; i < int(buff.size()) && buff[i] == '[';) {
                int j = StringFind(buff, ']', ++i);
                if (j < i) {
                    break;
                }

                std::wstring tag = StringMid(buff, i, j - i);
                FastTrim(tag);
                StringLower(tag);

                i += j - i;

                j = StringFind(buff, '[', ++i);
                if (j < 0) {
                    j = int(buff.size());
                }

                std::wstring param = StringMid(buff, i, j - i);
                StringTrim(param, L" \\t,");

                i = j;

                if (tag == L"font") {
                    font = StringICompare(def.fontName, WToT(param)) ? param.c_str() : L"";
                } else if (tag == L"colf") {
                    color = def.colors[0] != (DWORD)wcstol((param.c_str()) + 2, 0, 16) ? param.c_str() : L"";
                } else if (tag == L"size") {
                    size = def.fontSize != (double)wcstol(param.c_str(), 0, 10) ? param.c_str() : L"";
                } else if (tag == L"style") {
                    if (StringFind(param, L"no") >= 0) {
                        fBold = fItalic = fStriked = fUnderline = false;
                    } else {
                        fBold = def.fontWeight < FW_BOLD && StringFind(param, L"bd") >= 0;
                        fItalic = def.fItalic && StringFind(param, L"it") >= 0;
                        fStriked = def.fStrikeOut && StringFind(param, L"st") >= 0;
                        fUnderline = def.fUnderline && StringFind(param, L"ud") >= 0;
                    }
                }
            }

            continue;
        }

        WCHAR sep;
        int hh1, mm1, ss1, hs1, hh2, mm2, ss2, hs2;
        int c = swscanf_s(buff.c_str(), L"%d:%d:%d%c%d,%d:%d:%d%c%d\n",
                          &hh1, &mm1, &ss1, &sep, 1,
                          &hs1, &hh2, &mm2, &ss2, &sep, 1, &hs2);

        if (c == 10) {
            std::wstring str;
            VERIFY(file->ReadString(str));

            StringReplace(str, L"[br]", L"\\N");

            std::wstring prefix;
            if (!font.empty()) {
                prefix += L"\\fn" + font;
            }
            if (!color.empty()) {
                prefix += L"\\c" + color;
            }
            if (!size.empty()) {
                prefix += L"\\fs" + size;
            }
            if (fBold) {
                prefix += L"\\b1";
            }
            if (fItalic) {
                prefix += L"\\i1";
            }
            if (fStriked) {
                prefix += L"\\s1";
            }
            if (fUnderline) {
                prefix += L"\\u1";
            }
            if (!prefix.empty()) {
                str = L"{" + prefix + L"}" + str;
            }

            ret.Add(str,
                    file->IsUnicode(),
                    MS2RT((((hh1 * 60i64 + mm1) * 60i64) + ss1) * 1000i64 + hs1 * 10i64),
                    MS2RT((((hh2 * 60i64 + mm2) * 60i64) + ss2) * 1000i64 + hs2 * 10i64));
        } else if (c != EOF) { // might be another format
            return false;
        }
    }

    return !ret.empty();
}

static STSStyle* GetMicroDVDStyle(tstring str, int CharSet)
{
    STSStyle* ret = new STSStyle();
    if (!ret) {
        return nullptr;
    }

    for (int i = 0, len = int(str.size()); i < len; i++) {
        int j = StringFind(str, '{', i);
        if (j < 0) {
            j = len;
        }

        if (j >= len) {
            break;
        }

        int k = StringFind(str, '}', j);
        if (k < 0) {
            k = len;
        }

        tstring code = StringMid(str, j, k - j);
        if (int(code.size()) > 2) {
            code[1] = (TCHAR)towlower(code[1]);
        }

        if (!_tcsnicmp(code.c_str(), _T("{c:$"), 4)) {
            _stscanf_s(code.c_str(), _T("{c:$%lx"), &ret->colors[0]);
        } else if (!_tcsnicmp(code.c_str(), _T("{f:"), 3)) {
            ret->fontName = StringMid(code, 3);
        } else if (!_tcsnicmp(code.c_str(), _T("{s:"), 3)) {
            double f;
            if (1 == _stscanf_s(code.c_str(), _T("{s:%lf"), &f)) {
                ret->fontSize = f;
            }
        } else if (!_tcsnicmp(code.c_str(), _T("{h:"), 3)) {
            _stscanf_s(code.c_str(), _T("{h:%d"), &ret->charSet);
        } else if (!_tcsnicmp(code.c_str(), _T("{y:"), 3)) {
            StringLower(code);
            if (StringFind(code, 'b') >= 0) {
                ret->fontWeight = FW_BOLD;
            }
            if (StringFind(code, 'i') >= 0) {
                ret->fItalic = true;
            }
            if (StringFind(code, 'u') >= 0) {
                ret->fUnderline = true;
            }
            if (StringFind(code, 's') >= 0) {
                ret->fStrikeOut = true;
            }
        } else if (!_tcsnicmp(code.c_str(), _T("{p:"), 3)) {
            int p;
            _stscanf_s(code.c_str(), _T("{p:%d"), &p);
            ret->scrAlignment = (p == 0) ? 8 : 2;
        }

        i = k;
    }

    return ret;
}

static std::wstring MicroDVD2SSA(std::wstring str, bool fUnicode, int CharSet)
{
    std::wstring ret;

    enum {
        COLOR = 0,
        FONTNAME,
        FONTSIZE,
        FONTCHARSET,
        BOLD,
        ITALIC,
        UNDERLINE,
        STRIKEOUT
    };
    bool fRestore[8];
    int fRestoreLen = 8;

    ZeroMemory(fRestore, sizeof(bool)*fRestoreLen);

    for (int pos = 0, eol; pos < int(str.size()); pos++) {
        if ((eol = FindChar(str, '|', pos, fUnicode, CharSet)) < 0) {
            eol = int(str.size());
        }

        std::wstring line = StringMid(str, pos, eol - pos);

        pos = eol;

        for (int i = 0, j, k, len = int(line.size()); i < len; i++) {
            if ((j = FindChar(line, '{', i, fUnicode, CharSet)) < 0) {
                j = int(str.size());
            }

            ret += StringMid(line, i, j - i);

            if (j >= len) {
                break;
            }

            if ((k = FindChar(line, '}', j, fUnicode, CharSet)) < 0) {
                k = len;
            }

            {
                std::wstring code = StringMid(line, j, k - j);

                if (!_wcsnicmp(code.c_str(), L"{c:$", 4)) {
                    fRestore[COLOR] = (iswupper(code[1]) == 0);
                    StringLower(code);

                    int color;
                    swscanf_s(code.c_str(), L"{c:$%x", &color);
                    code = ::Format(L"{\\c&H%x&}", color);
                    ret += code;
                } else if (!_wcsnicmp(code.c_str(), L"{f:", 3)) {
                    fRestore[FONTNAME] = (iswupper(code[1]) == 0);

                    code = ::Format(L"{\\fn%s}", StringMid(code, 3).c_str());
                    ret += code;
                } else if (!_wcsnicmp(code.c_str(), L"{s:", 3)) {
                    fRestore[FONTSIZE] = (iswupper(code[1]) == 0);
                    StringLower(code);

                    double size;
                    swscanf_s(code.c_str(), L"{s:%lf", &size);
                    code = ::Format(L"{\\fs%f}", size);
                    ret += code;
                } else if (!_wcsnicmp(code.c_str(), L"{h:", 3)) {
                    fRestore[COLOR] = (_istupper(code[1]) == 0);
                    StringLower(code);

                    int iCharSet;
                    swscanf_s(code.c_str(), L"{h:%d", &iCharSet);
                    code = ::Format(L"{\\fe%d}", iCharSet);
                    ret += code;
                } else if (!_wcsnicmp(code.c_str(), L"{y:", 3)) {
                    bool f = (_istupper(code[1]) == 0);

                    StringLower(code);

                    ret += L'{';
                    if (StringFind(code, 'b') >= 0) {
                        ret += L"\\b1";
                        fRestore[BOLD] = f;
                    }
                    if (StringFind(code, 'i') >= 0) {
                        ret += L"\\i1";
                        fRestore[ITALIC] = f;
                    }
                    if (StringFind(code, 'u') >= 0) {
                        ret += L"\\u1";
                        fRestore[UNDERLINE] = f;
                    }
                    if (StringFind(code, 's') >= 0) {
                        ret += L"\\s1";
                        fRestore[STRIKEOUT] = f;
                    }
                    ret += L'}';
                } else if (!_wcsnicmp(code.c_str(), L"{o:", 3)) {
                    StringLower(code);

                    int x, y;
                    TCHAR c;
                    swscanf_s(code.c_str(), L"{o:%d%c%d", &x, &c, 1, &y);
                    code = ::Format(L"{\\move(%d,%d,0,0,0,0)}", x, y);
                    ret += code;
                } else {
                    ret += code;
                }
            }

            i = k;
        }

        if (pos >= int(str.size())) {
            break;
        }

        for (ptrdiff_t i = 0; i < fRestoreLen; i++) {
            if (fRestore[i]) {
                switch (i) {
                    case COLOR:
                        ret += L"{\\c}";
                        break;
                    case FONTNAME:
                        ret += L"{\\fn}";
                        break;
                    case FONTSIZE:
                        ret += L"{\\fs}";
                        break;
                    case FONTCHARSET:
                        ret += L"{\\fe}";
                        break;
                    case BOLD:
                        ret += L"{\\b}";
                        break;
                    case ITALIC:
                        ret += L"{\\i}";
                        break;
                    case UNDERLINE:
                        ret += L"{\\u}";
                        break;
                    case STRIKEOUT:
                        ret += L"{\\s}";
                        break;
                    default:
                        ASSERT(FALSE); // Shouldn't happen
                        break;
                }
            }
        }

        ZeroMemory(fRestore, sizeof(bool)*fRestoreLen);

        ret += L"\\N";
    }

    return ret;
}

static bool OpenMicroDVD(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet)
{
    bool fCheck = false, fCheck2 = false;

    tstring style(_T("Default"));

    std::wstring buff;
    while (file->ReadString(buff)) {
        FastTrim(buff);
        if (buff.empty()) {
            continue;
        }

        LONGLONG start, end;
        int c = swscanf_s(buff.c_str(), L"{%lld}{%lld}", &start, &end);

        if (c != 2) {
            c = swscanf_s(buff.c_str(), L"{%lld}{}", &start) + 1;
            end = start + 60;
            fCheck = true;
        }

        if (c != 2) {
            int i;
            if (StringFind(buff, '{') == 0 && (i = StringFind(buff, '}')) > 1 && i < int(buff.size())) {
                if (STSStyle* s = GetMicroDVDStyle(WToT(StringMid(buff, i + 1)), CharSet)) {
                    style = StringMid(buff, 1, i - 1);
                    StringUpper(style);
                    if (int(style.size())) {
                        tstring str = StringMid(style, 1);
                        StringLower(str);
                        style = StringLeft(style, 1) + str;
                    }
                    ret.AddStyle(style, s);
                    CharSet = s->charSet;
                    continue;
                }
            }
        }

        if (c == 2) {
            if (fCheck2 && ret.size()) {
                STSEntry& stse = ret[ret.size() - 1];
                stse.end = std::min(stse.end, start);
                fCheck2 = false;
            }

            ret.Add(MicroDVD2SSA(StringMid(buff, StringFind(buff, '}', StringFind(buff, '}') + 1) + 1), file->IsUnicode(), CharSet),
                    file->IsUnicode(), start, end, style);

            if (fCheck) {
                fCheck = false;
                fCheck2 = true;
            }
        } else if (c != EOF) { // might be another format
            return false;
        }
    }

    return !ret.empty();
}

static void ReplaceNoCase(std::wstring& str, std::wstring from, std::wstring to)
{
    std::wstring lstr = str;
    StringLower(lstr);

    for (int i = 0, j = int(str.size()); i < j;) {
        int k = -1;
        if ((k = StringFind(lstr, from, i)) >= 0) {
            StringDelete(str, k, int(from.size()));
            StringDelete(lstr, k, int(from.size()));
            StringInsert(str, k, to);
            StringInsert(lstr, k, to);
            i = k + int(to.size());
            j = int(str.size());
        } else {
            break;
        }
    }
}

static std::wstring SMI2SSA(std::wstring str, int CharSet)
{
    ReplaceNoCase(str, L"&nbsp;", L" ");
    ReplaceNoCase(str, L"&quot;", L"\"");
    ReplaceNoCase(str, L"<br>", L"\\N");
    ReplaceNoCase(str, L"<i>", L"{\\i1}");
    ReplaceNoCase(str, L"</i>", L"{\\i}");
    ReplaceNoCase(str, L"<b>", L"{\\b1}");
    ReplaceNoCase(str, L"</b>", L"{\\b}");

    std::wstring lstr = str;
    StringLower(lstr);

    // maven@maven.de
    // now parse line
    for (int i = 0, j = int(str.size()); i < j;) {
        int k;
        if ((k = StringFind(lstr, '<', i)) < 0) {
            break;
        }

        int chars_inserted = 0;

        int l = 1;
        for (; k + l < j && lstr[k + l] != '>'; l++) {
            ;
        }
        l++;

        // Modified by Cookie Monster
        if (StringFind(lstr, L"<font ", k) == k) {
            std::wstring args = StringMid(lstr, k + 6, l - 6); // delete "<font "
            std::wstring arg;

            StringRemove(args, '\"');
            StringRemove(args, '#');   // may include 2 * " + #
            StringTrimLeft(arg);
            StringTrimRight(arg, L" >");

            for (;;) {
                StringTrimLeft(args);
                arg = StringSpanExcluding(args, L" \t>");
                args = StringMid(args, int(arg.size()));

                if (arg.empty()) {
                    break;
                }
                if (StringFind(arg, L"color=") == 0) {
                    arg = StringMid(arg, 6);   // delete "color="
                    if (arg.empty()) {
                        continue;
                    }

                    std::wstring colorTag = SSAColorTag(arg);

                    StringInsert(lstr, k + l + chars_inserted, colorTag);
                    StringInsert(str, k + l + chars_inserted, colorTag);
                    chars_inserted += 5 + int(colorTag.size()) + 2;
                }
            }
        }

        else if (StringFind(lstr, L"</font>", k) == k) {
            StringInsert(lstr, k + l + chars_inserted, L"{\\c}");
            StringInsert(str, k + l + chars_inserted, L"{\\c}");
            chars_inserted += 4;
        }

        StringDelete(str, k, l);
        StringDelete(lstr, k, l);
        i = k + chars_inserted;
        j = int(str.size());
    }

    return str;
}

static bool OpenSami(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet)
{
    std::wstring buff, caption;
    ULONGLONG pos = file->GetPosition();
    bool fSAMI = false;

    while (file->ReadString(buff) && !fSAMI) {
        if (StringFind(StringUpper(buff), L"<SAMI>") >= 0) {
            fSAMI = true;
        }
    }

    if (!fSAMI) {
        return false;
    }

    file->Seek(pos, CTextFile::begin);

    bool fComment = false;
    int start_time = 0;

    while (file->ReadString(buff)) {
        FastTrim(buff);
        if (buff.empty()) {
            continue;
        }

        std::wstring ubuff = buff;
        StringUpper(ubuff);

        if (StringFind(ubuff, L"<!--") >= 0 || StringFind(ubuff, L"<TITLE>") >= 0) {
            fComment = true;
        }

        if (!fComment) {
            int i;

            if ((i = StringFind(ubuff, L"<SYNC START=")) >= 0) {
                int time = 0;

                for (i = 12; i < int(ubuff.size()); i++) {
                    if (ubuff[i] != '>' && ubuff[i] != 'M') {
                        if (iswdigit(ubuff[i])) {
                            time *= 10;
                            time += ubuff[i] - 0x30;
                        }
                    } else {
                        break;
                    }
                }

                ret.Add(
                    SMI2SSA(caption, CharSet),
                    file->IsUnicode(),
                    MS2RT(start_time), MS2RT(time));

                start_time = time;
                caption.clear();
            }

            caption += buff;
        }

        if (StringFind(ubuff, L"-->") >= 0 || StringFind(ubuff, L"</TITLE>") >= 0) {
            fComment = false;
        }
    }

    ret.Add(
        SMI2SSA(caption, CharSet),
        file->IsUnicode(),
        MS2RT(start_time), LONGLONG_MAX);

    return true;
}

static bool OpenVPlayer(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet)
{
    std::wstring buff;
    while (file->ReadString(buff)) {
        FastTrim(buff);
        if (buff.empty()) {
            continue;
        }

        for (int i = 0; i < int(buff.size()); i++) {
            if ((i = FindChar(buff, '|', i, file->IsUnicode(), CharSet)) < 0) {
                break;
            }
            buff[i] = '\n';
        }

        int hh, mm, ss;
        int c = swscanf_s(buff.c_str(), L"%d:%d:%d:", &hh, &mm, &ss);

        if (c == 3) {
            std::wstring str = StringMid(buff, StringFind(buff, ':', StringFind(buff, ':', StringFind(buff, ':') + 1) + 1) + 1);
            ret.Add(str,
                    file->IsUnicode(),
                    MS2RT((((hh * 60i64 + mm) * 60i64) + ss) * 1000i64),
                    MS2RT((((hh * 60i64 + mm) * 60i64) + ss) * 1000i64 + 1000i64 + 50i64 * int(str.size())));
        } else if (c != EOF) { // might be another format
            return false;
        }
    }

    return !ret.empty();
}

static void GetStrW(LPCWSTR& pszBuff, int& nLength, WCHAR sep, LPCWSTR& pszMatch, int& nMatchLength)
{
    // Trim left whitespace
    while (iswspace(*pszBuff)) {
        pszBuff++;
        nLength--;
    }

    LPCWSTR pEnd = wcschr(pszBuff, sep);
    if (pEnd == nullptr) {
        if (nLength < 1) {
            throw 1;
        }
        nMatchLength = nLength;
    } else {
        nMatchLength = int(pEnd - pszBuff);
    }

    pszMatch = pszBuff;
    if (nMatchLength < nLength) {
        pszBuff = pEnd + 1;
        nLength -= nMatchLength + 1;
    }
}

static std::wstring GetStrW(LPCWSTR& pszBuff, int& nLength, WCHAR sep = L',')
{
    LPCWSTR pszMatch;
    int nMatchLength;
    GetStrW(pszBuff, nLength, sep, pszMatch, nMatchLength);

    return std::wstring(pszMatch, nMatchLength);
}

static int GetInt(LPCWSTR& pszBuff, int& nLength, WCHAR sep = L',')
{
    LPCWSTR pszMatch;
    int nMatchLength;
    GetStrW(pszBuff, nLength, sep, pszMatch, nMatchLength);

    LPWSTR strEnd;
    int ret;
    if (nMatchLength > 2
            && ((pszMatch[0] == L'&' && towlower(pszMatch[1]) == L'h')
                || (pszMatch[0] == L'0' && towlower(pszMatch[1]) == L'x'))) {
        pszMatch += 2;
        nMatchLength -= 2;
        // Read hexadecimal integer as unsigned
        ret = (int)wcstoul(pszMatch, &strEnd, 16);
    } else {
        ret = wcstol(pszMatch, &strEnd, 10);
    }

    if (pszMatch == strEnd) { // Ensure something was parsed
        throw 1;
    }

    return ret;
}

static double GetFloat(LPCWSTR& pszBuff, int& nLength, WCHAR sep = L',')
{
    if (sep == L'.') { // Parsing a float with '.' as separator doesn't make much sense...
        ASSERT(FALSE);
        return GetInt(pszBuff, nLength, sep);
    }

    LPCWSTR pszMatch;
    int nMatchLength;
    GetStrW(pszBuff, nLength, sep, pszMatch, nMatchLength);

    LPWSTR strEnd;
    double ret = wcstod(pszMatch, &strEnd);
    if (pszMatch == strEnd) { // Ensure something was parsed
        throw 1;
    }

    return ret;
}

static bool LoadFont(const tstring& font)
{
    int len = int(font.size());

    std::unique_ptr<BYTE[]> pData(len > 0 ? new(std::nothrow) BYTE[len] : nullptr);
    if (len == 0 || (len & 3) == 1 || !pData) {
        return false;
    }

    const TCHAR* s = font.c_str();
    const TCHAR* e = s + len;
    for (BYTE* p = pData.get(); s < e; s++, p++) {
        *p = BYTE(*s - 33);
    }

    for (ptrdiff_t i = 0, j = 0, k = len & ~3; i < k; i += 4, j += 3) {
        pData[j + 0] = ((pData[i + 0] & 63) << 2) | ((pData[i + 1] >> 4) & 3);
        pData[j + 1] = ((pData[i + 1] & 15) << 4) | ((pData[i + 2] >> 2) & 15);
        pData[j + 2] = ((pData[i + 2] &  3) << 6) | ((pData[i + 3] >> 0) & 63);
    }

    int datalen = (len & ~3) * 3 / 4;

    if ((len & 3) == 2) {
        pData[datalen++] = ((pData[(len & ~3) + 0] & 63) << 2) | ((pData[(len & ~3) + 1] >> 4) & 3);
    } else if ((len & 3) == 3) {
        pData[datalen++] = ((pData[(len & ~3) + 0] & 63) << 2) | ((pData[(len & ~3) + 1] >> 4) & 3);
        pData[datalen++] = ((pData[(len & ~3) + 1] & 15) << 4) | ((pData[(len & ~3) + 2] >> 2) & 15);
    }

    HANDLE hFont = INVALID_HANDLE_VALUE;

    if (HMODULE hModule = LoadLibrary(_T("gdi32.dll"))) {
        typedef HANDLE(WINAPI * PAddFontMemResourceEx)(IN PVOID, IN DWORD, IN PVOID, IN DWORD*);
        if (PAddFontMemResourceEx f = (PAddFontMemResourceEx)GetProcAddress(hModule, "AddFontMemResourceEx")) {
            DWORD cFonts;
            hFont = f(pData.get(), datalen, nullptr, &cFonts);
        }

        FreeLibrary(hModule);
    }

    if (hFont == INVALID_HANDLE_VALUE) {
        TCHAR path[MAX_PATH];
        GetTempPath(MAX_PATH, path);

        DWORD chksum = 0;
        for (ptrdiff_t i = 0, j = datalen >> 2; i < j; i++) {
            chksum += ((DWORD*)(BYTE*)pData.get())[i];
        }

        tstring fn;
        fn = ::Format(_T("%sfont%08lx.ttf"), path, chksum);

        if (!PathUtils::Exists(fn.c_str())) {
            if (FilePtr f = OpenFile(fn, L"wb")) {
                fwrite(pData.get(), 1, datalen, f.get());
            }
        }

        AddFontResource(fn.c_str());
    }

    return true;
}

static bool LoadUUEFont(CTextFile* file)
{
    tstring s, font;
    while (file->ReadString(s)) {
        FastTrim(s);
        if (s.empty()) {
            break;
        }
        if (s[0] == '[') { // check for some standard blocks
            if (StringFind(s, _T("[Script Info]")) == 0) {
                break;
            }
            if (StringFind(s, _T("[V4+ Styles]")) == 0) {
                break;
            }
            if (StringFind(s, _T("[V4 Styles]")) == 0) {
                break;
            }
            if (StringFind(s, _T("[Events]")) == 0) {
                break;
            }
            if (StringFind(s, _T("[Fonts]")) == 0) {
                break;
            }
            if (StringFind(s, _T("[Graphics]")) == 0) {
                break;
            }
        }
        if (StringFind(s, _T("fontname:")) == 0) {
            LoadFont(font);
            font.clear();
            continue;
        }

        font += s;
    }

    if (!font.empty()) {
        LoadFont(font);
    }

    return true;
}

static bool OpenSubStationAlpha(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet)
{
    bool fRet = false;
    int version = 3, sver = 3;
    std::wstring buff;
    int ignore_count = 0;
    bool first_line = true;

    while (file->ReadString(buff)) {
        FastTrim(buff);
        if (buff.empty() || buff[0] == L';') {
            continue;
        }

        if (first_line) {
            if (buff == L"1") {
                // SRT file
                return false;
            }
            if (buff == L"WEBVTT") {
                return false;
            }
            first_line = false;
        }

        LPCWSTR pszBuff = buff.c_str();
        int nBuffLength = int(buff.size());
        std::wstring entry = GetStrW(pszBuff, nBuffLength, L':');
        StringLower(entry);

        if (entry == L"dialogue") {
            try {
                int hh1, mm1, ss1, ms1_div10, hh2, mm2, ss2, ms2_div10, layer = 0;
                RECT marginRect = {};

                if (version <= 4) {
                    GetStrW(pszBuff, nBuffLength, L'=');      /* Marked = */
                    GetInt(pszBuff, nBuffLength);
                }
                if (version >= 5) {
                    layer = GetInt(pszBuff, nBuffLength);
                }
                hh1 = GetInt(pszBuff, nBuffLength, L':');
                mm1 = GetInt(pszBuff, nBuffLength, L':');
                ss1 = GetInt(pszBuff, nBuffLength, L'.');
                ms1_div10 = GetInt(pszBuff, nBuffLength);
                hh2 = GetInt(pszBuff, nBuffLength, L':');
                mm2 = GetInt(pszBuff, nBuffLength, L':');
                ss2 = GetInt(pszBuff, nBuffLength, L'.');
                ms2_div10 = GetInt(pszBuff, nBuffLength);
                tstring style = WToT(GetStrW(pszBuff, nBuffLength));
                tstring actor = WToT(GetStrW(pszBuff, nBuffLength));
                marginRect.left = GetInt(pszBuff, nBuffLength);
                marginRect.right = GetInt(pszBuff, nBuffLength);
                marginRect.top = marginRect.bottom = GetInt(pszBuff, nBuffLength);
                if (version >= 6) {
                    marginRect.bottom = GetInt(pszBuff, nBuffLength);
                }

                tstring effect = WToT(GetStrW(pszBuff, nBuffLength));
                int len = std::min(int(effect.size()), nBuffLength);
                if (StringLeft(effect, len) == WToT(std::wstring(pszBuff, len))) {
                    effect.clear();
                }

                StringTrimLeft(style, _T('*'));
                if (!StringICompare(style, _T("Default"))) {
                    style = _T("Default");
                }

                ret.Add(pszBuff,
                        file->IsUnicode(),
                        MS2RT((((hh1 * 60i64 + mm1) * 60i64) + ss1) * 1000i64 + ms1_div10 * 10i64),
                        MS2RT((((hh2 * 60i64 + mm2) * 60i64) + ss2) * 1000i64 + ms2_div10 * 10i64),
                        style, actor, effect,
                        marginRect,
                        layer);
            } catch (...) {
                return false;
            }
        } else if (entry == L"style") {
            STSStyle* style = new STSStyle;
            if (!style) {
                return false;
            }

            try {
                tstring styleName = WToT(GetStrW(pszBuff, nBuffLength));
                style->fontName = WToT(GetStrW(pszBuff, nBuffLength));
                style->fontSize = GetFloat(pszBuff, nBuffLength);
                for (size_t i = 0; i < 4; i++) {
                    style->colors[i] = (COLORREF)GetInt(pszBuff, nBuffLength);
                }
                style->fontWeight = GetInt(pszBuff, nBuffLength) ? FW_BOLD : FW_NORMAL;
                style->fItalic = GetInt(pszBuff, nBuffLength);
                if (sver >= 5)  {
                    style->fUnderline = GetInt(pszBuff, nBuffLength);
                    style->fStrikeOut = GetInt(pszBuff, nBuffLength);
                    style->fontScaleX = GetFloat(pszBuff, nBuffLength);
                    style->fontScaleY = GetFloat(pszBuff, nBuffLength);
                    style->fontSpacing = GetFloat(pszBuff, nBuffLength);
                    style->fontAngleZ = GetFloat(pszBuff, nBuffLength);
                }
                if (sver >= 4)  {
                    style->borderStyle = GetInt(pszBuff, nBuffLength);
                }
                style->outlineWidthX = style->outlineWidthY = GetFloat(pszBuff, nBuffLength);
                style->shadowDepthX = style->shadowDepthY = GetFloat(pszBuff, nBuffLength);
                style->scrAlignment = GetInt(pszBuff, nBuffLength);
                style->marginRect.left = GetInt(pszBuff, nBuffLength);
                style->marginRect.right = GetInt(pszBuff, nBuffLength);
                style->marginRect.top = style->marginRect.bottom = GetInt(pszBuff, nBuffLength);
                if (sver >= 6)  {
                    style->marginRect.bottom = GetInt(pszBuff, nBuffLength);
                }

                int alpha = 0;
                if (sver <= 4)  {
                    alpha = GetInt(pszBuff, nBuffLength);
                }
                style->charSet = GetInt(pszBuff, nBuffLength);
                if (sver >= 6)  {
                    style->relativeTo = (STSStyle::RelativeTo)GetInt(pszBuff, nBuffLength);
                }

                if (sver <= 4)  {
                    style->colors[2] = style->colors[3];    // style->colors[2] is used for drawing the outline
                    alpha = std::max(std::min(alpha, 0xff), 0);
                    for (size_t i = 0; i < 3; i++) {
                        style->alpha[i] = (BYTE)alpha;
                    }
                    style->alpha[3] = 0x80;
                }
                if (sver >= 5) {
                    for (size_t i = 0; i < 4; i++) {
                        style->alpha[i] = (BYTE)(style->colors[i] >> 24);
                        style->colors[i] &= 0xffffff;
                    }
                    style->fontScaleX = std::max(style->fontScaleX, 0.0);
                    style->fontScaleY = std::max(style->fontScaleY, 0.0);
                }
                style->fontAngleX = style->fontAngleY = 0;
                style->borderStyle = style->borderStyle == 1 ? 0 : style->borderStyle == 3 ? 1 : 0;
                style->outlineWidthX = std::max(style->outlineWidthX, 0.0);
                style->outlineWidthY = std::max(style->outlineWidthY, 0.0);
                style->shadowDepthX = std::max(style->shadowDepthX, 0.0);
                style->shadowDepthY = std::max(style->shadowDepthY, 0.0);
                if (sver <= 4) {
                    style->scrAlignment = (style->scrAlignment & 4) ? ((style->scrAlignment & 3) + 6) // top
                                          : (style->scrAlignment & 8) ? ((style->scrAlignment & 3) + 3) // mid
                                          : (style->scrAlignment & 3); // bottom
                }

                StringTrimLeft(styleName, _T('*'));

                ret.AddStyle(styleName, style);
            } catch (...) {
                delete style;
                return false;
            }
        } else if (entry == L"[script info]") {
            fRet = true;
        } else if (entry == L"playresx") {
            try {
                ret.m_playRes.cx = GetInt(pszBuff, nBuffLength);
            } catch (...) {
                ret.m_playRes = MakeSize(0, 0);
                return false;
            }

            if (ret.m_playRes.cy <= 0) {
                ret.m_playRes.cy = (ret.m_playRes.cx == 1280)
                                         ? 1024
                                         : ret.m_playRes.cx * 3 / 4;
            }
        } else if (entry == L"playresy") {
            try {
                ret.m_playRes.cy = GetInt(pszBuff, nBuffLength);
            } catch (...) {
                ret.m_playRes = MakeSize(0, 0);
                return false;
            }

            if (ret.m_playRes.cx <= 0) {
                ret.m_playRes.cx = (ret.m_playRes.cy == 1024)
                                         ? 1280
                                         : ret.m_playRes.cy * 4 / 3;
            }
        } else if (entry == L"layoutresx") {
            try {
                ret.m_layoutRes.cx = GetInt(pszBuff, nBuffLength);
            } catch (...) {
                ret.m_layoutRes = MakeSize(0, 0);
                return false;
            }
        } else if (entry == L"layoutresy") {
            try {
                ret.m_layoutRes.cy = GetInt(pszBuff, nBuffLength);
            } catch (...) {
                ret.m_layoutRes = MakeSize(0, 0);
                return false;
            }
        } else if (entry == L"wrapstyle") {
            try {
                ret.m_defaultWrapStyle = GetInt(pszBuff, nBuffLength);
            } catch (...) {
                ret.m_defaultWrapStyle = 1;
                return false;
            }
        } else if (entry == L"scripttype") {
            if (int(buff.size()) >= 4 && !StringICompare(StringRight(buff, 4), L"4.00")) {
                version = sver = 4;
            } else if (int(buff.size()) >= 5 && !StringICompare(StringRight(buff, 5), L"4.00+")) {
                version = sver = 5;
            } else if (int(buff.size()) >= 6 && !StringICompare(StringRight(buff, 6), L"4.00++")) {
                version = sver = 6;
            }
        } else if (entry == L"collisions") {
            if (nBuffLength) {
                buff = GetStrW(pszBuff, nBuffLength);
                StringLower(buff);
                ret.m_collisions = StringFind(buff, L"reverse") >= 0 ? 1 : 0;
            }
        } else if (entry == L"scaledborderandshadow") {
            if (nBuffLength) {
                buff = GetStrW(pszBuff, nBuffLength);
                StringLower(buff);
                ret.m_fScaledBAS = StringFind(buff, L"yes") >= 0;
            }
        } else if (entry == L"[v4 styles]") {
            fRet = true;
            sver = 4;
        } else if (entry == L"[v4+ styles]") {
            fRet = true;
            sver = 5;
        } else if (entry == L"[v4++ styles]") {
            fRet = true;
            sver = 6;
        } else if (entry == L"[events]") {
            fRet = true;
        } else if (entry == L"fontname") {
            LoadUUEFont(file);
        } else if (entry == L"ycbcr matrix") {
            if (nBuffLength) {
                ret.m_sYCbCrMatrix = GetStrW(pszBuff, nBuffLength);
                StringUpper(ret.m_sYCbCrMatrix);
            }
        } else if (entry == L"format") {
            // ToDo: Parse this line and use it to correctly parse following style and dialogue lines
            // Currently the contents of the format lines are assumed to have a standard string value based on script version.
            if (version < 5 && StringFind(tstring(pszBuff), _T("Layer,")) >= 0) {
                version = 5;
            }
        } else {
            TRACE(_T("Ignoring unknown SSA entry: %s\n"), static_cast<LPCWSTR>(entry));
            if (!fRet) {
                if (++ignore_count >= 10) {
                    return false;
                }
            }
        }
    }

    return fRet;
}

static bool OpenXombieSub(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet)
{
    //  CMapStringToPtr stylemap;

    std::wstring buff;
    while (file->ReadString(buff)) {
        FastTrim(buff);
        if (buff.empty() || buff[0] == L';') {
            continue;
        }

        LPCWSTR pszBuff = buff.c_str();
        int nBuffLength = int(buff.size());
        std::wstring entry = GetStrW(pszBuff, nBuffLength, L'=');
        StringLower(entry);

        /*if (entry == L"version") {
            double version = GetFloat(buff);
        } else*/
        if (entry == L"screenhorizontal") {
            try {
                ret.m_storageRes.cx = GetInt(pszBuff, nBuffLength);
            } catch (...) {
                ret.m_storageRes = MakeSize(0, 0);
                return false;
            }

            if (ret.m_storageRes.cy <= 0) {
                ret.m_storageRes.cy = (ret.m_storageRes.cx == 1280)
                                         ? 1024
                                         : ret.m_storageRes.cx * 3 / 4;
            }
        } else if (entry == L"screenvertical") {
            try {
                ret.m_storageRes.cy = GetInt(pszBuff, nBuffLength);
            } catch (...) {
                ret.m_storageRes = MakeSize(0, 0);
                return false;
            }

            if (ret.m_storageRes.cx <= 0) {
                ret.m_storageRes.cx = (ret.m_storageRes.cy == 1024)
                                         ? 1280
                                         : ret.m_storageRes.cy * 4 / 3;
            }
        } else if (entry == L"style") {
            STSStyle* style = new STSStyle;
            if (!style) {
                return false;
            }

            try {
                tstring styleName = WToT(GetStrW(pszBuff, nBuffLength)) + _T("_") + WToT(GetStrW(pszBuff, nBuffLength));
                style->fontName = WToT(GetStrW(pszBuff, nBuffLength));
                style->fontSize = GetFloat(pszBuff, nBuffLength);
                for (size_t i = 0; i < 4; i++) {
                    style->colors[i] = (COLORREF)GetInt(pszBuff, nBuffLength);
                }
                for (size_t i = 0; i < 4; i++) {
                    style->alpha[i] = (BYTE)GetInt(pszBuff, nBuffLength);
                }
                style->fontWeight = GetInt(pszBuff, nBuffLength) ? FW_BOLD : FW_NORMAL;
                style->fItalic = GetInt(pszBuff, nBuffLength);
                style->fUnderline = GetInt(pszBuff, nBuffLength);
                style->fStrikeOut = GetInt(pszBuff, nBuffLength);
                style->fBlur = GetInt(pszBuff, nBuffLength);
                style->fontScaleX = GetFloat(pszBuff, nBuffLength);
                style->fontScaleY = GetFloat(pszBuff, nBuffLength);
                style->fontSpacing = GetFloat(pszBuff, nBuffLength);
                style->fontAngleX = GetFloat(pszBuff, nBuffLength);
                style->fontAngleY = GetFloat(pszBuff, nBuffLength);
                style->fontAngleZ = GetFloat(pszBuff, nBuffLength);
                style->borderStyle = GetInt(pszBuff, nBuffLength);
                style->outlineWidthX = style->outlineWidthY = GetFloat(pszBuff, nBuffLength);
                style->shadowDepthX = style->shadowDepthY = GetFloat(pszBuff, nBuffLength);
                style->scrAlignment = GetInt(pszBuff, nBuffLength);
                style->marginRect.left = GetInt(pszBuff, nBuffLength);
                style->marginRect.right = GetInt(pszBuff, nBuffLength);
                style->marginRect.top = style->marginRect.bottom = GetInt(pszBuff, nBuffLength);
                style->charSet = GetInt(pszBuff, nBuffLength);

                style->fontScaleX = std::max(style->fontScaleX, 0.0);
                style->fontScaleY = std::max(style->fontScaleY, 0.0);
                style->fontSpacing = std::max(style->fontSpacing, 0.0);
                style->borderStyle = style->borderStyle == 1 ? 0 : style->borderStyle == 3 ? 1 : 0;
                style->outlineWidthX = std::max(style->outlineWidthX, 0.0);
                style->outlineWidthY = std::max(style->outlineWidthY, 0.0);
                style->shadowDepthX = std::max(style->shadowDepthX, 0.0);
                style->shadowDepthY = std::max(style->shadowDepthY, 0.0);

                ret.AddStyle(styleName, style);
            } catch (...) {
                delete style;
                return false;
            }
        } else if (entry == L"line") {
            try {
                int hh1, mm1, ss1, ms1, hh2, mm2, ss2, ms2, layer = 0;
                RECT marginRect = {};

                if (GetStrW(pszBuff, nBuffLength) != L"D") {
                    continue;
                }
                tstring id = GetStrW(pszBuff, nBuffLength);
                layer = GetInt(pszBuff, nBuffLength);
                hh1 = GetInt(pszBuff, nBuffLength, L':');
                mm1 = GetInt(pszBuff, nBuffLength, L':');
                ss1 = GetInt(pszBuff, nBuffLength, L'.');
                ms1 = GetInt(pszBuff, nBuffLength);
                hh2 = GetInt(pszBuff, nBuffLength, L':');
                mm2 = GetInt(pszBuff, nBuffLength, L':');
                ss2 = GetInt(pszBuff, nBuffLength, L'.');
                ms2 = GetInt(pszBuff, nBuffLength);
                tstring style = WToT(GetStrW(pszBuff, nBuffLength)) + _T("_") + WToT(GetStrW(pszBuff, nBuffLength));
                tstring actor = WToT(GetStrW(pszBuff, nBuffLength));
                marginRect.left = GetInt(pszBuff, nBuffLength);
                marginRect.right = GetInt(pszBuff, nBuffLength);
                marginRect.top = marginRect.bottom = GetInt(pszBuff, nBuffLength);

                StringTrimLeft(style, '*');
                if (!StringICompare(style, _T("Default"))) {
                    style = _T("Default");
                }

                ret.Add(pszBuff,
                        file->IsUnicode(),
                        MS2RT((((hh1 * 60i64 + mm1) * 60i64) + ss1) * 1000i64 + ms1),
                        MS2RT((((hh2 * 60i64 + mm2) * 60i64) + ss2) * 1000i64 + ms2),
                        style, actor, _T(""),
                        marginRect,
                        layer);
            } catch (...) {
                return false;
            }
        } else if (entry == L"fontname") {
            LoadUUEFont(file);
        }
    }

    return !ret.empty();
}

static bool OpenUSF(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet)
{
    tstring str;
    while (file->ReadString(str)) {
        if (StringFind(str, _T("USFSubtitles")) >= 0) {
            CUSFSubtitles usf;
            if (usf.Read(file->GetFilePath().c_str()) && usf.ConvertToSTS(ret)) {
                return true;
            }

            break;
        }
    }

    return false;
}

static std::wstring MPL22SSA(std::wstring str, bool fUnicode, int CharSet)
{
    // Convert MPL2 italic tags to MicroDVD italic tags
    if (str[0] == L'/') {
        str = L"{y:i}" + StringMid(str, 1);
    }
    StringReplace(str, L"|/", L"|{y:i}");

    return MicroDVD2SSA(str, fUnicode, CharSet);
}

static bool OpenMPL2(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet)
{
    std::wstring buff;
    while (file->ReadString(buff)) {
        FastTrim(buff);
        if (buff.empty()) {
            continue;
        }

        int start, end;
        int c = swscanf_s(buff.c_str(), L"[%d][%d]", &start, &end);

        if (c == 2) {
            ret.Add(
                MPL22SSA(StringMid(buff, StringFind(buff, ']', StringFind(buff, ']') + 1) + 1), file->IsUnicode(), CharSet),
                file->IsUnicode(),
                MS2RT(start * 100i64),
                MS2RT(end * 100i64));
        } else if (c != EOF) { // might be another format
            return false;
        }
    }

    return !ret.empty();
}

typedef bool (*STSOpenFunct)(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet);

static bool OpenRealText(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet);

struct OpenFunctStruct {
    STSOpenFunct open;
    tmode mode;
    Subtitle::SubType type;
};

static OpenFunctStruct OpenFuncts[] = {
    OpenSubRipper, TIME, Subtitle::SRT,
    OpenOldSubRipper, TIME, Subtitle::SRT,
    OpenSubViewer, TIME, Subtitle::SRT,
    OpenMicroDVD, FRAME, Subtitle::SUB,
    OpenVPlayer, TIME, Subtitle::SRT,
    OpenSubStationAlpha, TIME, Subtitle::SSA,
    OpenVTT, TIME, Subtitle::VTT,
    OpenXombieSub, TIME, Subtitle::XSS,
    OpenMPL2, TIME, Subtitle::SRT,
    OpenRealText, TIME, Subtitle::RT,
    OpenSami, TIME, Subtitle::SMI,
    OpenUSF, TIME, Subtitle::USF,
};

static int nOpenFuncts = _countof(OpenFuncts);

static std::vector<int> PreferredOpenFuncts(tstring fn) {
    std::vector<int> functs;
    auto fileExt = StringTrimLeft(PathUtils::FileExt(fn.c_str()), '.');
    for (int i = 0; i < nOpenFuncts; i++) {
        if (fileExt == _T("vtt")) {
            if (OpenFuncts[i].open == OpenVTT) functs.insert(functs.begin(), i);
            else if (OpenFuncts[i].open == OpenSubRipper) functs.push_back(i);
        } else if (fileExt == _T("srt")) {
            if (OpenFuncts[i].open == OpenSubRipper) functs.insert(functs.begin(), i);
            else functs.push_back(i);
        } else if (fileExt == _T("ssa") || fileExt == _T("ass")) {
            if (OpenFuncts[i].open == OpenSubStationAlpha) functs.insert(functs.begin(), i);
            else if (OpenFuncts[i].open == OpenSubRipper) functs.push_back(i);
        } else if (fileExt == _T("xss")) {
            if (OpenFuncts[i].open == OpenXombieSub) functs.insert(functs.begin(), i);
        } else if (fileExt == _T("sub")) {
            if (OpenFuncts[i].open == OpenSubViewer) functs.insert(functs.begin(), i);
            else functs.push_back(i);
        } else if (fileExt == _T("txt")) {
            if (OpenFuncts[i].open == OpenMicroDVD) functs.insert(functs.begin(), i);
            else functs.push_back(i);
        } else if (fileExt == _T("rt")) {
            if (OpenFuncts[i].open == OpenRealText) functs.insert(functs.begin(), i);
        } else if (fileExt == _T("smi")) {
            if (OpenFuncts[i].open == OpenSami) functs.insert(functs.begin(), i);
        } else if (fileExt == _T("usf")) {
            if (OpenFuncts[i].open == OpenUSF) functs.insert(functs.begin(), i);
        } else if (fileExt == _T("style")) {
            if (OpenFuncts[i].open == OpenSubStationAlpha) functs.push_back(i);
        } else if (fileExt == _T("tmp")) { // used for embedded subs and downloaded subs
            if (OpenFuncts[i].open == OpenSubRipper || OpenFuncts[i].open == OpenSubStationAlpha || OpenFuncts[i].open == OpenVTT) functs.insert(functs.begin(), i);
            else functs.push_back(i);
        } else {
            functs.push_back(i);
        }
    }
    return functs;
}

//

CSimpleTextSubtitle::CSimpleTextSubtitle()
    : m_lcid(0)
    , m_langname()
    , m_subtitleType(Subtitle::SRT)
    , m_mode(TIME)
    , m_encoding(CTextFile::DEFAULT_ENCODING)
    , m_provider(_T("Local"))
    , m_eHearingImpaired(Subtitle::HI_UNKNOWN)
    , m_storageRes(MakeSize(0, 0))
    , m_playRes(MakeSize(0, 0))
    , m_layoutRes(MakeSize(0, 0))
    , m_defaultWrapStyle(0)
    , m_collisions(0)
    , m_fScaledBAS(false)
    , m_bUsingPlayerDefaultStyle(false)
    , m_ePARCompensationType(EPCTDisabled)
    , m_dPARCompensation(1.0)
#if USE_LIBASS
    , m_renderUsingLibass(true)
    , m_openTypeLangHint()
    , m_assloaded(false)
    , m_assfontloaded(false)
    , m_pGraph(nullptr)
    , m_pPin(nullptr)
    , m_ass(nullptr)
    , m_renderer(nullptr)
    , m_track(nullptr)
#endif
{
}

CSimpleTextSubtitle::~CSimpleTextSubtitle()
{
#if USE_LIBASS
    UnloadASS();
#endif

    Empty();
}
/*
CSimpleTextSubtitle::CSimpleTextSubtitle(CSimpleTextSubtitle& sts)
{
    *this = sts;
}

CSimpleTextSubtitle& CSimpleTextSubtitle::operator = (CSimpleTextSubtitle& sts)
{
    Copy(sts);

    return *this;
}
*/

void CSimpleTextSubtitle::Copy(CSimpleTextSubtitle& sts)
{
    if (this != &sts) {
        Empty();

        m_name = sts.m_name;
        m_mode = sts.m_mode;
        m_path = sts.m_path;
        m_subtitleType = sts.m_subtitleType;
        m_storageRes = sts.m_storageRes;
        m_playRes = sts.m_playRes;
        m_layoutRes = sts.m_layoutRes;
        m_defaultWrapStyle = sts.m_defaultWrapStyle;
        m_collisions = sts.m_collisions;
        m_fScaledBAS = sts.m_fScaledBAS;
        m_encoding = sts.m_encoding;
        m_bUsingPlayerDefaultStyle = sts.m_bUsingPlayerDefaultStyle;
        m_provider = sts.m_provider;
        m_eHearingImpaired = sts.m_eHearingImpaired;
        CopyStyles(sts.m_styles);
        m_segments = sts.m_segments;
        static_cast<std::vector<STSEntry>&>(*this) = sts;

#if USE_LIBASS
        if (m_assloaded) {
            LoadASSFile(m_subtitleType);
        }
#endif
    }
}

void CSimpleTextSubtitle::Append(CSimpleTextSubtitle& sts, REFERENCE_TIME timeoff)
{
    if (timeoff < 0) {
        timeoff = !empty() ? at(size() - 1).end : 0;
    }

    for (size_t i = 0, j = size(); i < j; i++) {
        if (at(i).start > timeoff) {
            erase(begin() + i, begin() + j);
            break;
        }
    }

    CopyStyles(sts.m_styles, true);

    for (size_t i = 0, j = sts.size(); i < j; i++) {
        STSEntry stse = sts.at(i);
        stse.start += timeoff;
        stse.end += timeoff;
        stse.readorder += (int)size();
        push_back(stse);
    }

    CreateSegments();
}

void CSTSStyleMap::Free()
{
    for (const auto& style : *this) {
        delete style.second;
    }

    clear();
}

bool CSimpleTextSubtitle::CopyStyles(const CSTSStyleMap& styles, bool fAppend)
{
    if (!fAppend) {
        m_styles.Free();
    }

    for (const auto& style : styles) {
        const tstring& key = style.first;
        STSStyle* val = style.second;

        STSStyle* s = new STSStyle;
        if (!s) {
            return false;
        }

        *s = *val;

        AddStyle(key, s);
    }

    return true;
}

void CSimpleTextSubtitle::Empty()
{
    m_storageRes = MakeSize(0, 0);
    m_playRes = MakeSize(0, 0);
    m_layoutRes = MakeSize(0, 0);
    m_styles.Free();
    m_segments.clear();
    clear();
}

static bool SegmentCompStart(const STSSegment& segment, REFERENCE_TIME start)
{
    return (segment.start < start);
}

void CSimpleTextSubtitle::Add(std::wstring str, bool fUnicode, REFERENCE_TIME start, REFERENCE_TIME end, tstring style, tstring actor, tstring effect, const RECT& marginRect, int layer, int readorder)
{
    FastTrim(str);
    if (str.empty() || start > end) {
        return;
    }
    //TRACE(_T("CSimpleTextSubtitle::Add (%d) = %s\n"), m_segments.GetCount(), str.GetString());
    if (m_subtitleType == Subtitle::VTT) {
        WebVTTCueStrip(str);
        WebVTT2SSA(str);
        if (str.empty()) return;
    }

    StringRemove(str, '\r');
    StringReplace(str, L"\n", L"\\N");
    if (style.empty()) {
        style = _T("Default");
    }
    StringTrimLeft(style, '*');

    STSEntry sub;
    sub.str = str;
    sub.fUnicode = fUnicode;
    sub.style = style;
    sub.actor = actor;
    sub.effect = effect;
    sub.marginRect = marginRect;
    sub.layer = layer;
    sub.start = start;
    sub.end = end;
    sub.readorder = readorder < 0 ? (int)size() : readorder;

    int n = (int)__super::size();

    // Entries with a null duration don't belong to any segments since
    // they are not to be rendered. We choose not to skip them completely
    // so that they are not lost when saving a subtitle file from MPC-HC
    // and so that one can change the timings of such entries using the
    // Subresync bar if necessary.
    if (start == end) {
        return;
    }

    size_t segmentsCount = m_segments.size();

    if (segmentsCount == 0) { // First segment
        push_back(sub);
        n = (int)(size() - 1);
        STSSegment stss(start, end);
        stss.subs.push_back(n);
        m_segments.push_back(stss);
    } else {
        STSSegment* segmentsStart = m_segments.data();
        STSSegment* segmentsEnd   = segmentsStart + segmentsCount;
        STSSegment* segment = std::lower_bound(segmentsStart, segmentsEnd, start, SegmentCompStart);

        if (m_subtitleType == Subtitle::VTT && start == segment->start && end == segment->end) {
            // ToDo: compare new sub with existing one to verify if it is really a duplicate
            //TRACE(_T("Dropping duplicate WebVTT sub (n=%d)\n"), n);
            return;
        }

        push_back(sub);
        n = (int)(size() - 1);

        size_t i = segment - segmentsStart;
        if (i > 0 && m_segments[i - 1].end > start) {
            // The beginning of i-1th segment isn't modified
            // by the new entry so separate it in two segments
            STSSegment stss(m_segments[i - 1].start, start);
            stss.subs = m_segments[i - 1].subs;
            m_segments[i - 1].start = start;
            m_segments.insert(m_segments.begin() + i - 1, stss);
        } else if (i < segmentsCount && start < m_segments[i].start) {
            // The new entry doesn't start in an existing segment.
            // It might even not overlap with any segment at all
            STSSegment stss(start, std::min(end, m_segments[i].start));
            stss.subs.push_back(n);
            m_segments.insert(m_segments.begin() + i, stss);
            i++;
        }

        REFERENCE_TIME lastEnd = _I64_MAX;
        for (; i < m_segments.size() && m_segments[i].start < end; i++) {
            STSSegment& s = m_segments[i];

            if (lastEnd < s.start) {
                // There is a gap between the segments overlapped by
                // the new entry so we have to create a new one
                STSSegment stss(lastEnd, s.start);
                stss.subs.push_back(n);
                lastEnd = s.start; // s might not point on the right segment after inserting so do the modification now
                m_segments.insert(m_segments.begin() + i, stss);
            } else {
                if (end < s.end) {
                    // The end of current segment isn't modified
                    // by the new entry so separate it in two segments
                    STSSegment stss(end, s.end);
                    stss.subs = s.subs;
                    s.end = end; // s might not point on the right segment after inserting so do the modification now
                    m_segments.insert(m_segments.begin() + i + 1, stss);
                }

                // The array might have been reallocated so create a new reference
                STSSegment& sAdd = m_segments[i];

                // Add the entry to the current segment now that we are you it belongs to it
                size_t entriesCount = sAdd.subs.size();
                // Take a shortcut when possible
                if (!entriesCount || sub.readorder >= at(sAdd.subs[entriesCount - 1]).readorder) {
                    sAdd.subs.push_back(n);
                } else {
                    for (size_t j = 0; j < entriesCount; j++) {
                        if (sub.readorder < at(sAdd.subs[j]).readorder) {
                            sAdd.subs.insert(sAdd.subs.begin() + j, n);
                            break;
                        }
                    }
                }

                lastEnd = sAdd.end;
            }
        }

        if (end > m_segments[i - 1].end) {
            // The new entry ends after the last overlapping segment.
            // It might even not overlap with any segment at all
            STSSegment stss(std::max(start, m_segments[i - 1].end), end);
            stss.subs.push_back(n);
            m_segments.insert(m_segments.begin() + i, stss);
        }
    }
}

STSStyle* CSimpleTextSubtitle::CreateDefaultStyle(int CharSet)
{
    tstring def(_T("Default"));

    STSStyle* ret = nullptr;

    if (!Lookup(m_styles, def, ret)) {
        STSStyle* style = new STSStyle();
        //*style = AfxGetAppSettings().subtitlesDefStyle;
        *style = GetAppDefaultStyle();
        if (CharSet != DEFAULT_CHARSET) {
            style->charSet = CharSet;
        }
        AddStyle(def, style);
        Lookup(m_styles, def, ret);

        m_bUsingPlayerDefaultStyle = true;
    }

    return ret;
}

void CSimpleTextSubtitle::ChangeUnknownStylesToDefault()
{
    std::map<tstring, STSStyle*> unknown;
    bool fReport = false; // skip unknown style warnings

    for (size_t i = 0; i < size(); i++) {
        STSEntry& stse = at(i);

        STSStyle* val;
        if (!Lookup(m_styles, stse.style, val)) {
            if (!Lookup(unknown, stse.style, val)) {
                if (fReport) {
                    tstring msg;
                    msg = ::Format(_T("Unknown style found: \"%s\", changed to \"Default\"!\n\nPress Cancel to ignore further warnings."), stse.style.c_str());
                    if (MessageBox(nullptr, msg.c_str(), _T("Warning"), MB_OKCANCEL | MB_ICONWARNING) != IDOK) {
                        fReport = false;
                    }
                }

                unknown[stse.style] = nullptr;
            }

            stse.style = _T("Default");
        }
    }
}

void CSimpleTextSubtitle::AddStyle(tstring name, STSStyle* style)
{
    if (name.empty()) {
        name = _T("Default");
    }

    if (m_bUsingPlayerDefaultStyle && name == _T("Default")) {
        m_bUsingPlayerDefaultStyle = false;
    }

    STSStyle* val;
    if (Lookup(m_styles, name, val)) {
        if (*val == *style) {
            delete style;
            return;
        }

        int i;
        int len = int(name.size());

        for (i = len; i > 0 && _istdigit(name[i - 1]); i--) {
            ;
        }

        int idx = 1;

        tstring name2 = name;

        if (i < len && _stscanf_s(StringRight(name, len - i).c_str(), _T("%d"), &idx) == 1) {
            name2 = StringLeft(name, i);
        }

        idx++;

        tstring name3;
        do {
            name3 = ::Format(_T("%s%d"), name2.c_str(), idx);
            idx++;
        } while ((m_styles.find(name3) != m_styles.end()));

        m_styles.erase(name);
        m_styles[name3] = val;

        for (size_t j = 0, count = size(); j < count; j++) {
            STSEntry& stse = at(j);
            if (stse.style == name) {
                stse.style = name3;
            }
        }
    }

    m_styles[name] = style;
}

bool CSimpleTextSubtitle::SetDefaultStyle(const STSStyle& s)
{
    STSStyle* val;
    if (Lookup(m_styles, _T("Default"), val)) {
        *val = s;
    } else {
        val = new STSStyle();
        *val = s;
        m_styles[L"Default"] = val;
        m_bUsingPlayerDefaultStyle = true;
    }

    return true;
}

bool CSimpleTextSubtitle::GetDefaultStyle(STSStyle& s) const
{
    STSStyle* val;
    if (!Lookup(m_styles, _T("Default"), val)) {
        return false;
    }
    s = *val;
    return true;
}

void CSimpleTextSubtitle::ConvertToTimeBased(double fps)
{
    if (m_mode == TIME) {
        return;
    }

    for (size_t i = 0, j = size(); i < j; i++) {
        STSEntry& stse = (*this)[i];
        stse.start = std::llround(stse.start * UNITS_FLOAT / fps);
        stse.end   = std::llround(stse.end * UNITS_FLOAT / fps);
    }

    m_mode = TIME;

    CreateSegments();
}

void CSimpleTextSubtitle::ConvertToFrameBased(double fps)
{
    if (m_mode == FRAME) {
        return;
    }

    for (size_t i = 0, j = size(); i < j; i++) {
        STSEntry& stse = (*this)[i];
        stse.start = std::llround(stse.start * fps / UNITS);
        stse.end   = std::llround(stse.end * fps / UNITS);
    }

    m_mode = FRAME;

    CreateSegments();
}

int CSimpleTextSubtitle::SearchSub(REFERENCE_TIME t, double fps)
{
    int i = 0, j = (int)size() - 1, ret = -1;

    if (j >= 0 && t >= TranslateStart(j, fps)) {
        return j;
    }

    while (i < j) {
        int mid = (i + j) >> 1;

        REFERENCE_TIME midt = TranslateStart(mid, fps);

        if (t == midt) {
            while (mid > 0 && t == TranslateStart(mid - 1, fps)) {
                --mid;
            }
            ret = mid;
            break;
        } else if (t < midt) {
            ret = -1;
            if (j == mid) {
                mid--;
            }
            j = mid;
        } else if (t > midt) {
            ret = mid;
            if (i == mid) {
                ++mid;
            }
            i = mid;
        }
    }

    return ret;
}

const STSSegment* CSimpleTextSubtitle::SearchSubs(REFERENCE_TIME t, double fps, /*[out]*/ int* iSegment, int* nSegments)
{
    int i = 0, j = (int)m_segments.size() - 1, ret = -1;

    if (nSegments) {
        *nSegments = j + 1;
    }

    // last segment
    if (j >= 0 && t >= TranslateSegmentStart(j, fps) && t < TranslateSegmentEnd(j, fps)) {
        if (iSegment) {
            *iSegment = j;
        }
        return &m_segments[j];
    }

    // after last segment
    if (j >= 0 && t >= TranslateSegmentEnd(j, fps)) {
        if (iSegment) {
            *iSegment = j + 1;
        }
        return nullptr;
    }

    // before first segment
    if (j > 0 && t < TranslateSegmentStart(i, fps)) {
        if (iSegment) {
            *iSegment = -1;
        }
        return nullptr;
    }

    while (i < j) {
        int mid = (i + j) >> 1;

        REFERENCE_TIME midt = TranslateSegmentStart(mid, fps);

        if (t == midt) {
            ret = mid;
            break;
        } else if (t < midt) {
            ret = -1;
            if (j == mid) {
                mid--;
            }
            j = mid;
        } else if (t > midt) {
            ret = mid;
            if (i == mid) {
                mid++;
            }
            i = mid;
        }
    }

    if (0 <= ret && (size_t)ret < m_segments.size()) {
        if (iSegment) {
            *iSegment = ret;
        }
    }

    if (0 <= ret && (size_t)ret < m_segments.size()
            && !m_segments[ret].subs.empty()
            && TranslateSegmentStart(ret, fps) <= t && t < TranslateSegmentEnd(ret, fps)) {
        return &m_segments[ret];
    }

    return nullptr;
}

REFERENCE_TIME CSimpleTextSubtitle::TranslateStart(int i, double fps)
{
    return (i < 0 || size() <= (size_t)i ? -1 :
            m_mode == TIME ? at(i).start :
            m_mode == FRAME ? std::llround(at(i).start * UNITS_FLOAT / fps) :
            0);
}

REFERENCE_TIME CSimpleTextSubtitle::TranslateEnd(int i, double fps)
{
    return (i < 0 || size() <= (size_t)i ? -1 :
            m_mode == TIME ? at(i).end :
            m_mode == FRAME ? std::llround(at(i).end * UNITS_FLOAT / fps) :
            0);
}

REFERENCE_TIME CSimpleTextSubtitle::TranslateSegmentStart(int i, double fps)
{
    return (i < 0 || m_segments.size() <= (size_t)i ? -1 :
            m_mode == TIME ? m_segments[i].start :
            m_mode == FRAME ? std::llround(m_segments[i].start * UNITS_FLOAT / fps) :
            0);
}

REFERENCE_TIME CSimpleTextSubtitle::TranslateSegmentEnd(int i, double fps)
{
    return (i < 0 || m_segments.size() <= (size_t)i ? -1 :
            m_mode == TIME ? m_segments[i].end :
            m_mode == FRAME ? std::llround(m_segments[i].end * UNITS_FLOAT / fps) :
            0);
}

STSStyle* CSimpleTextSubtitle::GetStyle(int i)
{
    STSStyle* style = nullptr;
    Lookup(m_styles, at(i).style, style);

    if (!style) {
        Lookup(m_styles, _T("Default"), style);
    }

    return style;
}

bool CSimpleTextSubtitle::GetStyle(int i, STSStyle& stss)
{
    STSStyle* style = nullptr;
    Lookup(m_styles, at(i).style, style);

    STSStyle* defstyle = nullptr;
    Lookup(m_styles, _T("Default"), defstyle);

    if (!style) {
        if (!defstyle) {
            defstyle = CreateDefaultStyle(DEFAULT_CHARSET);
        }

        style = defstyle;
    }

    if (!style) {
        ASSERT(0);
        return false;
    }

    stss = *style;
    if (stss.relativeTo == STSStyle::AUTO && defstyle) {
        stss.relativeTo = defstyle->relativeTo;
        // If relative to is set to "auto" even for the default style, decide based on the subtitle type
        if (stss.relativeTo == STSStyle::AUTO) {
            if (m_subtitleType == Subtitle::ASS || m_subtitleType == Subtitle::SSA) {
                stss.relativeTo = STSStyle::VIDEO;
            } else {
                stss.relativeTo = STSStyle::WINDOW;
            }
        }
    }

    return true;
}

bool CSimpleTextSubtitle::GetStyle(tstring styleName, STSStyle& stss)
{
    STSStyle* style = nullptr;
    Lookup(m_styles, styleName, style);
    if (!style) {
        return false;
    }

    stss = *style;

    STSStyle* defstyle = nullptr;
    Lookup(m_styles, _T("Default"), defstyle);
    if (defstyle && stss.relativeTo == STSStyle::AUTO) {
        stss.relativeTo = defstyle->relativeTo;
        // If relative to is set to "auto" even for the default style, decide based on the subtitle type
        if (stss.relativeTo == STSStyle::AUTO) {
            if (m_subtitleType == Subtitle::ASS || m_subtitleType == Subtitle::SSA) {
                stss.relativeTo = STSStyle::VIDEO;
            } else {
                stss.relativeTo = STSStyle::WINDOW;
            }
        }
    }

    return true;
}

int CSimpleTextSubtitle::GetCharSet(int i)
{
    const STSStyle* stss = GetStyle(i);
    return stss ? stss->charSet : DEFAULT_CHARSET;
}

bool CSimpleTextSubtitle::IsEntryUnicode(int i)
{
    return at(i).fUnicode;
}

void CSimpleTextSubtitle::ConvertUnicode(int i, bool fUnicode)
{
    STSEntry& stse = at(i);

    if (stse.fUnicode ^ fUnicode) {
        int CharSet = GetCharSet(i);

        stse.str = fUnicode
                   ? MBCSSSAToUnicode(stse.str, CharSet)
                   : UnicodeSSAToMBCS(stse.str, CharSet);

        stse.fUnicode = fUnicode;
    }
}

std::string CSimpleTextSubtitle::GetStrA(int i, bool fSSA)
{
    return WToA(GetStrWA(i, fSSA));
}

std::wstring CSimpleTextSubtitle::GetStrW(int i, bool fSSA)
{
    STSEntry const& stse = at(i);
    int CharSet = GetCharSet(i);

    std::wstring str = stse.str;

    if (!stse.fUnicode) {
        str = MBCSSSAToUnicode(str, CharSet);
    }

    if (!fSSA) {
        str = RemoveSSATags(str, true, CharSet);
    }

    return str;
}

std::wstring CSimpleTextSubtitle::GetStrWA(int i, bool fSSA)
{
    STSEntry const& stse = at(i);
    int CharSet = GetCharSet(i);

    std::wstring str = stse.str;

    if (stse.fUnicode) {
        str = UnicodeSSAToMBCS(str, CharSet);
    }

    if (!fSSA) {
        str = RemoveSSATags(str, false, CharSet);
    }

    return str;
}

void CSimpleTextSubtitle::SetStr(int i, std::string str, bool fUnicode)
{
    SetStr(i, AToW(str), false);
}

void CSimpleTextSubtitle::SetStr(int i, std::wstring str, bool fUnicode)
{
    STSEntry& stse = at(i);

    StringReplace(str, L"\n", L"\\N");

    if (stse.fUnicode && !fUnicode) {
        stse.str = MBCSSSAToUnicode(str, GetCharSet(i));
    } else if (!stse.fUnicode && fUnicode) {
        stse.str = UnicodeSSAToMBCS(str, GetCharSet(i));
    } else {
        stse.str = str;
    }
}

static inline bool comp1(const STSEntry& lhs, const STSEntry& rhs)
{
    if (lhs.start != rhs.start) {
        return lhs.start < rhs.start;
    }
    if (lhs.layer != rhs.layer) {
        return lhs.layer < rhs.layer;
    }
    return lhs.readorder < rhs.readorder;
}

static inline bool comp2(const STSEntry& lhs, const STSEntry& rhs)
{
    return lhs.readorder < rhs.readorder;
}

void CSimpleTextSubtitle::Sort(bool fRestoreReadorder)
{
    std::sort(data(), data() + size(), !fRestoreReadorder ? comp1 : comp2);
    CreateSegments();
}

struct Breakpoint {
    REFERENCE_TIME t;
    bool isStart;

    Breakpoint(REFERENCE_TIME t, bool isStart) : t(t), isStart(isStart) {};
};

static int BreakpointComp(const void* e1, const void* e2)
{
    const Breakpoint* bp1 = (const Breakpoint*)e1;
    const Breakpoint* bp2 = (const Breakpoint*)e2;

    return SGN(bp1->t - bp2->t);
}

void CSimpleTextSubtitle::CreateSegments()
{
    m_segments.clear();

    std::vector<Breakpoint> breakpoints;

    for (size_t i = 0; i < size(); i++) {
        STSEntry& stse = at(i);
        breakpoints.push_back(Breakpoint(stse.start, true));
        breakpoints.push_back(Breakpoint(stse.end, false));
    }

    qsort(breakpoints.data(), breakpoints.size(), sizeof(Breakpoint), BreakpointComp);

    ptrdiff_t startsCount = 0;
    for (size_t i = 1, end = breakpoints.size(); i < end; i++) {
        startsCount += breakpoints[i - 1].isStart ? +1 : -1;
        if (breakpoints[i - 1].t != breakpoints[i].t && startsCount > 0) {
            m_segments.push_back(STSSegment(breakpoints[i - 1].t, breakpoints[i].t));
        }
    }

    STSSegment* segmentsStart = m_segments.data();
    STSSegment* segmentsEnd   = segmentsStart + m_segments.size();
    for (size_t i = 0; i < size(); i++) {
        const STSEntry& stse = at(i);
        STSSegment* segment = std::lower_bound(segmentsStart, segmentsEnd, stse.start, SegmentCompStart);
        for (size_t j = segment - segmentsStart; j < m_segments.size() && m_segments[j].end <= stse.end; j++) {
            m_segments[j].subs.push_back(int(i));
        }
    }

    OnChanged();
    /*
        for (size_t i = 0, j = m_segments.GetCount(); i < j; i++) {
            STSSegment& stss = m_segments[i];

            TRACE(_T("%d - %d"), stss.start, stss.end);

            for (size_t k = 0, l = stss.subs.GetCount(); k < l; k++) {
                TRACE(_T(", %d"), stss.subs[k]);
            }

            TRACE(_T("\n"));
        }
    */
}

bool CSimpleTextSubtitle::Open(tstring fn, int CharSet, tstring name, tstring videoName)
{
    Empty();

    CWebTextFile f(CTextFile::UTF8);
    if (!f.Open(fn.c_str())) {
        return false;
    }

    if (name.empty()) {
        name = Subtitle::GuessSubtitleName(fn, videoName, m_lcid, m_langname, m_eHearingImpaired);
    }

    return Open(&f, CharSet, name);
}

bool CSimpleTextSubtitle::Open(BYTE* data, int length, int CharSet, tstring provider, tstring lang, tstring ext) {
    Empty();

    m_provider = provider;
    tstring name;
    name = ::Format(_T("%s.%s"), lang.c_str(), ext.c_str());
    std::string temp = StringToA(lang);
    m_lcid = ISOLang::ISO6391ToLcid(temp.c_str());
    if (m_lcid > 0) {
        m_langname = ISOLang::LCIDToLanguage(m_lcid);
    }
    if (m_langname.empty()) {
        m_langname = ISOLang::ISO639XToLanguage(temp.c_str());
    }
    return Open(data, length, CharSet, name);
}

bool CSimpleTextSubtitle::Open(tstring data, CTextFile::enc SaveCharSet, int ReadCharSet, tstring provider, tstring lang, tstring ext) {
    Empty();

    m_provider = provider;
    tstring name;
    name = ::Format(_T("%s.%s"), lang.c_str(), ext.c_str());
    std::string temp = StringToA(lang);
    m_lcid = ISOLang::ISO6391ToLcid(temp.c_str());
    if (m_lcid > 0) {
        m_langname = ISOLang::LCIDToLanguage(m_lcid);
    }
    if (m_langname.empty()) {
        m_langname = ISOLang::ISO639XToLanguage(temp.c_str());
    }
    TCHAR path[MAX_PATH];
    if (!GetTempPath(MAX_PATH, path)) {
        return false;
    }

    TCHAR fn[MAX_PATH];
    if (!GetTempFileName(path, _T("vs"), 0, fn)) {
        return false;
    }

    CTextFile f;
    if (!f.Save(fn, SaveCharSet)) {
        return false;
    }

    f.WriteString(data);
    f.Flush();
    f.Close();

    bool fRet = Open(fn, ReadCharSet, name);

    _tremove(fn);

    m_path = _T("");

    return fRet;
}

bool CSimpleTextSubtitle::Open(CTextFile* f, int CharSet, tstring name) {
    Empty();

    if (m_langname.empty() && m_lcid > 0) {
        m_langname = ISOLang::LCIDToLanguage(m_lcid);
    }

#if USE_LIBASS
    if (m_renderUsingLibass) {
        if (lstrcmpi(PathFindExtensionW(f->GetFilePath()), L".ass") == 0 || lstrcmpi(PathFindExtensionW(f->GetFilePath()), L".ssa") == 0) {
            CreateDefaultStyle(CharSet);
            m_path = f->GetFilePath();
            LoadASSFile(Subtitle::SubType::SSA);
            m_subtitleType = Subtitle::SubType::SSA;
            OpenSubStationAlpha(f, *this, CharSet);
            CWebTextFile f2(CTextFile::UTF8);
            if (f2.Open(f->GetFilePath() + _T(".style"))) {
                OpenSubStationAlpha(&f2, *this, CharSet);
            }
        } else if (lstrcmpi(PathFindExtensionW(f->GetFilePath()), L".srt") == 0) {
            CreateDefaultStyle(CharSet);
            m_path = f->GetFilePath();
            LoadASSFile(Subtitle::SubType::SRT);
            m_subtitleType = Subtitle::SubType::SRT;
            OpenSubRipper(f, *this, CharSet);
        }

        if (m_assloaded) {
            m_name = name;
            m_encoding = f->GetEncoding();
            m_mode = TIME;

            ChangeUnknownStylesToDefault();

            if (m_storageRes.cx <= 0 || m_storageRes.cy <= 0) {
                if (m_layoutRes.cx > 0 && m_layoutRes.cy > 0) {
                    m_storageRes = m_layoutRes;
                } else if (m_playRes.cx > 0 && m_playRes.cy > 0) {
                    m_storageRes = m_playRes;
                } else {
                    m_storageRes = MakeSize(384, 288);
                }
            }
            if (m_playRes.cx <= 0 || m_playRes.cy <= 0) {
                m_playRes = m_storageRes;
            }
            return true;
        }
    }
#endif

    ULONGLONG pos = f->GetPosition();

    auto functs = PreferredOpenFuncts(f->GetFilePath());

    for (int i: functs) {
        if (!OpenFuncts[i].open(f, *this, CharSet)) {
            if (!empty()) {
                tstring lastLine;
                size_t n = CountLines(f, pos, f->GetPosition(), lastLine);
                tstring msg;
                msg = ::Format(_T("Unable to parse the subtitle file. Syntax error at line %Iu:\n\"%s\""), n + 1, lastLine.c_str());
                MessageBox(GetActiveWindow(), msg.c_str(), nullptr, MB_OK | MB_ICONERROR);
                Empty();
                break;
            }

            f->Seek(pos, CTextFile::begin);
            Empty();
            continue;
        }

        m_name = name;
        m_subtitleType = OpenFuncts[i].type;
        m_mode = OpenFuncts[i].mode;
        m_encoding = f->GetEncoding();
        m_path = f->GetFilePath();

        // No need to call Sort() or CreateSegments(), everything is done on the fly

        CWebTextFile f2(CTextFile::UTF8);
        if (f2.Open((f->GetFilePath() + _T(".style")).c_str())) {
            OpenSubStationAlpha(&f2, *this, CharSet);
        }

        CreateDefaultStyle(CharSet);

        ChangeUnknownStylesToDefault();

        if (m_layoutRes.cx > 0 && m_layoutRes.cy > 0) {
            m_storageRes = m_layoutRes;
        } else if (m_storageRes.cx <= 0 || m_storageRes.cy <= 0) {
            if (m_playRes.cx > 0 && m_playRes.cy > 0) {
                m_storageRes = m_playRes;
            } else {
                m_storageRes = MakeSize(384, 288);
            }
        }
        if (m_playRes.cx <= 0 || m_playRes.cy <= 0) {
            m_playRes = m_storageRes;
        }

        return true;
    }

    m_path = _T("");
    return false;
}

#if USE_LIBASS

void CSimpleTextSubtitle::SetSubRenderSettings(SubRendererSettings settings) {
    bool wasUsingLibass = subRendererSettings.renderUsingLibass;
    subRendererSettings = settings;
    if (settings.renderUsingLibass || wasUsingLibass) {
        ResetASS();
    }
}

void CSimpleTextSubtitle::ResetASS() {
    if (subRendererSettings.renderUsingLibass) { 
        m_renderUsingLibass = true;
        if (!m_path.IsEmpty()) {
            LoadASSFile(m_subtitleType);
        }
    } else {
        if (m_assloaded) {
            UnloadASS();
        }
        m_renderUsingLibass = false;
    }
}

bool CSimpleTextSubtitle::LoadASSFile(Subtitle::SubType subType) {
    if (m_path.IsEmpty() || !PathUtils::Exists(m_path)) return false;
    UnloadASS();

    m_assfontloaded = false;

    m_ass = decltype(m_ass)(ass_library_init());
    ass_set_extract_fonts(m_ass.get(), true);
    ass_set_style_overrides(m_ass.get(), NULL);

    m_renderer = decltype(m_renderer)(ass_renderer_init(m_ass.get()));
    ass_set_use_margins(m_renderer.get(), false);
    ass_set_font_scale(m_renderer.get(), 1.0);

    STSStyle defStyle;
    GetDefaultStyle(defStyle);
    if (subType == Subtitle::SRT) {
        m_track = decltype(m_track)(srt_read_file(m_ass.get(), const_cast<char*>((const char*)(std::string)m_path), defStyle.charSet, defStyle, subRendererSettings));
        if (m_storageRes == MakeSize(0, 0)) {
            m_storageRes = MakeSize(defStyle.SrtResX, defStyle.SrtResY);
        }
    } else { //subType == Subtitle::SSA/ASS
        m_track = decltype(m_track)(ass_read_file(m_ass.get(), const_cast<char*>((const char*)(std::string)m_path), "UTF-8"));
        if (m_storageRes == MakeSize(0, 0)) {
            m_storageRes = MakeSize(defStyle.SrtResX, defStyle.SrtResY);
        }
    }

    if (!m_track) return false;

    std::string tmpFontName = StringToA(defStyle.fontName);
    ass_set_fonts(m_renderer.get(), NULL, std::string(tmpFontName).c_str(), ASS_FONTPROVIDER_DIRECTWRITE, NULL, 0);

    m_assloaded = true;
    m_assfontloaded = true;

    return true;
}

bool CSimpleTextSubtitle::LoadASSTrack(char* data, int size, Subtitle::SubType subType) {
    UnloadASS();
    m_assfontloaded = false;

    m_ass = decltype(m_ass)(ass_library_init());
    m_renderer = decltype(m_renderer)(ass_renderer_init(m_ass.get()));
    m_track = decltype(m_track)(ass_new_track(m_ass.get()));

    if (!m_track) return false;

    STSStyle defStyle;
    GetDefaultStyle(defStyle);
    if (subType == Subtitle::SRT) {
        std::stringstream srtData;
        srtData.write(data, size);
        srt_read_data(m_ass.get(), m_track.get(), srtData, defStyle.charSet, defStyle, subRendererSettings);
    } else { //subType == Subtitle::SSA/ASS
        ass_process_codec_private(m_track.get(), data, size);
    }
    std::string tmpFontName = StringToA(defStyle.fontName);
    ass_set_fonts(m_renderer.get(), NULL, std::string(tmpFontName).c_str(), ASS_FONTPROVIDER_DIRECTWRITE, NULL, 0);
    //don't set m_assfontloaded here, in case we can load embedded fonts later?

    m_assloaded = true;
    return true;
}

void CSimpleTextSubtitle::LoadASSFont(IPin* pPin, ASS_Library* ass, ASS_Renderer* renderer) {
    // Try to load fonts in the container
    ComPtr<IAMGraphStreams> graphStreams;
    ComPtr<IDSMResourceBag> bag;
    if (m_pGraph && SUCCEEDED(m_pGraph->QueryInterface(IID_PPV_ARGS(&graphStreams))) &&
        SUCCEEDED(graphStreams->FindUpstreamInterface(pPin, IID_PPV_ARGS(&bag), AM_INTF_SEARCH_FILTER))) {
        for (DWORD i = 0; i < bag->ResGetCount(); ++i) {
            _bstr_t name, desc, mime;
            BYTE* pData = nullptr;
            DWORD len = 0;
            if (SUCCEEDED(bag->ResGet(i, &name.GetBSTR(), &desc.GetBSTR(), &mime.GetBSTR(), &pData, &len, nullptr))) {
                if (wcscmp(mime.GetBSTR(), L"application/x-truetype-font") == 0 ||
                    wcscmp(mime.GetBSTR(), L"application/vnd.ms-opentype") == 0) // TODO: more mimes?
                {
                    ass_add_font(ass, (char*)name, (char*)pData, len);
                    // TODO: clear these fonts somewhere?
                }
                CoTaskMemFree(pData);
            }
        }
        m_assfontloaded = true;
        STSStyle defStyle;
        GetDefaultStyle(defStyle);
        std::string tmpFontName = StringToA(defStyle.fontName);
        ass_set_fonts(renderer, NULL, std::string(tmpFontName).c_str(), ASS_FONTPROVIDER_DIRECTWRITE, NULL, 0);
    }
}

void CSimpleTextSubtitle::UnloadASS() {
    m_assloaded = false;
    if (m_track) m_track.reset();
    if (m_renderer) m_renderer.reset();
    if (m_ass) m_ass.reset();
}

void CSimpleTextSubtitle::LoadASSSample(char *data, int dataSize, REFERENCE_TIME tStart, REFERENCE_TIME tStop) {
    if (m_renderUsingLibass) {
        if (m_subtitleType == Subtitle::SRT) { //received SRT sample, try to use libass to handle
            if (!m_assloaded) { //create ass header
                UnloadASS();
                m_assfontloaded = false;

                m_ass = decltype(m_ass)(ass_library_init());
                m_renderer = decltype(m_renderer)(ass_renderer_init(m_ass.get()));
                m_track = decltype(m_track)(ass_new_track(m_ass.get()));

                char outBuffer[1024];
                STSStyle defStyle;
                GetDefaultStyle(defStyle);
                srt_header(outBuffer, defStyle, subRendererSettings);
                ass_process_codec_private(m_track.get(), outBuffer, static_cast<int>(strnlen_s(outBuffer, sizeof(outBuffer))));
                m_assloaded = true;
            }

            if (m_assloaded) {
                char subLineData[1024]{};
                strncpy_s(subLineData, _countof(subLineData), data, dataSize);
                std::string str = subLineData;

                // This is the way i use to get a unique id for the subtitle line
                // It will only fail in the case there is 2 or more lines with the same start timecode
                // (Need to check if the matroska muxer join lines in such a case)
                REFERENCE_TIME m_iSubLineCount = tStart / 10000;

                // Change srt tags to ass tags
                STSStyle defStyle;
                GetDefaultStyle(defStyle);
                ParseSrtLine(str, defStyle);

                // Add the custom tags
                std::string tmpCustomTags = StringToA(defStyle.customTags);
                str.insert(0, std::string(tmpCustomTags));

                // Add blur
                char blur[20]{};
                _snprintf_s(blur, _TRUNCATE, "{\\blur%u}", defStyle.fBlur);
                str.insert(0, blur);

                // ASS in MKV: ReadOrder, Layer, Style, Name, MarginL, MarginR, MarginV, Effect, Text
                char outBuffer[1024]{};
                _snprintf_s(outBuffer, _TRUNCATE, "%lld,0,Default,Main,0,0,0,,%s", m_iSubLineCount, str.c_str());
                ass_process_chunk(m_track.get(), outBuffer, static_cast<int>(strnlen_s(outBuffer, sizeof(outBuffer))), tStart / 10000, (tStop - tStart) / 10000);
            }
        }
    }
}

#endif

bool CSimpleTextSubtitle::Open(tstring provider, BYTE* data, int len, int CharSet, tstring name, Subtitle::HearingImpairedType eHearingImpaired, LCID lcid)
{
    m_provider = provider;
    m_eHearingImpaired = eHearingImpaired;
    m_lcid = lcid;

    return Open(data, len, CharSet, name);
}

bool CSimpleTextSubtitle::Open(BYTE* data, int len, int CharSet, tstring name)
{
    TCHAR path[MAX_PATH];
    if (!GetTempPath(MAX_PATH, path)) {
        return false;
    }

    TCHAR fn[MAX_PATH];
    if (!GetTempFileName(path, _T("vs"), 0, fn)) {
        return false;
    }

    FILE* tmp = nullptr;
    if (_tfopen_s(&tmp, fn, _T("wb"))) {
        return false;
    }

    int i = 0;
    for (; i <= (len - 1024); i += 1024) {
        fwrite(&data[i], 1024, 1, tmp);
    }
    if (len > i) {
        fwrite(&data[i], len - i, 1, tmp);
    }

    fclose(tmp);

    bool fRet = Open(fn, CharSet, name);

    _tremove(fn);

    m_path = _T("");

    return fRet;
}

bool CSimpleTextSubtitle::SaveAs(tstring fn, Subtitle::SubType type,
                                 double fps /*= -1*/, LONGLONG delay /*= 0*/,
                                 CTextFile::enc e /*= CTextFile::DEFAULT_ENCODING*/, bool bCreateExternalStyleFile /*= true*/)
{
    LPCTSTR ext = Subtitle::GetSubtitleFileExt(type);
    if (ext && StringICompare(StringMid(fn, StringRFind(fn, '.') + 1), ext)) {
        if (fn[int(fn.size()) - 1] != '.') {
            fn += _T(".");
        }
        fn += ext;
    }

    CTextFile f;
    if (!f.Save(fn.c_str(), e)) {
        return false;
    }

    if (type == Subtitle::SMI) {
        tstring str;

        str += _T("<SAMI>\n<HEAD>\n");
        str += _T("<STYLE TYPE=\"text/css\">\n");
        str += _T("<!--\n");
        str += _T("P {margin-left: 16pt; margin-right: 16pt; margin-bottom: 16pt; margin-top: 16pt;\n");
        str += _T("   text-align: center; font-size: 18pt; font-family: calibri; font-weight: bold; color: #f0f0f0;}\n");
        str += _T(".UNKNOWNCC {Name:Unknown; lang:en-US; SAMIType:CC;}\n");
        str += _T("-->\n");
        str += _T("</STYLE>\n");
        str += _T("</HEAD>\n");
        str += _T("\n");
        str += _T("<BODY>\n");

        f.WriteString(str);
    } else if (type == Subtitle::SSA || type == Subtitle::ASS) {
        tstring str;

        str  = _T("[Script Info]\n");
        str += _T("; Note: This file was saved by MPC-HC.\n");
        str += (type == Subtitle::SSA) ? _T("ScriptType: v4.00\n") : _T("ScriptType: v4.00+\n");
        str += (m_collisions == 0) ? _T("Collisions: Normal\n") : _T("Collisions: Reverse\n");
        if (type == Subtitle::ASS && m_fScaledBAS) {
            str += _T("ScaledBorderAndShadow: Yes\n");
        }
        if (m_sYCbCrMatrix.empty()) {
            str += _T("YCbCr Matrix: None\n");
        } else {
            str += _T("YCbCr Matrix: ") + m_sYCbCrMatrix + _T("\n");
        }
        str += ::Format(_T("PlayResX: %d\n"), m_playRes.cx);
        str += ::Format(_T("PlayResY: %d\n"), m_playRes.cy);
        if (m_layoutRes.cx > 0 && m_layoutRes.cy > 0) {
            str += ::Format(_T("LayoutResX: %d\n"), m_layoutRes.cx);
            str += ::Format(_T("LayoutResY: %d\n"), m_layoutRes.cy);
        }
        str += _T("Timer: 100.0000\n");
        str += _T("\n");
        str += (type == Subtitle::SSA)
               ? _T("[V4 Styles]\nFormat: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding\n")
               : _T("[V4+ Styles]\nFormat: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\n");

        f.WriteString(str);

        str  = (type == Subtitle::SSA)
               ? _T("Style: %s,%s,%d,&H%06x,&H%06x,&H%06x,&H%06x,%d,%d,%d,%.2f,%.2f,%d,%d,%d,%d,%d,%d\n")
               : _T("Style: %s,%s,%d,&H%08x,&H%08x,&H%08x,&H%08x,%d,%d,%d,%d,%.2f,%.2f,%.2f,%.2f,%d,%.2f,%.2f,%d,%d,%d,%d,%d\n");

        for (const auto& style : m_styles) {
            const tstring& key = style.first;
            STSStyle* s = style.second;

            if (type == Subtitle::SSA) {
                tstring str2;
                str2 = ::Format(str, key.c_str(), s->fontName.c_str(), (int)s->fontSize, s->colors[0] & 0xffffff, s->colors[1] & 0xffffff, s->colors[2] & 0xffffff, s->colors[3] & 0xffffff, s->fontWeight > FW_NORMAL ? -1 : 0, s->fItalic ? -1 : 0, s->borderStyle == 0 ? 1 : s->borderStyle == 1 ? 3 : 0, s->outlineWidthY, s->shadowDepthY, s->scrAlignment <= 3 ? s->scrAlignment : s->scrAlignment <= 6 ? ((s->scrAlignment - 3) | 8) : s->scrAlignment <= 9 ? ((s->scrAlignment - 6) | 4) : 2, s->marginRect.left, s->marginRect.right, (s->marginRect.top + s->marginRect.bottom) / 2, s->alpha[0], s->charSet);
                f.WriteString(str2);
            } else {
                tstring str2;
                str2 = ::Format(str, key.c_str(), s->fontName.c_str(), (int)s->fontSize, (s->colors[0] & 0xffffff) | (s->alpha[0] << 24), (s->colors[1] & 0xffffff) | (s->alpha[1] << 24), (s->colors[2] & 0xffffff) | (s->alpha[2] << 24), (s->colors[3] & 0xffffff) | (s->alpha[3] << 24), s->fontWeight > FW_NORMAL ? -1 : 0, s->fItalic ? -1 : 0, s->fUnderline ? -1 : 0, s->fStrikeOut ? -1 : 0, s->fontScaleX, s->fontScaleY, s->fontSpacing, s->fontAngleZ, s->borderStyle == 0 ? 1 : s->borderStyle == 1 ? 3 : 0, s->outlineWidthY, s->shadowDepthY, s->scrAlignment, s->marginRect.left, s->marginRect.right, (int)((s->marginRect.top + s->marginRect.bottom) / 2), s->charSet);
                f.WriteString(str2);
            }
        }

        if (!empty()) {
            str  = _T("\n");
            str += _T("[Events]\n");
            str += (type == Subtitle::SSA)
                   ? _T("Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\n")
                   : _T("Format: Layer, Start, End, Style, Actor, MarginL, MarginR, MarginV, Effect, Text\n");
            f.WriteString(str);
        }
    }

    std::wstring fmt =
        type == Subtitle::SRT ? L"%d\n%02d:%02d:%02d,%03d --> %02d:%02d:%02d,%03d\n%s\n\n" :
        type == Subtitle::SUB ? L"{%d}{%d}%s\n" :
        type == Subtitle::SMI ? L"<SYNC Start=%d><P Class=UNKNOWNCC>\n%s\n<SYNC Start=%d><P Class=UNKNOWNCC>&nbsp;\n" :
        type == Subtitle::PSB ? L"{%d:%02d:%02d}{%d:%02d:%02d}%s\n" :
        type == Subtitle::SSA ? L"Dialogue: Marked=0,%d:%02d:%02d.%02d,%d:%02d:%02d.%02d,%s,%s,%04d,%04d,%04d,%s,%s\n" :
        type == Subtitle::ASS ? L"Dialogue: %d,%d:%02d:%02d.%02d,%d:%02d:%02d.%02d,%s,%s,%04d,%04d,%04d,%s,%s\n" :
        L"";
    //  Sort(true);

    if (m_mode == FRAME) {
        delay = std::lround(delay * fps / 1000.0);
    }

    for (int i = 0, j = (int)size(), k = 0; i < j; i++) {
        STSEntry& stse = at(i);

        int t1 = (int)(RT2MS(TranslateStart(i, fps)) + delay);
        if (t1 < 0) {
            k++;
            continue;
        }

        int t2 = (int)(RT2MS(TranslateEnd(i, fps)) + delay);

        int hh1 = (t1 / 60 / 60 / 1000);
        int mm1 = (t1 / 60 / 1000) % 60;
        int ss1 = (t1 / 1000) % 60;
        int ms1 = (t1) % 1000;
        int hh2 = (t2 / 60 / 60 / 1000);
        int mm2 = (t2 / 60 / 1000) % 60;
        int ss2 = (t2 / 1000) % 60;
        int ms2 = (t2) % 1000;

        std::wstring str = f.IsUnicode()
                       ? GetStrW(i, type == Subtitle::SSA || type == Subtitle::ASS)
                       : GetStrWA(i, type == Subtitle::SSA || type == Subtitle::ASS);

        std::wstring str2;

        if (type == Subtitle::SRT) {
            str2 = ::Format(fmt, i - k + 1, hh1, mm1, ss1, ms1, hh2, mm2, ss2, ms2, str.c_str());
        } else if (type == Subtitle::SUB) {
            StringReplace(str, '\n', '|');
            str2 = ::Format(fmt, int(t1 * fps / 1000), int(t2 * fps / 1000), str.c_str());
        } else if (type == Subtitle::SMI) {
            StringReplace(str, L"\n", L"<br>");
            str2 = ::Format(fmt, t1, str.c_str(), t2);
        } else if (type == Subtitle::PSB) {
            StringReplace(str, '\n', '|');
            str2 = ::Format(fmt, hh1, mm1, ss1, hh2, mm2, ss2, str.c_str());
        } else if (type == Subtitle::SSA) {
            StringReplace(str, L"\n", L"\\N");
            str2 = ::Format(fmt, hh1, mm1, ss1, ms1 / 10, hh2, mm2, ss2, ms2 / 10, TToW(stse.style).c_str(), TToW(stse.actor).c_str(), stse.marginRect.left, stse.marginRect.right, (stse.marginRect.top + stse.marginRect.bottom) / 2, TToW(stse.effect).c_str(), str.c_str());
        } else if (type == Subtitle::ASS) {
            StringReplace(str, L"\n", L"\\N");
            str2 = ::Format(fmt, stse.layer, hh1, mm1, ss1, ms1 / 10, hh2, mm2, ss2, ms2 / 10, TToW(stse.style).c_str(), TToW(stse.actor).c_str(), stse.marginRect.left, stse.marginRect.right, (stse.marginRect.top + stse.marginRect.bottom) / 2, TToW(stse.effect).c_str(), str.c_str());
        }

        f.WriteString(str2);
    }

    //  Sort();

    if (type == Subtitle::SMI) {
        f.WriteString(_T("</BODY>\n</SAMI>\n"));
    }

    STSStyle* s;
    if (bCreateExternalStyleFile && !m_bUsingPlayerDefaultStyle && Lookup(m_styles, _T("Default"), s) && type != Subtitle::SSA && type != Subtitle::ASS) {
        CTextFile file;
        if (!file.Save((fn + _T(".style")).c_str(), e)) {
            return false;
        }

        tstring str, str2;

        str += _T("ScriptType: v4.00+\n");
        str += _T("PlayResX: %d\n");
        str += _T("PlayResY: %d\n");
        str += _T("\n");
        str += _T("[V4+ Styles]\nFormat: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\n");
        str2 = ::Format(str, m_storageRes.cx, m_storageRes.cy);
        file.WriteString(str2);

        str  = _T("Style: Default,%s,%d,&H%08x,&H%08x,&H%08x,&H%08x,%d,%d,%d,%d,%.2f,%.2f,%.2f,%.2f,%d,%.2f,%.2f,%d,%d,%d,%d,%d\n");
        str2 = ::Format(str, s->fontName.c_str(), (int)s->fontSize, (s->colors[0] & 0xffffff) | (s->alpha[0] << 24), (s->colors[1] & 0xffffff) | (s->alpha[1] << 24), (s->colors[2] & 0xffffff) | (s->alpha[2] << 24), (s->colors[3] & 0xffffff) | (s->alpha[3] << 24), s->fontWeight > FW_NORMAL ? -1 : 0, s->fItalic ? -1 : 0, s->fUnderline ? -1 : 0, s->fStrikeOut ? -1 : 0, s->fontScaleX, s->fontScaleY, s->fontSpacing, s->fontAngleZ, s->borderStyle == 0 ? 1 : s->borderStyle == 1 ? 3 : 0, s->outlineWidthY, s->shadowDepthY, s->scrAlignment, s->marginRect.left, s->marginRect.right, (int)((s->marginRect.top + s->marginRect.bottom) / 2), s->charSet);
        file.WriteString(str2);
    }

    m_provider = _T("Local");
    m_path = fn;

    return true;
}

////////////////////////////////////////////////////////////////////

STSStyle::STSStyle()
{
    SetDefault();
}

void STSStyle::SetDefault()
{
    marginRect = MakeRect(20, 20, 20, 20);
    scrAlignment = 2;
    borderStyle = 0;
    outlineWidthX = outlineWidthY = 2;
    shadowDepthX = shadowDepthY = 3;
    colors[0] = 0x00ffffff;
    colors[1] = 0x0000ffff;
    colors[2] = 0x00000000;
    colors[3] = 0x00000000;
    alpha[0] = 0x00;
    alpha[1] = 0x00;
    alpha[2] = 0x00;
    alpha[3] = 0x80;
    charSet = DEFAULT_CHARSET;
    fontName = _T("Calibri");
    fontSize = 18;
    fontScaleX = fontScaleY = 100;
    fontSpacing = 0;
    fontWeight = FW_BOLD;
    fItalic = false;
    fUnderline = false;
    fStrikeOut = false;
    fBlur = 0;
    fGaussianBlur = 0;
    fontShiftX = fontShiftY = fontAngleZ = fontAngleX = fontAngleY = 0;
    relativeTo = STSStyle::AUTO;
}

bool STSStyle::operator == (const STSStyle& s) const
{
    return (RectEqual(marginRect, s.marginRect)
            && scrAlignment == s.scrAlignment
            && borderStyle == s.borderStyle
            && outlineWidthX == s.outlineWidthX
            && outlineWidthY == s.outlineWidthY
            && shadowDepthX == s.shadowDepthX
            && shadowDepthY == s.shadowDepthY
            && colors == s.colors
            && alpha == s.alpha
            && fBlur == s.fBlur
            && fGaussianBlur == s.fGaussianBlur
            && relativeTo == s.relativeTo
            && IsFontStyleEqual(s));
}

bool STSStyle::IsFontStyleEqual(const STSStyle& s) const
{
    return (
               charSet == s.charSet
               && fontName == s.fontName
               && fontSize == s.fontSize
               && fontScaleX == s.fontScaleX
               && fontScaleY == s.fontScaleY
               && fontSpacing == s.fontSpacing
               && fontWeight == s.fontWeight
               && fItalic == s.fItalic
               && fUnderline == s.fUnderline
               && fStrikeOut == s.fStrikeOut
               && fontAngleZ == s.fontAngleZ
               && fontAngleX == s.fontAngleX
               && fontAngleY == s.fontAngleY
               // patch f001. fax fay patch (many instances at line)
               && fontShiftX == s.fontShiftX
               && fontShiftY == s.fontShiftY);
}

STSStyle& STSStyle::operator = (LOGFONT& lf)
{
    charSet = lf.lfCharSet;
    fontName = lf.lfFaceName;
    HDC hDC = GetDC(nullptr);
    fontSize = -MulDiv(lf.lfHeight, 72, GetDeviceCaps(hDC, LOGPIXELSY));
    ReleaseDC(nullptr, hDC);
    //  fontAngleZ = lf.lfEscapement/10.0;
    fontWeight = lf.lfWeight;
    fItalic = lf.lfItalic;
    fUnderline = lf.lfUnderline;
    fStrikeOut = lf.lfStrikeOut;
    return *this;
}

LOGFONTA& operator <<= (LOGFONTA& lfa, const STSStyle& s)
{
    lfa.lfCharSet = (BYTE)s.charSet;
    strncpy_s(lfa.lfFaceName, LF_FACESIZE, StringToA(s.fontName).c_str(), _TRUNCATE);
    HDC hDC = GetDC(nullptr);
    lfa.lfHeight = -MulDiv((int)(s.fontSize + 0.5), GetDeviceCaps(hDC, LOGPIXELSY), 72);
    ReleaseDC(nullptr, hDC);
    lfa.lfWeight = s.fontWeight;
    lfa.lfItalic = s.fItalic ? -1 : 0;
    lfa.lfUnderline = s.fUnderline ? -1 : 0;
    lfa.lfStrikeOut = s.fStrikeOut ? -1 : 0;
    return lfa;
}

LOGFONTW& operator <<= (LOGFONTW& lfw, const STSStyle& s)
{
    lfw.lfCharSet = (BYTE)s.charSet;
    wcsncpy_s(lfw.lfFaceName, LF_FACESIZE, StringToW(s.fontName).c_str(), _TRUNCATE);
    HDC hDC = GetDC(nullptr);
    lfw.lfHeight = -MulDiv((int)(s.fontSize + 0.5), GetDeviceCaps(hDC, LOGPIXELSY), 72);
    ReleaseDC(nullptr, hDC);
    lfw.lfWeight = s.fontWeight;
    lfw.lfItalic = s.fItalic ? -1 : 0;
    lfw.lfUnderline = s.fUnderline ? -1 : 0;
    lfw.lfStrikeOut = s.fStrikeOut ? -1 : 0;
    return lfw;
}

tstring& operator <<= (tstring& style, const STSStyle& s)
{
    style = ::Format(_T("%d;%d;%d;%d;%d;%d;%f;%f;%f;%f;0x%06lx;0x%06lx;0x%06lx;0x%06lx;0x%02x;0x%02x;0x%02x;0x%02x;%d;%s;%f;%f;%f;%f;%ld;%d;%d;%d;%d;%f;%f;%f;%f;%d"), s.marginRect.left, s.marginRect.right, s.marginRect.top, s.marginRect.bottom, s.scrAlignment, s.borderStyle, s.outlineWidthX, s.outlineWidthY, s.shadowDepthX, s.shadowDepthY, s.colors[0], s.colors[1], s.colors[2], s.colors[3], s.alpha[0], s.alpha[1], s.alpha[2], s.alpha[3], s.charSet, s.fontName.c_str(), s.fontSize, s.fontScaleX, s.fontScaleY, s.fontSpacing, s.fontWeight, s.fItalic, s.fUnderline, s.fStrikeOut, s.fBlur, s.fGaussianBlur, s.fontAngleZ, s.fontAngleX, s.fontAngleY, s.relativeTo);

    return style;
}

STSStyle& operator <<= (STSStyle& s, const tstring& style)
{
    s.SetDefault();

    try {
        std::wstring str = TToW(style);
        LPCWSTR pszBuff = str.c_str();
        int nBuffLength = int(str.size());
        if (StringFind(str, ';') >= 0) {
            s.marginRect.left = GetInt(pszBuff, nBuffLength, L';');
            s.marginRect.right = GetInt(pszBuff, nBuffLength, L';');
            s.marginRect.top = GetInt(pszBuff, nBuffLength, L';');
            s.marginRect.bottom = GetInt(pszBuff, nBuffLength, L';');
            s.scrAlignment = GetInt(pszBuff, nBuffLength, L';');
            s.borderStyle = GetInt(pszBuff, nBuffLength, L';');
            s.outlineWidthX = GetFloat(pszBuff, nBuffLength, L';');
            s.outlineWidthY = GetFloat(pszBuff, nBuffLength, L';');
            s.shadowDepthX = GetFloat(pszBuff, nBuffLength, L';');
            s.shadowDepthY = GetFloat(pszBuff, nBuffLength, L';');
            for (size_t i = 0; i < 4; i++) {
                s.colors[i] = (COLORREF)GetInt(pszBuff, nBuffLength, L';');
            }
            for (size_t i = 0; i < 4; i++) {
                s.alpha[i] = (BYTE)GetInt(pszBuff, nBuffLength, L';');
            }
            s.charSet = GetInt(pszBuff, nBuffLength, L';');
            s.fontName = WToT(GetStrW(pszBuff, nBuffLength, L';'));
            s.fontSize = GetFloat(pszBuff, nBuffLength, L';');
            s.fontScaleX = GetFloat(pszBuff, nBuffLength, L';');
            s.fontScaleY = GetFloat(pszBuff, nBuffLength, L';');
            s.fontSpacing = GetFloat(pszBuff, nBuffLength, L';');
            s.fontWeight = GetInt(pszBuff, nBuffLength, L';');
            s.fItalic = GetInt(pszBuff, nBuffLength, L';');
            s.fUnderline = GetInt(pszBuff, nBuffLength, L';');
            s.fStrikeOut = GetInt(pszBuff, nBuffLength, L';');
            s.fBlur = GetInt(pszBuff, nBuffLength, L';');
            s.fGaussianBlur = GetFloat(pszBuff, nBuffLength, L';');
            s.fontAngleZ = GetFloat(pszBuff, nBuffLength, L';');
            s.fontAngleX = GetFloat(pszBuff, nBuffLength, L';');
            s.fontAngleY = GetFloat(pszBuff, nBuffLength, L';');
            s.relativeTo = (STSStyle::RelativeTo)GetInt(pszBuff, nBuffLength, L';');
        }
    } catch (...) {
        s.SetDefault();
    }

    return s;
}

static bool OpenRealText(CTextFile* file, CSimpleTextSubtitle& ret, int CharSet)
{
    std::wstring szFile;
    std::wstring buff;

    while (file->ReadString(buff)) {
        FastTrim(buff);
        if (buff.empty()) {
            continue;
        }

        // Make sure that the subtitle file starts with a <window> tag
        if (szFile.empty() && StringICompare(buff, _T("<window")) < 0) {
            return false;
        }

        szFile += _T("\n") + buff;
    }

    CRealTextParser RealTextParser;
    if (!RealTextParser.ParseRealText(szFile)) {
        return false;
    }

    CRealTextParser::Subtitles crRealText = RealTextParser.GetParsedSubtitles();

    for (auto i = crRealText.m_mapLines.cbegin(); i != crRealText.m_mapLines.cend(); ++i) {
        ret.Add(
            SubRipper2SSA(i->second.c_str()),
            file->IsUnicode(),
            MS2RT(i->first.first),
            MS2RT(i->first.second));
    }

    return !ret.empty();
}
