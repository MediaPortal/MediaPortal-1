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
#include <Windows.h>
#include "..\..\shared\SectionDecoder.h"
#include "..\..\shared\ChannelInfo.h"
#include <vector>
#include <map>
using namespace std;

extern void LogDebug(const char *fmt, ...);

#define PID_NIT 0x10

#define MIN_CABLE_FREQUENCY_KHZ 20000
#define MAX_CABLE_FREQUENCY_KHZ 900000

#define MIN_SATELLITE_FREQUENCY_KHZ 3000000
#define MAX_SATELLITE_FREQUENCY_KHZ 15000000

#define MIN_TERRESTRIAL_FREQUENCY_KHZ 40000
#define MAX_TERRESTRIAL_FREQUENCY_KHZ 900000

typedef struct NitMultiplexDetail
{
  int OriginalNetworkId;
  int TransportStreamId;

  virtual bool Equals(NitMultiplexDetail* mux)
  {
    try
    {
      if (mux != NULL && mux->OriginalNetworkId == OriginalNetworkId && mux->TransportStreamId == TransportStreamId)
      {
        return true;
      }
    }
    catch (...)
    {
      LogDebug("NitMultiplexDetail: unhandled exception in Equals()");
    }
    return false;
  }

  virtual void Clone(NitMultiplexDetail* clone)
  {
    try
    {
      if (clone == NULL)
      {
        return;
      }
      clone->OriginalNetworkId = OriginalNetworkId;
      clone->TransportStreamId = TransportStreamId;
    }
    catch (...)
    {
      LogDebug("NitMultiplexDetail: unhandled exception in Clone()");
    }
  }
}NitMultiplexDetail;

typedef struct NitCableMultiplexDetail : public NitMultiplexDetail
{
  int Frequency;        // unit = kHz
  int OuterFecMethod;
  int Modulation;
  int SymbolRate;       // unit = ks/s
  int InnerFecRate;

  bool Equals(NitMultiplexDetail* mux)
  {
    try
    {
      if (!NitMultiplexDetail::Equals(mux))
      {
        return false;
      }
      NitCableMultiplexDetail* cableMux = dynamic_cast<NitCableMultiplexDetail*>(mux);
      if (cableMux != NULL && cableMux->Frequency == Frequency && cableMux->Modulation == Modulation && cableMux->SymbolRate == SymbolRate)
      {
        return true;
      }
    }
    catch (...)
    {
      LogDebug("NitCableMultiplexDetail: unhandled exception in Equals()");
    }
    return false;
  }

  void Clone(NitMultiplexDetail* clone)
  {
    try
    {
      if (clone == NULL)
      {
        return;
      }
      NitCableMultiplexDetail* cableMuxClone = dynamic_cast<NitCableMultiplexDetail*>(clone);
      if (cableMuxClone == NULL)
      {
        return;
      }
      NitMultiplexDetail::Clone(cableMuxClone);
      cableMuxClone->Frequency = Frequency;
      cableMuxClone->OuterFecMethod = OuterFecMethod;
      cableMuxClone->Modulation = Modulation;
      cableMuxClone->SymbolRate = SymbolRate;
      cableMuxClone->InnerFecRate = InnerFecRate;
    }
    catch (...)
    {
      LogDebug("NitCableMultiplexDetail: unhandled exception in Clone()");
    }
  }
}NitCableMultiplexDetail;

typedef struct NitSatelliteMultiplexDetail : public NitMultiplexDetail
{
  int Frequency;        // unit = kHz
  int OrbitalPosition;  // unit = tenths of a degree
  bool WestEastFlag;
  int Polarisation;
  int Modulation;
  int SymbolRate;       // unit = ks/s
  int InnerFecRate;
  int RollOff;
  bool IsS2;
  bool MultipleInputStreamFlag;
  bool BackwardsCompatibilityIndicator;
  int ScramblingSequenceIndex;
  int InputStreamIdentifier;

  bool Equals(NitMultiplexDetail* mux)
  {
    try
    {
      if (!NitMultiplexDetail::Equals(mux))
      {
        return false;
      }
      NitSatelliteMultiplexDetail* satelliteMux = dynamic_cast<NitSatelliteMultiplexDetail*>(mux);
      if (satelliteMux != NULL && satelliteMux->Frequency == Frequency && satelliteMux->Modulation == Modulation &&
        satelliteMux->Polarisation == Polarisation && satelliteMux->SymbolRate == SymbolRate &&
        satelliteMux->InnerFecRate == InnerFecRate && satelliteMux->IsS2 == IsS2 &&
        satelliteMux->OrbitalPosition == OrbitalPosition && satelliteMux->WestEastFlag == WestEastFlag &&
        satelliteMux->InputStreamIdentifier == InputStreamIdentifier)
      {
        return true;
      }
    }
    catch (...)
    {
      LogDebug("NitSatelliteMultiplexDetail: unhandled exception in Equals()");
    }
    return false;
  }

  void Clone(NitSatelliteMultiplexDetail* clone)
  {
    try
    {
      if (clone == NULL)
      {
        return;
      }
      NitSatelliteMultiplexDetail* satelliteMuxClone = dynamic_cast<NitSatelliteMultiplexDetail*>(clone);
      if (satelliteMuxClone == NULL)
      {
        return;
      }
      NitMultiplexDetail::Clone(satelliteMuxClone);
      satelliteMuxClone->Frequency = Frequency;
      satelliteMuxClone->OrbitalPosition = OrbitalPosition;
      satelliteMuxClone->WestEastFlag = WestEastFlag;
      satelliteMuxClone->Polarisation = Polarisation;
      satelliteMuxClone->Modulation = Modulation;
      satelliteMuxClone->SymbolRate = SymbolRate;
      satelliteMuxClone->InnerFecRate = InnerFecRate;
      satelliteMuxClone->RollOff = RollOff;
      satelliteMuxClone->IsS2 = IsS2;
      satelliteMuxClone->MultipleInputStreamFlag = MultipleInputStreamFlag;
      satelliteMuxClone->BackwardsCompatibilityIndicator = BackwardsCompatibilityIndicator;
      satelliteMuxClone->ScramblingSequenceIndex = ScramblingSequenceIndex;
      satelliteMuxClone->InputStreamIdentifier = InputStreamIdentifier;
    }
    catch (...)
    {
      LogDebug("NitSatelliteMultiplexDetail: unhandled exception in Clone()");
    }
  }
}NitSatelliteMultiplexDetail;

typedef struct NitTerrestrialMultiplexDetail : public NitMultiplexDetail
{
  int CentreFrequency;  // unit = kHz
  int Bandwidth;        // unit = kHz
  bool IsHighPriority;
  bool TimeSlicingIndicator;
  bool MpeFecIndicator;
  int Constellation;
  bool IndepthInterleaverUsed;
  int HierarchyInformation;
  int CoderateHpStream;
  int CoderateLpStream;
  int GuardInterval;
  int TransmissionMode;
  bool OtherFrequencyFlag;
  int CellId;
  int CellIdExtension;
  bool MultipleInputStreamFlag;
  bool TimeFrequencySlicingFlag;
  int PlpId;
  int T2SystemId;

  bool Equals(NitMultiplexDetail* mux)
  {
    try
    {
      if (!NitMultiplexDetail::Equals(mux))
      {
        return false;
      }
      NitTerrestrialMultiplexDetail* terrestrialMux = dynamic_cast<NitTerrestrialMultiplexDetail*>(mux);
      if (terrestrialMux != NULL && terrestrialMux->CentreFrequency == CentreFrequency && terrestrialMux->Bandwidth == Bandwidth &&
        terrestrialMux->CellId == CellId && terrestrialMux->CellIdExtension == CellIdExtension &&
        terrestrialMux->PlpId == PlpId)
      {
        return true;
      }
    }
    catch (...)
    {
      LogDebug("NitTerrestrialMultiplexDetail: unhandled exception in Equals()");
    }
    return false;
  }

  void Clone(NitTerrestrialMultiplexDetail* clone)
  {
    try
    {
      if (clone == NULL)
      {
        return;
      }
      NitTerrestrialMultiplexDetail* terrestrialMuxClone = dynamic_cast<NitTerrestrialMultiplexDetail*>(clone);
      if (terrestrialMuxClone == NULL)
      {
        return;
      }
      NitMultiplexDetail::Clone(terrestrialMuxClone);
      terrestrialMuxClone->CentreFrequency = CentreFrequency;
      terrestrialMuxClone->Bandwidth = Bandwidth;
      terrestrialMuxClone->IsHighPriority = IsHighPriority;
      terrestrialMuxClone->TimeSlicingIndicator = TimeSlicingIndicator;
      terrestrialMuxClone->MpeFecIndicator = MpeFecIndicator;
      terrestrialMuxClone->Constellation = Constellation;
      terrestrialMuxClone->IndepthInterleaverUsed = IndepthInterleaverUsed;
      terrestrialMuxClone->HierarchyInformation = HierarchyInformation;
      terrestrialMuxClone->CoderateHpStream = CoderateHpStream;
      terrestrialMuxClone->CoderateLpStream = CoderateLpStream;
      terrestrialMuxClone->GuardInterval = GuardInterval;
      terrestrialMuxClone->TransmissionMode = TransmissionMode;
      terrestrialMuxClone->OtherFrequencyFlag = OtherFrequencyFlag;
      terrestrialMuxClone->CellId = CellId;
      terrestrialMuxClone->CellIdExtension = CellIdExtension;
      terrestrialMuxClone->MultipleInputStreamFlag = MultipleInputStreamFlag;
      terrestrialMuxClone->TimeFrequencySlicingFlag = TimeFrequencySlicingFlag;
      terrestrialMuxClone->PlpId = PlpId;
      terrestrialMuxClone->T2SystemId = T2SystemId;
    }
    catch (...)
    {
      LogDebug("NitTerrestrialMultiplexDetail: unhandled exception in Clone()");
    }
  }
}NitTerrestrialMultiplexDetail;

class CNitParser : public CSectionDecoder
{
  public:
    CNitParser(void);
    virtual ~CNitParser(void);
    void Reset();
    void OnNewSection(CSection& sections);
    bool IsReady();

    int GetLogicialChannelNumber(int originalNetworkId, int transportStreamId, int serviceId);
    void GetNetworkIds(int originalNetworkId, int transportStreamId, int serviceId, vector<int>* networkIds);
    void GetAvailableInCells(int originalNetworkId, int transportStreamId, int serviceId, vector<int>* cellIds);
    void GetTargetRegionIds(int originalNetworkId, int transportStreamId, int serviceId, vector<__int64>* targetRegionIds);
    void GetAvailableInCountries(int originalNetworkId, int transportStreamId, int serviceId, vector<unsigned int>* availableInCountries);
    void GetUnavailableInCountries(int originalNetworkId, int transportStreamId, int serviceId, vector<unsigned int>* unavailableInCountries);

    int GetNetworkNameCount(int networkId);
    void GetNetworkName(int networkId, int index, unsigned int* language, char** name);
    int GetTargetRegionNameCount(__int64 regionId);
    void GetTargetRegionName(__int64 regionId, int index, unsigned int* language, char** name);

    int GetMultiplexCount();
    NitMultiplexDetail* GetMultiplexDetail(int idx);

  protected:
    void CleanUp();

    void DecodeLogicalChannelNumberDescriptor(byte* b, int length, map<int, int>* lcns);
    bool DecodeCableDeliverySystemDescriptor(byte* b, int length, NitCableMultiplexDetail* mux);
    bool DecodeSatelliteDeliverySystemDescriptor(byte* b, int length, NitSatelliteMultiplexDetail* mux);
    bool DecodeS2SatelliteDeliverySystemDescriptor(byte* b, int length, NitSatelliteMultiplexDetail* mux);
    bool DecodeTerrestrialDeliverySystemDescriptor(byte* b, int length, NitTerrestrialMultiplexDetail* mux);
    bool DecodeT2TerrestrialDeliverySystemDescriptor(byte* b, int length, NitTerrestrialMultiplexDetail* mux, map<int, int>* frequencies);
    void DecodeFrequencyListDescriptor(byte* b, int length, vector<int>* frequencies);
    void DecodeServiceListDescriptor(byte* b, int length, vector<int>* services);
    void DecodeNameDescriptor(byte* b, int length, char** name);
    void DecodeMultilingualNameDescriptor(byte* b, int length, map<unsigned int, char*>* names);
    void DecodeCellFrequencyLinkDescriptor(byte* b, int length, map<int, int>* frequencies);
    void DecodeCountryAvailabilityDescriptor(byte* b, int length, vector<unsigned int>* availableInCountries, vector<unsigned int>* unavailableInCountries);
    void DecodeTargetRegionDescriptor(byte* b, int length, vector<__int64>* targetRegions);
    void DecodeTargetRegionNameDescriptor(byte* b, int length, map<__int64, char*>* names, unsigned int* language);

    int DecodeCableFrequency(byte* b);
    int DecodeSatelliteFrequency(byte* b);
    int DecodeTerrestrialFrequency(byte* b);

    void AddLogicalChannelNumbers(int originalNetworkId, int transportStreamId, map<int, int>* lcns);
    void AddGroupNames(int groupId, map<unsigned int, char*>* names);
    void AddTargetRegionNames(map<__int64, char*>* names, unsigned int language);
    void AddServiceDetails(int groupId, int originalNetworkId, int transportStreamId, vector<int>* serviceIds,
                            map<int, int>* cellFrequencies, vector<__int64>* targetRegions,
                            vector<unsigned int>* availableInCountries, vector<unsigned int>* unavailableInCountries);
    void AddMultiplexDetails(NitCableMultiplexDetail* cableMux, NitSatelliteMultiplexDetail* satelliteMux,
                              NitTerrestrialMultiplexDetail* terrestrialMux,
                              map<int, int>* cellFrequencies, vector<int>* frequencies);
    void AddCableMux(NitCableMultiplexDetail* mux);
    void AddSatelliteMux(NitSatelliteMultiplexDetail* mux);
    void AddTerrestrialMux(NitTerrestrialMultiplexDetail* mux);

    char* m_sName;
    vector<int> m_vTableIds;
    map<unsigned int, bool> m_mSeenSections;
    bool m_bIsReady;
    map<int, map<unsigned int, char*>*> m_mGroupNames;                  // network/bouquet ID -> [language -> name]
    map<__int64, map<unsigned int, char*>*> m_mTargetRegionNames;       // target region composite ID -> [language -> name]
    map<__int64, int> m_mLogicalChannelNumbers;                         // ONID | TSID | SID -> LCN
    map<__int64, map<int, bool>*> m_mGroupIds;                          // ONID | TSID | SID -> [network/bouquet ID -> TRUE]
    map<__int64, map<int, bool>*> m_mAvailableInCells;                  // ONID | TSID | SID -> [cell ID | cell ID extension -> TRUE]
    map<__int64, map<__int64, bool>*> m_mTargetRegions;                 // ONID | TSID | SID -> [target region composite ID -> TRUE]
    map<__int64, map<unsigned int, bool>*> m_mAvailableInCountries;     // ONID | TSID | SID -> [country ID -> TRUE]
    map<__int64, map<unsigned int, bool>*> m_mUnavailableInCountries;   // ONID | TSID | SID -> [country ID -> TRUE]
    vector<NitCableMultiplexDetail*> m_vCableMuxes;
    vector<NitSatelliteMultiplexDetail*> m_vSatelliteMuxes;
    vector<NitTerrestrialMultiplexDetail*> m_vTerrestrialMuxes;
};
