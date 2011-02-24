#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "MultiWriterFileSink.h"
#include "GroupsockHelper.hh"
#include "OutputFile.hh"
#include "MPEG1or2Demux.hh"

////////// CMultiWriterFileSink //////////

extern void LogDebug(const char *fmt, ...) ;
CMultiWriterFileSink::CMultiWriterFileSink(UsageEnvironment& env, MultiFileWriter* fid, unsigned bufferSize,char const* perFrameFileNamePrefix) 
: MediaSink(env), fOutFid(fid), fBufferSize(bufferSize) 
{
	fBuffer = new unsigned char[bufferSize];
	m_startPcr=0;
	m_highestPcr=0;
	m_bDetermineNewStartPcr=false;
	m_bStartPcrFound=false;

	m_iWriteBufferSize = THROTTLE_MAXIMUM_TV_PACKETS * TS_PACKET_SIZE;
	m_pWriteBuffer = new byte[m_iWriteBufferSize];
	m_iWriteBufferPos = 0;

	//	Populate the throttle
	m_iThrottleBufferSizes[0] = 2;		//	2 total
	m_iThrottleBufferSizes[1] = 3;		//	5
	m_iThrottleBufferSizes[2] = 5;		//	10
	m_iThrottleBufferSizes[3] = 5;		//	15
	m_iThrottleBufferSizes[4] = 5;		//	20
	m_iThrottleBufferSizes[5] = 5;		//	25
	m_iThrottleBufferSizes[6] = 5;		//	30
	m_iThrottleBufferSizes[7] = 10;		//	40
	m_iThrottleBufferSizes[8] = 10;		//	50
	m_iThrottleBufferSizes[9] = 10;		//	60
	m_iThrottleBufferSizes[10] = 10;	//	70
	m_iThrottleBufferSizes[11] = 10;	//	80
	m_iThrottleBufferSizes[12] = 20;	//	100
	m_iThrottleBufferSizes[13] = 20;	//	120
	m_iThrottleBufferSizes[14] = 20;	//	140
	m_iThrottleBufferSizes[15] = 32;	//	172	*sync with streamingserver
	m_iThrottleBufferSizes[16] = 40;	//	212
	m_iThrottleBufferSizes[17] = 50;	//	262
	m_iThrottleBufferSizes[18] = 82;	//	344	*sync with streamingserver
	m_iThrottleBufferSizes[19] = 172;	//	516	*sync with streamingserver

	m_iWriteBufferThrottle = 0;
	m_bThrottleAtMax = FALSE;

	LogDebug("CMultiWriterFileSink::ctor");

}

CMultiWriterFileSink::~CMultiWriterFileSink() 
{
	LogDebug("CMultiWriterFileSink::dtor");
	if (fOutFid != NULL) 
	{
		fOutFid->CloseFile();
		delete fOutFid;
		fOutFid=NULL;
	}
	delete[] fBuffer;
	delete[] m_pWriteBuffer;
}

CMultiWriterFileSink* CMultiWriterFileSink::createNew(UsageEnvironment& env, char const* fileName,int minFiles, int maxFiles, ULONG maxFileSize,unsigned bufferSize, Boolean oneFilePerFrame, int channelType) 
{
	do 
	{
		LogDebug("CMultiWriterFileSink::create file:%s",fileName);
		MultiFileWriter* fid = new MultiFileWriter();
		fid->setMinTSFiles(minFiles);
		fid->setMaxTSFiles(maxFiles);
		fid->setChunkReserve(maxFileSize);
		fid->setMaxTSFileSize(maxFileSize);
		WCHAR wstrFileName[2048];
		MultiByteToWideChar(CP_ACP,0,fileName,-1,wstrFileName,1+strlen(fileName));
		if (FAILED(fid->OpenFile(wstrFileName)))
		{
			LogDebug("CMultiWriterFileSink::create file:%s failed",fileName);
			delete fid;
			return NULL;
		}
		CMultiWriterFileSink *newMultiWriterFileSink = new CMultiWriterFileSink(env, fid, bufferSize, NULL);

		//	Set the channel type
		newMultiWriterFileSink->SetChannelType(channelType);

		return newMultiWriterFileSink;
	} while (0);

	return NULL;
}

//	Sets the channel type
void CMultiWriterFileSink::SetChannelType(int channelType)
{
	try
	{
		//	If tv (0)
		if(channelType == 0)
			m_eChannelType = TV;

		//	Else assume radio
		else
			m_eChannelType = Radio;

		//	Reset the throttle
		m_iWriteBufferThrottle = 0;	
		m_bThrottleAtMax = FALSE;
	}
	catch(...)
	{
		LogDebug("SetCCMultiWriterFileSink::SetChannelType() - Exception");
	}
}

Boolean CMultiWriterFileSink::continuePlaying() 
{
	if (fSource == NULL) return False;

	fSource->getNextFrame(fBuffer, fBufferSize,afterGettingFrame, this,onSourceClosure, this);

	return True;
}

void CMultiWriterFileSink::afterGettingFrame(void* clientData, unsigned frameSize,
											 unsigned /*numTruncatedBytes*/,
struct timeval presentationTime,
	unsigned /*durationInMicroseconds*/) {
		CMultiWriterFileSink* sink = (CMultiWriterFileSink*)clientData;
		sink->afterGettingFrame1(frameSize, presentationTime);
} 

void CMultiWriterFileSink::addData(unsigned char* data, unsigned dataSize,struct timeval presentationTime) 
{
	if (fOutFid != NULL && data != NULL) 
	{
		fOutFid->Write(data, dataSize);
	}
}

void CMultiWriterFileSink::afterGettingFrame1(unsigned frameSize,struct timeval presentationTime) 
{
	CAutoLock BufferLock(&m_Lock);
	OnRawData(fBuffer, frameSize);
	//addData(fBuffer, frameSize, presentationTime);
	// Then try getting the next frame:
	continuePlaying();
}


void CMultiWriterFileSink::OnTsPacket(byte* tsPacket)
{
	if(m_iWriteBufferPos < 0 || (m_iWriteBufferPos + TS_PACKET_LEN) > m_iWriteBufferSize)
	{
		LogDebug("CMultiWriterFileSink::OnTsPacket() - Buffer overflow");
		m_iWriteBufferPos = 0;
	}

	memcpy(&m_pWriteBuffer[m_iWriteBufferPos],tsPacket,TS_PACKET_LEN);
	m_iWriteBufferPos += TS_PACKET_LEN;
	

	//	Perform throttling
	if(m_iWriteBufferThrottle < 0)
		m_iWriteBufferThrottle = 0;

	if(m_iWriteBufferThrottle >= NUMBER_THROTTLE_BUFFER_SIZES)
		m_iWriteBufferThrottle = NUMBER_THROTTLE_BUFFER_SIZES - 1;

	int currentThrottlePackets = m_iThrottleBufferSizes[m_iWriteBufferThrottle];
	int currentThrottleBufferSize = currentThrottlePackets * TS_PACKET_LEN;


    if (m_iWriteBufferPos >= currentThrottleBufferSize)
	{
		int throttleToNumberPackets = THROTTLE_MAXIMUM_TV_PACKETS;

		//	If radio, we want to throttle to a smaller buffer size
		if(m_eChannelType == Radio)
			throttleToNumberPackets = THROTTLE_MAXIMUM_RADIO_PACKETS;

		//	Throttle up if we are not at maximum		
		if(currentThrottlePackets < throttleToNumberPackets)
		{	
			if(m_iWriteBufferThrottle < (NUMBER_THROTTLE_BUFFER_SIZES - 1))
				LogDebug("CMultiWriterFileSink::OnTsPacket() - Throttle to %d bytes", m_iWriteBufferPos);

			m_iWriteBufferThrottle++;
		}
		else if(currentThrottlePackets == throttleToNumberPackets && m_bThrottleAtMax == FALSE)
		{
			m_bThrottleAtMax = TRUE;
			LogDebug("CMultiWriterFileSink::OnTsPacket() - Throttle to %d bytes (max)", m_iWriteBufferPos);
		}

		fOutFid->Write(m_pWriteBuffer, m_iWriteBufferPos);
		m_iWriteBufferPos = 0;
	}
}


void CMultiWriterFileSink::ClearStreams()
{
	m_bDetermineNewStartPcr=true;

	m_iWriteBufferThrottle = 0;
}

