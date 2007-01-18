/* 
 *	Copyright (C) 2006 Team MediaPortal
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

#include "packetsync.h"
#include "multiplexer.h"
#include "videoanalyzer.h"
#include "channelscan.h"
#include "pmtgrabber.h"
#include "recorder.h"
#include "timeshifting.h"
#include "teletextgrabber.h"
#include "cagrabber.h"

// {C564CEB9-FC77-4776-8CB8-96DD87624161}
DEFINE_GUID(IID_TSChannel, 0xc564ceb9, 0xfc77, 0x4776, 0x8c, 0xb8, 0x96, 0xdd, 0x87, 0x62, 0x41, 0x61);

DECLARE_INTERFACE_(ITSChannel, IUnknown)
{
  STDMETHOD(Test)()PURE;
};

class CTsChannel: public CUnknown, public ITSChannel
{
public:
	CTsChannel(LPUNKNOWN pUnk, HRESULT *phr);
	~CTsChannel(void);
  STDMETHODIMP QueryInterface(REFIID riid, void **ppv);        
  STDMETHODIMP_(ULONG) AddRef() {                             \
      return GetOwner()->AddRef();                            \
  };                                                          \
  STDMETHODIMP_(ULONG) Release() {                            \
      return GetOwner()->Release();                           \
  };

	STDMETHODIMP Test();
  void OnTsPacket(byte* tsPacket);

private:
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);
	CVideoAnalyzer* m_pVideoAnalyzer;
	CPmtGrabber*		m_pPmtGrabber;
	CRecorder*			m_pRecorder;
	CTimeShifting*	m_pTimeShifting;
	CTeletextGrabber*	m_pTeletextGrabber;
  CCaGrabber*     m_pCaGrabber;
};
