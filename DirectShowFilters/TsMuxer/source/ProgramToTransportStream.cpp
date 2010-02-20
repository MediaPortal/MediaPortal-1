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
	TaskScheduler* scheduler = BasicTaskScheduler::createNew();
	m_env = BasicUsageEnvironment::createNew(*scheduler);
	m_outputSink=NULL;
	m_inputSource=NULL;
	m_tsFrames=NULL;
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

void CProgramToTransportStream::Initialize(CTsMuxerTsOutputPin* outputPin)
{
	LogDebug("CProgramToTransportStream::Initialize");
	m_buffer.Clear();


	m_inputSource = CMemoryStreamSource::createNew(*m_env, m_buffer);
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

	m_outputSink = CMemoryStreamSink::createNew(*m_env, outputPin);
	if (m_outputSink == NULL) 
	{
		*m_env << "Unable to open memory stream sink\n";
		return;
	}
	m_buffer.Clear();
	m_iPacketsToSkip=50;
	m_bStarting=true;
	m_bRunning=true;
}

void CProgramToTransportStream::Flush()
{
	LogDebug("CProgramToTransportStream::Flush()");
	m_iPacketsToSkip=10;
	m_buffer.Clear();
}

void CProgramToTransportStream::ClearStreams()
{
	LogDebug("CProgramToTransportStream::ClearStreams()");
	//m_outputSink->ClearStreams();
}

void CProgramToTransportStream::Write(byte* data, int len)
{ 
	if (m_bRunning)
	{
		if (m_iPacketsToSkip>0) 
		{
			m_iPacketsToSkip--;
			return;
		}
		m_buffer.PutBuffer(data, len, 1);
		if (m_bStarting && m_buffer.Size()>300000)
		{
			m_bStarting=false;
			if (m_outputSink->startPlaying(*m_tsFrames, afterPlaying, m_tsFrames)==True)
			{
				LogDebug("CProgramToTransportStream::Output sink started");
			}
			else
			{
				LogDebug("CProgramToTransportStream::Failed to start output sink");
			}
		}
		while (m_buffer.Size()>300000)
		{
			m_env->taskScheduler().doEventLoop(); 
		}
	}
}

void CProgramToTransportStream::Close()
{
	LogDebug("CProgramToTransportStream::Close()");
	m_bRunning=false;
	m_buffer.Stop();

	if (m_outputSink!=NULL)
		Medium::close(m_outputSink);
	if (m_inputSource!=NULL)
		Medium::close(m_inputSource);

	if (m_tsFrames!=NULL)
		Medium::close(m_tsFrames);
	m_outputSink=NULL;
	m_inputSource=NULL;
	m_tsFrames=NULL;
	LogDebug("CProgramToTransportStream::Close finished");

}
