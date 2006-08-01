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
#include "patparser.h"

#pragma once
// {1663DC42-D169-41da-BCE2-EEEC482CB9FB}
DEFINE_GUID(IID_ITSChannelScan, 0x1663dc42, 0xd169, 0x41da, 0xbc, 0xe2, 0xee, 0xec, 0x48, 0x2c, 0xb9, 0xfb);

DECLARE_INTERFACE_(ITSChannelScan, IUnknown)
{
	STDMETHOD(Reset)(THIS_)PURE;
	STDMETHOD(GetCount)(THIS_ int* channelCount)PURE;
	STDMETHOD(GetChannel)(THIS_ int index,
										 int* networkId,
										 int* transportId,
										 int* serviceId,
										 int* majorChannel,
										 int* minorChannel,
										 int* frequency,
										 int* EIT_schedule_flag,
										 int* EIT_present_following_flag,
										 int* runningStatus,
										 int* freeCAMode,
										 int* serviceType,
										 int* modulation,
										 char** providerName,
										 char** serviceName,
										 int* pcrPid,
										 int* pmtPid,
										 int* videoPid,
										 int* audio1Pid,
										 int* audio2Pid,
										 int* audio3Pid,
										 int* ac3Pid,
										 char** audioLanguage1,
										 char** audioLanguage2,
										 char** audioLanguage3,
										 int* teletextPid,
										 int* subtitlePid)PURE;

};


class CChannelScan: public CUnknown, public ITSChannelScan
{
public:
	CChannelScan(LPUNKNOWN pUnk, HRESULT *phr);
	~CChannelScan(void);
	
  DECLARE_IUNKNOWN
	
	STDMETHODIMP Reset();
	STDMETHODIMP GetCount(int* channelCount);
	STDMETHODIMP GetChannel(int index,
										 int* networkId,
										 int* transportId,
										 int* serviceId,
										 int* majorChannel,
										 int* minorChannel,
										 int* frequency,
										 int* EIT_schedule_flag,
										 int* EIT_present_following_flag,
										 int* runningStatus,
										 int* freeCAMode,
										 int* serviceType,
										 int* modulation,
										 char** providerName,
										 char** serviceName,
										 int* pcrPid,
										 int* pmtPid,
										 int* videoPid,
										 int* audio1Pid,
										 int* audio2Pid,
										 int* audio3Pid,
										 int* ac3Pid,
										 char** audioLanguage1,
										 char** audioLanguage2,
										 char** audioLanguage3,
										 int* teletextPid,
										 int* subtitlePid);


	void OnTsPacket(byte* tsPacket);
private:
	CPatParser m_patParser;
};
