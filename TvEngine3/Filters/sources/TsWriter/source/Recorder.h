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
#include "filewriter.h"

// {B45662E3-2749-4a34-993A-0C1659E86E83}
DEFINE_GUID(IID_ITsRecorder,0xb45662e3, 0x2749, 0x4a34, 0x99, 0x3a, 0xc, 0x16, 0x59, 0xe8, 0x6e, 0x83);

// video anayzer interface
DECLARE_INTERFACE_(ITsRecorder, IUnknown)
{
	STDMETHOD(SetPcrPid)(THIS_ int pcrPid)PURE;
	STDMETHOD(AddPesStream)(THIS_ int pid)PURE;
	
  STDMETHOD(SetRecordingFileName)(THIS_ char* pszFileName)PURE;
  STDMETHOD(StartRecord)(THIS_ )PURE;
  STDMETHOD(StopRecord)(THIS_ )PURE;
};

class CRecorder: public CUnknown, public ITsRecorder, public IFileWriter
{
public:
	CRecorder(LPUNKNOWN pUnk, HRESULT *phr);
	~CRecorder(void);
  DECLARE_IUNKNOWN
	
	STDMETHODIMP SetPcrPid(int pcrPid);
	STDMETHODIMP AddPesStream(int pid);
	STDMETHODIMP SetRecordingFileName(char* pszFileName);
	STDMETHODIMP StartRecord();
	STDMETHODIMP StopRecord();

	void OnTsPacket(byte* tsPacket);
	void Write(byte* buffer, int len);
private:
	CMultiplexer m_multiPlexer;
	bool				 m_bRecording;
	char				 m_szFileName[2048];
	FileWriter* m_pRecordFile;
};
