/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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
#pragma warning(disable : 4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include "ConditionalAccess.h"

CConditionalAccess::CConditionalAccess(IFilterGraph *graph)
{
	m_pFireDtv = new CFireDtv(graph);
}

CConditionalAccess::~CConditionalAccess(void)
{
	delete m_pFireDtv;
}

bool CConditionalAccess::SetPids(vector<int> pids)
{
	if (m_pFireDtv->IsFireDtv())
	{
		return m_pFireDtv->SetPids(pids);
	}
	return true;
}
void CConditionalAccess::DisablePidFiltering()
{
	if (m_pFireDtv->IsFireDtv())
	{
		m_pFireDtv->DisablePidFiltering();
	}
}
