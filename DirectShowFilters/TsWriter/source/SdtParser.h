/* 
 *  Copyright (C) 2006-2008 Team MediaPortal
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
#include "..\..\shared\SectionDecoder.h"
#include "..\..\shared\ChannelInfo.h"
#include <map>
#include <vector>
using namespace std;

class ISdtCallBack
{
  public:
    virtual void OnSdtReceived(const CChannelInfo& sdtInfo) = 0;
};

#define PID_SDT 0x11

class CSdtParser : public CSectionDecoder
{
  public:
    CSdtParser(void);
    virtual ~CSdtParser(void);
    void Reset(bool parseSdtOther);
    void SetCallBack(ISdtCallBack* callBack);
    void OnNewSection(CSection& sections);
    bool IsReady();
    char* GetBouquetName(int bouquetId);

  private:
    void CleanUp();

    void DecodeServiceDescriptor(byte* b, int length, int* serviceType, char** providerName, char** serviceName);
    bool DecodeComponentDescriptor(byte* b, int length, bool* isVideo, bool* isAudio, bool* isHighDefinition, unsigned int* language);
    void DecodeServiceAvailabilityDescriptor(byte* b, int length, vector<int>* availableInCells, vector<int>* unavailableInCells);
    void DecodeCountryAvailabilityDescriptor(byte* b, int length, vector<unsigned int>* availableInCountries, vector<unsigned int>* unavailableInCountries);
    void DecodeBouquetNameDescriptor(byte* b, int length, char** name);
    void DecodeTargetRegionDescriptor(byte* b, int length, vector<__int64>* targetRegions);
    void DecodeServiceRelocatedDescriptor(byte* b, int length, int* previousOriginalNetworkId, int* previousTransportStreamId, int* previousServiceId);

    ISdtCallBack* m_pCallBack;
    map<unsigned __int64, bool> m_mSeenSections;
    bool m_bIsReady;
    bool m_bParseSdtOther;
    map<int, char*> m_mBouquetNames;  // fake bouquet ID -> bouquet name; we use fake bouquet IDs because the real ID is not available from the SDT
    int m_iNextBouquetId;             // next fake bouquet ID
};
