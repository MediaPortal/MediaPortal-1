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
#include "StdAfx.h"
#include "ChannelInfo.h"

CChannelInfo::CChannelInfo(const CChannelInfo& info)
{
  Copy(info);
}
CChannelInfo::CChannelInfo(void)
{
  Reset();
}

CChannelInfo::~CChannelInfo(void)
{
}

void CChannelInfo::Reset()
{
	LCN=10000;
  NetworkId=0;
  TransportId=0;
  ServiceId=0;
	ServiceType=0;
  FreeCAMode=0;
  MajorChannel=0;
  MinorChannel=0;
  Frequency=0;
  Modulation=0;
  strcpy(ProviderName,"");
  strcpy(ServiceName,"");
	hasVideo=0;
	hasAudio=0;
	OtherMux=false;
	PmtReceived=false;
	SdtReceived=false;
}
CChannelInfo& CChannelInfo::operator = (const CChannelInfo &info)
{
  if (&info==this) return *this;
  Copy(info);
  return *this;
}

void CChannelInfo::Copy(const CChannelInfo &info)
{
	LCN=info.LCN;
  NetworkId=info.NetworkId;
  TransportId=info.TransportId;
  ServiceId=info.ServiceId;
	ServiceType=info.ServiceType;
  FreeCAMode=info.FreeCAMode;
  ServiceType=info.ServiceType;
  MajorChannel=info.MajorChannel;
  MinorChannel=info.MinorChannel;
  Frequency=info.Frequency;
  Modulation=info.Modulation;
  strcpy(ProviderName,info.ProviderName);
  strcpy(ServiceName,info.ServiceName);
	hasVideo=info.hasVideo;
	hasAudio=info.hasAudio;
	OtherMux=info.OtherMux;
	PmtReceived=info.PmtReceived;
	SdtReceived=info.SdtReceived;
  PidTable=info.PidTable;
}
