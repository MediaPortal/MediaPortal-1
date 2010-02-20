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
#pragma warning(disable : 4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "AdaptionField.h"
#include "recorder.h"
#define ERROR_FILE_TOO_LARGE 223
#define RECORD_BUFFER_SIZE 256000
#define IGNORE_AFTER_TUNE 75        // how many TS packets to ignore after tuning

extern void LogDebug(const char *fmt, ...) ;

//FILE* fpOut=NULL;
CRecorder::CRecorder(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsRecorder"), pUnk)
{
	strcpy(m_szFileName,"");
  m_timeShiftMode=ProgramStream;
	m_bRecording=false;
  m_hFile=INVALID_HANDLE_VALUE;
  m_pWriteBuffer = new byte[RECORD_BUFFER_SIZE];
  m_iWriteBufferPos=0;
  m_iServiceId=-1;
  m_iPmtPid=-1;
  m_pmtVersion=-1;
	m_multiPlexer.SetFileWriterCallBack(this);
  m_TsPacketCount=0;
  m_pPmtParser=new CPmtParser();
  if (m_pPmtParser)
  {
    m_pPmtParser->SetPmtCallBack(this);
    m_pPmtParser->SetTableId(2);
  }
}
CRecorder::~CRecorder(void)
{
  if (m_hFile!=INVALID_HANDLE_VALUE)
  {
	  CloseHandle(m_hFile);
	  m_hFile = INVALID_HANDLE_VALUE; // Invalidate the file
  }
  delete [] m_pWriteBuffer;
  delete m_pPmtParser;
}

void CRecorder::OnTsPacket(byte* tsPacket)
{
	if (m_bRecording)
	{
	  m_tsHeader.Decode(tsPacket);
    if (m_tsHeader.SyncByte!=0x47) return;
	  if (m_tsHeader.TransportError) return;
	  CEnterCriticalSection enter(m_section);
    
    if (m_tsHeader.Pid==m_iPmtPid && m_pPmtParser )
    {
      m_pPmtParser->OnTsPacket(tsPacket);
    }
    
    if (m_timeShiftMode==ProgramStream)
    {
		  m_multiPlexer.OnTsPacket(tsPacket);
    }
    else
    {
      WriteTs(tsPacket);
    }
	}
}

STDMETHODIMP CRecorder::SetMode(int mode) 
{
  m_timeShiftMode=(TimeShiftingMode)mode;
  if (mode==ProgramStream)
			LogDebug("Recorder:program stream mode");
  else
      LogDebug("Recorder:transport stream mode");
	return S_OK;
}

STDMETHODIMP CRecorder::GetMode(int *mode) 
{
  *mode=(int)m_timeShiftMode;
	return S_OK;
}

STDMETHODIMP CRecorder::SetPcrPid(int pcrPid)
{
	CEnterCriticalSection enter(m_section);
	LogDebug("Recorder:pcr pid:%x",pcrPid);
  m_TsPacketCount=0;
	m_pcrPid = pcrPid;
  
	m_multiPlexer.SetPcrPid(pcrPid);
	return S_OK;
}

STDMETHODIMP CRecorder::SetPmtPid(int pmtPid, int serviceId )
{
	CEnterCriticalSection enter(m_section);
	m_iPmtPid=pmtPid;
  m_iServiceId=serviceId;
	LogDebug("Recorder:pmt pid:%x",m_iPmtPid);
  LogDebug("Recorder:serviceId:%x",serviceId);
  if (m_pPmtParser)
  {
    m_pPmtParser->SetPid(pmtPid);
  }
	return S_OK;
}

STDMETHODIMP CRecorder::AddStream(int pid,bool isAc3,bool isAudio,bool isVideo)
{
	CEnterCriticalSection enter(m_section);
  bool pidFound=false;
  itvecPids it = m_vecPids.begin();

  // Do not add duplicate PIDs in the vector, easier to do here than in the OnPidsReceived
  while (it!=m_vecPids.end())
  {
    if (pid==*it)
    {
      pidFound = true;
    }
    ++it;
  }

  if( !pidFound )
  {
	  if (isAudio)
		  LogDebug("Recorder:add audio stream pid:%x",pid);
	  else if (isVideo)
		  LogDebug("Recorder:add video stream pid:%x",pid);
	  else 
		  LogDebug("Recorder:add private stream pid:%x",pid);
    
    m_vecPids.push_back(pid);
	  m_multiPlexer.AddPesStream(pid,isAc3,isAudio,isVideo);
  }
  return S_OK;
}

STDMETHODIMP CRecorder::RemoveStream(int pid)
{
	CEnterCriticalSection enter(m_section);
	LogDebug("Recorder:remove pes stream pid:%x",pid);
	m_multiPlexer.RemovePesStream(pid);
  itvecPids it = m_vecPids.begin();
  while (it!=m_vecPids.end())
  {
    if (*it==pid)
    {
      it=m_vecPids.erase(it);
    }
    else
    {
      ++it;
    }
  }
	return S_OK;
}

STDMETHODIMP CRecorder::SetRecordingFileName(char* pszFileName)
{
	CEnterCriticalSection enter(m_section);
  m_vecPids.clear();
	m_multiPlexer.Reset();
	strcpy(m_szFileName,pszFileName);
	return S_OK;
}
STDMETHODIMP CRecorder::StartRecord()
{
	CEnterCriticalSection enter(m_section);
	if (strlen(m_szFileName)==0) return E_FAIL;
	::DeleteFile((LPCTSTR) m_szFileName);
  m_iPart=2;
	if (m_hFile!=INVALID_HANDLE_VALUE)
	{
    CloseHandle(m_hFile);
    m_hFile=INVALID_HANDLE_VALUE;
  }
	m_hFile = CreateFile(m_szFileName,      // The filename
						 (DWORD) GENERIC_WRITE,         // File access
						 (DWORD) FILE_SHARE_READ,       // Share access
						 NULL,                  // Security
						 (DWORD) OPEN_ALWAYS,           // Open flags
//						 (DWORD) FILE_FLAG_RANDOM_ACCESS,
//						 (DWORD) FILE_FLAG_WRITE_THROUGH,             // More flags
						 (DWORD) 0,             // More flags
						 NULL);                 // Template
	if (m_hFile == INVALID_HANDLE_VALUE)
	{
    LogDebug("Recorder:unable to create file:'%s' %d",m_szFileName, GetLastError());
		return E_FAIL;
	}
	LogDebug("Recorder:Start Recording:'%s'",m_szFileName);
  m_iWriteBufferPos=0;
	m_bRecording=true;
	//::DeleteFile("out.ts");
	//fpOut =fopen("out.ts","wb+");
	return S_OK;
}
STDMETHODIMP CRecorder::StopRecord()
{
	CEnterCriticalSection enter(m_section);
  if (m_bRecording)
	  LogDebug("Recorder:Stop Recording:'%s'",m_szFileName);
	m_bRecording=false;
	m_multiPlexer.Reset();
	if (m_hFile!=INVALID_HANDLE_VALUE)
	{
    if (m_iWriteBufferPos>0)
    {
	    DWORD written = 0;
	    WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL);
      m_iWriteBufferPos=0;
    }
		CloseHandle(m_hFile);
		m_hFile=INVALID_HANDLE_VALUE;
	}
  m_iPmtPid=-1;
	return S_OK;
}


void CRecorder::Write(byte* buffer, int len)
{
	if (!m_bRecording) return;
  if (buffer==NULL) return;
  if (len <=0) return;
	CEnterCriticalSection enter(m_section);
  if (len + m_iWriteBufferPos >= RECORD_BUFFER_SIZE)
  {
	  try
	  {
      if (m_iWriteBufferPos > 0)
      {
		    if (m_hFile != INVALID_HANDLE_VALUE)
		    {
	        DWORD written = 0;
	        if (FALSE == WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL))
          {
            //On fat16/fat32 we can only create files of max. 2gb/4gb
            if (ERROR_FILE_TOO_LARGE == GetLastError())
            {
              LogDebug("Recorder:Maximum filesize reached for file:'%s' %d",m_szFileName);
                //close the file...
		          CloseHandle(m_hFile);
              m_hFile=INVALID_HANDLE_VALUE;

              //create a new file
              char ext[MAX_PATH];
              char fileName[MAX_PATH];
              char part[100];
						  int len=strlen(m_szFileName)-1;
						  int pos=len-1;
						  while (pos>0)
						  {
							  if (m_szFileName[pos]=='.') break;
							  pos--;
						  }
              strcpy(ext, &m_szFileName[pos]);
              strncpy(fileName, m_szFileName, pos);
              fileName[pos]=0;
              sprintf(part,"_p%d",m_iPart);
              char newFileName[MAX_PATH];
              sprintf(newFileName,"%s%s%s",fileName,part,ext);

						  LogDebug("Recorder:Create new  file:'%s' %d",newFileName);
	            m_hFile = CreateFile(newFileName,      // The filename
						             (DWORD) GENERIC_WRITE,         // File access
						             (DWORD) FILE_SHARE_READ,       // Share access
						             NULL,                  // Security
						             (DWORD) OPEN_ALWAYS,           // Open flags
						             (DWORD) 0,             // More flags
						             NULL);                 // Template
	            if (m_hFile == INVALID_HANDLE_VALUE)
	            {
                LogDebug("Recorder:unable to create file:'%s' %d",newFileName, GetLastError());
                m_iWriteBufferPos=0;
		            return ;
	            }
              m_iPart++;
              WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL);
            }//of if (ERROR_FILE_TOO_LARGE == GetLastError())
						else
						{				 
							LogDebug("Recorder:unable to write file:'%s' %d %d %x",m_szFileName, GetLastError(),m_iWriteBufferPos,m_hFile);
						}
          }//of if (FALSE == WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL))
		    }//if (m_hFile!=INVALID_HANDLE_VALUE)
      }//if (m_iWriteBufferPos>0)
      m_iWriteBufferPos=0;
	  }
	  catch(...)
	  {
		  LogDebug("Timeshifter:Write exception");
	  }
  }// of if (len + m_iWriteBufferPos >= RECORD_BUFFER_SIZE)

  if ( (m_iWriteBufferPos+len) < RECORD_BUFFER_SIZE && len > 0)
  {
    memcpy(&m_pWriteBuffer[m_iWriteBufferPos],buffer,len);
    m_iWriteBufferPos+=len;
  }
}

void CRecorder::WriteTs(byte* tsPacket)
{
	if (!m_bRecording) return;
  m_TsPacketCount++;
  if( m_TsPacketCount < IGNORE_AFTER_TUNE ) return;

  bool writePid = false;

  int PayLoadUnitStart=0;
  if (m_tsHeader.PayloadUnitStart) PayLoadUnitStart=1;

  if (m_tsHeader.Pid==0 ||m_tsHeader.Pid==0x11 || m_tsHeader.Pid==m_iPmtPid || m_tsHeader.Pid==m_pcrPid)
  {
    //PAT/PMT/SDT/PCR
    writePid = true;
  }

  itvecPids it = m_vecPids.begin();
  while (it!=m_vecPids.end())
  {
    if (m_tsHeader.Pid==*it)
    {
      writePid = true;
    }
    ++it;
  }

  // Patch PCR & PTS/DTS to start from beginning & fill PCR "holes"
  if( writePid )
  {
    CAdaptionField field;
    field.Decode(m_tsHeader,tsPacket);

	  byte pkt[200];
	  memcpy(pkt,tsPacket,188);
	  int pid=m_tsHeader.Pid;
	  pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
	  pkt[2]=(pid&0xff);
	  if (m_tsHeader.Pid==m_pcrPid)  PatchPcr(pkt,m_tsHeader);
	  if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
	  {
	    if (PayLoadUnitStart) PatchPtsDts(pkt,m_tsHeader,m_startPcr);
		  //info.ContintuityCounter=m_tsHeader.ContinuityCounter;
      //m_iPacketCounter++;
	  }
    Write(pkt,188);
  }
}

void CRecorder::OnPmtReceived(int pmtPid)
{
  //LogDebug("Recorder: OnPmtReceived");
}

void CRecorder::OnPidsReceived(const CPidTable& info)
{
  if (m_pPmtParser && m_pmtVersion!=m_pPmtParser->GetPmtVersion() && m_iServiceId == info.ServiceId )
  {
    LogDebug("Recorder: PMT version changed from %d to %d - ServiceId %x", m_pmtVersion, m_pPmtParser->GetPmtVersion(), m_iServiceId );
    LogDebug("Recorder: pmt:0x%x pcr:0x%x video:0x%x audio1:0x%x audio2:0x%x audio3:%x audio4:0x%x audio5:0x%x video:0x%x teletext:0x%x subtitle:0x%x subtitle:0x%x subtitle:0x%x subtitle:0x%x",
      info.PmtPid,info.PcrPid,info.VideoPid,info.AudioPid1,info.AudioPid2,info.AudioPid3, info.AudioPid4,info.AudioPid5,info.VideoPid,info.TeletextPid,
      info.SubtitlePid1,info.SubtitlePid2,info.SubtitlePid3,info.SubtitlePid4);

    if (m_pmtVersion==-1)
    {
      LogDebug("Recorder: first PMT change, ignore it!" );
      m_pmtVersion=m_pPmtParser->GetPmtVersion();
      return;
    }

    LogDebug("Recorder: clear PIDs vector" );
    m_vecPids.clear();
    m_pmtVersion=m_pPmtParser->GetPmtVersion();
    
    // AddStream() makes sure that duplicates aren't inserted
    if (info.SubtitlePid1!=0)AddStream(info.SubtitlePid1, false, false, false);
    if (info.SubtitlePid2!=0)AddStream(info.SubtitlePid2, false, false, false);
    if (info.SubtitlePid3!=0)AddStream(info.SubtitlePid3, false, false, false);
    if (info.SubtitlePid4!=0)AddStream(info.SubtitlePid4, false, false, false);
    if (info.AC3Pid!=0)AddStream(info.AC3Pid, true, false, false);
    if (info.AudioPid1!=0)AddStream(info.AudioPid1, false, true, false);
    if (info.AudioPid2!=0)AddStream(info.AudioPid2, false, true, false);
    if (info.AudioPid3!=0)AddStream(info.AudioPid3, false, true, false);
    if (info.AudioPid4!=0)AddStream(info.AudioPid4, false, true, false);
    if (info.AudioPid5!=0)AddStream(info.AudioPid5, false, true, false);
    if (info.VideoPid!=0)AddStream(info.VideoPid, false, false, true); 
    if (info.TeletextPid!=0)AddStream(info.TeletextPid, false, false, false); 
  }
}
//*******************************************************************
//* PatchPcr()
//* Patches the PCR so the start of the .ts file always starts
//* with a PCR of 0. 
//*******************************************************************
void CRecorder::PatchPcr(byte* tsPacket,CTsHeader& header)
{
  if (header.PayLoadOnly()) return;
  m_adaptionField.Decode(header,tsPacket);
  if (m_adaptionField.PcrFlag==false) return;
  CPcr pcrNew=m_adaptionField.Pcr;
	bool logNextPcr = false;  
  if (m_bStartPcrFound)
	{
		if (m_bDetermineNewStartPcr )
		{
			m_bDetermineNewStartPcr=false;
      m_startPcr=pcrNew; 
			m_highestPcr.Reset();
		}
	}
  
	if (m_bStartPcrFound==false)
	{
		m_bDetermineNewStartPcr=false;
		m_bStartPcrFound=true;
		m_startPcr  = pcrNew;
    m_highestPcr= pcrNew;
    LogDebug("Pcr new start pcr :%s - pid:%x", m_startPcr.ToString() , header.Pid);
	} 

  CPcr pcrHi=pcrNew;
  CPcr diff;

	if (pcrNew > m_highestPcr)
	{
		m_highestPcr = pcrNew;
	}

  if (pcrNew > m_prevPcr)
  {
    diff = pcrNew - m_prevPcr;
  }
  else
  {
    diff = m_prevPcr - pcrNew;
  }

  if(diff.ToClock() > 10L && m_prevPcr.ToClock() > 0)
  {
    bool logNextPcr = true;
    if( diff.ToClock() > 95443L ) // Max PCR value 95443.71768
    {
      m_bPCRRollover = true;
      m_pcrDuration = m_prevPcr;
      m_pcrDuration -= m_startPcr;
      m_pcrHole.Reset();
      m_backwardsPcrHole.Reset();
      m_startPcr.Reset();
      m_highestPcr.Reset();

      LogDebug( "PCR rollover detected!" );
    }
    else
    {      
      CPcr step;
      step.FromClock(0.02); // an estimated PCR step
      
      if (pcrNew > m_prevPcr)
      {
        m_pcrHole += diff;
        m_pcrHole -= step; 
        LogDebug( "Jump forward in PCR detected!" );
      }
      else
      {
				m_backwardsPcrHole += diff;
				m_backwardsPcrHole += step;
        LogDebug( "Jump backward in PCR detected!" );
      }
    }
  }

  /*pcrHi -= m_startPcr;
  pcrHi += m_backwardsPcrHole;
  pcrHi -= m_pcrHole;*/

  double result = pcrHi.ToClock() - m_startPcr.ToClock() + m_backwardsPcrHole.ToClock() - m_pcrHole.ToClock();
  pcrHi.FromClock(result);

  if( m_bPCRRollover )
  {
    pcrHi += m_pcrDuration;
  }

  if( logNextPcr )
  {
    LogDebug("PCR: %s new: %s prev: %s start: %s diff: %s hole: %s holeB: %s  - pid:%x", pcrHi.ToString(), pcrNew.ToString(), m_prevPcr.ToString(), m_startPcr.ToString(), diff.ToString(), m_pcrHole.ToString(), m_backwardsPcrHole.ToString(), header.Pid );
  }
  tsPacket[6] = (byte)(((pcrHi.PcrReferenceBase>>25)&0xff));
  tsPacket[7] = (byte)(((pcrHi.PcrReferenceBase>>17)&0xff));
  tsPacket[8] = (byte)(((pcrHi.PcrReferenceBase>>9)&0xff));
  tsPacket[9] = (byte)(((pcrHi.PcrReferenceBase>>1)&0xff));
  tsPacket[10]=	(byte)(((pcrHi.PcrReferenceBase&0x1)<<7) + 0x7e+ ((pcrHi.PcrReferenceExtension>>8)&0x1));
  tsPacket[11]= (byte)(pcrHi.PcrReferenceExtension&0xff);

  m_prevPcr = pcrNew;
}

//*******************************************************************
//* PatchPtsDts()
//* Patches the PTS/DTS timestamps in a audio/video transport packet
//* so the start of the timeshifting file always starts
//* with a PTS/DTS of 0. 
//*******************************************************************
void CRecorder::PatchPtsDts(byte* tsPacket,CTsHeader& header,CPcr& startPcr)
{
  if (false==header.PayloadUnitStart) return;

  int start=header.PayLoadStart;
  if (tsPacket[start] !=0 || tsPacket[start+1] !=0  || tsPacket[start+2] !=1) return; 

  byte* pesHeader=&tsPacket[start];
	CPcr pts;
	CPcr dts;
  if (!CPcr::DecodeFromPesHeader(pesHeader,pts,dts))
  {
		return ;
	}
	if (pts.IsValid)
	{
    CPcr ptsorg=pts;
		pts -= startPcr;
    pts -= m_pcrHole;

    if( m_bPCRRollover )
    {
      pts += m_pcrDuration;
    }
		// 9       10        11        12      13
		//76543210 76543210 76543210 76543210 76543210
		//0011pppM pppppppp pppppppM pppppppp pppppppM 
		//LogDebug("pts: org:%s new:%s start:%s - pid:%x", ptsorg.ToString(),pts.ToString(),startPcr.ToString(), header.Pid); 
		byte marker=0x21;
		if (dts.PcrReferenceBase!=0) marker=0x31;
		pesHeader[13]=(byte)((( (pts.PcrReferenceBase&0x7f)<<1)+1));   pts.PcrReferenceBase>>=7;
		pesHeader[12]=(byte)(   (pts.PcrReferenceBase&0xff));				   pts.PcrReferenceBase>>=8;
		pesHeader[11]=(byte)((( (pts.PcrReferenceBase&0x7f)<<1)+1));   pts.PcrReferenceBase>>=7;
		pesHeader[10]=(byte)(   (pts.PcrReferenceBase&0xff));					 pts.PcrReferenceBase>>=8;
		pesHeader[9] =(byte)( (((pts.PcrReferenceBase&7)<<1)+marker)); 
    
		if (dts.IsValid)
		{
			CPcr dtsorg=dts;
			dts -= startPcr;
      dts -= m_pcrHole;

      if( m_bPCRRollover )
      {
        dts += m_pcrDuration;
      }

			// 14       15        16        17      18
			//76543210 76543210 76543210 76543210 76543210
			//0001pppM pppppppp pppppppM pppppppp pppppppM 
	 		//LogDebug("dts: org:%s new:%s start:%s - pid:%x", dtsorg.ToString(),dts.ToString(),startPcr.ToString(), header.Pid); 
			pesHeader[18]=(byte)( (((dts.PcrReferenceBase&0x7f)<<1)+1));  dts.PcrReferenceBase>>=7;
			pesHeader[17]=(byte)(   (dts.PcrReferenceBase&0xff));				  dts.PcrReferenceBase>>=8;
			pesHeader[16]=(byte)( (((dts.PcrReferenceBase&0x7f)<<1)+1));  dts.PcrReferenceBase>>=7;
			pesHeader[15]=(byte)(   (dts.PcrReferenceBase&0xff));					dts.PcrReferenceBase>>=8;
			pesHeader[14]=(byte)( (((dts.PcrReferenceBase&7)<<1)+0x11)); 
		}
	
		//pts.Reset();
		//dts.Reset();
		//if (CPcr::DecodeFromPesHeader(pesHeader,pts,dts))
		//{
		//	LogDebug("pts:%s dts:%s", pts.ToString(),dts.ToString());
		//}
	}
}
