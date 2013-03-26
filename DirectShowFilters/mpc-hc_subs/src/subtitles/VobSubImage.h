/*
 *  $Id: VobSubImage.h 2786 2010-12-17 16:42:55Z XhmikosR $
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

typedef struct {
	CAtlArray<CPoint> pa;
	CAtlArray<int> da;
	void RemoveAll() {
		pa.RemoveAll();
		da.RemoveAll();
	}
	void Add(CPoint p, int d) {
		pa.Add(p);
		da.Add(d);
	}
} COutline;

class CVobSubImage
{
	friend class CVobSubFile;

private:
	CSize org;
	RGBQUAD* lpTemp1;
	RGBQUAD* lpTemp2;

	WORD nOffset[2], nPlane;
	bool fCustomPal;
	char fAligned; // we are also using this for calculations, that's why it is char instead of bool...
	int tridx;
	RGBQUAD* orgpal /*[16]*/,* cuspal /*[4]*/;

	bool Alloc(int w, int h);
	void Free();

	BYTE GetNibble(BYTE* lpData);
	void DrawPixels(CPoint p, int length, int colorid);
	void TrimSubImage();

public:
	int iLang, iIdx;
	bool fForced;
	__int64 start, delay;
	CRect rect;
	typedef struct {
		BYTE pal: 4, tr: 4;
	} SubPal;
	SubPal pal[4];
	RGBQUAD* lpPixels;

	CVobSubImage();
	virtual ~CVobSubImage();

	void Invalidate() {
		iLang = iIdx = -1;
	}

	void GetPacketInfo(BYTE* lpData, int packetsize, int datasize);
	bool Decode(BYTE* lpData, int packetsize, int datasize,
				bool fCustomPal,
				int tridx,
				RGBQUAD* orgpal /*[16]*/, RGBQUAD* cuspal /*[4]*/,
				bool fTrim);

	/////////

private:
	CAutoPtrList<COutline>* GetOutlineList(CPoint& topleft);
	int GrabSegment(int start, COutline& o, COutline& ret);
	void SplitOutline(COutline& o, COutline& o1, COutline& o2);
	void AddSegment(COutline& o, CAtlArray<BYTE>& pathTypes, CAtlArray<CPoint>& pathPoints);

public:
	bool Polygonize(CAtlArray<BYTE>& pathTypes, CAtlArray<CPoint>& pathPoints, bool fSmooth, int scale);
	bool Polygonize(CStringW& assstr, bool fSmooth = true, int scale = 3);

	void Scale2x();
};
