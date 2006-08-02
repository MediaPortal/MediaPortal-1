/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#include "multiplexer.h"
#include "multifilewriter.h"

// {89459BF6-D00E-4d28-928E-9DA8F76B6D3A}
DEFINE_GUID(IID_ITsTimeshifting,0x89459bf6, 0xd00e, 0x4d28, 0x92, 0x8e, 0x9d, 0xa8, 0xf7, 0x6b, 0x6d, 0x3a);

// video anayzer interface
DECLARE_INTERFACE_(ITsTimeshifting, IUnknown)
{
	STDMETHOD(SetPcrPid)(THIS_ int pcrPid)PURE;
	STDMETHOD(AddPesStream)(THIS_ int pid)PURE;
	
  STDMETHOD(SetTimeShiftingFileName)(THIS_ char* pszFileName)PURE;
  STDMETHOD(Start)(THIS_ )PURE;
  STDMETHOD(Stop)(THIS_ )PURE;
};

class CTimeShifting: public CUnknown, public ITsTimeshifting, public IFileWriter
{
public:
	CTimeShifting(LPUNKNOWN pUnk, HRESULT *phr);
	~CTimeShifting(void);
  DECLARE_IUNKNOWN
	
	STDMETHODIMP SetPcrPid(int pcrPid);
	STDMETHODIMP AddPesStream(int pid);
	STDMETHODIMP SetTimeShiftingFileName(char* pszFileName);
	STDMETHODIMP Start();
	STDMETHODIMP Stop();

	void OnTsPacket(byte* tsPacket);
	void Write(byte* buffer, int len);
private:
	CMultiplexer m_multiPlexer;
	bool				 m_bTimeShifting;
	char				 m_szFileName[2048];
	MultiFileWriter* m_pTimeShiftFile;
};
