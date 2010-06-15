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
#include <cassert>

#include "timeshifting.h"
#include "pmtparser.h"

#define WRITE_BUFFER_SIZE 32900
#define IGNORE_AFTER_TUNE 25                        // how many TS packets to ignore after tuning
#define TS_QUEUE_SIZE 50                           // how many TS packets fits in TS buffer

#define PID_PAT                               0     // PID for PAT table
#define TABLE_ID_PAT                          0     // TABLE ID for PAT
#define TABLE_ID_SDT                        0x42    // TABLE ID for SDT

#define ADAPTION_FIELD_LENGTH_OFFSET        0x4     // offset in TS header to the adaption field length
#define PCR_FLAG_OFFSET                     0x5     // offset in TS header to the PCR 
#define DISCONTINUITY_FLAG_BIT              0x80    // bitmask for the DISCONTINUITY flag
#define RANDOM_ACCESS_FLAG_BIT              0x40    // bitmask for the RANDOM_ACCESS_FLAG flag
#define ES_PRIORITY_FLAG_BIT                0x20    // bitmask for the ES_PRIORITY_FLAG flag
#define PCR_FLAG_BIT                        0x10    // bitmask for the PCR flag
#define OPCR_FLAG_BIT                       0x8     // bitmask for the OPCR flag
#define SPLICING_FLAG_BIT                   0x4     // bitmask for the SPLICING flag
#define TRANSPORT_PRIVATE_DATA_FLAG_BIT     0x2     // bitmask for the TRANSPORT_PRIVATE_DATA flag
#define ADAPTION_FIELD_EXTENSION_FLAG_BIT   0x1     // bitmask for the DAPTION_FIELD_EXTENSION flag

int FAKE_NETWORK_ID   = 0x456;                // network id we use in our PAT
int FAKE_TRANSPORT_ID = 0x4;                  // transport id we use in our PAT
int FAKE_SERVICE_ID   = 0x89;                 // service id we use in our PAT
int FAKE_PMT_PID      = 0x20;                 // pid we use for our PMT
int FAKE_PCR_PID      = 0x30;//0x21;          // pid we use for our PCR
int FAKE_VIDEO_PID    = 0x30;                 // pid we use for the video stream
int FAKE_AUDIO_PID    = 0x40;                 // pid we use for the audio strean
int FAKE_SUBTITLE_PID = 0x50;                 // pid we use for subtitles
int FAKE_TELETEXT_PID = 0x60; // Ziphnor

double MAX_ALLOWED_PCR_DIFF = 10.0; // (clock value) if pcr/pts/dts difference between last and current value is higher than
								    //			   this value a hole/jump is assumed

extern void LogDebug(const char *fmt, ...) ;

/* CRC table for PSI sections */
static DWORD crc_table[256] = {
0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9, 0x130476dc, 0x17c56b6b,
0x1a864db2, 0x1e475005, 0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61,
0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd, 0x4c11db70, 0x48d0c6c7,
0x4593e01e, 0x4152fda9, 0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,
0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011, 0x791d4014, 0x7ddc5da3,
0x709f7b7a, 0x745e66cd, 0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039,
0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5, 0xbe2b5b58, 0xbaea46ef,
0xb7a96036, 0xb3687d81, 0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,
0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49, 0xc7361b4c, 0xc3f706fb,
0xceb42022, 0xca753d95, 0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1,
0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d, 0x34867077, 0x30476dc0,
0x3d044b19, 0x39c556ae, 0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,
0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16, 0x018aeb13, 0x054bf6a4, 
0x0808d07d, 0x0cc9cdca, 0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde,
0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02, 0x5e9f46bf, 0x5a5e5b08,
0x571d7dd1, 0x53dc6066, 0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,
0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e, 0xbfa1b04b, 0xbb60adfc,
0xb6238b25, 0xb2e29692, 0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6,
0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a, 0xe0b41de7, 0xe4750050,
0xe9362689, 0xedf73b3e, 0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,
0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686, 0xd5b88683, 0xd1799b34,
0xdc3abded, 0xd8fba05a, 0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637,
0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb, 0x4f040d56, 0x4bc510e1,
0x46863638, 0x42472b8f, 0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,
0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47, 0x36194d42, 0x32d850f5,
0x3f9b762c, 0x3b5a6b9b, 0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff,
0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623, 0xf12f560e, 0xf5ee4bb9,
0xf8ad6d60, 0xfc6c70d7, 0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,
0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f, 0xc423cd6a, 0xc0e2d0dd,
0xcda1f604, 0xc960ebb3, 0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7,
0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b, 0x9b3660c6, 0x9ff77d71,
0x92b45ba8, 0x9675461f, 0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,
0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640, 0x4e8ee645, 0x4a4ffbf2,
0x470cdd2b, 0x43cdc09c, 0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8,
0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24, 0x119b4be9, 0x155a565e,
0x18197087, 0x1cd86d30, 0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,
0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088, 0x2497d08d, 0x2056cd3a,
0x2d15ebe3, 0x29d4f654, 0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0,
0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c, 0xe3a1cbc1, 0xe760d676,
0xea23f0af, 0xeee2ed18, 0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,
0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0, 0x9abc8bd5, 0x9e7d9662,
0x933eb0bb, 0x97ffad0c, 0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668,
0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4};

//*******************************************************************
//* calculate crc for a data block
//* data : block of data   
//* len  : length of data
//*******************************************************************
DWORD crc32 (char *data, int len)
{
	register int i;
	DWORD crc = 0xffffffff;

	for (i=0; i<len; i++)
		crc = (crc << 8) ^ crc_table[((crc >> 24) ^ *data++) & 0xff];

	return crc;
}

//*******************************************************************
//* ctor
//*******************************************************************
CTimeShifting::CTimeShifting(LPUNKNOWN pUnk, HRESULT *phr) 
	:CUnknown( NAME ("MpTsTimeshifting"), pUnk)
{
	m_bPaused=FALSE;
	m_params.chunkSize=268435424;
	m_params.maxFiles=20;
	m_params.maxSize=268435424;
	m_params.minFiles=6;

	m_pmtPid=-1;
	m_pcrPid=-1;
	m_timeShiftMode=ProgramStream;
	m_bTimeShifting=false;
	m_pTimeShiftFile=NULL;
	m_multiPlexer.SetFileWriterCallBack(this);

	m_bStartPcrFound=false;
	m_startPcr.Reset();
	m_highestPcr.Reset();
	m_bDetermineNewStartPcr=false;
	m_iPatVersion=0;
	m_iPmtVersion=0;
	m_pWriteBuffer = new byte[WRITE_BUFFER_SIZE];
	m_iWriteBufferPos=0;
  m_TsPacketCount=0;
	m_fDump=NULL;
	m_bIgnoreNextPcrJump=false;
  m_bClearTsQueue=false;
	rclock=new CPcrRefClock();
	m_mapLastPtsDts.clear();
}
//*******************************************************************
//* dtor
//*******************************************************************
CTimeShifting::~CTimeShifting(void)
{
	delete [] m_pWriteBuffer;
  for (int i=0; i < m_tsQueue.size();++i)
  {
    delete[] m_tsQueue[i];
  }    
  m_tsQueue.clear();
}

//*******************************************************************
//* OnTsPacket gets called when a new mpeg-2 transport packet has been received
//* Method will check if we are timeshifting and ifso write the packet to
//* the timeshift file if pid is correct
//*******************************************************************
void CTimeShifting::OnTsPacket(byte* tsPacket)
{
	if (m_bPaused) return;
	if (m_bTimeShifting)
	{
		CTsHeader header(tsPacket);
		if (header.Pid==0x1fff) return;

		if (header.SyncByte!=0x47) return;
		if (header.TransportError) return;
		CEnterCriticalSection enter(m_section);
		//* timeshifting to mpeg-2 program stream
		if (m_timeShiftMode==ProgramStream)
		{
			//* then feed packet to multiplexer for transform transport stream into program stream
			m_multiPlexer.OnTsPacket(tsPacket);
		}
		else
		{
			//* timeshifting to mpeg-2 transport stream. Just write packet to file
			WriteTs(tsPacket);
		}
	}
}


//*******************************************************************
//* Pause or continue timeshifting
//* This method allows the client to pause and continue timeshifting
//* Normally client pauses the timeshifting when updating the pids
//* and continues timeshifting when all pids have been set
//* onOff : 0 = continue, 1=pause
//*******************************************************************
STDMETHODIMP CTimeShifting::Pause( BYTE onOff) 
{
	CEnterCriticalSection enter(m_section);
  if (onOff!=0)
  {
		m_bClearTsQueue=true;
    m_bPaused=TRUE;
  }
	else
  {
		m_bPaused=FALSE;
  }
	if (m_bPaused)
  {
		LogDebug("Timeshifter:paused:yes"); 
  }
	else
	{
		LogDebug("Timeshifter:paused:no"); 
		Flush();
		m_iPacketCounter=200;//write fake pat/pmt
	}
	return S_OK;
}

//*******************************************************************
//* Sets the pcr pid to timeshift
//* pcrPid = the PCR pid
//*******************************************************************
STDMETHODIMP CTimeShifting::SetPcrPid(int pcrPid)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:pcr pid:0x%x",pcrPid); 
    LogDebug("Timeshifter:SetPcrPid clear old PIDs");
    m_vecPids.clear();
    m_bClearTsQueue=true;
    m_TsPacketCount=0;

		m_multiPlexer.ClearStreams();
		m_multiPlexer.SetPcrPid(pcrPid);
		if (m_bTimeShifting)
		{
			LogDebug("Timeshifter:determine new start pcr"); 
			m_bDetermineNewStartPcr=true;
			m_bIgnoreNextPcrJump=true;
		}
		m_pcrPid=pcrPid;
		m_vecPids.clear();
		FAKE_NETWORK_ID   = 0x456;
		FAKE_TRANSPORT_ID = 0x4;
		FAKE_SERVICE_ID   = 0x89;
		FAKE_PMT_PID      = 0x20;
		FAKE_PCR_PID      = 0x30;//0x21;
		FAKE_VIDEO_PID    = 0x30;
		FAKE_AUDIO_PID    = 0x40;
		FAKE_SUBTITLE_PID = 0x50;
		m_iPatVersion++;
		if (m_iPatVersion>15) 
			m_iPatVersion=0;
		m_iPmtVersion++;
		if (m_iPmtVersion>15) 
			m_iPmtVersion=0;
	}
	catch(...)
	{
		LogDebug("Timeshifter:SetPcrPid exception");
	}
	return S_OK;
}

//*******************************************************************
//* Sets the PMT pid to timeshift
//* pmtPid = the PMT pid
//*******************************************************************
STDMETHODIMP CTimeShifting::SetPmtPid(int pmtPid)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:pmt pid:0x%x",pmtPid);
    m_pmtPid=pmtPid;
	}
	catch(...)
	{
		LogDebug("Timeshifter:SetPmtPid exception");
	}
	return S_OK;
}

//*******************************************************************
//* Sets the timeshifting mode
//* This can be mpeg-2 program stream or mpeg-2 transport stream
//* mode 0 = ProgramStream, 1= transport stream
//*******************************************************************
STDMETHODIMP CTimeShifting::SetMode(int mode) 
{
	m_timeShiftMode=(TimeShiftingMode)mode;
	if (mode==ProgramStream)
		LogDebug("Timeshifter:program stream mode");
	else
		LogDebug("Timeshifter:transport stream mode");
	return S_OK;
}

//*******************************************************************
//* Returns the timeshifting mode
//* This can be mpeg-2 program stream or mpeg-2 transport stream
//* mode 0 = ProgramStream, 1= transport stream
//*******************************************************************
STDMETHODIMP CTimeShifting::GetMode(int *mode) 
{
	*mode=(int)m_timeShiftMode;
	return S_OK;
}

//*******************************************************************
//* Adds a stream to the timeshift file (unsupported streams ignored)
//* Original descriptor_data required. WriteFakePMT will write the original
//* descriptor into the PMT associating it with the fake PID.
//* pid             : pid to add
//* descriptor_data : original DVB SI descriptor from the PMT
// * Added by Ziphnor
//*******************************************************************
STDMETHODIMP CTimeShifting::AddStreamWithDescriptor(int pid, const byte* descriptor_data, int data_length, bool isAC3, bool isMpeg1, bool isMpeg2){
	//LogDebug("AddStreamWDesc PID %i", pid);
	if (pid==0) return S_OK;
	CEnterCriticalSection enter(m_section);
	itvecPids it=m_vecPids.begin();
	while (it!=m_vecPids.end())
	{
		PidInfo& info=*it;
		if (info.realPid==pid) return S_OK;
		++it;
	}

	try{
		PidInfo info;
		int pointer=0;
		int info_pointer=0;
		int len = data_length;
		while ( len > 0 ) {
			bool copy_descriptor = false;
			byte descriptor_tag = descriptor_data[pointer];
			int descriptor_length = descriptor_data[pointer+1] + 2;
			const byte* x = descriptor_data + pointer;
			
			if(descriptor_tag == DESCRIPTOR_DVB_TELETEXT){
				info.realPid=pid;
				info.fakePid=FAKE_TELETEXT_PID;
				info.seenStart=false;
				info.serviceType = 0x06;
				info.ContintuityCounter=0;
				//strcpy(info.language,language);
				copy_descriptor = true;

				LogDebug("Timeshifter:add (with descriptor) teletext stream real pid:0x%x fake pid:0x%x type:%x",info.realPid,info.fakePid,info.serviceType);
				FAKE_TELETEXT_PID++;
				m_multiPlexer.AddPesStream(pid,false,false,true);
			}
			else if(descriptor_tag == DESCRIPTOR_DVB_SUBTITLING){
				info.realPid=pid;
				info.fakePid=FAKE_SUBTITLE_PID;
				info.seenStart=false;
				info.ContintuityCounter=0;
				//strcpy(info.language,language);
				copy_descriptor = true;
				info.serviceType=0x06;
				LogDebug("Timeshifter:add (with descriptor) subtitle stream real pid:0x%x fake pid:0x%x type:%x",info.realPid,info.fakePid,info.serviceType);
				FAKE_SUBTITLE_PID++;
				m_multiPlexer.AddPesStream(pid,false,false,true);
			}
			else if (descriptor_tag == DESCRIPTOR_MPEG_ISO639_Lang)
			{			  			 
				if (m_pcrPid == pid)
				{
					FAKE_PCR_PID = FAKE_AUDIO_PID;
				}
				info.realPid=pid;
				info.fakePid=FAKE_AUDIO_PID;
				info.seenStart=false;
				if (isMpeg1)
				{
				  info.serviceType = SERVICE_TYPE_AUDIO_MPEG1;
				}
				else if (isMpeg2)
				{
				  info.serviceType = SERVICE_TYPE_AUDIO_MPEG2;
				}
				else if (isAC3)
				{
				  info.serviceType = SERVICE_TYPE_AUDIO_AC3;
				}
				
				info.ContintuityCounter=0;
				//strcpy(info.language,language);
				copy_descriptor = true;

				LogDebug("Timeshifter:add (with descriptor) ISO639 stream real pid:0x%x fake pid:0x%x type:%x",info.realPid,info.fakePid,info.serviceType);
				FAKE_AUDIO_PID++;
				
				if (isMpeg1 || isMpeg2)
				{
				  m_multiPlexer.AddPesStream(pid,false,true,false);
				}
				else if (isAC3)
				{
				  m_multiPlexer.AddPesStream(pid,true,false,false);
				}
								
			} else if ( descriptor_tag == DESCRIPTOR_STREAM_IDENTIFIER ) {
				copy_descriptor = true;
			}
			// feel free to add more descriptor types here, they will automatically work with 
			// WriteFakePMT as the descriptor is given
			else{
				LogDebug("WARNING: AddStreamWithDesc(pid,descriptor) doesnt support descriptor type 0x%x, ignoring", descriptor_tag);
			}

			if ( copy_descriptor ) {
				LogDebug("Copying Descriptor: stream_type: %d, tag: %d, length: %d",
					info.serviceType, x[0], descriptor_length);
				memcpy(info.descriptor_data+info_pointer,x,descriptor_length);
				info_pointer += descriptor_length;
				info.descriptor_valid = true;
			}
			pointer += descriptor_length;
			len -= descriptor_length;
		}
		if ( info.descriptor_valid ) {
			m_vecPids.push_back(info);
		}
	}
	catch(...)
	{
		LogDebug("Timeshifter:AddStream(pid,byte*) exception");
	}
	return S_OK;
}

//*******************************************************************
//* Adds an audio/video stream to the timeshift file
//* pid         : pid to add
//* serviceType : service type of pid. Indicates if its audio/video and which encoding is ued
//* language    : The video/audio language
//*******************************************************************
STDMETHODIMP CTimeShifting::AddStream(int pid, int serviceType, char* language)
{
	if (pid==0) return S_OK;
	CEnterCriticalSection enter(m_section);
	itvecPids it=m_vecPids.begin();
	while (it!=m_vecPids.end())
	{
		PidInfo& info=*it;
		if (info.realPid==pid) return S_OK;
		++it;
	}

	try
	{
		if (SERVICE_TYPE_AUDIO_MPEG1==serviceType||SERVICE_TYPE_AUDIO_MPEG2==serviceType||serviceType==SERVICE_TYPE_AUDIO_AC3)
		{
			if (m_pcrPid == pid)
			{
				FAKE_PCR_PID = FAKE_AUDIO_PID;
			}
			PidInfo info;
			info.realPid=pid;
			info.fakePid=FAKE_AUDIO_PID;
			info.seenStart=false;
			info.serviceType=serviceType;
			info.ContintuityCounter=0;
			strcpy(info.language,language);
			m_vecPids.push_back(info);

			LogDebug("Timeshifter:add audio stream real pid:0x%x fake pid:0x%x type:%x",info.realPid,info.fakePid,info.serviceType);
			FAKE_AUDIO_PID++;
			if (serviceType==SERVICE_TYPE_AUDIO_AC3)
				m_multiPlexer.AddPesStream(pid,true,false,false);
			else
				m_multiPlexer.AddPesStream(pid,false,true,false);
		}
		else if (serviceType==SERVICE_TYPE_VIDEO_MPEG1||serviceType==SERVICE_TYPE_VIDEO_MPEG2||serviceType==SERVICE_TYPE_VIDEO_MPEG4||serviceType==SERVICE_TYPE_VIDEO_H264)
		{
			//if (m_pcrPid == pid)
			//{
			//	FAKE_PCR_PID = FAKE_VIDEO_PID;
			//}
			//LogDebug("Timeshifter:add video pes stream pid:%x",pid);
			PidInfo info;
			info.realPid=pid;
			info.fakePid=FAKE_VIDEO_PID;
			info.seenStart=false;
			info.serviceType=serviceType;
			info.ContintuityCounter=0;
			strcpy(info.language,language);
			m_vecPids.push_back(info);
			LogDebug("Timeshifter:add video stream real pid:0x%x fake pid:0x%x type:%x",info.realPid,info.fakePid,info.serviceType);
			FAKE_VIDEO_PID++;
			m_multiPlexer.AddPesStream(pid,false,false,true);
		}
		else if (serviceType==SERVICE_TYPE_DVB_SUBTITLES1||serviceType==SERVICE_TYPE_DVB_SUBTITLES2)
		{
			PidInfo info;
			info.realPid=pid;
			info.fakePid=FAKE_SUBTITLE_PID;
			info.seenStart=false;
			info.serviceType=serviceType;
			info.ContintuityCounter=0;
			strcpy(info.language,language);
			m_vecPids.push_back(info);
			LogDebug("Timeshifter:add subtitle stream real pid:0x%x fake pid:0x%x type:%x",info.realPid,info.fakePid,info.serviceType);
			FAKE_SUBTITLE_PID++;
			m_multiPlexer.AddPesStream(pid,false,false,true);
		}
		else 
		{
			PidInfo info;
			info.realPid=pid;
			info.fakePid=pid;
			info.serviceType=serviceType;
			info.seenStart=false;
			info.ContintuityCounter=0;
			strcpy(info.language,language);
			LogDebug("Timeshifter:add stream real pid:0x%x fake pid:0x%x type:%x",info.realPid,info.fakePid,info.serviceType);
			m_vecPids.push_back(info);
		}
	}
	catch(...)
	{
		LogDebug("Timeshifter:AddPesStream exception");
	}
	return S_OK;
}

//*******************************************************************
//* Removes a stream from the timeshift file
//* pid         : pid to remove
//*******************************************************************
STDMETHODIMP CTimeShifting::RemoveStream(int pid)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("Timeshifter:remove pes stream pid:%x",pid);
		m_multiPlexer.RemovePesStream(pid);
	}
	catch(...)
	{
		LogDebug("Timeshifter:RemovePesStream exception");
	}
	return S_OK;
}

//*******************************************************************
//* Sets the filename for the timeshift file
//* pszFileName : full path and filename
//*******************************************************************
STDMETHODIMP CTimeShifting::SetTimeShiftingFileName(char* pszFileName)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:set filename:%s",pszFileName);
		m_iPacketCounter=0;
		m_pmtPid=-1;
		m_pcrPid=-1;
		m_vecPids.clear();
		m_startPcr.Reset();
		m_bStartPcrFound=false;
		m_highestPcr.Reset();
		m_bDetermineNewStartPcr=false;
		m_multiPlexer.Reset();
		strcpy(m_szFileName,pszFileName);
		strcat(m_szFileName,".tsbuffer");
	}
	catch(...)
	{
		LogDebug("Timeshifter:SetTimeShiftingFileName exception");
	}
	return S_OK;
}

//*******************************************************************
//* Starts timeshifting
//*******************************************************************
STDMETHODIMP CTimeShifting::Start()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		/*m_fDump=fopen("c:\\dump.txt","r");
		if (m_fDump!=NULL)
		{
		fclose(m_fDump);
		::DeleteFile("c:\\dump.ts");
		m_fDump = fopen("c:\\dump.ts","wb+");
		}*/
		if (strlen(m_szFileName)==0) return E_FAIL;
		::DeleteFile((LPCTSTR) m_szFileName);
		WCHAR wstrFileName[2048];
		MultiByteToWideChar(CP_ACP,0,m_szFileName,-1,wstrFileName,1+strlen(m_szFileName));

		//fTsFile = fopen("c:\\users\\public\\test.ts","wb+");
		m_pTimeShiftFile = new MultiFileWriter(&m_params);
		if (FAILED(m_pTimeShiftFile->OpenFile(wstrFileName))) 
		{
			LogDebug("Timeshifter:failed to open filename:%s %d",m_szFileName,GetLastError());
			m_pTimeShiftFile->CloseFile();
			delete m_pTimeShiftFile;
			m_pTimeShiftFile=NULL;
			return E_FAIL;
		}

		m_iPmtContinuityCounter=-1;
		m_iPatContinuityCounter=-1;
		m_bDetermineNewStartPcr=false;
		m_startPcr.Reset();
		m_bStartPcrFound=false;
		m_highestPcr.Reset();
		m_mapLastPtsDts.clear();
		m_iPacketCounter=0;
		m_iWriteBufferPos=0;
		LogDebug("Timeshifter:Start timeshifting:'%s'",m_szFileName);
		LogDebug("Timeshifter:real pcr:%x fake pcr:%x",m_pcrPid,FAKE_PCR_PID);
		LogDebug("Timeshifter:real pmt:%x fake pmt:%x",m_pmtPid,FAKE_PMT_PID);
		itvecPids it=m_vecPids.begin();
		while (it!=m_vecPids.end())
		{
			PidInfo& info=*it;
			LogDebug("Timeshifter:real pid:%x fake pid:%x type:%x",info.realPid,info.fakePid,info.serviceType);
			++it;
		}
		m_bTimeShifting=true;
		if (m_timeShiftMode==TransportStream)
		{
			WriteFakePAT();
			WriteFakePMT();
		}
		m_bPaused=FALSE;
	}
	catch(...)
	{
		LogDebug("Timeshifter:Start timeshifting exception");
	}
	return S_OK;
}

//*******************************************************************
//* Resets the timeshifter
//*******************************************************************
STDMETHODIMP CTimeShifting::Reset()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:Reset");
		m_pmtPid=-1;
		m_pcrPid=-1;
		m_bDetermineNewStartPcr=false;
		m_startPcr.Reset();
		m_bStartPcrFound=false;
		m_highestPcr.Reset();
		m_vecPids.clear();
		m_multiPlexer.Reset();
    m_tsQueue.clear();
    m_pcrHole.Reset();
    m_backwardsPcrHole.Reset();
		FAKE_NETWORK_ID   = 0x456;
		FAKE_TRANSPORT_ID = 0x4;
		FAKE_SERVICE_ID   = 0x89;
		FAKE_PMT_PID      = 0x20;
		FAKE_PCR_PID      = 0x30;//0x21;
		FAKE_VIDEO_PID    = 0x30;
		FAKE_AUDIO_PID    = 0x40;
		FAKE_SUBTITLE_PID = 0x50;
		m_iPacketCounter=0;
		m_bPaused=FALSE;
		m_mapLastPtsDts.clear();
	}
	catch(...)
	{
		LogDebug("Timeshifter:Reset timeshifting exception");
	}
	return S_OK;
}


//*******************************************************************
//* Stops timeshifting
//*******************************************************************
STDMETHODIMP CTimeShifting::Stop()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//fclose(fTsFile );

		LogDebug("Timeshifter:Stop timeshifting:'%s'",m_szFileName);
		m_bTimeShifting=false;
		m_multiPlexer.Reset();
		if (m_pTimeShiftFile!=NULL)
		{
			m_pTimeShiftFile->CloseFile();
			delete m_pTimeShiftFile;
			m_pTimeShiftFile=NULL;
		}
		if (m_fDump!=NULL)
		{
			fclose(m_fDump);
			m_fDump=NULL;
		}
		Reset();
	}
	catch(...)
	{
		LogDebug("Timeshifter:Stop timeshifting exception");
	}
	return S_OK;
}


//*******************************************************************
//* Flush i/o buffer to timeshifting file
//*******************************************************************
void CTimeShifting::Flush()
{
	try
	{
		if (m_iWriteBufferPos>0)
		{
			if (m_pTimeShiftFile!=NULL)
			{
				m_pTimeShiftFile->Write(m_pWriteBuffer,m_iWriteBufferPos);
				m_iWriteBufferPos=0;
			}
		}
	}
	catch(...)
	{
		LogDebug("Timeshifter:Flush exception");
	}
}


//*******************************************************************
//* Write a datablock to i/o buffer
//* When the i/o buffer is full, it is flushed to the timeshifting file
//* Uses internal TS packet queue to allow "rollback" of TS packets
//* on a channel change
//*
//* buffer: block of data
//* len   : length of buffer
//*******************************************************************
void CTimeShifting::Write(byte* buffer, int len)
{
  if (!m_bTimeShifting) return;
  if (buffer == NULL) return;
  if (len != 188) return; //sanity check
  if (m_pWriteBuffer == NULL) return; //sanity check
  if (m_bPaused || m_bClearTsQueue) 
  {
     try
     {
        LogDebug("Timeshifter: clear TS packet queue"); 
        for (int i=0; i < m_tsQueue.size();++i)
        {
          delete[] m_tsQueue[i];
        }
        m_tsQueue.clear();
        m_bClearTsQueue = false;
        ZeroMemory(m_pWriteBuffer, WRITE_BUFFER_SIZE);
        m_iWriteBufferPos = 0;
      }
      catch(...)
      {
        LogDebug("Timeshifter:Write exception - 1");
      }
      return;
    }
    CEnterCriticalSection enter(m_section);

    if (m_tsQueue.size() < TS_QUEUE_SIZE)
    {
      // Put all TS packets to the queue so we can drop some TS packets
      // when tuning to new channel
      try
      {
        char* tmp = new char[len];
        if (tmp!=NULL)
        {
          memcpy(tmp,buffer,len);
          m_tsQueue.push_back(tmp);
        }
      }
      catch(...)
      {
        LogDebug("Timeshifter:Write exception - 2");
        return;
      }
    }

    if (m_tsQueue.size() >= TS_QUEUE_SIZE)
    {
      try  
      {      
        // Copy first TS packet from the queue to the I/O buffer
        if (m_iWriteBufferPos >= 0 && m_iWriteBufferPos+188 <= WRITE_BUFFER_SIZE)
        {
          vector<char*>::iterator it = m_tsQueue.begin();
          char* tmp = *it;
          m_tsQueue.erase(it);
          memcpy(&m_pWriteBuffer[m_iWriteBufferPos], tmp,188);
          delete[] tmp;
          m_iWriteBufferPos+=188;
        }
        else
        {
           LogDebug("Timeshifter:Write m_iWriteBufferPos overflow!");
           m_iWriteBufferPos=0;
        }
      }
      catch(...)
      {
        LogDebug("Timeshifter:Write exception - 3");
        return;
      }

      if (m_iWriteBufferPos >= WRITE_BUFFER_SIZE)
      {
        Flush();
      }
    }  
  }

//*******************************************************************
//* returns the buffer size
//*******************************************************************
STDMETHODIMP CTimeShifting::GetBufferSize(long *size)
{
	CheckPointer(size, E_POINTER);
	*size = 0;
	return S_OK;
}

//*******************************************************************
//* returns the number of timeshifting files in use
//*******************************************************************
STDMETHODIMP CTimeShifting::GetNumbFilesAdded(WORD *numbAdd)
{
	CheckPointer(numbAdd, E_POINTER);
	*numbAdd = (WORD)m_pTimeShiftFile->getNumbFilesAdded();
	return NOERROR;
}

//*******************************************************************
//* returns the number of timeshifting files removed since start
//*******************************************************************
STDMETHODIMP CTimeShifting::GetNumbFilesRemoved(WORD *numbRem)
{
	CheckPointer(numbRem, E_POINTER);
	*numbRem = (WORD)m_pTimeShiftFile->getNumbFilesRemoved();
	return NOERROR;
}

//*******************************************************************
//* returns the current timeshifting file
//*******************************************************************
STDMETHODIMP CTimeShifting::GetCurrentFileId(WORD *fileID)
{
	CheckPointer(fileID, E_POINTER);
	*fileID = (WORD)m_pTimeShiftFile->getCurrentFileId();
	return NOERROR;
}

//*******************************************************************
//* returns the minimum amount of timeshifting files
//*******************************************************************
STDMETHODIMP CTimeShifting::GetMinTSFiles(WORD *minFiles)
{
	CheckPointer(minFiles, E_POINTER);
	*minFiles = (WORD) m_params.minFiles;
	return NOERROR;
}

//*******************************************************************
//* Sets the minimum amount of timeshifting files
//*******************************************************************
STDMETHODIMP CTimeShifting::SetMinTSFiles(WORD minFiles)
{
	m_params.minFiles=(long)minFiles;
	return NOERROR;
}

//*******************************************************************
//* returns the maxmimum amount of timeshifting files
//*******************************************************************
STDMETHODIMP CTimeShifting::GetMaxTSFiles(WORD *maxFiles)
{
	CheckPointer(maxFiles, E_POINTER);
	*maxFiles = (WORD) m_params.maxFiles;
	return NOERROR;
}

//*******************************************************************
//* sets the maxmimum amount of timeshifting files
//*******************************************************************
STDMETHODIMP CTimeShifting::SetMaxTSFiles(WORD maxFiles)
{
	m_params.maxFiles=(long)maxFiles;
	return NOERROR;
}

//*******************************************************************
//* returns the maxmimum filesize for a timeshifting file
//*******************************************************************
STDMETHODIMP CTimeShifting::GetMaxTSFileSize(__int64 *maxSize)
{
	CheckPointer(maxSize, E_POINTER);
	*maxSize = m_params.maxSize;
	return NOERROR;
}

//*******************************************************************
//* sets the maxmimum filesize for a timeshifting file
//*******************************************************************
STDMETHODIMP CTimeShifting::SetMaxTSFileSize(__int64 maxSize)
{
	m_params.maxSize=maxSize;
	return NOERROR;
}

//*******************************************************************
//* returns the initial file length for a timeshifting file
//*******************************************************************
STDMETHODIMP CTimeShifting::GetChunkReserve(__int64 *chunkSize)
{
	CheckPointer(chunkSize, E_POINTER);
	*chunkSize = m_params.chunkSize;
	return NOERROR;
}

//*******************************************************************
//* sets the initial file length for a timeshifting file
//*******************************************************************
STDMETHODIMP CTimeShifting::SetChunkReserve(__int64 chunkSize)
{
	m_params.chunkSize=chunkSize;
	return NOERROR;
}

//*******************************************************************
//* returns the current filesize of all timeshift files
//*******************************************************************
STDMETHODIMP CTimeShifting::GetFileBufferSize(__int64 *lpllsize)
{
	CheckPointer(lpllsize, E_POINTER);
	m_pTimeShiftFile->GetFileSize(lpllsize);
	return NOERROR;
}

//*******************************************************************
//* WriteTs()
//* this method checks if the pid of the tspacket needs to be written to
//* the timeshift file. Ifso.. the pid is written to the timeshifting file
//* This method also inserts the pat/pmt packets in the stream
//* and adjust the PCR/PTS timestamps
//*
//* tsPacket : mpeg-2 transport stream packet of 188 bytes
//*******************************************************************
void CTimeShifting::WriteTs(byte* tsPacket)
{	  	  
	m_TsPacketCount++;
  if( m_TsPacketCount < IGNORE_AFTER_TUNE ) return;
  if (m_pcrPid<0 || m_vecPids.size()==0 || m_pmtPid<0) return;

	m_tsHeader.Decode(tsPacket);
	if (m_tsHeader.TransportError) 
	{
	  LogDebug("  m_tsHeader.TransportError - IGNORE TS PACKET!");
	  return;
	}
	/*if (m_tsHeader.TScrambling!=0) 
	{
	  LogDebug("  m_tsHeader.TScrambling!=0 - IGNORE TS PACKET!");
	  return;
	}
	*/

	bool writeTS = true;
	int start=0;

	if (m_iPacketCounter>=100)
	{
		WriteFakePAT();
		WriteFakePMT();
		m_iPacketCounter=0;
	}

	int PayLoadUnitStart=0;
	if (m_tsHeader.PayloadUnitStart) PayLoadUnitStart=1;

	itvecPids it=m_vecPids.begin();
	itvecPids itPcr=m_vecPids.end();
	while (it!=m_vecPids.end())
	{
		PidInfo& info=*it;
		if (info.fakePid==FAKE_PCR_PID)
			itPcr=it;

		if (m_tsHeader.Pid==info.realPid)
		{
			// writeTS determines if a TS packet gets written to the timeshifting file or not.
			// invalid headers are skipped, such as scrambled packets.				
			writeTS = true; 
			if (PayLoadUnitStart)
			{
			  byte pkt[200];
			  memcpy(pkt,tsPacket,188);
			  PatchPtsDts(pkt,m_tsHeader,m_startPcr);					  					  
			  start=m_tsHeader.PayLoadStart;
			  if (tsPacket[start] !=0 || tsPacket[start+1] !=0  || tsPacket[start+2] !=1) writeTS = false; 
			}
			if (writeTS)
			{
			  if (info.serviceType==SERVICE_TYPE_VIDEO_MPEG1 || info.serviceType==SERVICE_TYPE_VIDEO_MPEG2||info.serviceType==SERVICE_TYPE_VIDEO_MPEG4||info.serviceType==SERVICE_TYPE_VIDEO_H264)
			  {
				  //        PatchPcr(tsPacket,m_tsHeader);
				  //video
				  if (!info.seenStart) 
				  {
					  if (PayLoadUnitStart)
					  {
						  info.seenStart=true;
						  LogDebug("timeshift: start of video detected");
					  }
				  }
				  if (!info.seenStart) return;
				  //LogDebug("vid:%x->%x %x %x", info.realPid,info.fakePid,m_tsHeader.ContinuityCounter,m_tsHeader.AdaptionControl);
				  byte pkt[200];
				  memcpy(pkt,tsPacket,188);
				  int pid=info.fakePid;
				  pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
				  pkt[2]=(pid&0xff);
				  if (m_tsHeader.Pid==m_pcrPid)  PatchPcr(pkt,m_tsHeader);

				  if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
				  {						
					  if (PayLoadUnitStart) PatchPtsDts(pkt,m_tsHeader,m_startPcr);					  					  												
					  info.ContintuityCounter=m_tsHeader.ContinuityCounter;
					  Write(pkt,188);
					  m_iPacketCounter++;
				  }
				  return;
			  }

			  if (info.serviceType==SERVICE_TYPE_AUDIO_MPEG1 || info.serviceType==SERVICE_TYPE_AUDIO_MPEG2|| info.serviceType==SERVICE_TYPE_AUDIO_AC3)
			  {
				  //audio
				  if (!info.seenStart)
				  {
					  if (PayLoadUnitStart)
					  {
						  info.seenStart=true;
						  LogDebug("timeshift: start of audio detected");
					  }
				  }
				  if (!info.seenStart) return;

				  byte pkt[200];
				  memcpy(pkt,tsPacket,188);
				  int pid=info.fakePid;
				  pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
				  pkt[2]=(pid&0xff);
				  if (m_tsHeader.Pid==m_pcrPid) PatchPcr(pkt,m_tsHeader);

				  if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
				  {						  
					  if (PayLoadUnitStart) PatchPtsDts(pkt,m_tsHeader,m_startPcr);					  					  							
					  info.ContintuityCounter=m_tsHeader.ContinuityCounter;
					  Write(pkt,188);
					  m_iPacketCounter++;
				  }
				  return;
			  }

			  if (info.serviceType==SERVICE_TYPE_DVB_SUBTITLES1 || info.serviceType==SERVICE_TYPE_DVB_SUBTITLES2)
			  {
				  //subtitle pid...
				  byte pkt[200];
				  memcpy(pkt,tsPacket,188);
				  int pid=info.fakePid;
				  pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
				  pkt[2]=(pid&0xff);
				  if (m_tsHeader.Pid==m_pcrPid) PatchPcr(pkt,m_tsHeader);

				  if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
				  {						  						  
					  if (PayLoadUnitStart) PatchPtsDts(pkt,m_tsHeader,m_startPcr);					  					  												
					  info.ContintuityCounter=m_tsHeader.ContinuityCounter;
					  Write(pkt,188);
					  m_iPacketCounter++;
				  }
				  return;
			  }

			  //private pid...
			  byte pkt[200];
			  memcpy(pkt,tsPacket,188);
			  int pid=info.fakePid;
			  pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
			  pkt[2]=(pid&0xff);
			  if (m_tsHeader.Pid==m_pcrPid) PatchPcr(pkt,m_tsHeader);

			  if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
			  {							  
				  info.ContintuityCounter=m_tsHeader.ContinuityCounter;
				  Write(pkt,188);
				  m_iPacketCounter++;
			  }
			}
			return;
		}
		++it;
	}

	if (m_tsHeader.Pid==m_pcrPid)
	{
		byte pkt[200];
		memcpy(pkt,tsPacket,188);
		int pid=FAKE_PCR_PID;
		PatchPcr(pkt,m_tsHeader);
		pkt[1]=( (pid>>8) & 0x1f);
		pkt[2]=(pid&0xff);
		pkt[3]=(2<<4);// Adaption Field Control==adaptation field only, no payload
		pkt[4]=0xb7;

		if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
		{						
			if(itPcr!=m_vecPids.end())
			{				  
				PidInfo& info=*itPcr;					
				pkt[3] &=0xf0;
				pkt[3] += (info.ContintuityCounter&0xf);
				//LogDebug("pcr:%x->%x %d", m_pcrPid,FAKE_PCR_PID,info.ContintuityCounter);
			}				

			Write(pkt,188);
			m_iPacketCounter++;
		}
		return;
	}
}


//*******************************************************************
//* WriteFakePAT()
//* Constructs a PAT table and writes it to the timeshifting file
//*******************************************************************
void CTimeShifting::WriteFakePAT()
{
	int tableId=TABLE_ID_PAT;
	int transportId=FAKE_TRANSPORT_ID;
	int pmtPid=FAKE_PMT_PID;
	int sectionLenght=9+4;
	int current_next_indicator=1;
	int section_number = 0;
	int last_section_number = 0;

	int pid=PID_PAT;
	int PayLoadUnitStart=1;
	int AdaptionControl=1;
	m_iPatContinuityCounter++;
	if (m_iPatContinuityCounter>0xf) m_iPatContinuityCounter=0;

	BYTE pat[200];
	memset(pat,0,sizeof(pat));
	pat[0]=0x47;
	pat[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
	pat[2]=(pid&0xff);
	pat[3]=(AdaptionControl<<4) +m_iPatContinuityCounter;		//0x10
	pat[4]=0;																								//0

	pat[5]=tableId;//table id																//0
	pat[6]=0xb0+((sectionLenght>>8)&0xf);										//0xb0
	pat[7]=sectionLenght&0xff;
	pat[8]=(transportId>>8)&0xff;
	pat[9]=(transportId)&0xff;
	pat[10]=((m_iPatVersion&0x1f)<<1)+current_next_indicator;
	pat[11]=section_number;
	pat[12]=last_section_number;
	pat[13]=(FAKE_SERVICE_ID>>8)&0xff;
	pat[14]=(FAKE_SERVICE_ID)&0xff;
	pat[15]=((pmtPid>>8)&0xff)|0xe0;
	pat[16]=(pmtPid)&0xff;

	int len=17;
	DWORD crc= crc32((char*)&pat[5],len-5);
	pat[len]=(byte)((crc>>24)&0xff);
	pat[len+1]=(byte)((crc>>16)&0xff);
	pat[len+2]=(byte)((crc>>8)&0xff);
	pat[len+3]=(byte)((crc)&0xff);
	Write(pat,188);
}

//*******************************************************************
//* WriteFakePMT()
//* Constructs a PMT table and writes it to the timeshifting file
//*******************************************************************
void CTimeShifting::WriteFakePMT()
{
	int program_info_length=0;
	int sectionLenght=9+2*5+5;

	int current_next_indicator=1;
	int section_number = 0;
	int last_section_number = 0;
	int transportId=FAKE_TRANSPORT_ID;

	int tableId=2;
	int pid=FAKE_PMT_PID;
	int PayLoadUnitStart=1;
	int AdaptionControl=1;

	m_iPmtContinuityCounter++;
	if (m_iPmtContinuityCounter>0xf) m_iPmtContinuityCounter=0;

	// Ziphnor: is this enough (and we only write 188 bytes of it at the end)?
	// a single descriptor can contain up to 257 bytes (up to 255 bytes after descriptor_length)
	BYTE pmt[256]; 

	memset(pmt,0xff,sizeof(pmt));
	pmt[0]=0x47;
	pmt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
	pmt[2]=(pid&0xff);
	pmt[3]=(AdaptionControl<<4) +m_iPmtContinuityCounter;
	pmt[4]=0;
	byte* pmtPtr=&pmt[4];
	pmt[5]=tableId;//table id
	pmt[6]=0;
	pmt[7]=0;
	pmt[8]=(FAKE_SERVICE_ID>>8)&0xff;
	pmt[9]=(FAKE_SERVICE_ID)&0xff;
	pmt[10]=((m_iPmtVersion&0x1f)<<1)+current_next_indicator;
	pmt[11]=section_number;
	pmt[12]=last_section_number;
	pmt[13]=(FAKE_PCR_PID>>8)&0xff;
	pmt[14]=(FAKE_PCR_PID)&0xff;
	pmt[15]=(program_info_length>>8)&0xff;
	pmt[16]=(program_info_length)&0xff;

	int pmtLength=9+4;
	int offset=17;
	itvecPids it=m_vecPids.begin();

	vector<PidInfo> toEndOfPMT; // hack: delays teletext descriptor until last so its only stored if it fits (Ziphnor)
	// TODO: Fix this method so it can write a PMT thats larger than one TsPacket!! (Ziphnor)

	while (it!=m_vecPids.end())
	{
		PidInfo& info=*it;

		if(info.descriptor_valid)
    { // we have the full descriptor data so we just copy it into the PMT
			// hack: delay until last part of PMT
			toEndOfPMT.push_back(info);
			it++;
			continue;
		}

		int serviceType=info.serviceType;
		if (serviceType==SERVICE_TYPE_AUDIO_AC3)
		{
			//AC3 is represented as stream type 6
			serviceType=SERVICE_TYPE_DVB_SUBTITLES2; // ugly hack
		}

		pmt[offset++]=serviceType;
		pmt[offset++]=0xe0+((info.fakePid>>8)&0x1F); // reserved; elementary_pid (high)
		pmt[offset++]=(info.fakePid)&0xff; // elementary_pid (low)
		pmt[offset++]=0xF0;// reserved; ES_info_length (high)
		pmtLength+=4;

		if (info.serviceType==SERVICE_TYPE_AUDIO_AC3)
		{
			int esLen=0;
			pmt[offset++]=esLen+2;						// ES_info_length (low)
			pmt[offset++]=DESCRIPTOR_DVB_AC3; // descriptor indicator
			pmt[offset++]=esLen;
			pmtLength+=3;
		}
		else if (info.serviceType==SERVICE_TYPE_DVB_SUBTITLES1 || info.serviceType==SERVICE_TYPE_DVB_SUBTITLES2)
		{ 
			//use constant here and in for loop, or strlen() in both?
			int esLen=strlen(info.language)+5;
			pmt[offset++]=esLen+2;   // ES_info_length (low)
			pmt[offset++]=DESCRIPTOR_DVB_SUBTITLING;   // descriptor indicator
			pmt[offset++]=esLen;
			pmtLength+=3;
			for (int i=0; i < strlen(info.language);++i)
			{
				pmt[offset++]=info.language[i];
				pmtLength++;
			}
			pmt[offset++]=0x10;
			pmt[offset++]=0x00;
			pmt[offset++]=0x01;
			pmt[offset++]=0x00;
			pmt[offset++]=0x01;
			pmtLength+=5;
		}
		else
		{
			pmt[offset++]=0;   // ES_info_length (low)
			pmtLength++;
		}
		++it;
	}

	it = toEndOfPMT.begin();
	//LogDebug("WRiting %i pid descriptors at end of fake pmt",toEndOfPMT.size());
	while(it != toEndOfPMT.end())
  {
			PidInfo& info = *it;
			int descriptor_length = info.descriptor_data[1] + 2; // 0 is tag, 1 is length		
			//LogDebug("Descriptor (total) length is %i", descriptor_length);
			if(pmtLength + 4 >= 188) // if it will make the PMT too big
			{ 
				LogDebug("Cant fit new descriptor for fake PID %i because it would make fake PMT not fit in one ts packet! ",info.fakePid);
				it++;
				continue;
			}
			pmt[offset++]=info.serviceType;
			pmt[offset++]=0xe0+((info.fakePid>>8)&0x1F); // reserved; elementary_pid (high)
			pmt[offset++]=(info.fakePid)&0xff; // elementary_pid (low)
			pmt[offset++]=0xF0;// reserved; ES_info_length (high)
			pmtLength+=4;

			// NOTE: This is completely generic and will work with all stream types
			// (if we have the original descriptor we just reuse it)
			
			//LogDebug("Writing Fake PMT with original DESCRIPTOR tag %X for real PID %i, fake pid %i", info.descriptor_data[0], info.realPid, info.fakePid );
			int pointer=0;
			byte* pEsInfoLength = pmt + offset;
			offset++;
			pmtLength++;
			*pEsInfoLength = 0;
			while ( info.descriptor_data[pointer] != 0xFF && pointer < sizeof(info.descriptor_data)) {
				descriptor_length = info.descriptor_data[pointer+1]+2;
				//LogDebug("Adding stream descriptor: stream_type: %d, type: %d, length: %d", 
				//	info.serviceType, info.descriptor_data[pointer], descriptor_length);
				if(pmtLength + descriptor_length + 4 >= 188) // if it will make the PMT too big
				{ 
					LogDebug("Cant fit another descriptor for fake PID %i because it would make fake PMT not fit in one ts packet (pmtLength: %d, descriptor_length: %d)! ",info.fakePid, pmtLength, descriptor_length );
					break;
				}
				*pEsInfoLength += descriptor_length; // ES_info_length(low) // what is the descriptor is so big it needs the high bits as well?

				memcpy(&pmt[offset], info.descriptor_data+pointer, descriptor_length); // copy descriptor content
				offset += descriptor_length;
				pmtLength += descriptor_length;
				pointer += descriptor_length;
			}
			it++;
	}

	unsigned section_length = (pmtLength);
	pmt[6]=0xb0+((section_length>>8)&0xf);
	pmt[7]=section_length&0xff;

	DWORD crc= crc32((char*)&pmt[5],offset-5);
	pmt[offset++]=(byte)((crc>>24)&0xff);
	pmt[offset++]=(byte)((crc>>16)&0xff);
	pmt[offset++]=(byte)((crc>>8)&0xff);
	pmt[offset++]=(byte)((crc)&0xff);

	if(pmtLength > 188) LogDebug("ERROR: Pmt length : %i ( >188 )!!!!",pmtLength);

	Write(pmt,188);
}


//*******************************************************************
//* PatchPcr()
//* Patches the PCR so the start of the timeshifting file always starts
//* with a PCR of 0. 
//* When zapping from channel A<-> channel B it will also make sure
//* that the PCR is continuious
//*******************************************************************
void CTimeShifting::PatchPcr(byte* tsPacket,CTsHeader& header)
{
	CPcr myPcr=rclock->GetAsPCR();
	//LogDebug("pcr pid:%x  head:%02.2x %02.2x %02.2x %02.2x %02.2x %02.2x",header.Pid, tsPacket[0],tsPacket[1],tsPacket[2],tsPacket[3],tsPacket[4],tsPacket[5]);
	if (header.PayLoadOnly()) return;
	//LogDebug(" pcrflag:%x", (tsPacket[5]&0x10));
	//LogDebug(" opcrflag:%x", (tsPacket[5]&0x8));
	m_adaptionField.Decode(header,tsPacket);
	if (m_adaptionField.PcrFlag==false) return;
	CPcr pcrNew=m_adaptionField.Pcr;
	bool logNextPcr = false;
  if (m_bStartPcrFound)
	{
		if (m_bDetermineNewStartPcr )
		{
			m_bDetermineNewStartPcr=false;

			CPcr duration=m_highestPcr ;
			duration-=m_startPcr;
			CPcr newStartPcr=pcrNew;
			newStartPcr-=duration ;
			LogDebug("Pcr start    :%s",m_startPcr.ToString());
			LogDebug("Pcr high     :%s",m_highestPcr.ToString());
			LogDebug("Pcr duration :%s",duration.ToString());
			LogDebug("Pcr current  :%s",pcrNew.ToString());
			LogDebug("Pcr newstart :%s",newStartPcr.ToString());

			m_startPcr  = newStartPcr;
			m_highestPcr= newStartPcr;
			m_prevPcr   = newStartPcr;
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

  if (m_bDetermineNewStartPcr)
  {
    LogDebug( "not allowed to patch PCRs yet - m_bDetermineNewStartPcr = true" );
  }

	// PCR value has jumped too much in the stream
  if (diff.ToClock() > MAX_ALLOWED_PCR_DIFF && m_prevPcr.ToClock() > 0 && !m_bDetermineNewStartPcr )
	{
    logNextPcr = true;
    if (m_bIgnoreNextPcrJump)
		{
			LogDebug( "Ignoring first PCR jump after channel change" );
			m_bIgnoreNextPcrJump=false;
		}
		else
		{
			if (diff.ToClock() > 95443L) // Max PCR value 95443.71768
			{
				m_bPCRRollover = true;
				m_pcrDuration = m_prevPcr;
				m_pcrDuration -= m_startPcr;
				m_pcrHole.Reset();
        m_backwardsPcrHole.Reset();
				m_startPcr.Reset();
				m_highestPcr.Reset();

				LogDebug( "PCR rollover detected" );
			}
			else
			{      
				logNextPcr = true;
        CPcr step;
				step.FromClock(0.02); // an estimated PCR step, to be fixed with some average from the stream...

				if (pcrNew > m_prevPcr)
				{
					m_pcrHole += diff;
					m_pcrHole -= step;
					LogDebug( "Jump forward in PCR detected" );
				}
				else
				{
					m_backwardsPcrHole += diff;
					m_backwardsPcrHole += step;
          LogDebug( "Jump backward in PCR detected" );
				}
			}
		}
	}

  /*pcrHi -= m_startPcr;
  pcrHi += m_backwardsPcrHole;
  pcrHi -= m_pcrHole;*/

  double result = pcrHi.ToClock() - m_startPcr.ToClock() + m_backwardsPcrHole.ToClock() - m_pcrHole.ToClock();
  pcrHi.FromClock(result);

	if (m_bPCRRollover)
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
	CPcr tmpPcr=m_startPcr+myPcr;
	//LogDebug("myPcr: %s tsPCR: %s",tmpPcr.ToString(),m_prevPcr.ToString());
}

//*******************************************************************
//* PatchPtsDts()
//* Patches the PTS/DTS timestamps in a audio/video transport packet
//* so the start of the timeshifting file always starts
//* with a PTS/DTS of 0. 
//* When zapping from channel A<-> channel B it will also make sure
//* that the PTS/TS is continuious
//*******************************************************************
void CTimeShifting::PatchPtsDts(byte* tsPacket,CTsHeader& header,CPcr& startPcr)
{
	if (false==header.PayloadUnitStart) return;

	int start=header.PayLoadStart;
	if (start>=188) 
	{
		LogDebug("ERROR: payload start>188. payloadStart=%d adaptionControl=%d adaptionFieldLength=%d",start,header.AdaptionControl,header.AdaptionFieldLength);
		return;
	}
	if (tsPacket[start] !=0 || tsPacket[start+1] !=0  || tsPacket[start+2] !=1) return; 

	byte* pesHeader=&tsPacket[start];
	CPcr pts;
	CPcr dts;
	if (!CPcr::DecodeFromPesHeader(pesHeader,pts,dts))
	{
		return ;
	}
	imapLastPtsDts it=m_mapLastPtsDts.find(header.Pid);
	if (it==m_mapLastPtsDts.end())
	{
		LastPtsDtsRecord lastRec;
		lastRec.pts.Reset();
		lastRec.dts.Reset();
		m_mapLastPtsDts[header.Pid]=lastRec;
		it=m_mapLastPtsDts.find(header.Pid);
	}
	LastPtsDtsRecord &lastPtsDts=it->second;
	if (pts.IsValid)
	{
		CPcr ptsorg=pts;
		pts -= startPcr;
		pts -= m_pcrHole;
		if( m_bPCRRollover )
			pts += m_pcrDuration;
		/* GEMX: deactivated. code is currently being tested
		if (lastPtsDts.pts.IsValid)
		{
			double diff=0;
			if (pts.ToClock()>lastPtsDts.pts.ToClock())
				diff=pts.ToClock()-lastPtsDts.pts.ToClock();
			else
				diff=lastPtsDts.pts.ToClock()-pts.ToClock();
			// Only apply pcr hole patching if pts also jumped
			lastPtsDts.pts=pts;
			if (diff>MAX_ALLOWED_PCR_DIFF && m_pcrHole.IsValid)
					pts -= m_pcrHole;
			else
			{
				if (!m_pcrHole.IsValid)
					LogDebug("Pcr hole detected but pts did not jump. Don't applying pts hole patching");
			}
		}
		else
			lastPtsDts.pts=pts;
		*/

		// 9       10        11        12      13
		//76543210 76543210 76543210 76543210 76543210
		//0011pppM pppppppp pppppppM pppppppp pppppppM 
		//LogDebug("pts: org:%s new:%s start:%s", ptsorg.ToString(),pts.ToString(),startPcr.ToString()); 
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
				dts += m_pcrDuration;
			/* GEMX: deactivated. code is currently being tested
			if (lastPtsDts.dts.IsValid)
			{
				double diff=0;
				if (dts.ToClock()>lastPtsDts.dts.ToClock())
					diff=dts.ToClock()-lastPtsDts.dts.ToClock();
				else
					diff=lastPtsDts.dts.ToClock()-dts.ToClock();
				// Only apply pcr hole patching if dts also jumped
				lastPtsDts.dts=dts;
				if (diff>MAX_ALLOWED_PCR_DIFF && m_pcrHole.IsValid)
					dts -= m_pcrHole;
				else
				{
					if (!m_pcrHole.IsValid)
						LogDebug("Pcr hole detected but dts did not jump. Don't applying pts hole patching");
				}
			}
			else
				lastPtsDts.dts=dts;
			*/

			// 14       15        16        17      18
			//76543210 76543210 76543210 76543210 76543210
			//0001pppM pppppppp pppppppM pppppppp pppppppM 
			//LogDebug("dts: org:%s new:%s start:%s", dtsorg.ToString(),dts.ToString(),startPcr.ToString()); 
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
