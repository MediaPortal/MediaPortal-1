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

#include "cagrabber.h"


extern void LogDebug(const char *fmt, ...) ;

FILE* fDump=NULL;


CCaGrabber::CCaGrabber(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsCaGrabber"), pUnk)
{
  FILE* fTest=fopen("dump.txt","r");
  if (fTest!=NULL)
  {
    fclose(fTest);
    ::DeleteFile("C:\\dump.ts");
    fDump=fopen("c:\\dump.ts","wb+");
  }

	m_pCallback=NULL;
	Reset();
}
CCaGrabber::~CCaGrabber(void)
{
  if (fDump!=NULL)
    fclose(fDump);
  fDump=NULL;
}


STDMETHODIMP CCaGrabber::Reset()
{
	LogDebug("cagrabber: reset");
	CSectionDecoder::Reset();
	CSectionDecoder::SetPid(1);
	CSectionDecoder::SetTableId(1);
	memset(m_caPrevData,0,sizeof(m_caPrevData));
	m_iCaVersion=-1;
	return S_OK;
}
STDMETHODIMP CCaGrabber::SetCallBack( ICACallback* callback)
{
	CEnterCriticalSection enter(m_section);
	LogDebug("cagrabber: set callback:%x", callback);
  m_pCallback=callback;
	return S_OK;
}

void CCaGrabber::OnTsPacket(byte* tsPacket)
{
	if (m_pCallback==NULL) return;

  if (fDump!=NULL)
  {
    fwrite(tsPacket,1,188,fDump);
  }

  int pid=((tsPacket[1] & 0x1F) <<8)+tsPacket[2];
  if (pid != 1) return;
	CEnterCriticalSection enter(m_section);
	CSectionDecoder::OnTsPacket(tsPacket);

}

void CCaGrabber::OnNewSection(CSection& section)
{
	try
	{
 		if (section.Version == m_iCaVersion) return;
	  CEnterCriticalSection enter(m_section);

		m_tsHeader.Decode(section.Data);
		int start=m_tsHeader.PayLoadStart;
    int table_id = section.Data[start];
	  if (table_id!=1) return;
    if (section.SectionLength<0 || section.SectionLength>=MAX_SECTION_LENGTH) return;

		LogDebug("cagrabber: got ca version:%d %d", section.Version,m_iCaVersion);
		m_iCaVersion=section.Version;
		m_iCaLength=section.SectionLength+3;

		memcpy(m_caData,&section.Data[start],m_iCaLength);
		if (memcmp(m_caData,m_caPrevData,m_iCaLength)!=0)
		{
			memcpy(m_caPrevData,m_caData,m_iCaLength);
			if (m_pCallback!=NULL)
			{
				LogDebug("cagrabber: do calback");
        m_pCallback->OnCaReceived();
			}
		}
	}
	catch(...)
	{
		LogDebug("CCaGrabber::OnNewSection exception");
	}
}

STDMETHODIMP CCaGrabber::GetCaData(BYTE *caData)
{
	try
	{
	  CEnterCriticalSection enter(m_section);
		if (m_iCaLength>0)
		{
			memcpy(caData,m_caData,m_iCaLength);
			return m_iCaLength;
		}
	}
	catch(...)
	{
		LogDebug("CCaGrabber::GetPMTData exception");
	}
	return 0;
}
