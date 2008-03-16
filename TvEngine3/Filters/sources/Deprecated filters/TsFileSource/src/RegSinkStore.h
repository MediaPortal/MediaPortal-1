/**
*  RegSinkStore.h
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2003  Shaun Faulds
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
*  bear can be reached on the forums at
*    http://forums.dvbowners.com/
*/
// RegSinkStore.h: interface for the RegSinkStore class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_REGSINKSTORE_H__73FB5892_2012_4F2F_831C_C10AD6E7C5B5__INCLUDED_)
#define AFX_REGSINKSTORE_H__73FB5892_2012_4F2F_831C_C10AD6E7C5B5__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "Windows.h"
#include <time.h>
#include <string>
#include <vector>
#include "SettingsSinkStore.h"

class CRegSinkStore  
{
public:
	CRegSinkStore(LPCSTR lpSubKey);
	virtual ~CRegSinkStore();

	int getInt(char *name, int def);
	BOOL setInt(char *name, int val);

	int getString(char *name, char *buff, int len);

	BOOL getSettingsInfo(CSettingsSinkStore *setStore);
	BOOL setSettingsInfo(CSettingsSinkStore *setStore);

private:
	HKEY rootkey;
	BOOL removeOld();

};

#endif // !defined(AFX_REGSINKSTORE_H__73FB5892_2012_4F2F_831C_C10AD6E7C5B5__INCLUDED_)
