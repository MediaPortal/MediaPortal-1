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
	m_pWriteBuffer = new byte[WRITE_BUFFER_SIZE];
	m_iWriteBufferPos = 0;

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

CMultiWriterFileSink* CMultiWriterFileSink::createNew(UsageEnvironment& env, char const* fileName,int minFiles, int maxFiles, ULONG maxFileSize,unsigned bufferSize, Boolean oneFilePerFrame) 
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
		return new CMultiWriterFileSink(env, fid, bufferSize, NULL);
	} while (0);

	return NULL;
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
	if(m_iWriteBufferPos +TS_PACKET_LEN >= WRITE_BUFFER_SIZE){
		fOutFid->Write(m_pWriteBuffer, m_iWriteBufferPos);
		m_iWriteBufferPos = 0;
	}
	memcpy(&m_pWriteBuffer[m_iWriteBufferPos],tsPacket,TS_PACKET_LEN);
	m_iWriteBufferPos += TS_PACKET_LEN;
}


void CMultiWriterFileSink::ClearStreams()
{
	m_bDetermineNewStartPcr=true;
}

