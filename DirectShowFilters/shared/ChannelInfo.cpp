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
  ServiceName = NULL;
  ProviderName = NULL;
  LogicalChannelNumber = NULL;
  Reset();
}

CChannelInfo::~CChannelInfo(void)
{
  ClearStrings();
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
  // Copy char* buffer members...
  ClearStrings();   // Remember to free the memory from the existing buffers.
  if (info.ServiceName != NULL)
  {
    ServiceName = new char[strlen(info.ServiceName) + 1];
    if (ServiceName != NULL)
    {
      strcpy(ServiceName, info.ServiceName);
    }
  }
  if (info.ProviderName != NULL)
  {
    ProviderName = new char[strlen(info.ProviderName) + 1];
    if (ProviderName != NULL)
    {
      strcpy(ProviderName, info.ProviderName);
    }
  }
  if (info.LogicalChannelNumber != NULL)
  {
    LogicalChannelNumber = new char[strlen(info.LogicalChannelNumber) + 1];
    if (LogicalChannelNumber != NULL)
    {
      strcpy(LogicalChannelNumber, info.LogicalChannelNumber);
    }
  }

  // Copy vector members...
  NetworkIds = info.NetworkIds;
  BouquetIds = info.BouquetIds;
  Languages = info.Languages;
  AvailableInCells = info.AvailableInCells;
  UnavailableInCells = info.UnavailableInCells;
  TargetRegions = info.TargetRegions;
  AvailableInCountries = info.AvailableInCountries;
  UnavailableInCountries = info.UnavailableInCountries;

  // Copy simple members...
  OriginalNetworkId = info.OriginalNetworkId;
  TransportStreamId = info.TransportStreamId;
  ServiceId = info.ServiceId;
  ServiceType = info.ServiceType;
  VideoStreamCount = info.VideoStreamCount;
  AudioStreamCount = info.AudioStreamCount;
  IsHighDefinition = info.IsHighDefinition;
  IsEncrypted = info.IsEncrypted;
  IsRunning = info.IsRunning;
  IsOtherMux = info.IsOtherMux;
  PmtPid = info.PmtPid;
  PreviousOriginalNetworkId = info.PreviousOriginalNetworkId;
  PreviousTransportStreamId = info.PreviousTransportStreamId;
  PreviousServiceId = info.PreviousServiceId;
  IsPmtReceived = info.IsPmtReceived;
  IsServiceInfoReceived = info.IsServiceInfoReceived;
  IsPidReceived = info.IsPidReceived;
}

void CChannelInfo::Reset()
{
  ClearStrings();
  OriginalNetworkId = 0;
  TransportStreamId = 0;
  ServiceId = 0;
  ServiceType = 0;
  VideoStreamCount = 0;
  AudioStreamCount = 0;
  IsHighDefinition = false;
  IsEncrypted = false;
  IsRunning = false;
  IsOtherMux = false;
  PmtPid = 0;
  PreviousOriginalNetworkId = 0;
  PreviousTransportStreamId = 0;
  PreviousServiceId = 0;
  NetworkIds.clear();
  BouquetIds.clear();
  Languages.clear();
  AvailableInCells.clear();
  UnavailableInCells.clear();
  TargetRegions.clear();
  AvailableInCountries.clear();
  UnavailableInCountries.clear();
  IsPmtReceived = false;
  IsServiceInfoReceived = false;
  IsPidReceived = false;
}

void CChannelInfo::ClearStrings()
{
  if (ServiceName != NULL)
  {
    delete[] ServiceName;
    ServiceName = NULL;
  }
  if (ProviderName != NULL)
  {
    delete[] ProviderName;
    ProviderName = NULL;
  }
  if (LogicalChannelNumber != NULL)
  {
    delete[] LogicalChannelNumber;
    LogicalChannelNumber = NULL;
  }
}
