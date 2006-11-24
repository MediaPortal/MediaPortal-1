/* 
 *	Copyright (C) 2006 Team MediaPortal
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
#include "multifilewriter.h"
#include "criticalsection.h"
#include "entercriticalsection.h"
#include "tsheader.h"
#include <vector>
#include <map>
using namespace std;
using namespace Mediaportal;

enum TimeShiftingMode
{
    ProgramStream=0,
    TransportStream=1
};
enum PidType
{
  Video=0,
  Audio=1,
  Other=2
};

// {89459BF6-D00E-4d28-928E-9DA8F76B6D3A}
DEFINE_GUID(IID_ITsTimeshifting,0x89459bf6, 0xd00e, 0x4d28, 0x92, 0x8e, 0x9d, 0xa8, 0xf7, 0x6b, 0x6d, 0x3a);

// video anayzer interface
DECLARE_INTERFACE_(ITsTimeshifting, IUnknown)
{
	STDMETHOD(SetPcrPid)(THIS_ int pcrPid)PURE;
	STDMETHOD(AddStream)(THIS_ int pid, int serviceType, char* language)PURE;
	STDMETHOD(RemoveStream)(THIS_ int pid)PURE;
	
  STDMETHOD(SetTimeShiftingFileName)(THIS_ char* pszFileName)PURE;
  STDMETHOD(Start)(THIS_ )PURE;
  STDMETHOD(Stop)(THIS_ )PURE;
  STDMETHOD(Reset)(THIS_ )PURE;
	STDMETHOD(GetBufferSize) (THIS_ long * size) PURE;
//	STDMETHOD(GetCurrentTSFile) (THIS_ FileWriter* fileWriter) PURE;
	STDMETHOD(GetNumbFilesAdded) (THIS_ WORD *numbAdd) PURE;
	STDMETHOD(GetNumbFilesRemoved) (THIS_ WORD *numbRem) PURE;
	STDMETHOD(GetCurrentFileId) (THIS_ WORD *fileID) PURE;
	STDMETHOD(GetMinTSFiles) (THIS_ WORD *minFiles) PURE;
	STDMETHOD(SetMinTSFiles) (THIS_ WORD minFiles) PURE;
	STDMETHOD(GetMaxTSFiles) (THIS_ WORD *maxFiles) PURE;
	STDMETHOD(SetMaxTSFiles) (THIS_ WORD maxFiles) PURE;
	STDMETHOD(GetMaxTSFileSize) (THIS_ __int64 *maxSize) PURE;
	STDMETHOD(SetMaxTSFileSize) (THIS_ __int64 maxSize) PURE;
	STDMETHOD(GetChunkReserve) (THIS_ __int64 *chunkSize) PURE;
	STDMETHOD(SetChunkReserve) (THIS_ __int64 chunkSize) PURE;
	STDMETHOD(GetFileBufferSize) (THIS_ __int64 *lpllsize) PURE;
	STDMETHOD(SetMode) (THIS_ int mode) PURE;
	STDMETHOD(GetMode) (THIS_ int *mode) PURE;
	STDMETHOD(SetPmtPid) (THIS_ int pmtPid) PURE;
	STDMETHOD(Pause) (THIS_ BYTE onOff) PURE;
};

class CTimeShifting: public CUnknown, public ITsTimeshifting, public IFileWriter
{
public:
	struct PidInfo
	{
		int  realPid;
		int  fakePid;
		int  serviceType;
		bool seenStart;
		char language[255];
	};
	CTimeShifting(LPUNKNOWN pUnk, HRESULT *phr);
	~CTimeShifting(void);
  DECLARE_IUNKNOWN
	
	STDMETHODIMP SetPcrPid(int pcrPid);
	STDMETHODIMP AddStream(int pid, int serviceType, char* language);
	STDMETHODIMP RemoveStream(int pid);
	STDMETHODIMP SetTimeShiftingFileName(char* pszFileName);
	STDMETHODIMP Start();
	STDMETHODIMP Stop();
	STDMETHODIMP Reset();

	STDMETHODIMP GetBufferSize( long * size) ;
//	STDMETHODIMP GetCurrentTSFile( FileWriter* fileWriter) ;
	STDMETHODIMP GetNumbFilesAdded( WORD *numbAdd) ;
	STDMETHODIMP GetNumbFilesRemoved( WORD *numbRem) ;
	STDMETHODIMP GetCurrentFileId( WORD *fileID) ;
	STDMETHODIMP GetMinTSFiles( WORD *minFiles) ;
	STDMETHODIMP SetMinTSFiles( WORD minFiles) ;
	STDMETHODIMP GetMaxTSFiles( WORD *maxFiles) ;
	STDMETHODIMP SetMaxTSFiles( WORD maxFiles) ;
	STDMETHODIMP GetMaxTSFileSize( __int64 *maxSize) ;
	STDMETHODIMP SetMaxTSFileSize( __int64 maxSize) ;
	STDMETHODIMP GetChunkReserve( __int64 *chunkSize) ;
	STDMETHODIMP SetChunkReserve( __int64 chunkSize) ;
	STDMETHODIMP GetFileBufferSize( __int64 *lpllsize) ;
	STDMETHODIMP SetMode(int mode) ;
	STDMETHODIMP GetMode(int *mode) ;
	STDMETHODIMP SetPmtPid(int pmtPid);
	STDMETHODIMP Pause( BYTE onOff) ;
	void OnTsPacket(byte* tsPacket);
	void Write(byte* buffer, int len);

private:
	void WriteTs(byte* tsPacket);
  void WriteFakePAT();  
  void WriteFakePMT();
	void PatchPcr(byte* tsPacket);
	void PatchPtsDts(byte* tsPacket);

  bool GetPtsDts(byte* pesHeader, UINT64& pts, UINT64& dts);
  void PatchPcr(byte* tsPacket,CTsHeader& header);
  void PatchPtsDts(byte* tsPacket,CTsHeader& header,UINT64 startPcr);

	MultiFileWriterParam m_params;
  TimeShiftingMode     m_timeShiftMode;
	CMultiplexer         m_multiPlexer;
	bool				         m_bTimeShifting;
	char				         m_szFileName[2048];
	MultiFileWriter*     m_pTimeShiftFile;
	CCriticalSection     m_section;
  int                  m_pmtPid;
  int                  m_pcrPid;
	vector<PidInfo>			 m_vecPids;
	typedef vector<PidInfo>::iterator itvecPids;
	bool								 m_bSeenAudioStart;
	bool								 m_bSeenVideoStart;
	int									 m_iPmtContinuityCounter;
	int									 m_iPatContinuityCounter;
  
  BOOL   m_bPaused;
	UINT64 m_startPcr;
	UINT64 m_highestPcr;
  bool    m_bDetermineNewStartPcr;
	bool		m_bStartPcrFound;
  int     m_iPacketCounter;
	int			m_iPatVersion;
	int			m_iPmtVersion;
  CTsHeader m_tsHeader;
};
