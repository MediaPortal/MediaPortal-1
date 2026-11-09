/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2013 see Authors.txt
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

#include "STS.h"

// metadata
struct author_t {
    std::wstring name, email, url;
};

struct language_t {
    std::wstring code, text;
};

struct metadata_t {
    std::wstring title, date, comment;
    author_t author;
    language_t language, languageext;
};

// style
struct posattriblist_t {
    std::wstring alignment, relativeto, horizontal_margin, vertical_margin, rotate[3];
};

struct fontstyle_t {
    std::wstring face, size, color[4], weight, italic, underline, alpha, outline, shadow, wrap;
};

struct style_t {
    std::wstring name;
    fontstyle_t fontstyle;
    posattriblist_t pal;
};

// effect
struct keyframe_t {
    std::wstring position;
    fontstyle_t fontstyle;
    posattriblist_t pal;
};

struct effect_t {
    std::wstring name;
    std::list<std::unique_ptr<keyframe_t>> keyframes;
};

// subtitle/text
struct text_t {
    int start, stop;
    std::wstring effect, style, str;
    posattriblist_t pal;
};

class CUSFSubtitles
{
    bool ParseUSFSubtitles(ComPtr<IXMLDOMNode> pNode);
    void ParseMetadata(ComPtr<IXMLDOMNode> pNode, metadata_t& m);
    void ParseStyle(ComPtr<IXMLDOMNode> pNode, style_t* s);
    void ParseFontstyle(ComPtr<IXMLDOMNode> pNode, fontstyle_t& fs);
    void ParsePal(ComPtr<IXMLDOMNode> pNode, posattriblist_t& pal);
    void ParseEffect(ComPtr<IXMLDOMNode> pNode, effect_t* e);
    void ParseKeyframe(ComPtr<IXMLDOMNode> pNode, keyframe_t* k);
    void ParseSubtitle(ComPtr<IXMLDOMNode> pNode, int start, int stop);
    void ParseText(ComPtr<IXMLDOMNode> pNode, std::wstring& assstr);
    void ParseShape(ComPtr<IXMLDOMNode> pNode);

public:
    CUSFSubtitles();
    virtual ~CUSFSubtitles();

    bool Read(LPCTSTR fn);
    //bool Write(LPCTSTR fn); // TODO

    metadata_t metadata;
    std::list<std::unique_ptr<style_t>> styles;
    std::list<std::unique_ptr<effect_t>> effects;
    std::list<std::unique_ptr<text_t>> texts;

    bool ConvertToSTS(CSimpleTextSubtitle& sts);
    //bool ConvertFromSTS(CSimpleTextSubtitle& sts); // TODO
};
