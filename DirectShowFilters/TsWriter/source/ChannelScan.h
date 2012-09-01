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
#include <map>
#include <vector>
#include "criticalsection.h"
#include "entercriticalsection.h"
#include "PatParser.h"
#include "PmtParser.h"
#include "SdtParser.h"
#include "NitParser.h"
#include "BatParser.h"
#include "VctParser.h"
#include "EncryptionAnalyser.h"

using namespace Mediaportal;

#pragma once

// Enum specifying possible broadcast standards or transport stream types.
enum BroadcastStandard
{
	BroadcastStandardNotSet = -1,
  Dvb = 0,
  Atsc = 1,
  Scte = 2,   // North American cable
  Isdb = 3
};

// {1663DC42-D169-41da-BCE2-EEEC482CB9FB}
DEFINE_GUID(IID_ITsChannelScan, 0x1663dc42, 0xd169, 0x41da, 0xbc, 0xe2, 0xee, 0xec, 0x48, 0x2c, 0xb9, 0xfb);

DECLARE_INTERFACE_(IChannelScanCallBack, IUnknown)
{
  STDMETHOD(OnScannerDone)()PURE;
};

DECLARE_INTERFACE_(ITsChannelScan, IUnknown)
{
  STDMETHOD(SetCallBack)(THIS_ IChannelScanCallBack* callBack)PURE;

  // SDT/VCT transponder-by-transponder scanning
  STDMETHOD(ScanStream)(THIS_ BroadcastStandard broadcastStandard)PURE;
  STDMETHOD(StopStreamScan)(THIS_)PURE;
  STDMETHOD(GetServiceCount)(THIS_ int* serviceCount)PURE;
  STDMETHOD(GetServiceDetail)(THIS_ int index,
                         long* networkId,
                         long* transportStreamId,
                         long* serviceId,
                         char** serviceName,
                         char** providerName,
                         char** networkNames,
                         char** logicalChannelNumber,
                         int* serviceType,
                         int* hasVideo,
                         int* hasAudio,
                         bool* isEncrypted,
                         int* pmtPid)PURE;

  // NIT scanning
  STDMETHOD(ScanNetwork)(THIS_)PURE;
  STDMETHOD(StopNetworkScan)(THIS_ bool* isOtherMuxServiceInfoAvailable)PURE;
  STDMETHOD(GetMultiplexCount)(THIS_ int* multiplexCount)PURE;
  STDMETHOD(GetMultiplexDetail)(THIS_ int index,
                            int* networkId,
                            int* transportStreamId,
                            int* type,
                            int* frequency,
                            int *polarisation,
                            int* modulation,
                            int* symbolRate,
                            int* bandwidth,
                            int* innerFecRate,
                            int* rollOff)PURE;
};

class CMpTsFilter;

class CChannelScan : public CUnknown, public ITsChannelScan, IPatCallBack, IPmtCallBack2, ISdtCallBack, IVctCallBack, public IEncryptionStateChangeCallBack
{
  public:
    CChannelScan(LPUNKNOWN pUnk, HRESULT *phr, CMpTsFilter* filter);
    ~CChannelScan(void);
  
    DECLARE_IUNKNOWN

    STDMETHODIMP SetCallBack(IChannelScanCallBack* callBack);
  
    STDMETHODIMP ScanStream(BroadcastStandard broadcastStandard);
    STDMETHODIMP StopStreamScan();
    STDMETHODIMP GetServiceCount(int* serviceCount);
    STDMETHODIMP GetServiceDetail(int index,
                             long* networkId,
                             long* transportStreamId,
                             long* serviceId,
                             char** serviceName,
                             char** providerName,
                             char** networkNames,
                             char** logicalChannelNumber,
                             int* serviceType,
                             int* hasVideo,
                             int* hasAudio,
                             bool* isEncrypted,
                             int* pmtPid);


    STDMETHODIMP ScanNetwork();
    STDMETHODIMP StopNetworkScan(bool* isOtherMuxServiceInfoAvailable);
    STDMETHODIMP GetMultiplexCount(int* multiplexCount);
    STDMETHODIMP GetMultiplexDetail(int index,
                                int* networkId,
                                int* transportStreamId,
                                int* type,
                                int* frequency,
                                int* polarisation,
                                int* modulation,
                                int* symbolRate,
                                int* bandwidth,
                                int* innerFecRate,
                                int* rollOff);

    void OnTsPacket(byte* tsPacket);
    void OnPatReceived(int serviceId, int pmtPid);
    void OnPmtReceived(const CPidTable& pidTable);
    void OnSdtReceived(const CChannelInfo& sdtInfo);
    void OnVctReceived(const CChannelInfo& vctInfo);
    STDMETHODIMP OnEncryptionStateChange(int pid, EncryptionState encryptionState);

  private:
    void CleanUp();

    CMpTsFilter* m_pFilter;
    CCriticalSection m_section;
    IChannelScanCallBack* m_pCallBack;
    bool m_bIsScanning;
    bool m_bIsScanningNetwork;
    BroadcastStandard m_broadcastStandard;
    bool m_bIsOtherMuxServiceInfoSeen;

    map<int, CChannelInfo*> m_mServices;
    vector<NitMultiplexDetail*> m_vMultiplexes;

    CPatParser m_patParser;
    vector<CPmtParser*> m_vPmtParsers;
    CEncryptionAnalyser* m_pEncryptionAnalyser;
    map<int, int> m_mPids;  // map directly linking a PID to a service - limitation a PID can only be linked to one service
    CSdtParser m_sdtParser;
    CNitParser m_nitParser;
    CBatParser m_batParser;
    CVctParser m_vctParser;
};
