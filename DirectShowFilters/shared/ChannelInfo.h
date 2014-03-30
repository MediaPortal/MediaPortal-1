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
#pragma once
#include <Windows.h>
#include <vector>
using namespace std;


class CChannelInfo
{
  public:
    CChannelInfo(void);
    CChannelInfo(const CChannelInfo& info);
    virtual ~CChannelInfo(void);

    CChannelInfo& operator = (const CChannelInfo &info);
    void Copy(const CChannelInfo& info);

    void Reset();
    void ClearStrings();
    void ReplaceServiceName(char* name);
    void ReplaceProviderName(char* name);
    void ReplaceLogicalChannelNumber(char* lcn);

    unsigned short OriginalNetworkId;
    unsigned short TransportStreamId;
    unsigned short ProgramNumber;
    char* ServiceName;
    char* ProviderName;
    char* LogicalChannelNumber;
    byte ServiceType;
    unsigned short VideoStreamCount;
    unsigned short AudioStreamCount;
    bool IsHighDefinition;
    bool IsThreeDimensional;
    bool IsEncrypted;
    bool IsRunning;
    bool IsOtherTransportStream;
    unsigned short PmtPid;
    unsigned short PreviousOriginalNetworkId;
    unsigned short PreviousTransportStreamId;
    unsigned short PreviousServiceId;

    // TODO use maps to keep these lists distinct?
    vector<unsigned short> NetworkIds;
    vector<unsigned short> BouquetIds;
    vector<unsigned long> Languages;                 // 3 x 1 byte characters with a NULL byte on the end
    vector<unsigned long> AvailableInCells;
    vector<unsigned long> UnavailableInCells;
    vector<__int64> TargetRegions;
    vector<unsigned long> AvailableInCountries;      // 3 x 1 byte characters with a NULL byte on the end
    vector<unsigned long> UnavailableInCountries;    // 3 x 1 byte characters with a NULL byte on the end

    bool IsPmtReceived;
    bool IsServiceInfoReceived;
    bool IsPidReceived;
};