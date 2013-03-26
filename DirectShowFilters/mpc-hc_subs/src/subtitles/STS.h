/*
 *  $Id: STS.h 2786 2010-12-17 16:42:55Z XhmikosR $
 *
 *  (C) 2003-2006 Gabest
 *  (C) 2006-2010 see AUTHORS
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#pragma once

#include <atlcoll.h>
#include <wxutil.h>
#include "TextFile.h"
#include "GFN.h"

typedef enum {TIME, FRAME} tmode; // the meaning of STSEntry::start/end

#ifdef _VSMOD // patch m003. random text points
class MOD_RANDOM
{
public:
	int X;
	int Y;
	int Z;
	int Seed; // random seed

	//MOD_RANDOM();
	bool operator == (MOD_RANDOM& mr);

	void clear();
};
#endif

#ifdef _VSMOD // patch m010. png background
#include <png.h>
class MOD_PNGIMAGE
{
public:
	CString		filename;
	int			width;
	int			height;

	int			xoffset;
	int			yoffset;

	int			bpp;

	BYTE		alpha;

	png_byte	color_type;
	png_byte	bit_depth;

	png_bytep*	pointer;

	MOD_PNGIMAGE();

	bool operator == (MOD_PNGIMAGE& png);
	/**/
	bool processData(png_structp png_ptr);
	bool initImage(CString m_fn);
	bool initImage(BYTE* data, CString m_fn);
	void freeImage();
};
#endif

#ifdef _VSMOD // patch m004. gradient colors
class MOD_GRADIENT
{
public:
	COLORREF	colors[4];		// c
	COLORREF	alphas[4];		// a
	COLORREF	color[4][4];	// vc
	BYTE		alpha[4][4];	// va
	int			mode[4];

	// for renderer
	int			height;
	int			width;
	int			xoffset;
	int			yoffset;

	BYTE		subpixx;
	BYTE		subpixy;
	BYTE		fadalpha;

	BYTE		img_alpha;

	// for background image
	MOD_PNGIMAGE	b_images[4];

	MOD_GRADIENT();
	bool operator == (MOD_GRADIENT& mg);

	void clear();
	DWORD getmixcolor(int tx, int ty, int i);
};
#endif

#ifdef _VSMOD // patch m008. distort
class MOD_DISTORT
{
public:
	double  pointsx[3]; //P1-P3
	double  pointsy[3]; //P1-P3
	bool enabled;

	MOD_DISTORT();

	bool operator == (MOD_DISTORT& md);
};
#endif

#ifdef _VSMOD // patch m011. jitter
class MOD_JITTER
{
public:
	CRect offset;	// left,top,right,left
	int seed;
	int period;		// ms
	bool enabled;

	MOD_JITTER();

	bool operator == (MOD_JITTER& mj);

	CPoint getOffset(REFERENCE_TIME rt);
};
#endif

class STSStyle
{
public:
	CRect		marginRect;		// measured from the sides
	int			scrAlignment;	// 1 - 9: as on the numpad, 0: default
	int			borderStyle;	// 0: outline, 1: opaque box
	double		outlineWidthX, outlineWidthY;
	double		shadowDepthX, shadowDepthY;
	COLORREF	colors[4];		// usually: {primary, secondary, outline/background, shadow}
	BYTE		alpha[4];
	int			charSet;
	CString		fontName;
	double		fontSize;		// height
	double		fontScaleX, fontScaleY;	// percent
	double		fontSpacing;	// +/- pixels
	int			fontWeight;
	bool		fItalic;
	bool		fUnderline;
	bool		fStrikeOut;
	int			fBlur;
	double		fGaussianBlur;
	double		fontAngleZ, fontAngleX, fontAngleY;
	double		fontShiftX, fontShiftY;
	int			relativeTo;		// 0: window, 1: video, 2: undefined (~window)
#ifdef _VSMOD
	// patch m001. Vertical fontspacing
	double  mod_verticalSpace;
	// patch m002. Z-coord
	double mod_z;
	// patch m003. random text points
	MOD_RANDOM mod_rand;
	// patch m004. gradient colors
	MOD_GRADIENT mod_grad;
	// patch m007. symbol rotating
	int mod_fontOrient;
	// patch m008. distort
	MOD_DISTORT mod_distort;
	// patch m011. jitter
	MOD_JITTER mod_jitter;
#endif

	STSStyle();
#ifdef _VSMOD
	STSStyle(STSStyle& s);
#endif

	void SetDefault();

	bool operator == (STSStyle& s);
	bool IsFontStyleEqual(STSStyle& s);
#ifdef _VSMOD
	void mod_CopyStyleFrom(STSStyle& s);

	STSStyle& operator = (const STSStyle& s);
#endif
	STSStyle& operator = (LOGFONT& lf);

	friend LOGFONTA& operator <<= (LOGFONTA& lfa, STSStyle& s);
	friend LOGFONTW& operator <<= (LOGFONTW& lfw, STSStyle& s);

	friend CString& operator <<= (CString& style, STSStyle& s);
	friend STSStyle& operator <<= (STSStyle& s, CString& style);
};

class CSTSStyleMap : public CAtlMap<CString, STSStyle*, CStringElementTraits<CString> >
{
public:
	CSTSStyleMap() {}
	virtual ~CSTSStyleMap() {
		Free();
	}
	void Free();
};

typedef struct {
	CStringW str;
	bool fUnicode;
	CString style, actor, effect;
	CRect marginRect;
	int layer;
	int start, end;
	int readorder;
#ifdef _VSMOD // patch m009. png graphics
	int mod_scripttype;
#endif
} STSEntry;

class STSSegment
{
public:
	int start, end;
	CAtlArray<int> subs;

	STSSegment() {};
	STSSegment(int s, int e) {
		start = s;
		end = e;
	}
	STSSegment(const STSSegment& stss) {
		*this = stss;
	}
	STSSegment& operator = (const STSSegment& stss) {
		if(this != &stss) {
			start = stss.start;
			end = stss.end;
			subs.Copy(stss.subs);
		}
		return *this;
	}
};

class CSimpleTextSubtitle : public CAtlArray<STSEntry>
{
	friend class CSubtitleEditorDlg;

protected:
	CAtlArray<STSSegment> m_segments;
	virtual void OnChanged() {}

public:
	CString m_name;
	LCID m_lcid;
	exttype m_exttype;
	tmode m_mode;
	CTextFile::enc m_encoding;
	CString m_path;

	CSize m_dstScreenSize;
	int m_defaultWrapStyle;
	int m_collisions;
	bool m_fScaledBAS;

	bool m_fUsingAutoGeneratedDefaultStyle;

	CSTSStyleMap m_styles;

#ifdef _VSMOD
	CAtlArray<MOD_PNGIMAGE> mod_images;

	// index array, for fast speed
	DWORD   ind_size; // size of array
	DWORD*  ind_time; // time array
	DWORD*  ind_pos;  // segment indexes array (start)
#endif

	enum EPARCompensationType {
		EPCTDisabled = 0,
		EPCTDownscale = 1,
		EPCTUpscale = 2,
		EPCTAccurateSize = 3
	};

	EPARCompensationType m_ePARCompensationType;
	double m_dPARCompensation;

public:
	CSimpleTextSubtitle();
	virtual ~CSimpleTextSubtitle();

	virtual void Copy(CSimpleTextSubtitle& sts);
	virtual void Empty();

	void Sort(bool fRestoreReadorder = false);
	void CreateSegments();

	void Append(CSimpleTextSubtitle& sts, int timeoff = -1);

	bool Open(CString fn, int CharSet, CString name = _T(""));
	bool Open(CTextFile* f, int CharSet, CString name);
	bool Open(BYTE* data, int len, int CharSet, CString name);
	bool SaveAs(CString fn, exttype et, double fps = -1, CTextFile::enc = CTextFile::ASCII);

#ifdef _VSMOD // load embedded images
	bool LoadUUEFile(CTextFile* file, CString m_fn);
	bool LoadEfile(CString& img, CString m_fn);

	void MakeIndex(int SizeOfSegment);
#endif
	void Add(CStringW str, bool fUnicode, int start, int end, CString style = _T("Default"), CString actor = _T(""), CString effect = _T(""), CRect marginRect = CRect(0,0,0,0), int layer = 0, int readorder = -1);
	STSStyle* CreateDefaultStyle(int CharSet);
	void ChangeUnknownStylesToDefault();
	void AddStyle(CString name, STSStyle* style); // style will be stored and freed in Empty() later
	bool CopyStyles(const CSTSStyleMap& styles, bool fAppend = false);

	bool SetDefaultStyle(STSStyle& s);
	bool GetDefaultStyle(STSStyle& s);

	void ConvertToTimeBased(double fps);
	void ConvertToFrameBased(double fps);

	int TranslateStart(int i, double fps);
	int TranslateEnd(int i, double fps);
	int SearchSub(int t, double fps);

	int TranslateSegmentStart(int i, double fps);
	int TranslateSegmentEnd(int i, double fps);
	const STSSegment* SearchSubs(int t, double fps, /*[out]*/ int* iSegment = NULL, int* nSegments = NULL);
	const STSSegment* GetSegment(int iSegment) {
		return iSegment >= 0 && iSegment < (int)m_segments.GetCount() ? &m_segments[iSegment] : NULL;
	}

	STSStyle* GetStyle(int i);
	bool GetStyle(int i, STSStyle& stss);
	int GetCharSet(int i);
	bool IsEntryUnicode(int i);
	void ConvertUnicode(int i, bool fUnicode);

	CStringA GetStrA(int i, bool fSSA = false);
	CStringW GetStrW(int i, bool fSSA = false);
	CStringW GetStrWA(int i, bool fSSA = false);

#ifdef UNICODE
#define GetStr GetStrW
#else
#define GetStr GetStrA
#endif

	void SetStr(int i, CStringA str, bool fUnicode /* ignored */);
	void SetStr(int i, CStringW str, bool fUnicode);
};

extern BYTE CharSetList[];
extern TCHAR* CharSetNames[];
extern int CharSetLen;

class CHtmlColorMap : public CAtlMap<CString, DWORD, CStringElementTraits<CString> >
{
public:
	CHtmlColorMap();
};
extern CHtmlColorMap g_colors;



