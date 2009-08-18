#include <streams.h>
#include "ProgramToTransportStreamRecorder.h"
#include "MemoryStreamSource.h"
extern void LogDebug(const char *fmt, ...) ;

CProgramToTransportStreamRecorder::CProgramToTransportStreamRecorder(void)
{
	m_bRunning=false;
	LogDebug("CProgramToTransportStreamRecorder::ctor");
	TaskScheduler* scheduler = BasicTaskScheduler::createNew();
	m_env = BasicUsageEnvironment::createNew(*scheduler);
	m_outputSink=NULL;
	m_inputSource=NULL;
	m_tsFrames=NULL;
	m_bSendVideoAudioObserverEvents = true;
}

CProgramToTransportStreamRecorder::~CProgramToTransportStreamRecorder(void)
{
	LogDebug("CProgramToTransportStreamRecorder::dtor");
}

void afterPlayingRecorder(void* clientData) 
{
	LogDebug("CProgramToTransportStreamRecorder afterPlaying");
	MPEG2TransportStreamFromPESSource* outputSink=(MPEG2TransportStreamFromPESSource*)clientData;
}
void CProgramToTransportStreamRecorder::Initialize(char* fileNameOut)
{
	LogDebug("CProgramToTransportStreamRecorder::Initialize %s",fileNameOut);
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

	m_outputSink = CFileSinkRecorder::createNew(*m_env, fileNameOut);
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
void CProgramToTransportStreamRecorder::Flush()
{
	m_bSendVideoAudioObserverEvents = true;
	// LogDebug("CProgramToTransportStreamRecorder::Flush()");
	// m_iPacketsToSkip=0;
	// m_buffer.Clear();
}
void CProgramToTransportStreamRecorder::ClearStreams()
{
	LogDebug("CProgramToTransportStreamRecorder::ClearStreams()");
	m_outputSink->ClearStreams();
}
void CProgramToTransportStreamRecorder::Write(byte* data, int len)
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
			if (m_outputSink->startPlaying(*m_tsFrames, afterPlayingRecorder, m_tsFrames)==True)
			{
				LogDebug("CProgramToTransportStreamRecorder::Thread playing()");
			}
			else
			{
				LogDebug("CProgramToTransportStreamRecorder::Failed to start output sink");
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

void CProgramToTransportStreamRecorder::Close()
{
	LogDebug("CProgramToTransportStreamRecorder::Close()");
	m_bRunning=false;
	m_buffer.Stop();
	StopBufferThread();
}


void CProgramToTransportStreamRecorder::StartBufferThread()
{
	LogDebug("CProgramToTransportStreamRecorder::StartBufferThread()");
	m_buffer.Clear();
}

void CProgramToTransportStreamRecorder::StopBufferThread()
{
	LogDebug("CProgramToTransportStreamRecorder::StopBufferThread()");
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
	LogDebug("CProgramToTransportStreamRecorder::Thread stopped()");

	m_BufferThreadActive = false;
}
void CProgramToTransportStreamRecorder::ThreadProc()
{
	HRESULT hr = S_OK;
	m_BufferThreadActive = TRUE;

	//	BoostThread Boost;

	//::SetThreadPriority(GetCurrentThread(),THREAD_PRIORITY_TIME_CRITICAL);

	LogDebug("CProgramToTransportStreamRecorder::Thread started()");

	if (m_outputSink->startPlaying(*m_tsFrames, afterPlayingRecorder, m_tsFrames)==True)
	{
		LogDebug("CProgramToTransportStreamRecorder::Thread playing()");
	}
	else
	{
		LogDebug("CProgramToTransportStreamRecorder::Failed to start output sink");
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
	LogDebug("CProgramToTransportStreamRecorder::Thread stopped()");
	m_BufferThreadActive = false;
	return;
}

void CProgramToTransportStreamRecorder::SetVideoAudioObserver(IAnalogVideoAudioObserver* callback){
	LogDebug("CProgramToTransportStream::SetVideoAudioObserver - %x", callback);
	m_pCallback = callback;
}
