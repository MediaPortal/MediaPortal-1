/* 
 *  Copyright (C) 2005-2013 Team MediaPortal
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
#include <DShow.h>      // REFERENCE_TIME
#include <streams.h>    // AMovieDllRegisterServer2(), CCritSec, CFactoryTemplate, CUnknown (IUnknown, LPUNKNOWN)
#include <WinError.h>   // HRESULT
#include <map>
#include "..\..\shared\DebugSettings.h"
#include "..\..\shared\PacketSync.h"
#include "..\..\shared\PidTable.h"
#include "CniRegister.h"
#include "ICallBackRds.h"
#include "IMuxInputPin.h"
#include "IStreamMultiplexer.h"
#include "ITsMuxer.h"
#include "ParserRds.h"
#include "TsMuxerFilter.h"

using namespace std;


// This has to be large enough to contain the longest string in the CNI
// register (currently 60), the longest possible string from a VBI line (29
// characters), and the longest possible RDS programme service name (8 UTF-16
// characters). Be generous, because there are UTF-8 characters in some of
// these strings (...which means that some characters require more than 1
// byte).
#define SERVICE_NAME_LENGTH 100


DEFINE_TVE_DEBUG_SETTING(TsMuxerDumpInput)
DEFINE_TVE_DEBUG_SETTING(TsMuxerDumpOutput)


class CTsMuxer :
  public CUnknown, ICallBackRds, public IStreamMultiplexer, public ITsMuxer
{
  public:
    CTsMuxer(LPUNKNOWN unk, HRESULT* hr);
    virtual ~CTsMuxer();

    static CUnknown* WINAPI CreateInstance(LPUNKNOWN unk, HRESULT* hr);

    DECLARE_IUNKNOWN

    HRESULT BreakConnect(IMuxInputPin* pin);
    HRESULT CompleteConnect(IMuxInputPin* pin);
    bool IsStarted() const;
    HRESULT Receive(IMuxInputPin* pin,
                    const unsigned char* data,
                    long dataLength,
                    REFERENCE_TIME dataStartTime);
    HRESULT Reset();
    HRESULT StreamTypeChange(IMuxInputPin* pin,
                              unsigned char oldStreamType,
                              unsigned char newStreamType);

    STDMETHODIMP ConfigureLogging(wchar_t* path);
    STDMETHODIMP_(void) DumpInput(long mask);
    STDMETHODIMP_(void) DumpOutput(bool enable);
    STDMETHODIMP SetActiveComponents(bool video,
                                      bool audio,
                                      bool rds,
                                      bool teletext,
                                      bool vps,
                                      bool wss);

  private:
    typedef struct StreamInfo
    {
      unsigned char PinId;
      unsigned char OriginalStreamId;
      unsigned short OriginalPid;

      bool IsIgnored;
      bool IsCompatible;
      clock_t PrevReceiveTime;

      unsigned short Pid;
      unsigned char StreamId;
      unsigned char StreamType;
      unsigned char ContinuityCounter;

      unsigned char* PmtDescriptorBytes;
      unsigned short PmtDescriptorLength;
    } StreamInfo;

    typedef struct ProgramStreamInfo
    {
      unsigned char PinId;
      bool IsCompatible;
      unsigned char VideoBound;
      unsigned char AudioBound;
      unsigned char CurrentMapVersion;
    } ProgramStreamInfo;

    typedef struct TransportStreamInfo
    {
      unsigned char PinId;
      bool IsCompatible;
      unsigned short TransportStreamId;
      unsigned short ServiceId;
      unsigned short PmtPid;
      unsigned short PcrPid;
      unsigned char StreamCount;
      unsigned char PatVersion;
      unsigned char PmtVersion;
    } TransportStreamInfo;

    STDMETHODIMP NonDelegatingQueryInterface(REFIID iid, void** ppv);
    bool CanDeliver();
    HRESULT ReceiveTransportStream(IMuxInputPin* pin,
                                    const unsigned char* data,
                                    long dataLength,
                                    REFERENCE_TIME dataStartTime);
    HRESULT ReceiveProgramOrSystemStream(IMuxInputPin* pin,
                                          const unsigned char* data,
                                          long dataLength,
                                          REFERENCE_TIME dataStartTime);

    void OnRdsProgrammeServiceNameReceived(const char* programmeServiceName);

    static HRESULT ReadProgramAssociationTable(const unsigned char* data,
                                                long dataLength,
                                                TransportStreamInfo& info);
    HRESULT ReadProgramMapTable(const unsigned char* data,
                                long dataLength,
                                TransportStreamInfo& info);
    HRESULT CreateOrUpdateTsPmtEs(const TransportStreamInfo& info,
                                  const BasePid* pid,
                                  bool isIgnored);
    static HRESULT ReadProgramOrSystemPack(const unsigned char* data,
                                            long dataLength,
                                            const ProgramStreamInfo& info,
                                            bool isFirstReceive,
                                            unsigned short& length,
                                            long long& systemClockReference);
    static HRESULT ReadProgramOrSystemHeader(const unsigned char* data,
                                              long dataLength,
                                              ProgramStreamInfo& info,
                                              bool isFirstReceive);
    static HRESULT ReadProgramStreamMap(const unsigned char* data,
                                        long dataLength,
                                        ProgramStreamInfo& info);
    static HRESULT ReadVideoStreamInfo(const unsigned char* data,
                                        long dataLength,
                                        StreamInfo& info);
    static HRESULT ReadAudioStreamInfo(const unsigned char* data,
                                        long dataLength,
                                        StreamInfo& info);

    void UpdatePat();
    HRESULT UpdatePmt();
    void ResetSdtInfo();
    HRESULT UpdateSdt();

    HRESULT WrapVbiData(const StreamInfo& info,
                        const unsigned char* inputData,
                        long inputDataLength,
                        long long systemClockReference,
                        unsigned char** outputData,
                        long& outputDataLength);
    HRESULT ReadChannelNameFromVbiTeletextData(const unsigned char* inputData,
                                                long inputDataLength);
    HRESULT ReadChannelNameFromVbiVpsData(const unsigned char* inputData, long inputDataLength);

    static HRESULT WrapElementaryStreamData(const StreamInfo& info,
                                            const unsigned char* inputData,
                                            long inputDataLength,
                                            long long systemClockReference,
                                            unsigned char** outputData,
                                            long& outputDataLength);
    static HRESULT WrapPacketisedElementaryStreamData(StreamInfo& info,
                                                      const unsigned char* inputData,
                                                      long inputDataLength,
                                                      long long systemClockReference,
                                                      unsigned short pcrPid,
                                                      unsigned char** outputData,
                                                      long& outputDataLength);
    HRESULT DeliverTransportStreamData(const unsigned char* inputData, long inputDataLength);

    CTsMuxerFilter* m_filter;
    CCritSec m_filterLock;                  // filter control lock
    CCritSec m_receiveLock;                 // sample receive lock

    bool m_isStarted;
    bool m_isVideoActive;
    bool m_isAudioActive;
    bool m_isRdsActive;
    bool m_isTeletextActive;
    bool m_isVpsActive;
    bool m_isWssActive;

    unsigned char m_patPacket[TS_PACKET_LEN];
    unsigned char m_patContinuityCounter;

    unsigned char m_pmtPacket[TS_PACKET_LEN];
    unsigned char m_pmtContinuityCounter;
    unsigned short m_pmtPid;
    unsigned char m_pmtVersion;

    unsigned char m_sdtPacket[TS_PACKET_LEN];
    unsigned char m_sdtContinuityCounter;
    unsigned char m_sdtVersion;
    static CCniRegister m_cniRegister;
    bool m_isCniName;
    CParserRds* m_parserRds;
    char m_serviceName[SERVICE_NAME_LENGTH + 1];
    unsigned char m_serviceType;
    clock_t m_sdtResetTime;

    unsigned short m_packetCounter;
    unsigned short m_pcrPid;
    unsigned short m_nextStreamPid;
    unsigned char m_nextVideoStreamId;
    unsigned char m_nextAudioStreamId;

    map<unsigned long, StreamInfo*> m_streamInfo;                     // key = (original PID << 16) | (original stream ID << 8) | pin ID
    map<unsigned char, ProgramStreamInfo*> m_programStreamInfo;       // key = pin ID
    map<unsigned char, TransportStreamInfo*> m_transportStreamInfo;   // key = pin ID
};