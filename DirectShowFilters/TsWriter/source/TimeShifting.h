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
#include "multifilewriter.h"
#include "criticalsection.h"
#include "entercriticalsection.h"
#include "..\..\shared\TsHeader.h"
#include "..\..\shared\adaptionfield.h"
#include "..\..\shared\pcr.h"
#include "PcrRefClock.h"
#include "videoaudioobserver.h"
#include <vector>
#include <map>
using namespace std;
using namespace Mediaportal;


//* enum which specified the timeshifting mode 
enum TimeShiftingMode
{
    ProgramStream=0,
    TransportStream=1
};
//* enum which specified the pid type 
enum PidType
{
  Video=0,
  Audio=1,
  Other=2
};

typedef struct stLastPtsDtsRecord
{
	CPcr pts;
	CPcr dts;
} LastPtsDtsRecord;

// {89459BF6-D00E-4d28-928E-9DA8F76B6D3A}
DEFINE_GUID(IID_ITsTimeshifting,0x89459bf6, 0xd00e, 0x4d28, 0x92, 0x8e, 0x9d, 0xa8, 0xf7, 0x6b, 0x6d, 0x3a);

// video anayzer interface
DECLARE_INTERFACE_(ITsTimeshifting, IUnknown)
{
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
	STDMETHOD(SetPmtPid) (THIS_ int pmtPid,int serviceId) PURE;
	STDMETHOD(Pause) (THIS_ BYTE onOff) PURE;
  STDMETHOD(SetVideoAudioObserver)(IVideoAudioObserver* callback)PURE;
};

//** timeshifting class
class CTimeShifting: public CUnknown, public ITsTimeshifting, public IFileWriter, IPmtCallBack2
{
public:
	struct PidInfo
	{
		int  realPid;
		int  fakePid;
		int  serviceType;
		bool seenStart;
		char language[255];
		byte descriptor_data[255]; // if descriptor_valid then this contains the original descriptor from the PMT
		bool descriptor_valid;
		int  ContintuityCounter;

		PidInfo(){
			descriptor_valid = false;
			memset(descriptor_data, 0xFF, sizeof(descriptor_data));
		}
	};
	CTimeShifting(LPUNKNOWN pUnk, HRESULT *phr);
	~CTimeShifting(void);
  DECLARE_IUNKNOWN
	
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
	STDMETHODIMP SetPmtPid(int pmtPid,int serviceId);
	STDMETHODIMP Pause( BYTE onOff) ;
	STDMETHODIMP SetVideoAudioObserver (IVideoAudioObserver* callback);
	void OnTsPacket(byte* tsPacket);
	void Write(byte* buffer, int len);

private:  
	void OnPmtReceived2(int pid,int serviceId,int pcrPid,vector<PidInfo2> pidInfos);
	void SetPcrPid(int pcrPid);
	bool IsStreamWanted(int stream_type);
	void AddStream(PidInfo2 pidInfo);
  void Flush();
	void WriteTs(byte* tsPacket);
  void WriteFakePAT();  
  void WriteFakePMT();

  void PatchPcr(byte* tsPacket,CTsHeader& header);
  void PatchPtsDts(byte* tsPacket,CTsHeader& header,CPcr& startPcr);

	MultiFileWriterParam m_params;
  TimeShiftingMode     m_timeShiftMode;
	CPmtParser*					m_pPmtParser;
	bool				         m_bTimeShifting;
	char				         m_szFileName[2048];
	MultiFileWriter*     m_pTimeShiftFile;
	CCriticalSection     m_section;
  int                  m_iPmtPid;
  int                  m_pcrPid;
	int									 m_iServiceId;
	vector<PidInfo2>			 m_vecPids;
	typedef vector<PidInfo>::iterator itvecPids;
	bool								 m_bSeenAudioStart;
	bool								 m_bSeenVideoStart;
	int									 m_iPmtContinuityCounter;
	int									 m_iPatContinuityCounter;
  
  BOOL            m_bPaused;
	CPcr            m_startPcr;
	CPcr            m_highestPcr;
  bool            m_bDetermineNewStartPcr;
	bool		        m_bStartPcrFound;
  int             m_iPacketCounter;
	int			        m_iPatVersion;
	int			        m_iPmtVersion;
  byte*           m_pWriteBuffer;
  int             m_iWriteBufferPos;
  CTsHeader       m_tsHeader;
  CAdaptionField  m_adaptionField;
  CPcr            m_prevPcr;
  CPcr            m_pcrHole;
  CPcr            m_backwardsPcrHole;
  CPcr            m_pcrDuration;
  bool            m_bPCRRollover;
  bool            m_bIgnoreNextPcrJump;
  FILE*           m_fDump;

  vector<char*>   m_tsQueue;
  bool            m_bClearTsQueue;
  unsigned long   m_TsPacketCount;
  CPcrRefClock*	  rclock;
  map<unsigned short,LastPtsDtsRecord> m_mapLastPtsDts;
  typedef map<unsigned short,LastPtsDtsRecord>::iterator imapLastPtsDts;
	IVideoAudioObserver *m_pVideoAudioObserver;
};
