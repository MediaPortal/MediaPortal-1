#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "ProgramToTransportStream.h"
#include "MemoryStreamSource.h"
extern void LogDebug(const char *fmt, ...) ;

CProgramToTransportStream::CProgramToTransportStream(void)
{
	m_bRunning=false;
	LogDebug("CProgramToTransportStream::ctor");
	TaskScheduler* scheduler = MPTaskScheduler::createNew();
	m_env = BasicUsageEnvironment::createNew(*scheduler);
	m_outputSink=NULL;
	m_inputSource=NULL;
	m_tsFrames=NULL;
	m_bSendVideoAudioObserverEvents = true;
}

CProgramToTransportStream::~CProgramToTransportStream(void)
{
	LogDebug("CProgramToTransportStream::dtor");
}

void afterPlaying(void* clientData) 
{
	LogDebug("CProgramToTransportStream afterPlaying");
	MPEG2TransportStreamFromPESSource* outputSink=(MPEG2TransportStreamFromPESSource*)clientData;
}
void CProgramToTransportStream::Initialize(char* fileNameOut)
{
	LogDebug("CProgramToTransportStream::Initialize %s",fileNameOut);
	m_BufferThreadActive=false;
	m_buffer.Clear();


	m_inputSource = CMemoryStreamSource::createNew(*m_env, "",m_buffer);
	if (m_inputSource == NULL) 
	{
		*m_env << "Unable to open memorystream as a byte-stream source\n";
		return;
	}

	// Create a MPEG demultiplexor that reads from that source.
	MPEG1or2Demux* baseDemultiplexor = MPEG1or2Demux::createNew(*m_env, m_inputSource);

	// Create, from this, a source that returns raw PES packets:
	MPEG1or2DemuxedElementaryStream* pesSource = baseDemultiplexor->newRawPESStream();

	// And, from this, a filter that converts to MPEG-2 Transport Stream frames:
	m_tsFrames  = MPEG2TransportStreamFromPESSource::createNew(*m_env, pesSource);

	m_outputSink = CMultiWriterFileSink::createNew(*m_env, fileNameOut,m_minFiles,m_maxFiles,m_maxFileSize);
	if (m_outputSink == NULL) 
	{
		*m_env << "Unable to open file \"" << fileNameOut << "\" as a file sink\n";
		return;
	}
	m_buffer.Clear();
	m_iPacketsToSkip=100;
	//StartBufferThread();
	m_bSendVideoAudioObserverEvents = true;
	m_bStarting=true;
	m_bRunning=true;
}
void CProgramToTransportStream::Flush()
{
	LogDebug("CProgramToTransportStream::Flush()");
	m_bSendVideoAudioObserverEvents = true;
	m_iPacketsToSkip=10;
	m_buffer.Clear();
}
void CProgramToTransportStream::SetTimeShiftParams( int minFiles, int maxFiles, ULONG maxFileSize)
{
	m_minFiles=minFiles;
	m_maxFiles=maxFiles;
	m_maxFileSize=maxFileSize;
}
void CProgramToTransportStream::ClearStreams()
{
	LogDebug("CProgramToTransportStream::ClearStreams()");
	m_outputSink->ClearStreams();
}
void CProgramToTransportStream::Write(byte* data, int len)
{ 
	if (m_bRunning)
	{
		if (m_iPacketsToSkip>0) 
		{
			m_iPacketsToSkip--;
			// LogDebug("skip:%d",m_iPacketsToSkip);
			return;
		}
		m_buffer.PutBuffer(data, len, 1);
		if (m_bStarting && m_buffer.Size()>300000)
		{
			m_bStarting=false;
			m_BufferThreadActive = true;
			if (m_outputSink->startPlaying(*m_tsFrames, afterPlaying, m_tsFrames)==True)
			{
				LogDebug("CProgramToTransportStream::Thread playing()");
			}
			else
			{
				LogDebug("CProgramToTransportStream::Failed to start output sink");
			}
		}
		while (m_buffer.Size()>300000)
		{
			if(m_bSendVideoAudioObserverEvents && m_pCallback != NULL){
				m_bSendVideoAudioObserverEvents = false;
				m_pCallback->OnNotify(Audio);
				m_pCallback->OnNotify(Video);
			}
			m_env->taskScheduler().doEventLoop(); 
		}
	}
}

void CProgramToTransportStream::Close()
{
	LogDebug("CProgramToTransportStream::Close()");
	m_bRunning=false;
	m_buffer.Stop();
	StopBufferThread();
}


void CProgramToTransportStream::StartBufferThread()
{
	LogDebug("CProgramToTransportStream::StartBufferThread()");
	m_buffer.Clear();
	/*
	if (!m_BufferThreadActive)
	{
	//StartThread();
	m_BufferThreadActive = true;


	if (m_outputSink->startPlaying(*m_tsFrames, afterPlaying, m_tsFrames)==True)
	{
	LogDebug("CProgramToTransportStream::Thread playing()");
	}
	else
	{
	LogDebug("CProgramToTransportStream::Failed to start output sink");
	}
	}*/
}

void CProgramToTransportStream::StopBufferThread()
{
	LogDebug("CProgramToTransportStream::StopBufferThread()");
	//	if (!m_BufferThreadActive)
	//		return;

	//StopThread(INFINITE);


	if (m_outputSink!=NULL)
		Medium::close(m_outputSink);
	if (m_inputSource!=NULL)
		Medium::close(m_inputSource);

	if (m_tsFrames!=NULL)
		Medium::close(m_tsFrames);
	m_outputSink=NULL;
	m_inputSource=NULL;
	m_tsFrames=NULL;
	LogDebug("CProgramToTransportStream::Thread stopped()");

	m_BufferThreadActive = false;
}
void CProgramToTransportStream::ThreadProc()
{
	HRESULT hr = S_OK;
	m_BufferThreadActive = TRUE;

	//	BoostThread Boost;

	//::SetThreadPriority(GetCurrentThread(),THREAD_PRIORITY_TIME_CRITICAL);

	LogDebug("CProgramToTransportStream::Thread started()");

	if (m_outputSink->startPlaying(*m_tsFrames, afterPlaying, m_tsFrames)==True)
	{
		LogDebug("CProgramToTransportStream::Thread playing()");
	}
	else
	{
		LogDebug("CProgramToTransportStream::Failed to start output sink");
	}
	while (m_env!=NULL && !ThreadIsStopping(0))
	{
		m_env->taskScheduler().doEventLoop(); 

	}
	Medium::close(m_outputSink);
	Medium::close(m_inputSource);
	Medium::close(m_tsFrames);
	m_outputSink=NULL;
	m_inputSource=NULL;
	LogDebug("CProgramToTransportStream::Thread stopped()");
	m_BufferThreadActive = false;
	return;
}

void CProgramToTransportStream::SetVideoAudioObserver(IAnalogVideoAudioObserver* callback){
	LogDebug("CProgramToTransportStream::SetVideoAudioObserver - %x", callback);
	m_pCallback = callback;
}

