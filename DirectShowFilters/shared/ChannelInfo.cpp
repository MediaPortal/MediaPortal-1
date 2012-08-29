/* 
 *  Copyright (C) 2006-2010 Team MediaPortal
 *  http://www.team-mediaportal.com
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
// For more details for memory leak detection see the alloctracing.h header
#include "..\alloctracing.h"

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
  NetworkId = 0;
  TransportId = 0;
  ServiceId = 0;
  strcpy(ServiceName, "");
  strcpy(ProviderName, "");
  strcpy(NetworkNames, "");
  strcpy(LogicalChannelNumber, "10000");
  ServiceType = 0;
  HasVideo = 0;
  HasAudio = 0;
  IsEncrypted = false;
  IsRunning = false;
  IsOtherMux = false;
  PmtPid = 0;
  IsPmtReceived = false;
  IsServiceInfoReceived = false;
  PatVersion = -1;
}

CChannelInfo& CChannelInfo::operator = (const CChannelInfo &info)
{
  if (&info == this)
  {
    return *this;
  }
  Copy(info);
  return *this;
}

void CChannelInfo::Copy(const CChannelInfo &info)
{
  NetworkId = info.NetworkId;
  TransportId = info.TransportId;
  ServiceId = info.ServiceId;
  strcpy(ServiceName, info.ServiceName);
  strcpy(ProviderName, info.ProviderName);
  strcpy(NetworkNames, info.NetworkNames);
  strcpy(LogicalChannelNumber, info.LogicalChannelNumber);
  ServiceType = info.ServiceType;
  HasVideo = info.HasVideo;
  HasAudio = info.HasAudio;
  IsEncrypted = info.IsEncrypted;
  IsRunning = info.IsRunning;
  IsOtherMux = info.IsOtherMux;
  PmtPid = info.PmtPid;
  IsPmtReceived = info.IsPmtReceived;
  IsServiceInfoReceived = info.IsServiceInfoReceived;
  //PatVersion = info.PatVersion;
}
