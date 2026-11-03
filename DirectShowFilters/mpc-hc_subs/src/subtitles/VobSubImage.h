/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2014 see Authors.txt
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


struct COutline {
    std::vector<POINT> pa;
    std::vector<int> da;
    void RemoveAll() {
        pa.clear();
        da.clear();
    }
    void Add(POINT p, int d) {
        pa.push_back(p);
        da.push_back(d);
    }
};

class CVobSubImage
{
    friend class CVobSubFile;

private:
    SIZE org = {};
    RGBQUAD* lpTemp1;
    RGBQUAD* lpTemp2;

    size_t nOffset[2], nPlane;
    bool bCustomPal;
    bool bAligned;
    int tridx;
    RGBQUAD* orgpal /*[16]*/, * cuspal /*[4]*/;

    bool Alloc(int w, int h);
    void Free();

    BYTE GetNibble(const BYTE* lpData);
    void DrawPixels(POINT p, int length, size_t colorId);
    void TrimSubImage();

public:
    size_t nLang, nIdx;
    bool bForced, bAnimated;
    int tCurrent;
    __int64 start, delay;
    RECT rect = {};
    struct SubPal {
        BYTE pal: 4, tr: 4;
    };
    SubPal pal[4];
    RGBQUAD* lpPixels;

    CVobSubImage();
    virtual ~CVobSubImage();

    void Invalidate() { nLang = nIdx = SIZE_T_ERROR; }

    void GetPacketInfo(const BYTE* lpData, size_t packetSize, size_t dataSize, int t = INT_MAX);
    bool Decode(BYTE* lpData, size_t packetSize, size_t dataSize, int t,
                bool bCustomPal,
                int tridx,
                RGBQUAD* orgpal /*[16]*/, RGBQUAD* cuspal /*[4]*/,
                bool bTrim);

private:
    std::list<std::unique_ptr<COutline>>* GetOutlineList(POINT& topleft);
    int  GrabSegment(int start, const COutline& o, COutline& ret);
    void SplitOutline(const COutline& o, COutline& o1, COutline& o2);
    void AddSegment(COutline& o, std::vector<BYTE>& pathTypes, std::vector<POINT>& pathPoints);

public:
    bool Polygonize(std::vector<BYTE>& pathTypes, std::vector<POINT>& pathPoints, bool bSmooth, int scale);
    bool Polygonize(std::wstring& assstr, bool bSmooth = true, int scale = 3);

    void Scale2x();
};
