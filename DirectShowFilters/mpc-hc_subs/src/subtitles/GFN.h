/*
 *  $Id: GFN.h 2585 2010-09-18 12:39:20Z xhmikosr $
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

enum exttype {EXTSRT = 0, EXTSUB, EXTSMI, EXTPSB, EXTSSA, EXTASS, EXTIDX, EXTUSF, EXTXSS, EXTRT};
extern TCHAR* exttypestr[];
typedef struct {
	CString fn; /*exttype ext;*/
} SubFile;
extern void GetSubFileNames(CString fn, CAtlArray<CString>& paths, CAtlArray<SubFile>& ret);
