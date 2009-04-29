/**
*  TSFileSourceGuids.h
*  Copyright (C) 2003      bisswanger
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2005      nate
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  bisswanger can be reached at WinSTB@hotmail.com
*    Homepage: http://www.winstb.de
*
*  bear and nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#ifndef TSFILESOURCEGUIDS_H
#define TSFILESOURCEGUIDS_H

// {4F8BF30C-3BEB-43a3-8BF2-10096FD28CF2} New
DEFINE_GUID(CLSID_TSFileSource,
0x4f8bf30c, 0x3beb, 0x43a3, 0x8b, 0xf2, 0x10, 0x9, 0x6f, 0xd2, 0x8c, 0xf2);

// Old Property Page
// {66ED15AB-CCAC-4346-BB0A-9CFFE671411A}
//DEFINE_GUID(CLSID_TSFileSourceProp,
//0x66ed15ab, 0xccac, 0x4346, 0xbb, 0x0a, 0x9c, 0xff, 0xe6, 0x71, 0x41, 0x1A);

// New Property Page
// {69046395-162D-48e8-98D3-1E4897F1C0F2} New
DEFINE_GUID(CLSID_TSFileSourceProp, 
0x69046395, 0x162d, 0x48e8, 0x98, 0xd3, 0x1e, 0x48, 0x97, 0xf1, 0xc0, 0xf2);

#endif

