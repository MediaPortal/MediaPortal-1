/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
#include "multiplexer.h"
#include "filewriter.h"
#include "criticalsection.h"
#include "entercriticalsection.h"
#include "timeshifting.h"
#include "..\..\shared\TsHeader.h"
#include "PmtParser.h"
#include <vector>
using namespace std;
using namespace Mediaportal;

// {B45662E3-2749-4a34-993A-0C1659E86E83}
DEFINE_GUID(IID_ITsRecorder,0xb45662e3, 0x2749, 0x4a34, 0x99, 0x3a, 0xc, 0x16, 0x59, 0xe8, 0x6e, 0x83);

// video anayzer interface
DECLARE_INTERFACE_(ITsRecorder, IUnknown)
{
  STDMETHOD(SetRecordingFileName)(THIS_ char* pszFileName)PURE;
  STDMETHOD(StartRecord)(THIS_ )PURE;
  STDMETHOD(StopRecord)(THIS_ )PURE;
	STDMETHOD(GetMode) (THIS_ int *mode) PURE;
	STDMETHOD(SetMode) (THIS_ int mode) PURE;
	STDMETHOD(SetPmtPid)(THIS_ int mtPid, int serviceId)PURE;
};

class CRecorder: public CUnknown, public ITsRecorder, public IFileWriter, IPmtCallBack2
{
public:
	CRecorder(LPUNKNOWN pUnk, HRESULT *phr);
	~CRecorder(void);
  DECLARE_IUNKNOWN
	
	STDMETHODIMP SetPmtPid(int pmtPid, int serviceId);
	STDMETHODIMP SetRecordingFileName(char* pszFileName);
	STDMETHODIMP StartRecord();
	STDMETHODIMP StopRecord();
	STDMETHODIMP GetMode(int *mode) ;
	STDMETHODIMP SetMode(int mode) ;

	void OnTsPacket(byte* tsPacket);

	void OnPmtReceived2(int pid,int serviceId,int pcrPid,vector<PidInfo2> pidInfos);

private:
	void SetPcrPid(int pcrPid);
	bool IsStreamWanted(int streamType);
	void AddStream(PidInfo2 pidInfo2);
	void Write(byte* buffer, int len);
  void WriteTs(byte* tsPacket);
	void WriteFakePAT();  
  void WriteFakePMT();
	void PatchPcr(byte* tsPacket,CTsHeader& header);
  void PatchPtsDts(byte* tsPacket,CTsHeader& header,CPcr& startPcr);
	CMultiplexer     m_multiPlexer;
	bool				     m_bRecording;
	char				     m_szFileName[2048];
  CTsHeader        m_tsHeader;
	HANDLE           m_hFile;
	CCriticalSection m_section;
  TimeShiftingMode m_timeShiftMode;
	vector<PidInfo2> m_vecPids;
	byte*            m_pWriteBuffer;
  int              m_iWriteBufferPos;
  int              m_iPmtPid;
  int              m_iPart;
  CPmtParser*      m_pPmtParser;
  int              m_pmtVersion;

  CPcr            m_prevPcr;
  CPcr            m_highestPcr;
  CPcr            m_startPcr;
  CPcr            m_pcrHole;
  CPcr            m_backwardsPcrHole;
  CPcr            m_pcrDuration;
  bool            m_bPCRRollover;
  
  CAdaptionField  m_adaptionField;
  bool            m_bStartPcrFound;
  bool            m_bDetermineNewStartPcr;

  int             m_pcrPid;
  int             m_iServiceId;
  unsigned long   m_TsPacketCount;
	unsigned long   m_FakeTsPacketCount;

	int							m_iPmtContinuityCounter;
	int							m_iPatContinuityCounter;
};
