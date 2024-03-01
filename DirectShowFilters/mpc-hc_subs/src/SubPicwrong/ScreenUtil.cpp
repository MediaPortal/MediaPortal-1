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
#include "../mpc-hc/Monitors.h"

CSize GetDesktopSize()
{
    return CSize(GetSystemMetrics(SM_CXVIRTUALSCREEN), GetSystemMetrics(SM_CYVIRTUALSCREEN));
}

CSize GetLargestScreenSize(CSize fallback)
{
    CSize result = CSize(0, 0);
    CSize desktop = GetDesktopSize();

    CMonitors monitors;
    if (monitors.GetCount() > 1) {
        // this includes virtual screens
        for (int i = 0; i < monitors.GetCount(); i++) {
            CMonitor monitor = monitors.GetMonitor(i);
            RECT rc;
            monitor.GetMonitorRect(&rc);
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

CSize GetBackBufferSize(CSize currentScreen, CSize largestScreen, bool use_desktop_size)
{
    if (use_desktop_size) {
        CSize desktop = GetDesktopSize();
        if (desktop.cx > 0 && desktop.cy > 0) {
            ASSERT(desktop.cx >= currentScreen.cx && desktop.cy >= currentScreen.cy);
            return desktop;
        }
    }

    CSize result = largestScreen;

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
