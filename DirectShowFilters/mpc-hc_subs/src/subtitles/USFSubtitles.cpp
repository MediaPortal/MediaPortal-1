/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2016 see Authors.txt
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
#include "USFSubtitles.h"
#include <MsXml.h>
#include <algorithm>

#define DeclareNameAndValue(pNode, name, val) \
    _bstr_t name##Raw;                        \
    pNode->get_nodeName(name##Raw.GetAddress());          \
    std::wstring name(StringWiden((const wchar_t*)name##Raw)); \
    StringLower(name);                        \
    _variant_t val;                           \
    pNode->get_nodeValue(&val);

#define BeginEnumAttribs(pNode, pChild)                                                               \
    {                                                                                                 \
        ComPtr<IXMLDOMNamedNodeMap> pAttribs;                                                        \
        if (SUCCEEDED(pNode->get_attributes(&pAttribs)) && pAttribs != nullptr) {                     \
            ComPtr<IXMLDOMNode> pChild;                                                              \
            for (pAttribs->nextNode(&pChild); pChild; pChild = nullptr, pAttribs->nextNode(&pChild)) {

#define EndEnumAttribs }}}

#define BeginEnumChildren(pNode, pChild)                                                                                \
    {                                                                                                                   \
        ComPtr<IXMLDOMNode> pChild, pChild##Next;                                                                      \
        for (pNode->get_firstChild(&pChild); pChild; pChild##Next = nullptr, pChild->get_nextSibling(&pChild##Next), pChild = pChild##Next) {

#define EndEnumChildren }}

static std::wstring GetText(ComPtr<IXMLDOMNode> pNode)
{
    _bstr_t bstr;
    pNode->get_text(bstr.GetAddress());

    return StringWiden((const wchar_t*)bstr);
}

static std::wstring GetXML(ComPtr<IXMLDOMNode> pNode)
{
    _bstr_t bstr;
    pNode->get_xml(bstr.GetAddress());
    std::wstring str(StringWiden((const wchar_t*)bstr));
    StringRemove(str, '\r');
    StringReplace(str, '\n', ' ');

    for (int i = 0; (i = StringFind(str, L" ", i)) >= 0;) {
        for (++i; i < int(str.size()) && (str[i] == ' ');) {
            StringDelete(str, i);
        }
    }
    return str;
}

static std::wstring GetAttrib(std::wstring attrib, ComPtr<IXMLDOMNode> pNode)
{
    std::wstring ret;

    BeginEnumAttribs(pNode, pChild) {
        DeclareNameAndValue(pChild, name, val);

        if (std::wstring(name) == attrib && val.vt == VT_BSTR) { // TODO: prepare for other types
            ret = StringWiden(val.bstrVal);
            break;
        }
    }
    EndEnumAttribs;

    return ret;
}

static int TimeToInt(std::wstring str)
{
    std::list<std::wstring> sl;
    int i = 0;
    for (std::wstring token = StringTokenize(str, L":.,", i); !token.empty(); token = StringTokenize(str, L":.,", i)) {
        sl.push_front(token);
    }

    if (sl.size() > 4) {
        return -1;
    }

    int time = 0;
    int mul[4] = {1, 1000, 60 * 1000, 60 * 60 * 1000};
    auto slIt = sl.begin();

    for (i = 0; slIt != sl.end(); i++, ++slIt) {
        const WCHAR* s = slIt->c_str();
        WCHAR* tmp = nullptr;
        int t = wcstol(s, &tmp, 10);
        if (s >= tmp) {
            return -1;
        }
        time += t * mul[i];
    }

    return time;
}

static DWORD StringToDWORD(std::wstring str)
{
    if (str.empty()) {
        return 0;
    }
    if (str[0] == '#') {
        return (DWORD)wcstol(str.c_str(), nullptr, 16);
    } else {
        return (DWORD)wcstol(str.c_str(), nullptr, 10);
    }
}

static DWORD ColorToDWORD(std::wstring str)
{
    if (str.empty()) {
        return 0;
    }

    DWORD ret = 0;

    if (str[0] == '#') {
        ret = (DWORD)wcstol(StringTrimLeft(str, '#').c_str(), nullptr, 16);
    } else {
        Lookup(g_colors, tstring(str), ret);
    }

    ret = ((ret & 0xff) << 16) | (ret & 0xff00ff00) | ((ret >> 16) & 0xff);

    return ret;
}

static int TranslateAlignment(std::wstring alignment)
{
    return
        !StringICompare(alignment, L"BottomLeft") ? 1 :
        !StringICompare(alignment, L"BottomCenter") ? 2 :
        !StringICompare(alignment, L"BottomRight") ? 3 :
        !StringICompare(alignment, L"MiddleLeft") ? 4 :
        !StringICompare(alignment, L"MiddleCenter") ? 5 :
        !StringICompare(alignment, L"MiddleRight") ? 6 :
        !StringICompare(alignment, L"TopLeft") ? 7 :
        !StringICompare(alignment, L"TopCenter") ? 8 :
        !StringICompare(alignment, L"TopRight") ? 9 :
        2;
}

static int TranslateMargin(std::wstring margin, int wndsize)
{
    int ret = 0;

    if (!margin.empty()) {
        ret = wcstol(margin.c_str(), nullptr, 10);
        if (StringFind(margin, '%') >= 0) {
            ret = wndsize * ret / 100;
        }
    }

    return ret;
}

////////////////

CUSFSubtitles::CUSFSubtitles()
{
}

CUSFSubtitles::~CUSFSubtitles()
{
}

bool CUSFSubtitles::Read(LPCTSTR fn)
{
    VARIANT_BOOL vb;
    ComPtr<IXMLDOMDocument> pDoc;
    if (FAILED(CoCreateInstance(CLSID_DOMDocument, nullptr, CLSCTX_ALL, IID_PPV_ARGS(&pDoc)))
            || FAILED(pDoc->put_async(VARIANT_FALSE))
            || FAILED(pDoc->load(_variant_t(fn), &vb)) || vb != VARIANT_TRUE) {
        return false;
    }

    styles.clear();
    effects.clear();
    texts.clear();

    if (!ParseUSFSubtitles(ComQuery<IXMLDOMNode>(pDoc))) {
        return false;
    }

    for (const auto& def : styles) {

        if (StringICompare(def->name, L"Default")) {
            continue;
        }

        for (const auto& s : styles) {

            if (!StringICompare(s->name, L"Default")) {
                continue;
            }

            if (s->fontstyle.face.empty()) {
                s->fontstyle.face = def->fontstyle.face;
            }
            if (s->fontstyle.size.empty()) {
                s->fontstyle.size = def->fontstyle.size;
            }
            if (s->fontstyle.color[0].empty()) {
                s->fontstyle.color[0] = def->fontstyle.color[0];
            }
            if (s->fontstyle.color[1].empty()) {
                s->fontstyle.color[1] = def->fontstyle.color[1];
            }
            if (s->fontstyle.color[2].empty()) {
                s->fontstyle.color[2] = def->fontstyle.color[2];
            }
            if (s->fontstyle.color[3].empty()) {
                s->fontstyle.color[3] = def->fontstyle.color[3];
            }
            if (s->fontstyle.italic.empty()) {
                s->fontstyle.italic = def->fontstyle.italic;
            }
            if (s->fontstyle.weight.empty()) {
                s->fontstyle.weight = def->fontstyle.weight;
            }
            if (s->fontstyle.underline.empty()) {
                s->fontstyle.underline = def->fontstyle.underline;
            }
            if (s->fontstyle.alpha.empty()) {
                s->fontstyle.alpha = def->fontstyle.alpha;
            }
            if (s->fontstyle.outline.empty()) {
                s->fontstyle.outline = def->fontstyle.outline;
            }
            if (s->fontstyle.shadow.empty()) {
                s->fontstyle.shadow = def->fontstyle.shadow;
            }
            if (s->fontstyle.wrap.empty()) {
                s->fontstyle.wrap = def->fontstyle.wrap;
            }
            if (s->pal.alignment.empty()) {
                s->pal.alignment = def->pal.alignment;
            }
            if (s->pal.relativeto.empty()) {
                s->pal.relativeto = def->pal.relativeto;
            }
            if (s->pal.horizontal_margin.empty()) {
                s->pal.horizontal_margin = def->pal.horizontal_margin;
            }
            if (s->pal.vertical_margin.empty()) {
                s->pal.vertical_margin = def->pal.vertical_margin;
            }
            if (s->pal.rotate[0].empty()) {
                s->pal.rotate[0] = def->pal.rotate[0];
            }
            if (s->pal.rotate[1].empty()) {
                s->pal.rotate[1] = def->pal.rotate[1];
            }
            if (s->pal.rotate[2].empty()) {
                s->pal.rotate[2] = def->pal.rotate[2];
            }
        }

        break;
    }

    for (const auto& t : texts) {
        if (t->style.empty()) {
            t->style = L"Default";
        }
    }

    return true;
}

bool CUSFSubtitles::ConvertToSTS(CSimpleTextSubtitle& sts)
{
    sts.m_name = metadata.language.text;
    sts.m_encoding = CTextFile::UTF8;
    sts.m_playRes = MakeSize(640, 480);
    sts.m_fScaledBAS = false;
    sts.m_defaultWrapStyle = 1;

    // TODO: map metadata.language.code to charset num (windows doesn't have such a function...)
    int charSet = DEFAULT_CHARSET;

    for (const auto& s : styles) {

        if (!StringICompare(s->name, L"Default") && !s->fontstyle.wrap.empty()) {
            sts.m_defaultWrapStyle =
                !StringICompare(s->fontstyle.wrap, L"no") ? 2 :
                /*!s->fontstyle.wrap.CompareNoCase(L"auto") ? 1 :*/
                1;
        }

        STSStyle* stss = new STSStyle;
        if (!stss) {
            continue;
        }

        if (!s->pal.horizontal_margin.empty()) {
            stss->marginRect.left = stss->marginRect.right = TranslateMargin(s->pal.horizontal_margin, sts.m_playRes.cx);
        }
        if (!s->pal.vertical_margin.empty()) {
            stss->marginRect.top = stss->marginRect.bottom = TranslateMargin(s->pal.vertical_margin, sts.m_playRes.cy);
        }

        stss->scrAlignment = TranslateAlignment(s->pal.alignment);

        if (!s->pal.relativeto.empty()) stss->relativeTo =
                !StringICompare(s->pal.relativeto, L"window") ? STSStyle::WINDOW :
                !StringICompare(s->pal.relativeto, L"video") ? STSStyle::VIDEO :
                STSStyle::WINDOW;

        stss->borderStyle = 0;
        if (!s->fontstyle.outline.empty()) {
            stss->outlineWidthX = stss->outlineWidthY = wcstol(s->fontstyle.outline.c_str(), nullptr, 10);
        }
        if (!s->fontstyle.shadow.empty()) {
            stss->shadowDepthX = stss->shadowDepthY = wcstol(s->fontstyle.shadow.c_str(), nullptr, 10);
        }

        for (size_t i = 0; i < 4; i++) {
            DWORD color = ColorToDWORD(s->fontstyle.color[i]);
            auto alpha = (BYTE)wcstol(s->fontstyle.alpha.c_str(), nullptr, 10);

            stss->colors[i] = color & 0xffffff;
            stss->alpha[i] = (BYTE)(color >> 24);

            stss->alpha[i] = BYTE(stss->alpha[i] + (255 - stss->alpha[i]) * std::min(std::max(alpha, 0ui8), 100ui8) / 100);
        }

        if (!s->fontstyle.face.empty()) {
            stss->fontName = s->fontstyle.face;
        }
        if (!s->fontstyle.size.empty()) {
            stss->fontSize = wcstol(s->fontstyle.size.c_str(), nullptr, 10);
        }
        if (!s->fontstyle.weight.empty()) stss->fontWeight =
                !StringICompare(s->fontstyle.weight, L"normal") ? FW_NORMAL :
                !StringICompare(s->fontstyle.weight, L"bold") ? FW_BOLD :
                !StringICompare(s->fontstyle.weight, L"lighter") ? FW_LIGHT :
                !StringICompare(s->fontstyle.weight, L"bolder") ? FW_SEMIBOLD :
                wcstol(s->fontstyle.weight.c_str(), nullptr, 10);
        if (stss->fontWeight == 0) {
            stss->fontWeight = FW_NORMAL;
        }
        if (!s->fontstyle.italic.empty()) {
            stss->fItalic = StringICompare(s->fontstyle.italic, L"yes") == 0;
        }
        if (!s->fontstyle.underline.empty()) {
            stss->fUnderline = StringICompare(s->fontstyle.underline, L"yes") == 0;
        }
        if (!s->pal.rotate[0].empty()) {
            stss->fontAngleZ = wcstol(s->pal.rotate[0].c_str(), nullptr, 10);
        }
        if (!s->pal.rotate[1].empty()) {
            stss->fontAngleX = wcstol(s->pal.rotate[1].c_str(), nullptr, 10);
        }
        if (!s->pal.rotate[2].empty()) {
            stss->fontAngleY = wcstol(s->pal.rotate[2].c_str(), nullptr, 10);
        }

        stss->charSet = charSet;

        sts.AddStyle(WToT(s->name), stss);
    }

    for (const auto& t : texts) {

        if (!t->pal.alignment.empty()) {
            std::wstring s;
            s = ::Format(L"{\\an%d}", TranslateAlignment(t->pal.alignment));
            t->str = s + t->str;
        }

        RECT marginRect = {};
        ::SetRectEmpty(&marginRect);

        if (!t->pal.horizontal_margin.empty()) {
            marginRect.left = marginRect.right = TranslateMargin(t->pal.horizontal_margin, sts.m_storageRes.cx);
        }
        if (!t->pal.vertical_margin.empty()) {
            marginRect.top = marginRect.bottom = TranslateMargin(t->pal.vertical_margin, sts.m_storageRes.cy);
        }

        WCHAR rtags[3][8] = {L"{\\rz%d}", L"{\\rx%d}", L"{\\ry%d}"};
        for (size_t i = 0; i < 3; i++) {
            if (int angle = wcstol(t->pal.rotate[i].c_str(), nullptr, 10)) {
                std::wstring str;
                str = ::Format(rtags[i], angle);
                t->str = str + t->str;
            }
        }

        if (StringICompare(t->style, L"Default") != 0) {
            for (const auto& s : styles) {
                if (s->name == t->style && !s->fontstyle.wrap.empty()) {
                    int WrapStyle =
                        !StringICompare(s->fontstyle.wrap, L"no") ? 2 :
                        /*!s->fontstyle.wrap.CompareNoCase(L"auto") ? 1 :*/
                        1;

                    if (WrapStyle != sts.m_defaultWrapStyle) {
                        std::wstring str;
                        str = ::Format(L"{\\q%d}", WrapStyle);
                        t->str = str + t->str;
                    }

                    break;
                }
            }
        }

        // TODO: apply effects as {\t(..)} after usf's standard clearly defines them

        sts.Add(t->str, true, t->start, t->stop, WToT(t->style), _T(""), _T(""), marginRect);
    }

    return true;
}

bool CUSFSubtitles::ParseUSFSubtitles(ComPtr<IXMLDOMNode> pNode)
{
    DeclareNameAndValue(pNode, name, val);

    if (name == L"usfsubtitles") {
        // metadata

        BeginEnumChildren(pNode, pChild) {
            DeclareNameAndValue(pChild, childName, childVal);

            if (childName == L"metadata") {
                ParseMetadata(pChild, metadata);
            }
        }
        EndEnumChildren;

        // styles

        BeginEnumChildren(pNode, pChild) {
            DeclareNameAndValue(pChild, childName, childVal);

            if (childName == L"styles") {
                BeginEnumChildren(pChild, pGrandChild) { // :)
                    DeclareNameAndValue(pGrandChild, grandChildName, grandChildVal);

                    if (grandChildName == L"style") {
                        std::unique_ptr<style_t> s(new style_t);
                        if (s) {
                            ParseStyle(pGrandChild, s.get());
                            styles.push_back(std::move(s));
                        }
                    }
                }
                EndEnumChildren;
            }
        }
        EndEnumChildren;

        // effects

        BeginEnumChildren(pNode, pChild) {
            DeclareNameAndValue(pChild, childName, childVal);

            if (childName == L"effects") {
                BeginEnumChildren(pChild, pGrandChild) { // :)
                    DeclareNameAndValue(pGrandChild, grandChildName, grandChildVal);

                    if (grandChildName == L"effect") {
                        std::unique_ptr<effect_t> e(new effect_t);
                        if (e) {
                            ParseEffect(pGrandChild, e.get());
                            effects.push_back(std::move(e));
                        }
                    }
                }
                EndEnumChildren;
            }
        }
        EndEnumChildren;

        // subtitles

        BeginEnumChildren(pNode, pChild) {
            DeclareNameAndValue(pChild, childName, childVal);

            if (childName == L"subtitles") {
                BeginEnumChildren(pChild, pGrandChild) { // :)
                    DeclareNameAndValue(pGrandChild, grandChildName, grandChildVal);

                    if (grandChildName == L"subtitle") {
                        std::wstring sstart = GetAttrib(L"start", pGrandChild);
                        std::wstring sstop = GetAttrib(L"stop", pGrandChild);
                        std::wstring sduration = GetAttrib(L"duration", pGrandChild);
                        if (sstart.empty() || (sstop.empty() && sduration.empty())) {
                            continue;
                        }

                        int start = TimeToInt(sstart);
                        int stop = !sstop.empty() ? TimeToInt(sstop) : (start + TimeToInt(sduration));

                        ParseSubtitle(pGrandChild, start, stop);
                    }
                }
                EndEnumChildren;
            }
        }
        EndEnumChildren;

        return true;
    }

    BeginEnumChildren(pNode, pChild) {
        if (ParseUSFSubtitles(pChild)) {
            return true;
        }
    }
    EndEnumChildren;

    return false;
}

void CUSFSubtitles::ParseMetadata(ComPtr<IXMLDOMNode> pNode, metadata_t& m)
{
    DeclareNameAndValue(pNode, name, val);

    if (name == L"title") {
        m.title = GetText(pNode);
    } else if (name == L"date") {
        m.date = GetText(pNode);
    } else if (name == L"comment") {
        m.comment = GetText(pNode);
    } else if (name == L"author") {
        BeginEnumChildren(pNode, pChild) {
            DeclareNameAndValue(pChild, childName, childVal);

            if (childName == L"name") {
                m.author.name = GetText(pChild);
            } else if (childName == L"email") {
                m.author.email = GetText(pChild);
            } else if (childName == L"url") {
                m.author.url = GetText(pChild);
            }
        }
        EndEnumChildren;

        return;
    } else if (name == L"language") {
        m.language.text = GetText(pNode);
        m.language.code = GetAttrib(L"code", pNode);
    } else if (name == L"languageext") {
        m.languageext.text = GetText(pNode);
        m.languageext.code = GetAttrib(L"code", pNode);
    }

    BeginEnumChildren(pNode, pChild) {
        ParseMetadata(pChild, metadata);
    }
    EndEnumChildren;
}

void CUSFSubtitles::ParseStyle(ComPtr<IXMLDOMNode> pNode, style_t* s)
{
    DeclareNameAndValue(pNode, name, val);

    if (name == L"style") {
        s->name = GetAttrib(L"name", pNode);
    } else if (name == L"fontstyle") {
        ParseFontstyle(pNode, s->fontstyle);
        return;
    } else if (name == L"position") {
        ParsePal(pNode, s->pal);
        return;
    }

    BeginEnumChildren(pNode, pChild) {
        ParseStyle(pChild, s);
    }
    EndEnumChildren;
}

void CUSFSubtitles::ParseFontstyle(ComPtr<IXMLDOMNode> pNode, fontstyle_t& fs)
{
    fs.face = GetAttrib(L"face", pNode);
    fs.size = GetAttrib(L"size", pNode);
    fs.color[0] = GetAttrib(L"color", pNode);
    fs.color[1] = GetAttrib(L"back-color", pNode);
    fs.color[2] = GetAttrib(L"outline-color", pNode);
    fs.color[3] = GetAttrib(L"shadow-color", pNode);
    fs.italic = GetAttrib(L"italic", pNode);
    fs.weight = GetAttrib(L"weight", pNode);
    fs.underline = GetAttrib(L"underline", pNode);
    fs.alpha = GetAttrib(L"alpha", pNode);
    fs.outline = GetAttrib(L"outline-level", pNode);
    fs.shadow = GetAttrib(L"shadow-level", pNode);
    fs.wrap = GetAttrib(L"wrap", pNode);
}

void CUSFSubtitles::ParsePal(ComPtr<IXMLDOMNode> pNode, posattriblist_t& pal)
{
    pal.alignment = GetAttrib(L"alignment", pNode);
    pal.relativeto = GetAttrib(L"relative-to", pNode);
    pal.horizontal_margin = GetAttrib(L"horizontal-margin", pNode);
    pal.vertical_margin = GetAttrib(L"vertical-margin", pNode);
    pal.rotate[0] = GetAttrib(L"rotate-z", pNode);
    pal.rotate[1] = GetAttrib(L"rotate-x", pNode);
    pal.rotate[2] = GetAttrib(L"rotate-y", pNode);
}

void CUSFSubtitles::ParseEffect(ComPtr<IXMLDOMNode> pNode, effect_t* e)
{
    DeclareNameAndValue(pNode, name, val);

    if (name == L"effect") {
        e->name = GetAttrib(L"name", pNode);
    } else if (name == L"keyframes") {
        BeginEnumChildren(pNode, pChild) {
            DeclareNameAndValue(pChild, childName, childVal);

            if (childName == L"keyframe") {
                std::unique_ptr<keyframe_t> k(new keyframe_t);
                if (k) {
                    ParseKeyframe(pChild, k.get());
                    e->keyframes.push_back(std::move(k));
                }
            }
        }
        EndEnumChildren;

        return;
    }

    BeginEnumChildren(pNode, pChild) {
        ParseEffect(pChild, e);
    }
    EndEnumChildren;
}

void CUSFSubtitles::ParseKeyframe(ComPtr<IXMLDOMNode> pNode, keyframe_t* k)
{
    DeclareNameAndValue(pNode, name, val);

    if (name == L"keyframe") {
        k->position = GetAttrib(L"position", pNode);
    } else if (name == L"fontstyle") {
        ParseFontstyle(pNode, k->fontstyle);
        return;
    } else if (name == L"position") {
        ParsePal(pNode, k->pal);
        return;
    }
}

void CUSFSubtitles::ParseSubtitle(ComPtr<IXMLDOMNode> pNode, int start, int stop)
{
    DeclareNameAndValue(pNode, name, val);

    if (name == L"text" || name == L"karaoke") {
        std::unique_ptr<text_t> t(new text_t);
        if (t) {
            t->start = start;
            t->stop = stop;
            t->style = GetAttrib(L"style", pNode);
            t->effect = GetAttrib(L"effect", pNode);
            ParsePal(pNode, t->pal);
            ParseText(pNode, t->str);
            texts.push_back(std::move(t));
        }

        return;
    }
    //  else if

    BeginEnumChildren(pNode, pChild) {
        ParseSubtitle(pChild, start, stop);
    }
    EndEnumChildren;
}

void CUSFSubtitles::ParseText(ComPtr<IXMLDOMNode> pNode, std::wstring& str)
{
    DeclareNameAndValue(pNode, name, val);

    std::wstring prefix, postfix;

    if (name == L"b") {
        prefix = L"{\\b1}";
        postfix = L"{\\b}";
    } else if (name == L"i") {
        prefix = L"{\\i1}";
        postfix = L"{\\i}";
    } else if (name == L"u") {
        prefix = L"{\\u1}";
        postfix = L"{\\u}";
    } else if (name == L"font") {
        fontstyle_t fs;
        ParseFontstyle(pNode, fs);

        if (!fs.face.empty()) {
            prefix += L"{\\fn" + fs.face + L"}";
            postfix += L"{\\fn}";
        }
        if (!fs.size.empty()) {
            prefix += L"{\\fs" + fs.size + L"}";
            postfix += L"{\\fs}";
        }
        if (!fs.outline.empty()) {
            prefix += L"{\\bord" + fs.outline + L"}";
            postfix += L"{\\bord}";
        }
        if (!fs.shadow.empty()) {
            prefix += L"{\\shad" + fs.shadow + L"}";
            postfix += L"{\\shad}";
        }

        for (size_t i = 0; i < 4; i++) {
            if (!fs.color[i].empty()) {
                std::wstring s;
                s = ::Format(L"{\\%uc&H%06x&}", i + 1, ColorToDWORD(fs.color[i]));
                prefix += s;
                s = ::Format(L"{\\%uc}", i + 1);
                postfix += s;
            }
        }
    } else if (name == L"k") {
        int t = wcstol(GetAttrib(L"t", pNode).c_str(), nullptr, 10);
        std::wstring s;
        s = ::Format(L"{\\kf%d}", t / 10);
        str += s;
        return;
    } else if (name == L"br") {
        str += L"\\N";
        return;
    } else if (name == L"#text") {
        str += GetXML(pNode);
        return;
    }

    BeginEnumChildren(pNode, pChild) {
        std::wstring s;
        ParseText(pChild, s);
        str += s;
    }
    EndEnumChildren;

    str = prefix + str + postfix;
}

void CUSFSubtitles::ParseShape(ComPtr<IXMLDOMNode> pNode)
{
    // no specs on this yet
}
