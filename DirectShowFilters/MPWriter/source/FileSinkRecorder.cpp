#include <streams.h>
#include "FileSinkRecorder.h"
#include "GroupsockHelper.hh"
#include "OutputFile.hh"
#include "MPEG1or2Demux.hh"

////////// CFileSinkRecorder //////////

extern void LogDebug(const char *fmt, ...) ;
CFileSinkRecorder::CFileSinkRecorder(UsageEnvironment& env, FileWriter* fid, unsigned bufferSize,char const* perFrameFileNamePrefix) 
: CBaseFileWriterSink(env,bufferSize,perFrameFileNamePrefix,RECORD_BUFFER_SIZE), fOutFid(fid)
{
	LogDebug("CFileSinkRecorder::ctor");
}

CFileSinkRecorder::~CFileSinkRecorder() 
{
	LogDebug("CFileSinkRecorder::dtor");
	delete [] m_pWriteBuffer;
	if (fOutFid != NULL) 
	{
		fOutFid->CloseFile();
		delete fOutFid;
		fOutFid=NULL;
	}
}

CFileSinkRecorder* CFileSinkRecorder::createNew(UsageEnvironment& env, char const* fileName,unsigned bufferSize) 
{
	do 
	{
		LogDebug("CFileSinkRecorder::create file:%s",fileName);
		FileWriter* fid = new FileWriter();
		WCHAR wstrFileName[2048];
		MultiByteToWideChar(CP_ACP,0,fileName,-1,wstrFileName,1+strlen(fileName));
		fid->SetFileName(wstrFileName);
		if (FAILED(fid->OpenFile()))
		{
			LogDebug("CFileSinkRecorder::create file:%s failed",fileName);
			delete fid;
			return NULL;
		}
		return new CFileSinkRecorder(env, fid, bufferSize, NULL);
	} while (0);

	return NULL;
}

void CFileSinkRecorder::OnTsPacket(byte* tsPacket)
{
	memcpy(&m_pWriteBuffer[m_iWriteBufferPos],tsPacket,TS_SIZE);
	m_iWriteBufferPos += TS_SIZE;
	if (m_iWriteBufferPos >= RECORD_BUFFER_SIZE)
	{
		fOutFid->Write(m_pWriteBuffer, RECORD_BUFFER_SIZE);
		m_iWriteBufferPos = 0;
	}

}
