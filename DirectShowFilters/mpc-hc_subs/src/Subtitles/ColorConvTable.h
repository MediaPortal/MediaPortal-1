/*
* (C) 2015 see Authors.txt
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

#define INCLUDE_COLOR_CONVERT 0

struct ColorConvTable {
    enum YuvMatrixType {
        AUTO,
        BT601,
        BT709,
        BT2020,
        NONE_RGB
    };

    enum YuvRangeType {
        RANGE_NONE,
        RANGE_TV,
        RANGE_PC
    };

    static void SetDefaultConvType(YuvMatrixType yuv_type, YuvRangeType range, bool bOutputTVRange, bool bVSFilterCorrection);

    static DWORD A8Y8U8V8_TO_ARGB(int a8, int y8, int u8, int v8, YuvMatrixType in_type);
    static DWORD RGB_PC_TO_TV(DWORD argb);

    static DWORD ColorCorrection(DWORD argb);

    ColorConvTable() = delete;
};
