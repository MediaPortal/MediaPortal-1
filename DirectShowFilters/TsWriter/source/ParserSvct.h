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
#include <ctime>
#include <vector>
#include "..\..\shared\CriticalSection.h"
#include "..\..\shared\Section.h"
#include "ICallBackSvct.h"
#include "IRecord.h"
#include "RecordStore.h"

using namespace MediaPortal;
using namespace std;


#define TABLE_ID_SVCT 0xc4


extern void LogDebug(const wchar_t* fmt, ...);

class CParserSvct
{
  public:
    CParserSvct();
    virtual ~CParserSvct();

    void Reset();
    void SetCallBack(ICallBackSvct* callBack);
    void OnNewSection(const CSection& section);
    bool IsSeen() const;
    bool IsReady() const;

    unsigned short GetDefinedChannelCount() const;
    bool GetDefinedChannel(unsigned short index,
                            unsigned char& transmissionMedium,
                            unsigned short& vctId,
                            unsigned short& virtualChannelNumber) const;

    unsigned short GetVirtualChannelCount() const;
    bool GetVirtualChannel(unsigned short index,
                            unsigned char& transmissionMedium,
                            unsigned short& vctId,
                            bool& splice,
                            unsigned long& activationTime,
                            bool& hdtvChannel,
                            bool& preferredSource,
                            unsigned short& virtualChannelNumber,
                            bool& applicationVirtualChannel,
                            unsigned char& bitstreamSelect,
                            unsigned char& pathSelect,
                            unsigned char& toneSelect,
                            unsigned char& transportType,
                            unsigned char& channelType,
                            unsigned short& sourceId,
                            unsigned short& nvodChannelBase,
                            unsigned char& cdtReference,
                            unsigned short& programNumber,
                            unsigned char& mmtReference,
                            bool& scrambled,
                            unsigned char& videoStandard,
                            bool& wideBandwidthAudio,
                            bool& compandedAudio,
                            unsigned char& matrixMode,
                            unsigned short& subcarrier2Offset,
                            unsigned short& subcarrier1Offset,
                            unsigned char& satelliteId,
                            unsigned char& transponder,
                            bool& suppressVideo,
                            unsigned char& audioSelection,
                            unsigned long& carrierFrequencyOverride,
                            unsigned long& symbolRateOverride,
                            unsigned short& majorChannelNumber,
                            unsigned short& minorChannelNumber,
                            unsigned short& transportStreamId,
                            bool& outOfBand,
                            bool& hideGuide,
                            unsigned char& serviceType) const;

    unsigned short GetInverseChannelCount() const;
    bool GetInverseChannel(unsigned short index,
                            unsigned char& transmissionMedium,
                            unsigned short& vctId,
                            unsigned short& sourceId,
                            unsigned short& virtualChannelNumber) const;

  private:
    class CRecordSvct : public IRecord
    {
      public:
        CRecordSvct()
        {
          TransmissionMedium = 0;
          VctId = 0;
          VirtualChannelNumber = 0;
        }

        virtual ~CRecordSvct()
        {
        }

        virtual bool Equals(const IRecord* record) const
        {
          const CRecordSvct* recordSvct = dynamic_cast<const CRecordSvct*>(record);
          if (
            recordSvct == NULL ||
            TransmissionMedium != recordSvct->TransmissionMedium ||
            VctId != recordSvct->VctId ||
            VirtualChannelNumber != recordSvct->VirtualChannelNumber
          )
          {
            return false;
          }
          return true;
        }

        virtual unsigned long long GetExpiryKey() const
        {
          return (TransmissionMedium << 16) | VctId;
        }

        unsigned char TransmissionMedium;
        unsigned short VctId;
        unsigned short VirtualChannelNumber;
    };

    class CRecordSvctDefinedChannel : public CRecordSvct
    {
      public:
        CRecordSvctDefinedChannel()
        {
        }

        ~CRecordSvctDefinedChannel()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordSvct::Equals(record))
          {
            return false;
          }

          const CRecordSvctDefinedChannel* recordDefinedChannel = dynamic_cast<const CRecordSvctDefinedChannel*>(record);
          return recordDefinedChannel != NULL;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)TransmissionMedium << 32) | (VctId << 16) | VirtualChannelNumber;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"SVCT: defined channel %s, transmission medium = %hhu, VCT ID = %hu, virtual channel number = %hu",
                    situation, TransmissionMedium, VctId, VirtualChannelNumber);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackSvct* callBackSvct = static_cast<ICallBackSvct*>(callBack);
          if (callBackSvct != NULL)
          {
            callBackSvct->OnSvctDcReceived(TransmissionMedium, VctId, VirtualChannelNumber);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackSvct* callBackSvct = static_cast<ICallBackSvct*>(callBack);
          if (callBackSvct != NULL)
          {
            callBackSvct->OnSvctDcChanged(TransmissionMedium, VctId, VirtualChannelNumber);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackSvct* callBackSvct = static_cast<ICallBackSvct*>(callBack);
          if (callBackSvct != NULL)
          {
            callBackSvct->OnSvctDcRemoved(TransmissionMedium, VctId, VirtualChannelNumber);
          }
        }
    };

    class CRecordSvctVirtualChannel : public CRecordSvct
    {
      public:
        CRecordSvctVirtualChannel()
        {
          Splice = false;
          ActivationTime = 0;
          HdtvChannel = false;
          PreferredSource = false;
          ApplicationVirtualChannel = false;
          BitstreamSelect = 0;
          PathSelect = 0;
          ToneSelect = 0;
          TransportType = 0;
          ChannelType = 0;
          SourceId = 0;
          NvodChannelBase = 0;
          CdtReference = 0;
          ProgramNumber = 0;
          MmtReference = 0;

          // Important: the standard digital virtual channel records don't have
          // an encryption indicator (!!!). We assume all channels are
          // encrypted unless otherwise specified (eg. through a channel
          // properties descriptor).
          Scrambled = true;

          VideoStandard = 0;
          WideBandwidthAudio = false;
          CompandedAudio = false;
          MatrixMode = 0;
          Subcarrier2Offset = 0;
          Subcarrier1Offset = 0;
          SatelliteId = 0;
          Transponder = 0;
          SuppressVideo = false;
          AudioSelection = 0;
          CarrierFrequencyOverride = 0;
          SymbolRateOverride = 0;
          MajorChannelNumber = 0;
          MinorChannelNumber = 0;
          TransportStreamId = 0;
          OutOfBand = false;
          HideGuide = false;
          ServiceType = 0;
        }

        ~CRecordSvctVirtualChannel()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordSvct::Equals(record))
          {
            return false;
          }

          const CRecordSvctVirtualChannel* recordVirtualChannel = dynamic_cast<const CRecordSvctVirtualChannel*>(record);
          if (
            recordVirtualChannel == NULL ||
            Splice != recordVirtualChannel->Splice ||
            ActivationTime != recordVirtualChannel->ActivationTime ||
            HdtvChannel != recordVirtualChannel->HdtvChannel ||
            PreferredSource != recordVirtualChannel->PreferredSource ||
            ApplicationVirtualChannel != recordVirtualChannel->ApplicationVirtualChannel ||
            BitstreamSelect != recordVirtualChannel->BitstreamSelect ||
            PathSelect != recordVirtualChannel->PathSelect ||
            ToneSelect != recordVirtualChannel->ToneSelect ||
            TransportType != recordVirtualChannel->TransportType ||
            ChannelType != recordVirtualChannel->ChannelType ||
            SourceId != recordVirtualChannel->SourceId ||
            NvodChannelBase != recordVirtualChannel->NvodChannelBase ||
            CdtReference != recordVirtualChannel->CdtReference ||
            ProgramNumber != recordVirtualChannel->ProgramNumber ||
            MmtReference != recordVirtualChannel->MmtReference ||
            Scrambled != recordVirtualChannel->Scrambled ||
            VideoStandard != recordVirtualChannel->VideoStandard ||
            WideBandwidthAudio != recordVirtualChannel->WideBandwidthAudio ||
            CompandedAudio != recordVirtualChannel->CompandedAudio ||
            MatrixMode != recordVirtualChannel->MatrixMode ||
            Subcarrier2Offset != recordVirtualChannel->Subcarrier2Offset ||
            Subcarrier1Offset != recordVirtualChannel->Subcarrier1Offset ||
            SatelliteId != recordVirtualChannel->SatelliteId ||
            Transponder != recordVirtualChannel->Transponder ||
            SuppressVideo != recordVirtualChannel->SuppressVideo ||
            AudioSelection != recordVirtualChannel->AudioSelection ||
            CarrierFrequencyOverride != recordVirtualChannel->CarrierFrequencyOverride ||
            SymbolRateOverride != recordVirtualChannel->SymbolRateOverride ||
            MajorChannelNumber != recordVirtualChannel->MajorChannelNumber ||
            MinorChannelNumber != recordVirtualChannel->MinorChannelNumber ||
            TransportStreamId != recordVirtualChannel->TransportStreamId ||
            OutOfBand != recordVirtualChannel->OutOfBand ||
            HideGuide != recordVirtualChannel->HideGuide ||
            ServiceType != recordVirtualChannel->ServiceType
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          if (MajorChannelNumber == 0 && MinorChannelNumber == 0)
          {
            return ((unsigned long long)TransmissionMedium << 48) | ((unsigned long long)VctId << 32) | VirtualChannelNumber;
          }
          return ((unsigned long long)TransmissionMedium << 48) | ((unsigned long long)VctId << 32) | (MajorChannelNumber << 16) | MinorChannelNumber;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"SVCT: virtual channel %s, transmission medium = %hhu, VCT ID = %hu, splice = %d, activation time = %lu, HDTV channel = %d, preferred source = %d, virtual channel number = %hu, application virtual channel = %d, bitstream select = %hhu, path select = %hhu, tone select = %hhu, transport type = %hhu, channel type = %hhu, source ID = %hu, NVOD channel base = %hu, CDT reference = %hhu, program number = %hu, MMT reference = %hhu, scrambled = %d, video standard = %hhu, wide bandwidth audio = %d, companded audio = %d, matrix mode = %hhu, subcarrier 2 offset = %hu kHz, subcarrier 1 offset = %hu kHz, satellite ID = %hhu, transponder = %hhu, suppress video = %d, audio selection = %hhu, carrier frequency override = %lu kHz, symbol rate override = %lu s/s, major channel number = %hu, minor channel number = %hu, TSID = %hu, out of band = %d, hide guide = %d, service type = %hhu",
                    situation, TransmissionMedium, VctId, Splice, ActivationTime,
                    HdtvChannel, PreferredSource, VirtualChannelNumber,
                    ApplicationVirtualChannel, BitstreamSelect, PathSelect,
                    ToneSelect, TransportType, ChannelType, SourceId,
                    NvodChannelBase, CdtReference, ProgramNumber, MmtReference,
                    Scrambled, VideoStandard, WideBandwidthAudio,
                    CompandedAudio, MatrixMode, Subcarrier2Offset,
                    Subcarrier1Offset, SatelliteId, Transponder, SuppressVideo,
                    AudioSelection, CarrierFrequencyOverride,
                    SymbolRateOverride, MajorChannelNumber, MinorChannelNumber,
                    TransportStreamId, OutOfBand, HideGuide, ServiceType);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackSvct* callBackSvct = static_cast<ICallBackSvct*>(callBack);
          if (callBackSvct != NULL)
          {
            callBackSvct->OnSvctVcReceived(TransmissionMedium,
                                            VctId,
                                            Splice,
                                            ActivationTime,
                                            HdtvChannel,
                                            PreferredSource,
                                            VirtualChannelNumber,
                                            ApplicationVirtualChannel,
                                            BitstreamSelect,
                                            PathSelect,
                                            ToneSelect,
                                            TransportType,
                                            ChannelType,
                                            SourceId,
                                            NvodChannelBase,
                                            CdtReference,
                                            ProgramNumber,
                                            MmtReference,
                                            Scrambled,
                                            VideoStandard,
                                            WideBandwidthAudio,
                                            CompandedAudio,
                                            MatrixMode,
                                            Subcarrier2Offset,
                                            Subcarrier1Offset,
                                            SatelliteId,
                                            Transponder,
                                            SuppressVideo,
                                            AudioSelection,
                                            CarrierFrequencyOverride,
                                            SymbolRateOverride,
                                            MajorChannelNumber,
                                            MinorChannelNumber,
                                            TransportStreamId,
                                            OutOfBand,
                                            HideGuide,
                                            ServiceType);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackSvct* callBackSvct = static_cast<ICallBackSvct*>(callBack);
          if (callBackSvct != NULL)
          {
            callBackSvct->OnSvctVcChanged(TransmissionMedium,
                                            VctId,
                                            Splice,
                                            ActivationTime,
                                            HdtvChannel,
                                            PreferredSource,
                                            VirtualChannelNumber,
                                            ApplicationVirtualChannel,
                                            BitstreamSelect,
                                            PathSelect,
                                            ToneSelect,
                                            TransportType,
                                            ChannelType,
                                            SourceId,
                                            NvodChannelBase,
                                            CdtReference,
                                            ProgramNumber,
                                            MmtReference,
                                            Scrambled,
                                            VideoStandard,
                                            WideBandwidthAudio,
                                            CompandedAudio,
                                            MatrixMode,
                                            Subcarrier2Offset,
                                            Subcarrier1Offset,
                                            SatelliteId,
                                            Transponder,
                                            SuppressVideo,
                                            AudioSelection,
                                            CarrierFrequencyOverride,
                                            SymbolRateOverride,
                                            MajorChannelNumber,
                                            MinorChannelNumber,
                                            TransportStreamId,
                                            OutOfBand,
                                            HideGuide,
                                            ServiceType);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackSvct* callBackSvct = static_cast<ICallBackSvct*>(callBack);
          if (callBackSvct != NULL)
          {
            callBackSvct->OnSvctVcRemoved(TransmissionMedium,
                                            VctId,
                                            Splice,
                                            ActivationTime,
                                            HdtvChannel,
                                            PreferredSource,
                                            VirtualChannelNumber,
                                            ApplicationVirtualChannel,
                                            BitstreamSelect,
                                            PathSelect,
                                            ToneSelect,
                                            TransportType,
                                            ChannelType,
                                            SourceId,
                                            NvodChannelBase,
                                            CdtReference,
                                            ProgramNumber,
                                            MmtReference,
                                            Scrambled,
                                            VideoStandard,
                                            WideBandwidthAudio,
                                            CompandedAudio,
                                            MatrixMode,
                                            Subcarrier2Offset,
                                            Subcarrier1Offset,
                                            SatelliteId,
                                            Transponder,
                                            SuppressVideo,
                                            AudioSelection,
                                            CarrierFrequencyOverride,
                                            SymbolRateOverride,
                                            MajorChannelNumber,
                                            MinorChannelNumber,
                                            TransportStreamId,
                                            OutOfBand,
                                            HideGuide,
                                            ServiceType);
          }
        }

        bool Splice;
        unsigned long ActivationTime;
        bool HdtvChannel;
        bool PreferredSource;
        bool ApplicationVirtualChannel;
        unsigned char BitstreamSelect;
        unsigned char PathSelect;
        unsigned char ToneSelect;
        unsigned char TransportType;
        unsigned char ChannelType;
        unsigned short SourceId;
        unsigned short NvodChannelBase;
        unsigned char CdtReference;
        unsigned short ProgramNumber;
        unsigned char MmtReference;
        bool Scrambled;
        unsigned char VideoStandard;
        bool WideBandwidthAudio;
        bool CompandedAudio;
        unsigned char MatrixMode;
        unsigned short Subcarrier2Offset;       // unit = kHz
        unsigned short Subcarrier1Offset;       // unit = kHz
        unsigned char SatelliteId;
        unsigned char Transponder;
        bool SuppressVideo;
        unsigned char AudioSelection;
        unsigned long CarrierFrequencyOverride; // unit = kHz
        unsigned long SymbolRateOverride;
        unsigned short MajorChannelNumber;
        unsigned short MinorChannelNumber;
        unsigned short TransportStreamId;
        bool OutOfBand;
        bool HideGuide;
        unsigned char ServiceType;
    };

    class CRecordSvctInverseChannel : public CRecordSvct
    {
      public:
        CRecordSvctInverseChannel()
        {
          SourceId = 0;
        }

        ~CRecordSvctInverseChannel()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordSvct::Equals(record))
          {
            return false;
          }

          const CRecordSvctInverseChannel* recordInverseChannel = dynamic_cast<const CRecordSvctInverseChannel*>(record);
          if (
            recordInverseChannel == NULL ||
            SourceId != recordInverseChannel->SourceId
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)TransmissionMedium << 32) | (VctId << 16) | SourceId;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"SVCT: inverse channel %s, transmission medium = %hhu, VCT ID = %hu, source ID = %hu, virtual channel number = %hu",
                    situation, TransmissionMedium, VctId, SourceId,
                    VirtualChannelNumber);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackSvct* callBackSvct = static_cast<ICallBackSvct*>(callBack);
          if (callBackSvct != NULL)
          {
            callBackSvct->OnSvctIcReceived(TransmissionMedium,
                                            VctId,
                                            SourceId,
                                            VirtualChannelNumber);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackSvct* callBackSvct = static_cast<ICallBackSvct*>(callBack);
          if (callBackSvct != NULL)
          {
            callBackSvct->OnSvctIcChanged(TransmissionMedium,
                                            VctId,
                                            SourceId,
                                            VirtualChannelNumber);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackSvct* callBackSvct = static_cast<ICallBackSvct*>(callBack);
          if (callBackSvct != NULL)
          {
            callBackSvct->OnSvctIcRemoved(TransmissionMedium,
                                            VctId,
                                            SourceId,
                                            VirtualChannelNumber);
          }
        }

        unsigned short SourceId;
    };

    bool IsReadyPrivate() const;
    template<class T> static void CleanUpRecords(vector<T*>& records);

    static bool DecodeSection(const CSection& section,
                              unsigned char& protocolVersion,
                              unsigned char& transmissionMedium,
                              unsigned char& tableSubtype,
                              unsigned short& vctId,
                              vector<CRecordSvct*>& records,
                              bool& seenRevisionDescriptor,
                              unsigned char& tableVersionNumber,
                              unsigned char& sectionNumber,
                              unsigned char& lastSectionNumber);

    static bool DecodeDefinedChannelMap(const unsigned char* data,
                                        unsigned short& pointer,
                                        unsigned short endOfSection,
                                        vector<CRecordSvctDefinedChannel*>& records);
    static bool DecodeVirtualChannelMap(const unsigned char* data,
                                        unsigned short& pointer,
                                        unsigned short endOfSection,
                                        unsigned char transmissionMedium,
                                        vector<CRecordSvctVirtualChannel*>& records);
    static bool DecodeInverseChannelMap(const unsigned char* data,
                                        unsigned short& pointer,
                                        unsigned short endOfSection,
                                        vector<CRecordSvctInverseChannel*>& records);

    static bool DecodeFrequencySpecDescriptor(const unsigned char* data,
                                              unsigned char dataLength,
                                              unsigned long& carrierFrequency);
    static bool DecodeRevisionDetectionDescriptor(const unsigned char* data,
                                                  unsigned char dataLength,
                                                  unsigned char& tableVersionNumber,
                                                  unsigned char& sectionNumber,
                                                  unsigned char& lastSectionNumber);
    static bool DecodeTwoPartChannelNumberDescriptor(const unsigned char* data,
                                                      unsigned char dataLength,
                                                      unsigned short& majorChannelNumber,
                                                      unsigned short& minorChannelNumber);
    static bool DecodeChannelPropertiesDescriptor(const unsigned char* data,
                                                  unsigned char dataLength,
                                                  unsigned short& channelTsid,
                                                  bool& outOfBand,
                                                  bool& accessControlled,
                                                  bool& hideGuide,
                                                  unsigned char& serviceType);

    CCriticalSection m_section;
    vector<unsigned long> m_seenSections;
    vector<unsigned long> m_unseenSections;
    bool m_isReady;
    ICallBackSvct* m_callBack;
    CRecordStore m_recordsDefinedChannel;
    CRecordStore m_recordsVirtualChannel;
    CRecordStore m_recordsInverseChannel;
    clock_t m_lastExpiredRecordCheckTime;
};