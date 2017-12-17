/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2012 see Authors.txt
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
#include "Rasterizer.h"
#include "../SubPic/SubPicProviderImpl.h"

class CMyFont : public CFont
{
public:
    int m_ascent, m_descent;

    CMyFont(STSStyle& style);
};

class CPolygon;

class CWord : public Rasterizer
{
    bool m_fDrawn;
    CPoint m_p;

    void Transform(CPoint org);

    void Transform_C(CPoint& org);
    void Transform_SSE2(CPoint& org);
    bool CreateOpaqueBox();

protected:
    double m_scalex, m_scaley;
    CStringW m_str;

    virtual bool CreatePath() = 0;

public:
    bool m_fWhiteSpaceChar, m_fLineBreak;

    STSStyle m_style;

    CPolygon* m_pOpaqueBox;

    int m_ktype, m_kstart, m_kend;

    int m_width, m_ascent, m_descent;

    CWord(STSStyle& style, CStringW str, int ktype, int kstart, int kend, double scalex, double scaley); // str[0] = 0 -> m_fLineBreak = true (in this case we only need and use the height of m_font from the whole class)
    virtual ~CWord();

    virtual CWord* Copy() = 0;
    virtual bool Append(CWord* w);

    void Paint(CPoint p, CPoint org);
};

class CText : public CWord
{
protected:
    virtual bool CreatePath();

public:
    CText(STSStyle& style, CStringW str, int ktype, int kstart, int kend, double scalex, double scaley);

    virtual CWord* Copy();
    virtual bool Append(CWord* w);
};

class CPolygon : public CWord
{
    bool GetLONG(CStringW& str, LONG& ret);
    bool GetPOINT(CStringW& str, POINT& ret);
    bool ParseStr();

protected:
    int m_baseline;

    CAtlArray<BYTE> m_pathTypesOrg;
    CAtlArray<CPoint> m_pathPointsOrg;

    virtual bool CreatePath();

public:
    CPolygon(STSStyle& style, CStringW str, int ktype, int kstart, int kend, double scalex, double scaley, int baseline);
    CPolygon(CPolygon&); // can't use a const reference because we need to use CAtlArray::Copy which expects a non-const reference
    virtual ~CPolygon();

    virtual CWord* Copy();
    virtual bool Append(CWord* w);
};

class CClipper : public CPolygon
{
private:
    CWord* Copy();
    virtual bool Append(CWord* w);

public:
    CClipper(CStringW str, CSize size, double scalex, double scaley, bool inverse, CPoint cpOffset);
    virtual ~CClipper();

    CSize m_size;
    bool m_inverse;
    CPoint m_cpOffset;
    BYTE* m_pAlphaMask;
};

class CLine : public CAtlList<CWord*>
{
public:
    int m_width, m_ascent, m_descent, m_borderX, m_borderY;

    virtual ~CLine();

    void Compact();

    CRect PaintShadow(SubPicDesc& spd, CRect& clipRect, BYTE* pAlphaMask, CPoint p, CPoint org, int time, int alpha);
    CRect PaintOutline(SubPicDesc& spd, CRect& clipRect, BYTE* pAlphaMask, CPoint p, CPoint org, int time, int alpha);
    CRect PaintBody(SubPicDesc& spd, CRect& clipRect, BYTE* pAlphaMask, CPoint p, CPoint org, int time, int alpha);
};

enum eftype {
    EF_MOVE = 0,    // {\move(x1=param[0], y1=param[1], x2=param[2], y2=param[3], t1=t[0], t2=t[1])} or {\pos(x=param[0], y=param[1])}
    EF_ORG,         // {\org(x=param[0], y=param[1])}
    EF_FADE,        // {\fade(a1=param[0], a2=param[1], a3=param[2], t1=t[0], t2=t[1], t3=t[2], t4=t[3])} or {\fad(t1=t[1], t2=t[2])
    EF_BANNER,      // Banner;delay=param[0][;lefttoright=param[1];fadeawaywidth=param[2]]
    EF_SCROLL       // Scroll up/down=param[3];top=param[0];bottom=param[1];delay=param[2][;fadeawayheight=param[4]]
};

#define EF_NUMBEROFEFFECTS 5

class Effect
{
public:
    enum eftype type;
    int param[9];
    int t[4];
};

class CSubtitle : public CAtlList<CLine*>
{
    int GetFullWidth();
    int GetFullLineWidth(POSITION pos);
    int GetWrapWidth(POSITION pos, int maxwidth);
    CLine* GetNextLine(POSITION& pos, int maxwidth);

public:
    int m_scrAlignment;
    int m_wrapStyle;
    bool m_fAnimated;
    int m_relativeTo;

    Effect* m_effects[EF_NUMBEROFEFFECTS];

    CAtlList<CWord*> m_words;

    CClipper* m_pClipper;

    CRect m_rect, m_clip;
    int m_topborder, m_bottomborder;
    bool m_clipInverse;

    double m_scalex, m_scaley;

public:
    CSubtitle();
    virtual ~CSubtitle();
    virtual void Empty();
    void EmptyEffects();

    void CreateClippers(CSize size);

    void MakeLines(CSize size, CRect marginRect);
};

class CScreenLayoutAllocator
{
    typedef struct {
        CRect r;
        int segment, entry, layer;
    } SubRect;

    CAtlList<SubRect> m_subrects;

public:
    /*virtual*/
    void Empty();

    void AdvanceToSegment(int segment, const CAtlArray<int>& sa);
    CRect AllocRect(CSubtitle* s, int segment, int entry, int layer, int collisions);
};

class __declspec(uuid("537DCACA-2812-4a4f-B2C6-1A34C17ADEB0"))
    CRenderedTextSubtitle : public CSimpleTextSubtitle, public CSubPicProviderImpl, public ISubStream
{
    CAtlMap<int, CSubtitle*> m_subtitleCache;

    CScreenLayoutAllocator m_sla;

    CSize m_size;
    CRect m_vidrect;

    // temp variables, used when parsing the script
    int m_time, m_delay;
    int m_animStart, m_animEnd;
    double m_animAccel;
    int m_ktype, m_kstart, m_kend;
    int m_nPolygon;
    int m_polygonBaselineOffset;
    STSStyle* m_pStyleOverride; // the app can decide to use this style instead of a built-in one
    bool m_doOverrideStyle;

    void ParseEffect(CSubtitle* sub, CString str);
    void ParseString(CSubtitle* sub, CStringW str, STSStyle& style);
    void ParsePolygon(CSubtitle* sub, CStringW str, STSStyle& style);
    bool ParseSSATag(CSubtitle* sub, CStringW str, STSStyle& style, STSStyle& org, bool fAnimate = false);
    bool ParseHtmlTag(CSubtitle* sub, CStringW str, STSStyle& style, STSStyle& org);

    double CalcAnimation(double dst, double src, bool fAnimate);

    CSubtitle* GetSubtitle(int entry);

protected:
    virtual void OnChanged();

public:
    CRenderedTextSubtitle(CCritSec* pLock, STSStyle* styleOverride = NULL, bool doOverride = false);
    virtual ~CRenderedTextSubtitle();

    virtual void Copy(CSimpleTextSubtitle& sts);
    virtual void Empty();

    // call to signal this RTS to ignore any of the styles and apply the given override style
    void SetOverride(bool doOverride = true, STSStyle* styleOverride = NULL) {
        m_doOverrideStyle = doOverride;
        if (styleOverride != NULL) {
            m_pStyleOverride = styleOverride;
        }
    }

public:
    bool Init(CSize size, CRect vidrect); // will call Deinit()
    void Deinit();

    DECLARE_IUNKNOWN
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);

    // ISubPicProvider
    STDMETHODIMP_(POSITION) GetStartPosition(REFERENCE_TIME rt, double fps);
    STDMETHODIMP_(POSITION) GetNext(POSITION pos);
    STDMETHODIMP_(REFERENCE_TIME) GetStart(POSITION pos, double fps);
    STDMETHODIMP_(REFERENCE_TIME) GetStop(POSITION pos, double fps);
    STDMETHODIMP_(bool) IsAnimated(POSITION pos);
    STDMETHODIMP Render(SubPicDesc& spd, REFERENCE_TIME rt, double fps, RECT& bbox);

    STDMETHODIMP_(SUBTITLE_TYPE) GetType() { return ST_TEXT; };

    // IPersist
    STDMETHODIMP GetClassID(CLSID* pClassID);

    // ISubStream
    STDMETHODIMP_(int) GetStreamCount();
    STDMETHODIMP GetStreamInfo(int i, WCHAR** ppName, LCID* pLCID);
    STDMETHODIMP_(int) GetStream();
    STDMETHODIMP SetStream(int iStream);
    STDMETHODIMP Reload();
};
