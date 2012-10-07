/* 
 *	Copyright (C) 2006-2010 Team MediaPortal
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
#include <vector>
using namespace std;

class CChannelInfo
{
  public:
    CChannelInfo(void);
    CChannelInfo(const CChannelInfo& info);
    virtual ~CChannelInfo(void);
    CChannelInfo& operator = (const CChannelInfo &info);
    void Copy(const CChannelInfo &info);
    void Reset();
    void ClearStrings();

    int OriginalNetworkId;
    int TransportStreamId;
    int ServiceId;
    char* ServiceName;
    char* ProviderName;
    char* LogicalChannelNumber;
    int ServiceType;
    int HasVideo;
    int HasAudio;
    bool IsHighDefinition;
    bool IsEncrypted;
    bool IsRunning;
    bool IsOtherMux;
    int PmtPid;
    int PreviousOriginalNetworkId;
    int PreviousTransportStreamId;
    int PreviousServiceId;

    vector<int> NetworkIds;
    vector<int> BouquetIds;
    vector<unsigned int> Languages;                 // 3 x 1 byte characters with a NULL byte on the end
    vector<int> AvailableInCells;
    vector<int> UnavailableInCells;
    vector<__int64> TargetRegions;
    vector<unsigned int> AvailableInCountries;      // 3 x 1 byte characters with a NULL byte on the end
    vector<unsigned int> UnavailableInCountries;    // 3 x 1 byte characters with a NULL byte on the end

    bool IsPmtReceived;
    bool IsServiceInfoReceived;
    bool IsPidReceived;
};
