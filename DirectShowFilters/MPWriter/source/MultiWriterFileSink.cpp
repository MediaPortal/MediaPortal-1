#include <streams.h>
#include "MultiWriterFileSink.h"
#include "GroupsockHelper.hh"
#include "OutputFile.hh"
#include "MPEG1or2Demux.hh"

////////// CMultiWriterFileSink //////////

extern void LogDebug(const char *fmt, ...) ;
CMultiWriterFileSink::CMultiWriterFileSink(UsageEnvironment& env, MultiFileWriter* fid, unsigned bufferSize,char const* perFrameFileNamePrefix) 
: CBaseFileWriterSink(env,bufferSize,perFrameFileNamePrefix,WRITE_BUFFER_SIZE), fOutFid(fid)
{
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

void CMultiWriterFileSink::OnTsPacket(byte* tsPacket)
{	
	memcpy(&m_pWriteBuffer[m_iWriteBufferPos],tsPacket,TS_SIZE);
	m_iWriteBufferPos += TS_SIZE;
	if (m_iWriteBufferPos >= WRITE_BUFFER_SIZE)
	{
		fOutFid->Write(m_pWriteBuffer, WRITE_BUFFER_SIZE);
		m_iWriteBufferPos = 0;
	}
}