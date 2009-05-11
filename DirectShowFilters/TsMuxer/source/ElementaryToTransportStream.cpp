#include <streams.h>
#include "ElementaryToTransportStream.h"
#include "MemoryStreamSource.h"
extern void LogDebug(const char *fmt, ...) ;

CElementaryToTransportStream::CElementaryToTransportStream(void)
{
	m_bRunning=false;
	LogDebug("CElementaryToTransportStream::ctor");
	TaskScheduler* scheduler = BasicTaskScheduler::createNew();
	m_env = BasicUsageEnvironment::createNew(*scheduler);
	m_outputSink=NULL;
	m_videoInputSource=NULL;
	m_audioInputSource=NULL;
	m_tsFrames=NULL;
}

CElementaryToTransportStream::~CElementaryToTransportStream(void)
{
	LogDebug("CElementaryToTransportStream::dtor");
}

void afterPlayingElementary(void* clientData) 
{
	LogDebug("CElementaryToTransportStream afterPlaying");
	MPEG2TransportStreamFromESSource* outputSink=(MPEG2TransportStreamFromESSource*)clientData;
}
void CElementaryToTransportStream::Initialize(CTsMuxerTsOutputPin* outputPin)
{
	LogDebug("CElementaryToTransportStream::Initialize");
	m_bufferVideo.Clear();
	m_bufferAudio.Clear();


	m_videoInputSource = CMemoryStreamSource::createNew(*m_env, m_bufferVideo,65535,0);
	if (m_videoInputSource == NULL) 
	{
		*m_env << "Unable to open memorystream as a byte-stream source\n";
		return;
	}

	m_audioInputSource = CMemoryStreamSource::createNew(*m_env, m_bufferAudio,1008,0);
	if (m_audioInputSource == NULL) 
	{
		*m_env << "Unable to open memorystream as a byte-stream source\n";
		return;
	}
	// And, from this, a filter that converts to MPEG-2 Transport Stream frames:
	m_tsFrames  = MPEG2TransportStreamFromESSource::createNew(*m_env);
	m_tsFrames->addNewVideoSource(m_videoInputSource,2);
	m_tsFrames->addNewAudioSource(m_audioInputSource,2);

	m_outputSink = CMemoryStreamSink::createNew(*m_env, outputPin);
	if (m_outputSink == NULL) 
	{
		*m_env << "Unable to open memory stream sink\n";
		return;
	}
	m_bufferVideo.Clear();
	m_bufferAudio.Clear();
	m_iPacketsToSkipVideo=50;
	m_iPacketsToSkipAudio=50;
	//StartBufferThread();
	m_bStarting=true;
	m_bRunning=true;
}
void CElementaryToTransportStream::Flush()
{
	LogDebug("CElementaryToTransportStream::Flush()");
	m_iPacketsToSkipVideo=10;
	m_iPacketsToSkipAudio=10;
	m_bufferVideo.Clear();
	m_bufferAudio.Clear();
}
void CElementaryToTransportStream::ClearStreams()
{
	LogDebug("CElementaryToTransportStream::ClearStreams()");
	//m_outputSink->ClearStreams();
}
void CElementaryToTransportStream::WriteVideo(byte* data, int len)
{ 
	if (m_bRunning)
	{
		if(m_iPacketsToSkipVideo == 50){
			LogDebug("This is the video len: %d",len);
		}
		if (m_iPacketsToSkipVideo>0) 
		{
			m_iPacketsToSkipVideo--;
			// LogDebug("skip:%d",m_iPacketsToSkip);
			return;
		}
		if(m_bufferVideo.Size() + len <500000){
			m_bufferVideo.PutBuffer(data, len, 1);
		}else{
			//LogDebug("DROPPING VIDEO PACKETS");
		}
		Write();
	}
}

void CElementaryToTransportStream::WriteAudio(byte* data, int len)
{ 
	if (m_bRunning)
	{
		if(m_iPacketsToSkipAudio == 50){
			LogDebug("This is the audio len: %d",len);
		}
		if (m_iPacketsToSkipAudio>0) 
		{
			m_iPacketsToSkipAudio--;
			// LogDebug("skip:%d",m_iPacketsToSkip);
			return;
		}
		if(m_bufferAudio.Size() + len <500000){
			m_bufferAudio.PutBuffer(data, len, 1);
		}else{
			//LogDebug("DROPPING AUDIO PACKETS");
		}
		Write();
	}
}

void CElementaryToTransportStream::Write()
{ 
	if (m_bRunning)
	{
		if (m_bStarting && m_bufferVideo.Size() > 300000 && m_bufferAudio.Size() >10000 && m_iPacketsToSkipVideo == 0 && m_iPacketsToSkipAudio == 0 )
		{
			m_bStarting=false;
			if (m_outputSink->startPlaying(*m_tsFrames, afterPlayingElementary, m_tsFrames)==True)
			{
				LogDebug("CElementaryToTransportStream::Thread playing()");
			}
			else
			{
				LogDebug("CElementaryToTransportStream::Failed to start output sink");
			}
		}
		int retries = 0;
		while (m_bufferVideo.Size() > 300000 && m_bufferAudio.Size() >10000 && retries <5)
		{
			LogDebug("CElementaryToTransportStream:: Do eventloop");
			m_env->taskScheduler().doEventLoop(); 
			retries++;
		}
	}
}

void CElementaryToTransportStream::Close()
{
	LogDebug("CElementaryToTransportStream::Close()");
	m_bRunning=false;
	m_bufferVideo.Stop();
	m_bufferAudio.Stop();

	if (m_outputSink!=NULL)
		Medium::close(m_outputSink);
	if (m_videoInputSource!=NULL)
		Medium::close(m_videoInputSource);
	if (m_audioInputSource!=NULL)
		Medium::close(m_audioInputSource);

	
	if (m_tsFrames!=NULL)
		Medium::close(m_tsFrames);
	m_outputSink=NULL;
	m_videoInputSource=NULL;
	m_audioInputSource=NULL;
	m_tsFrames=NULL;
	LogDebug("CElementaryToTransportStream::Thread stopped()");

}
