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
#include <vector>
#include "..\..\shared\CriticalSection.h"
#include "..\..\shared\Section.h"
#include "ICallBackNitAtsc.h"
#include "IRecord.h"
#include "RecordStore.h"

using namespace MediaPortal;
using namespace std;


#define TABLE_ID_NIT_ATSC 0xc2


extern void LogDebug(const wchar_t* fmt, ...);

class CParserNitAtsc
{
  public:
    CParserNitAtsc();
    virtual ~CParserNitAtsc();

    void Reset();
    void SetCallBack(ICallBackNitAtsc* callBack);
    void OnNewSection(CSection& section);
    bool IsSeen() const;
    bool IsReady() const;

    bool GetCarrierDefinition(unsigned char index,
                              unsigned char transmissionMedium,
                              unsigned long& frequency) const;
    bool GetModulationMode(unsigned char index,
                            unsigned char transmissionMedium,
                            unsigned char& transmissionSystem,
                            unsigned char& innerCodingMode,
                            bool& splitBitstreamMode,
                            unsigned char& modulationFormat,
                            unsigned long& symbolRate) const;
    bool GetSatelliteInformation(unsigned char transmissionMedium,
                                  unsigned char satelliteId,
                                  bool& youAreHere,
                                  unsigned char& frequencyBand,
                                  bool& outOfService,
                                  unsigned char& hemisphere,
                                  unsigned short& orbitalPosition,
                                  unsigned char& polarisationType,
                                  unsigned char& numberOfTransponders) const;
    bool GetTransponderData(unsigned char transmissionMedium,
                            unsigned char satelliteId,
                            unsigned char transponderNumber,
                            unsigned char& transportType,
                            unsigned char& polarisation,
                            unsigned char& cdtReference,
                            unsigned char& mmtReference,
                            unsigned short& vctId,
                            bool& rootTransponder,
                            bool& wideBandwidthVideo,
                            unsigned char& waveformStandard,
                            bool& wideBandwidthAudio,
                            bool& compandedAudio,
                            unsigned char& matrixMode,
                            unsigned short& subcarrier2Offset,
                            unsigned short& subcarrier1Offset,
                            unsigned long& carrierFrequencyOverride) const;

  private:
    class CRecordNit : public IRecord
    {
      public:
        CRecordNit()
        {
          Index = 0;
          TransmissionMedium = 0;
        }

        virtual ~CRecordNit()
        {
        }

        virtual bool Equals(const IRecord* record) const
        {
          const CRecordNit* recordNit = dynamic_cast<const CRecordNit*>(record);
          if (
            recordNit == NULL ||
            Index != recordNit->Index ||
            TransmissionMedium != recordNit->TransmissionMedium
          )
          {
            return false;
          }
          return true;
        }

        virtual unsigned long long GetKey() const
        {
          return (TransmissionMedium << 8) | Index;
        }

        virtual unsigned long long GetExpiryKey() const
        {
          return TransmissionMedium;
        }

        unsigned char Index;
        unsigned char TransmissionMedium;
    };

    class CRecordNitCarrierDefinition : public CRecordNit
    {
      public:
        CRecordNitCarrierDefinition()
        {
          Frequency = 0;
        }

        ~CRecordNitCarrierDefinition()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNit::Equals(record))
          {
            return false;
          }

          const CRecordNitCarrierDefinition* recordCarrierDefinition = dynamic_cast<const CRecordNitCarrierDefinition*>(record);
          if (
            recordCarrierDefinition == NULL ||
            Frequency != recordCarrierDefinition->Frequency
          )
          {
            return false;
          }
          return true;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NIT ATSC: carrier definition %s, index = %hhu, transmission medium = %hhu, frequency = %lu kHz",
                    situation, Index, TransmissionMedium, Frequency);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNitAtsc* callBackNit = static_cast<ICallBackNitAtsc*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitCdtReceived(Index, TransmissionMedium, Frequency);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNitAtsc* callBackNit = static_cast<ICallBackNitAtsc*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitCdtChanged(Index, TransmissionMedium, Frequency);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNitAtsc* callBackNit = static_cast<ICallBackNitAtsc*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitCdtRemoved(Index, TransmissionMedium, Frequency);
          }
        }

        unsigned long Frequency;  // unit = kHz
    };

    class CRecordNitModulationMode : public CRecordNit
    {
      public:
        CRecordNitModulationMode()
        {
          TransmissionSystem = 0;
          InnerCodingMode = 0;
          SplitBitstreamMode = false;
          ModulationFormat = 0;
          SymbolRate = 0;
        }

        ~CRecordNitModulationMode()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNit::Equals(record))
          {
            return false;
          }

          const CRecordNitModulationMode* recordModulationMode = dynamic_cast<const CRecordNitModulationMode*>(record);
          if (
            recordModulationMode == NULL ||
            TransmissionSystem != recordModulationMode->TransmissionSystem ||
            InnerCodingMode != recordModulationMode->InnerCodingMode ||
            SplitBitstreamMode != recordModulationMode->SplitBitstreamMode ||
            ModulationFormat != recordModulationMode->ModulationFormat ||
            SymbolRate != recordModulationMode->SymbolRate
          )
          {
            return false;
          }
          return true;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NIT ATSC: modulation mode %s, index = %hhu, transmission medium = %hhu, transmission system = %hhu, inner coding mode = %hhu, split bitstream mode = %d, modulation format = %hhu, symbol rate = %lu s/s",
                    situation, Index, TransmissionMedium, TransmissionSystem,
                    InnerCodingMode, SplitBitstreamMode, ModulationFormat,
                    SymbolRate);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNitAtsc* callBackNit = static_cast<ICallBackNitAtsc*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitMmtReceived(Index,
                                          TransmissionMedium,
                                          TransmissionSystem,
                                          InnerCodingMode,
                                          SplitBitstreamMode,
                                          ModulationFormat,
                                          SymbolRate);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNitAtsc* callBackNit = static_cast<ICallBackNitAtsc*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitMmtChanged(Index,
                                          TransmissionMedium,
                                          TransmissionSystem,
                                          InnerCodingMode,
                                          SplitBitstreamMode,
                                          ModulationFormat,
                                          SymbolRate);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNitAtsc* callBackNit = static_cast<ICallBackNitAtsc*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitMmtRemoved(Index,
                                          TransmissionMedium,
                                          TransmissionSystem,
                                          InnerCodingMode,
                                          SplitBitstreamMode,
                                          ModulationFormat,
                                          SymbolRate);
          }
        }

        unsigned char TransmissionSystem;
        unsigned char InnerCodingMode;
        bool SplitBitstreamMode;
        unsigned char ModulationFormat;
        unsigned long SymbolRate;   // unit = s/s
    };

    class CRecordNitSatelliteInformation : public CRecordNit
    {
      public:
        CRecordNitSatelliteInformation()
        {
          SatelliteId = 0;
          YouAreHere = false;
          FrequencyBand = 0;
          OutOfService = false;
          Hemisphere = 0;
          OrbitalPosition = 0;
          PolarisationType = 0;
          NumberOfTransponders = 0;
        }

        ~CRecordNitSatelliteInformation()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNit::Equals(record))
          {
            return false;
          }

          const CRecordNitSatelliteInformation* recordSatelliteInformation = dynamic_cast<const CRecordNitSatelliteInformation*>(record);
          if (
            recordSatelliteInformation == NULL ||
            SatelliteId != recordSatelliteInformation->SatelliteId ||
            YouAreHere != recordSatelliteInformation->YouAreHere ||
            FrequencyBand != recordSatelliteInformation->FrequencyBand ||
            OutOfService != recordSatelliteInformation->OutOfService ||
            Hemisphere != recordSatelliteInformation->Hemisphere ||
            OrbitalPosition != recordSatelliteInformation->OrbitalPosition ||
            PolarisationType != recordSatelliteInformation->PolarisationType ||
            NumberOfTransponders != recordSatelliteInformation->NumberOfTransponders
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (TransmissionMedium << 8) | SatelliteId;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NIT ATSC: satellite information %s, index = %hhu, transmission medium = %hhu, satellite ID = %hhu, you are here = %d, frequency band = %hhu, out of service = %d, hemisphere = %hhu, orbital position = %hu, polarisation type = %hhu, number of transponders = %hhu",
                    situation, Index, TransmissionMedium, SatelliteId,
                    YouAreHere, FrequencyBand, OutOfService, Hemisphere,
                    OrbitalPosition, PolarisationType, NumberOfTransponders);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNitAtsc* callBackNit = static_cast<ICallBackNitAtsc*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitSitReceived(Index,
                                          TransmissionMedium,
                                          SatelliteId,
                                          YouAreHere,
                                          FrequencyBand,
                                          OutOfService,
                                          Hemisphere,
                                          OrbitalPosition,
                                          PolarisationType,
                                          NumberOfTransponders);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNitAtsc* callBackNit = static_cast<ICallBackNitAtsc*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitSitChanged(Index,
                                          TransmissionMedium,
                                          SatelliteId,
                                          YouAreHere,
                                          FrequencyBand,
                                          OutOfService,
                                          Hemisphere,
                                          OrbitalPosition,
                                          PolarisationType,
                                          NumberOfTransponders);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNitAtsc* callBackNit = static_cast<ICallBackNitAtsc*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitSitRemoved(Index,
                                          TransmissionMedium,
                                          SatelliteId,
                                          YouAreHere,
                                          FrequencyBand,
                                          OutOfService,
                                          Hemisphere,
                                          OrbitalPosition,
                                          PolarisationType,
                                          NumberOfTransponders);
          }
        }

        unsigned char SatelliteId;
        bool YouAreHere;
        unsigned char FrequencyBand;
        bool OutOfService;
        unsigned char Hemisphere;
        unsigned short OrbitalPosition;
        unsigned char PolarisationType;
        unsigned char NumberOfTransponders;
    };

    class CRecordNitTransponderData : public CRecordNit
    {
      public:
        CRecordNitTransponderData()
        {
          SatelliteId = 0;
          TransportType = 0;
          Polarisation = 0;
          TransponderNumber = 0;
          CdtReference = 0;
          MmtReference = 0;
          VctId = 0;
          RootTransponder = false;
          WideBandwidthVideo = false;
          WaveformStandard = 0;
          WideBandwidthAudio = false;
          CompandedAudio = false;
          MatrixMode = 0;
          Subcarrier2Offset = 0;
          Subcarrier1Offset = 0;
          CarrierFrequencyOverride = 0;
        }

        ~CRecordNitTransponderData()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNit::Equals(record))
          {
            return false;
          }

          const CRecordNitTransponderData* recordTransponderData = dynamic_cast<const CRecordNitTransponderData*>(record);
          if (
            recordTransponderData == NULL ||
            SatelliteId != recordTransponderData->SatelliteId ||
            TransponderNumber != recordTransponderData->TransponderNumber ||
            TransportType != recordTransponderData->TransportType ||
            Polarisation != recordTransponderData->Polarisation ||
            CdtReference != recordTransponderData->CdtReference ||
            MmtReference != recordTransponderData->MmtReference ||
            VctId != recordTransponderData->VctId ||
            RootTransponder != recordTransponderData->RootTransponder ||
            WideBandwidthVideo != recordTransponderData->WideBandwidthVideo ||
            WaveformStandard != recordTransponderData->WaveformStandard ||
            WideBandwidthAudio != recordTransponderData->WideBandwidthAudio ||
            CompandedAudio != recordTransponderData->CompandedAudio ||
            MatrixMode != recordTransponderData->MatrixMode ||
            Subcarrier2Offset != recordTransponderData->Subcarrier2Offset ||
            Subcarrier1Offset != recordTransponderData->Subcarrier1Offset ||
            CarrierFrequencyOverride != recordTransponderData->CarrierFrequencyOverride
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (TransmissionMedium << 16) | (SatelliteId << 8) | TransponderNumber;
        }

        virtual unsigned long long GetExpiryKey() const
        {
          return (TransmissionMedium << 8) | SatelliteId;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NIT ATSC: transponder data %s, index = %hhu, transmission medium = %hhu, satellite ID = %hhu, transponder number = %hhu, transport type = %hhu, polarisation = %hhu, CDT reference = %hhu, MMT reference = %hhu, VCT ID = %hu, root transponder = %d, wide bandwidth video = %d, waveform standard = %hhu, wide bandwidth audio = %d, companded audio = %d, matrix mode = %hhu, sub-carrier 2 offset = %hu kHz, sub-carrier 1 offset = %hu kHz, carrier frequency override = %lu kHz",
                    situation, Index, TransmissionMedium, SatelliteId,
                    TransponderNumber, TransportType, Polarisation,
                    CdtReference, MmtReference, VctId, RootTransponder,
                    WideBandwidthVideo, WaveformStandard, WideBandwidthAudio,
                    CompandedAudio, MatrixMode, Subcarrier2Offset,
                    Subcarrier1Offset, CarrierFrequencyOverride);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNitAtsc* callBackNit = static_cast<ICallBackNitAtsc*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitTdtReceived(Index,
                                          TransmissionMedium,
                                          SatelliteId,
                                          TransportType,
                                          Polarisation,
                                          TransponderNumber,
                                          CdtReference,
                                          MmtReference,
                                          VctId,
                                          RootTransponder,
                                          WideBandwidthVideo,
                                          WaveformStandard,
                                          WideBandwidthAudio,
                                          CompandedAudio,
                                          MatrixMode,
                                          Subcarrier2Offset,
                                          Subcarrier1Offset,
                                          CarrierFrequencyOverride);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNitAtsc* callBackNit = static_cast<ICallBackNitAtsc*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitTdtChanged(Index,
                                          TransmissionMedium,
                                          SatelliteId,
                                          TransportType,
                                          Polarisation,
                                          TransponderNumber,
                                          CdtReference,
                                          MmtReference,
                                          VctId,
                                          RootTransponder,
                                          WideBandwidthVideo,
                                          WaveformStandard,
                                          WideBandwidthAudio,
                                          CompandedAudio,
                                          MatrixMode,
                                          Subcarrier2Offset,
                                          Subcarrier1Offset,
                                          CarrierFrequencyOverride);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNitAtsc* callBackNit = static_cast<ICallBackNitAtsc*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitTdtRemoved(Index,
                                          TransmissionMedium,
                                          SatelliteId,
                                          TransportType,
                                          Polarisation,
                                          TransponderNumber,
                                          CdtReference,
                                          MmtReference,
                                          VctId,
                                          RootTransponder,
                                          WideBandwidthVideo,
                                          WaveformStandard,
                                          WideBandwidthAudio,
                                          CompandedAudio,
                                          MatrixMode,
                                          Subcarrier2Offset,
                                          Subcarrier1Offset,
                                          CarrierFrequencyOverride);
          }
        }

        unsigned char SatelliteId;
        unsigned char TransportType;
        unsigned char Polarisation;
        unsigned char TransponderNumber;
        unsigned char CdtReference;
        unsigned char MmtReference;
        unsigned short VctId;
        bool RootTransponder;
        bool WideBandwidthVideo;
        unsigned char WaveformStandard;
        bool WideBandwidthAudio;
        bool CompandedAudio;
        unsigned char MatrixMode;
        unsigned short Subcarrier2Offset;   // unit = kHz
        unsigned short Subcarrier1Offset;   // unit = kHz

        unsigned long CarrierFrequencyOverride;
    };

    bool IsReadyPrivate(unsigned char transmissionMedium) const;
    static void CleanUpRecords(vector<CRecordNit*>& records);

    static bool DecodeSection(CSection& section,
                              unsigned char& protocolVersion,
                              unsigned char& transmissionMedium,
                              unsigned char& tableSubtype,
                              unsigned char& satelliteId,
                              vector<CRecordNit*>& records,
                              bool& seenRevisionDescriptor,
                              unsigned char& tableVersionNumber,
                              unsigned char& sectionNumber,
                              unsigned char& lastSectionNumber);

    static bool DecodeCarrierDefinitionSubTable(unsigned char* data,
                                                unsigned short& pointer,
                                                unsigned short endOfSection,
                                                unsigned char& numberOfCarriers,
                                                unsigned long& frequencySpacing,
                                                unsigned long& firstCarrierFrequency);
    static bool DecodeModulationModeSubTable(unsigned char* data,
                                              unsigned short& pointer,
                                              unsigned short endOfSection,
                                              CRecordNitModulationMode& record);
    static bool DecodeSatelliteInformationSubTable(unsigned char* data,
                                                    unsigned short& pointer,
                                                    unsigned short endOfSection,
                                                    CRecordNitSatelliteInformation& record);
    static bool DecodeTransponderDataSubTable(unsigned char* data,
                                              unsigned short& pointer,
                                              unsigned short endOfSection,
                                              CRecordNitTransponderData& record);

    static bool DecodeFrequencySpecDescriptor(unsigned char* data,
                                              unsigned char dataLength,
                                              unsigned long& carrierFrequency);
    static bool DecodeRevisionDetectionDescriptor(unsigned char* data,
                                                  unsigned char dataLength,
                                                  unsigned char& tableVersionNumber,
                                                  unsigned char& sectionNumber,
                                                  unsigned char& lastSectionNumber);

    CCriticalSection m_section;
    vector<unsigned long> m_seenSections;
    vector<unsigned long> m_unseenSections;
    bool m_isReady;
    ICallBackNitAtsc* m_callBack;
    CRecordStore m_recordsCarrierDefinition;
    CRecordStore m_recordsModulationMode;
    CRecordStore m_recordsSatelliteInformation;
    CRecordStore m_recordsTransponderData;
};