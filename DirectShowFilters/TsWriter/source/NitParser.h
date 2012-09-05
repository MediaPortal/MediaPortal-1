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
#include "..\..\shared\sectiondecoder.h"
#include "..\..\shared\section.h"
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

typedef struct NitLcn
{
  int NetworkId;
  int TransportStreamId;
  int ServiceId;
  int Lcn;
}NitLcn;

typedef struct NitNameSet
{
  int NetworkId;
  int TransportStreamId;
  int ServiceId;
  vector<char*> Names;
}NitNameSet;

typedef struct NitMultiplexDetail
{
  int NetworkId;
  int TransportStreamId;

  virtual bool Equals(NitMultiplexDetail* mux)
  {
    try
    {
      if (mux != NULL && mux->NetworkId == NetworkId && mux->TransportStreamId == TransportStreamId)
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
      clone->NetworkId = NetworkId;
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
  int Frequency;
  int OuterFecMethod;
  int Modulation;
  int SymbolRate;
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
  int Frequency;
  float OrbitalPosition;
  int WestEastFlag;
  int Polarisation;
  int Modulation;
  int SymbolRate;
  int InnerFecRate;
  int RollOff;
  int IsS2;

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
        satelliteMux->InnerFecRate == InnerFecRate && satelliteMux->IsS2 == IsS2)
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
    }
    catch (...)
    {
      LogDebug("NitSatelliteMultiplexDetail: unhandled exception in Clone()");
    }
  }
}NitSatelliteMultiplexDetail;

typedef struct NitTerrestrialMultiplexDetail : public NitMultiplexDetail
{
  int CentreFrequency;
  int Bandwidth;
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
  int OtherFrequencyFlag;

  bool Equals(NitMultiplexDetail* mux)
  {
    try
    {
      if (!NitMultiplexDetail::Equals(mux))
      {
        return false;
      }
      NitTerrestrialMultiplexDetail* terrestrialMux = dynamic_cast<NitTerrestrialMultiplexDetail*>(mux);
      if (terrestrialMux != NULL && terrestrialMux->CentreFrequency == CentreFrequency && terrestrialMux->Bandwidth == Bandwidth)
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
    int GetLogicialChannelNumber(int networkId, int transportStreamId, int serviceId);
    vector<char*>* GetGroupNames(int networkId, int transportStreamId, int serviceId);
    int GetMultiplexCount();
    NitMultiplexDetail* GetMultiplexDetail(int idx);

  protected:
    void CleanUp();

    void DecodeLogicalChannelNumberDescriptor(byte* b, int length, map<int, int>* lcns);
    void DecodeCableDeliverySystemDescriptor(byte* b, int length, NitCableMultiplexDetail* mux);
    void DecodeSatelliteDeliverySystemDescriptor(byte* b, int length, NitSatelliteMultiplexDetail* mux);
    void DecodeTerrestrialDeliverySystemDescriptor(byte* b, int length, NitTerrestrialMultiplexDetail* mux);
    void DecodeFrequencyListDescriptor(byte* b, int length, vector<int>* frequencies, int* frequencyType);
    void DecodeServiceListDescriptor(byte* b, int length, vector<int>* services);
    void DecodeNameDescriptor(byte* b, int length, char** name);
    void DecodeMultilingualNameDescriptor(byte* b, int length, vector<char*>* names);
    void DecodeCellFrequencyLinkDescriptor(byte* b, int length, vector<int>* frequencies);

    int DecodeCableFrequency(byte* b);
    int DecodeSatelliteFrequency(byte* b);
    int DecodeTerrestrialFrequency(byte* b);

    void AddLogicalChannelNumbers(int nid, int tsid, map<int, int>* lcns);
    void AddGroupNames(int nid, int tsid, int sid, vector<char*>* names);
    void AddSatelliteMux(NitSatelliteMultiplexDetail* mux, vector<int>* frequencies);
    void AddCableMux(NitCableMultiplexDetail* mux, vector<int>* frequencies);
    void AddTerrestrialMux(NitTerrestrialMultiplexDetail* mux, vector<int>* frequencies);

    char* m_sName;
    vector<int> m_vTableIds;
    map<unsigned int, bool> m_mSeenSections;
    bool m_bIsReady;
    vector<NitLcn*> m_vLcns;
    vector<NitNameSet*> m_vGroupNames;
    vector<NitCableMultiplexDetail*> m_vCableMuxes;
    vector<NitSatelliteMultiplexDetail*> m_vSatelliteMuxes;
    vector<NitTerrestrialMultiplexDetail*> m_vTerrestrialMuxes;
};
