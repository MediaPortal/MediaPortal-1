/*
* (C) 2014, 2016 see Authors.txt
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
#include "Ellipse.h"

CEllipse::CEllipse(int rx, int ry)
    : m_rx(rx)
    , m_ry(ry)
    , m_2rx(2 * rx)
    , m_2ry(2 * ry)
    , nIntersectCacheLineSize(rx > 0 ? 2 * (rx - 1) + 1 : 0)
{
    m_arc.resize(m_2ry + 1);
    m_arc[m_ry] = m_rx;
    for (int dy = 1; dy <= m_ry; dy++) {
        m_arc[m_ry - dy] = m_arc[m_ry + dy] = std::lround(m_rx * std::sqrt(1 - double(dy * dy) / (m_ry * m_ry)));
    }

    m_intersectCache.clear();
    m_intersectCache.resize(nIntersectCacheLineSize * m_2ry, NOT_CACHED);
}

int CEllipse::GetLeftIntersect(int dx, int dy)
{
    ASSERT(dy >= 0); // We are sure dy is always greater or equal to 0 in our case

    // Crude conditions to filter every case that won't intersect at all or not on the left
    if (dx > -m_rx && dx < m_rx /*&& dy > -m_2ry*/ && dy < m_2ry) {
        const size_t nCache = nIntersectCacheLineSize * dy + dx + m_rx - 1;
        int iRes = m_intersectCache[nCache];

        if (iRes == NOT_CACHED) {
            iRes = (dx > 0) ? NO_INTERSECT_INNER : NO_INTERSECT_OUTER;

            double dx_2rx = double(dx) / m_2rx;
            double dy_2ry = double(dy) / m_2ry;
            double C = 1.0 / (dx_2rx * dx_2rx + dy_2ry * dy_2ry) - 1.0;

            // We might not intersect even if the above conditions are true
            if (C >= 0.0) {
                double sqrtC = std::sqrt(C);
                // Make sure that we are on the left for real
                if (m_rx * (dx_2rx - dy_2ry * sqrtC) < std::min(0, dx)) {
                    iRes = int(std::floor(m_ry * (dy_2ry + dx_2rx * sqrtC)));
                    // Account for possible rounding
                    if (GetArc(iRes) < GetArc(iRes - dy) - dx) {
                        iRes--;
                    }
                }
            }

            m_intersectCache[nCache] = iRes;
        }

        return iRes;
    }

    return (dx > 0) ? NO_INTERSECT_INNER : NO_INTERSECT_OUTER;
}

template<typename IntersectFunction>
void CEllipseCenterGroup::AddPoint(std::list<EllipseCenter>& centers, IntersectFunction intersect, int x, int y)
{
    auto it = centers.end();
    while (it != centers.begin()) {
        --it;
        auto& center = *it;

        int dyIntersect = intersect(x - center.x, y - center.y);
        if (dyIntersect != CEllipse::NO_INTERSECT_INNER) {
            int yIntersect = (dyIntersect == CEllipse::NO_INTERSECT_OUTER) ? (y - m_pEllipse->GetYRadius() - 1) : (center.y + dyIntersect);
            if (yIntersect < center.yStopDrawing) {
                center.yStopDrawing = yIntersect;
                if (it != centers.begin() && center.yStopDrawing <= std::prev(it)->yStopDrawing) {
                    it = centers.erase(it);
                }
            } else {
                break;
            }
        }
    }

    centers.emplace_back();
    auto& center = centers.back();
    center.x = x;
    center.y = y;
    center.yStopDrawing = y + m_pEllipse->GetYRadius();
}

void CEllipseCenterGroup::AddSpan(int y, int xLeft, int xRight)
{
    AddPoint(m_leftCenters, [this](int dx, int dy) {
        return m_pEllipse->GetLeftIntersect(dx, dy);
    }, xLeft, y);
    AddPoint(m_rightCenters, [this](int dx, int dy) {
        return m_pEllipse->GetRightIntersect(dx, dy);
    }, xRight, y);
}

CEllipseCenterGroup::Position CEllipseCenterGroup::GetRelativePosition(int xLeft, int y)
{
    if (IsEmpty()) {
        return INSIDE;
    } else {
        int dx = xLeft - m_leftCenters.back().x;
        int dy = y - m_leftCenters.back().y;

        int dyIntersect = m_pEllipse->GetLeftIntersect(dx, dy);
        if (dyIntersect != CEllipse::NO_INTERSECT_INNER && dyIntersect != CEllipse::NO_INTERSECT_OUTER) {
            return INSIDE;
        } else if (dx > 0) {
            return AFTER;
        } else {
            return BEFORE;
        }
    }
}

void CEllipseCenterGroup::FlushLine(int y, std::vector<SpanEndPoint>& wideSpanEndPoints)
{
    for (auto it = m_leftCenters.begin(); it != m_leftCenters.end();) {
        const EllipseCenter& leftCenter = *it;
        if (y <= leftCenter.yStopDrawing) {
            int dx = m_pEllipse->GetArc(leftCenter.y - y);
            wideSpanEndPoints.emplace_back(leftCenter.x - dx, false);

            if (y == leftCenter.yStopDrawing) {
                m_leftCenters.erase(it);
            }
            break;
        } else {
            it = m_leftCenters.erase(it);
        }
    }

    for (auto it = m_rightCenters.begin(); it != m_rightCenters.end();) {
        const EllipseCenter& rightCenter = *it;
        if (y <= rightCenter.yStopDrawing) {
            int dx = m_pEllipse->GetArc(rightCenter.y - y);
            wideSpanEndPoints.emplace_back(rightCenter.x + dx, true);

            if (y == rightCenter.yStopDrawing) {
                m_rightCenters.erase(it);
            }
            break;
        } else {
            it = m_rightCenters.erase(it);
        }
    }
}
