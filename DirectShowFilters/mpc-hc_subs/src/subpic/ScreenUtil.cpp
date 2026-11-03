/*
 * (C) 2021 clsid2
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
#include "ScreenUtil.h"
#include <vector>

namespace
{
    BOOL CALLBACK MonitorRectProc(HMONITOR, HDC, LPRECT rc, LPARAM param)
    {
        reinterpret_cast<std::vector<RECT>*>(param)->push_back(*rc);
        return TRUE;
    }
}

SIZE GetDesktopSize()
{
    return MakeSize(GetSystemMetrics(SM_CXVIRTUALSCREEN), GetSystemMetrics(SM_CYVIRTUALSCREEN));
}

SIZE GetLargestScreenSize(SIZE fallback)
{
    SIZE result = MakeSize(0, 0);
    SIZE desktop = GetDesktopSize();

    std::vector<RECT> monitors;
    EnumDisplayMonitors(nullptr, nullptr, MonitorRectProc, reinterpret_cast<LPARAM>(&monitors));
    if (monitors.size() > 1) {
        // this includes virtual screens
        for (const RECT& rc : monitors) {
            LONG w = rc.right - rc.left;
            LONG h = rc.bottom - rc.top;
            if (w > result.cx) {
                result.cx = w;
            }
            if (h > result.cy) {
                result.cy = h;
            }
        }
        // largest screen can't be larger than the active desktop
        if (result.cx > desktop.cx || result.cy > desktop.cy) {
            if (desktop.cx > 0 && desktop.cy > 0) {
                ASSERT(FALSE);
                return desktop;
            }
        }
    } else {
        if (desktop.cx > 0 && desktop.cy > 0) {
            return desktop;
        } else {
            return fallback;
        }
    }

    if (result.cx == 0 || result.cy == 0) {
        if (desktop.cx * desktop.cy > fallback.cx * fallback.cy) {
            result = desktop;
        } else {
            result = fallback;
        }
    }

    return result;
}

SIZE GetBackBufferSize(SIZE currentScreen, SIZE largestScreen, bool use_desktop_size)
{
    if (use_desktop_size) {
        SIZE desktop = GetDesktopSize();
        if (desktop.cx > 0 && desktop.cy > 0) {
            ASSERT(desktop.cx >= currentScreen.cx && desktop.cy >= currentScreen.cy);
            return desktop;
        }
    }

    SIZE result = largestScreen;

    if (currentScreen.cx > result.cx) {
        result.cx = currentScreen.cx;
        ASSERT(FALSE);
    }
    if (currentScreen.cy > result.cy) {
        result.cy = currentScreen.cy;
        ASSERT(FALSE);
    }

    return result;
}
