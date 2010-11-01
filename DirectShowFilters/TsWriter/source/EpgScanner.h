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
#include "epgparser.h"
#include "mhwparser.h"
#include "criticalsection.h"
#include "entercriticalsection.h"
#include "..\..\shared\TsHeader.h"

using namespace Mediaportal;

// {5CDAC655-D9FB-4c71-8119-DD07FE86A9CE}
DEFINE_GUID(IID_ITsEpgScanner, 0x5cdac655, 0xd9fb, 0x4c71, 0x81, 0x19, 0xdd, 0x7, 0xfe, 0x86, 0xa9, 0xce);

// {FFAB5D98-2309-4d90-9C71-E4B2F490CF5A}
DEFINE_GUID(IID_IEpgCallback,0xffab5d98, 0x2309, 0x4d90,  0x9c, 0x71, 0xe4, 0xb2, 0xf4, 0x90, 0xcf, 0x5a );

DECLARE_INTERFACE_(IEpgCallback, IUnknown)
{
	STDMETHOD(OnEpgReceived)()PURE;
};


// video anayzer interface
DECLARE_INTERFACE_(ITsEpgScanner, IUnknown)
{
	//epg
	STDMETHOD(GrabEPG)(THIS_)PURE;
	STDMETHOD(IsEPGReady) (THIS_ BOOL* yesNo)PURE;
	STDMETHOD(GetEPGChannelCount) (THIS_ ULONG* channelCount)PURE;
	STDMETHOD(GetEPGEventCount) (THIS_ ULONG channel, ULONG* eventCount)PURE;
	STDMETHOD(GetEPGChannel) (THIS_ ULONG channel, WORD* networkId, WORD* transportid,WORD* service_id  )PURE;
	STDMETHOD(GetEPGEvent) (THIS_ ULONG channel, ULONG eventid,ULONG* language,ULONG* dateMJD,ULONG* timeUTC,ULONG* duration,char** genre,int* starRating,char** classification    )PURE;
	STDMETHOD(GetEPGLanguage) (THIS_ ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language,char** eventText, char** eventDescription, unsigned int* parentalRating  )PURE;

	//mhw
	STDMETHOD(GrabMHW)()PURE;
	STDMETHOD(IsMHWReady) (THIS_ BOOL* yesNo)PURE;
	STDMETHOD(GetMHWTitleCount)(THIS_ UINT* count)PURE;
	STDMETHOD(GetMHWTitle)(THIS_ UINT program, UINT* id, UINT* transportId, UINT* networkId, UINT* channelId, ULONG* programId, UINT* themeId, UINT* PPV, BYTE* Summaries, UINT* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName)PURE;
	STDMETHOD(GetMHWChannel)(THIS_ UINT channelNr, UINT* channelId, UINT* networkId, UINT* transportId, char** channelName)PURE;
	STDMETHOD(GetMHWSummary)(THIS_ ULONG programId, char** summary)PURE;
	STDMETHOD(GetMHWTheme)(THIS_ UINT themeId, char** theme)PURE;
	STDMETHOD(Reset)(THIS_)PURE;
	STDMETHOD(AbortGrabbing)(THIS_)PURE;
  
	STDMETHOD(SetCallBack)(THIS_ IEpgCallback* callback)PURE;
};

class CEpgScanner: public CUnknown, public ITsEpgScanner
{
public:
	CEpgScanner(LPUNKNOWN pUnk, HRESULT *phr);
	~CEpgScanner(void);

  DECLARE_IUNKNOWN
//EPG
	STDMETHODIMP GrabEPG();
	STDMETHODIMP IsEPGReady(BOOL* yesNo);
	STDMETHODIMP GetEPGChannelCount( ULONG* channelCount);
	STDMETHODIMP GetEPGEventCount( ULONG channel,  ULONG* eventCount);
	STDMETHODIMP GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  );
	STDMETHODIMP GetEPGEvent( ULONG channel,  ULONG eventid,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** genre,int* starRating,char** classification    );
	STDMETHODIMP GetEPGLanguage(THIS_ ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language,char** eventText, char** eventDescription,unsigned int* parentalRating    );
	//MHW
	STDMETHODIMP GrabMHW();
	STDMETHODIMP IsMHWReady(BOOL* yesNo);

	STDMETHODIMP GetMHWTitleCount(UINT* count);
	STDMETHODIMP GetMHWTitle(UINT program, UINT* id, UINT* transportId, UINT* networkId, UINT* channelId, ULONG* programId, UINT* themeId, UINT* PPV, BYTE* Summaries, UINT* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName);
	STDMETHODIMP GetMHWChannel(UINT channelNr, UINT* channelId, UINT* networkId, UINT* transportId, char** channelName);
	STDMETHODIMP GetMHWSummary(ULONG programId, char** summary);
	STDMETHODIMP GetMHWTheme(UINT themeId, char** theme);
	STDMETHODIMP Reset();
	STDMETHODIMP AbortGrabbing();
	STDMETHODIMP SetCallBack(IEpgCallback* callback);

	void OnTsPacket(byte* tsPacket);
protected:
	CEpgParser m_epgParser;
	CMhwParser m_mhwParser;
  IEpgCallback* m_pCallBack;

  	bool IsEPG_PID(int pid);
	bool IsMHW_PID(int pid);
	bool IsEIT_PID(int pid);
private:
	bool m_bGrabbing;
	CCriticalSection m_section;
  CTsHeader m_header;
};
