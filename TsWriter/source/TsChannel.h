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
#pragma once

#include "..\..\shared\packetsync.h"
#include "videoanalyzer.h"
#include "channelscan.h"
#include "pmtgrabber.h"
//#include "recorder.h"
//#include "timeshifting.h"
#include "DiskRecorder.h"
#include "teletextgrabber.h"
#include "cagrabber.h"

// {C564CEB9-FC77-4776-8CB8-96DD87624161}

class CTsChannel
{
public:
	CTsChannel(LPUNKNOWN pUnk, HRESULT *phr, int id);
	virtual ~CTsChannel(void);
  void OnTsPacket(byte* tsPacket);
	int Handle() { return m_id;}

	CVideoAnalyzer* m_pVideoAnalyzer;
	CPmtGrabber*		m_pPmtGrabber;
	CDiskRecorder*	m_pRecorder;
	CDiskRecorder*	m_pTimeShifting;
	CTeletextGrabber*	m_pTeletextGrabber;
  CCaGrabber*     m_pCaGrabber;
	int m_id;
};
