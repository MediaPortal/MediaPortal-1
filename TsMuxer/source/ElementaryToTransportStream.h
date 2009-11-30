#pragma once

#include "livemedia.hh"
#include "BasicUsageEnvironment.hh"
#include "MemoryBuffer.h"
#include "MemoryStreamSource.h" 
#include "MemoryStreamSink.h" 
#include "TsOutputPin.h"

class CElementaryToTransportStream
{
public:
	void ClearStreams();
	void Flush();
	CElementaryToTransportStream(void);
	virtual ~CElementaryToTransportStream(void);
	void Initialize(CTsMuxerTsOutputPin* outputPin);
	void Close();
	void WriteVideo(byte* data, int len);
	void WriteAudio(byte* data, int len);

private:
	void Write();
	UsageEnvironment* m_env;
	CMemoryBuffer m_bufferVideo;
	CMemoryBuffer m_bufferAudio;
	CMemoryStreamSink* m_outputSink;
	CMemoryStreamSource* m_videoInputSource;
	CMemoryStreamSource* m_audioInputSource;
	MPEG2TransportStreamFromESSource* m_tsFrames;
	bool m_bRunning;
	bool m_bStarting;
	int  m_iPacketsToSkipVideo;
	int  m_iPacketsToSkipAudio;

};
