#include <streams.h>
#include "ProgramToTransportStream.h"
#include "MemoryStreamSource.h"

CProgramToTransportStream::CProgramToTransportStream(void)
{
}

CProgramToTransportStream::~CProgramToTransportStream(void)
{
}

void afterPlaying(void* /*clientData*/) 
{
}
void CProgramToTransportStream::Initialize(char* fileNameOut)
{
  m_BufferThreadActive=false;
  m_buffer.Clear();
  TaskScheduler* scheduler = BasicTaskScheduler::createNew();
  m_env = BasicUsageEnvironment::createNew(*scheduler);
  
  
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
  m_outputSink = CMultiWriterFileSink::createNew(*m_env, fileNameOut);
  if (m_outputSink == NULL) 
  {
    *m_env << "Unable to open file \"" << fileNameOut << "\" as a file sink\n";
    return;
  }
  StartBufferThread();
}

void CProgramToTransportStream::ClearStreams()
{
  m_outputSink->ClearStreams();
}
void CProgramToTransportStream::Write(byte* data, int len)
{
  m_buffer.PutBuffer(data, len, 1);
}

void CProgramToTransportStream::Close()
{
  m_buffer.Stop();
  StopBufferThread();
}


void CProgramToTransportStream::StartBufferThread()
{
  m_buffer.Clear();
  if (!m_BufferThreadActive)
	{
		StartThread();
		m_BufferThreadActive = true;
	}
}

void CProgramToTransportStream::StopBufferThread()
{
	if (!m_BufferThreadActive)
		return;

	StopThread(5000);

	m_BufferThreadActive = false;
}
void CProgramToTransportStream::ThreadProc()
{
	HRESULT hr = S_OK;
	m_BufferThreadActive = TRUE;

//	BoostThread Boost;

	//::SetThreadPriority(GetCurrentThread(),THREAD_PRIORITY_TIME_CRITICAL);
  *m_env << "output thread started:" << "\"\n";
    
  m_outputSink->startPlaying(*m_tsFrames, afterPlaying, NULL);
  *m_env << "output thread playing:" << "\"\n";
	while (m_env!=NULL && !ThreadIsStopping(0))
	{
		m_env->taskScheduler().doEventLoop(); 
			
	}
  //m_outputSink->stopPlaying();
  //delete m_outputSink;
  m_outputSink=NULL;
  delete m_inputSource;
  m_inputSource=NULL;
  *m_env << "output thread stopped:" << "\"\n";
	m_BufferThreadActive = false;
	return;
}