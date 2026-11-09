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
#include <algorithm>

// RECT, POINT and SIZE of the Windows SDK are plain structures; these are the few helpers that the code needs on top of them

inline RECT MakeRect(int l, int t, int r, int b)
{
    return RECT{l, t, r, b};
}

inline RECT MakeRect(POINT p, SIZE s)
{
    return RECT{p.x, p.y, p.x + s.cx, p.y + s.cy};
}

inline RECT MakeRect(POINT topLeft, POINT bottomRight)
{
    return RECT{topLeft.x, topLeft.y, bottomRight.x, bottomRight.y};
}

inline POINT MakePoint(int x, int y)
{
    return POINT{x, y};
}

inline SIZE MakeSize(int cx, int cy)
{
    return SIZE{cx, cy};
}

inline int RectWidth(const RECT& r)
{
    return r.right - r.left;
}

inline int RectHeight(const RECT& r)
{
    return r.bottom - r.top;
}

inline SIZE RectSize(const RECT& r)
{
    return SIZE{r.right - r.left, r.bottom - r.top};
}

inline POINT RectTopLeft(const RECT& r)
{
    return POINT{r.left, r.top};
}

inline POINT RectBottomRight(const RECT& r)
{
    return POINT{r.right, r.bottom};
}

inline POINT RectCenter(const RECT& r)
{
    return POINT{(r.left + r.right) / 2, (r.top + r.bottom) / 2};
}

inline bool RectIsNull(const RECT& r)
{
    return r.left == 0 && r.top == 0 && r.right == 0 && r.bottom == 0;
}

// Swaps the sides that are the wrong way round
inline void NormalizeRect(RECT& r)
{
    if (r.left > r.right) {
        std::swap(r.left, r.right);
    }
    if (r.top > r.bottom) {
        std::swap(r.top, r.bottom);
    }
}

// Moves the sides in: the opposite of InflateRect
inline void DeflateRect(RECT& r, int x, int y)
{
    ::InflateRect(&r, -x, -y);
}

inline RECT RectIntersect(const RECT& a, const RECT& b)
{
    RECT r;
    ::IntersectRect(&r, &a, &b);
    return r;
}

inline RECT RectUnion(const RECT& a, const RECT& b)
{
    RECT r;
    ::UnionRect(&r, &a, &b);
    return r;
}

inline bool RectEqual(const RECT& a, const RECT& b)
{
    return !!::EqualRect(&a, &b);
}

inline void RectMove(RECT& r, POINT p)
{
    ::OffsetRect(&r, p.x, p.y);
}

inline void RectMove(RECT& r, SIZE s)
{
    ::OffsetRect(&r, s.cx, s.cy);
}

inline POINT operator-(const POINT& p)
{
    return POINT{-p.x, -p.y};
}

inline SIZE operator-(const SIZE& s)
{
    return SIZE{-s.cx, -s.cy};
}

inline bool RectIsEmpty(const RECT& r)
{
    return !!::IsRectEmpty(&r);
}

// Every side moves outwards by the matching side of 'by' (what CRect did for rect + rect)
inline RECT RectInflate(RECT r, const RECT& by)
{
    r.left -= by.left;
    r.top -= by.top;
    r.right += by.right;
    r.bottom += by.bottom;
    return r;
}

inline RECT RectOffset(RECT r, int x, int y)
{
    ::OffsetRect(&r, x, y);
    return r;
}

inline bool operator==(const POINT& a, const POINT& b)
{
    return a.x == b.x && a.y == b.y;
}

inline bool operator!=(const POINT& a, const POINT& b)
{
    return !(a == b);
}

inline bool operator==(const SIZE& a, const SIZE& b)
{
    return a.cx == b.cx && a.cy == b.cy;
}

inline bool operator!=(const SIZE& a, const SIZE& b)
{
    return !(a == b);
}

inline POINT operator+(const POINT& a, const POINT& b)
{
    return POINT{a.x + b.x, a.y + b.y};
}

inline POINT operator-(const POINT& a, const POINT& b)
{
    return POINT{a.x - b.x, a.y - b.y};
}

inline SIZE operator+(const SIZE& a, const SIZE& b)
{
    return SIZE{a.cx + b.cx, a.cy + b.cy};
}

inline SIZE operator-(const SIZE& a, const SIZE& b)
{
    return SIZE{a.cx - b.cx, a.cy - b.cy};
}
