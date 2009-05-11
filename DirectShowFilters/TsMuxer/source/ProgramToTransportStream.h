#pragma once

#include "livemedia.hh"
#include "BasicUsageEnvironment.hh"
#include "MemoryBuffer.h"
#include "MemoryStreamSource.h" 
#include "MemoryStreamSink.h" 
#include "TsOutputPin.h"

class CProgramToTransportStream
{
public:
	void ClearStreams();
	void Flush();
	CProgramToTransportStream(void);
	virtual ~CProgramToTransportStream(void);
	void Initialize(CTsMuxerTsOutputPin* outputPin);
	void Close();
	void Write(byte* data, int len);

private:
	UsageEnvironment* m_env;
	CMemoryBuffer m_buffer;
	CMemoryStreamSink* m_outputSink;
	CMemoryStreamSource* m_inputSource;
	FramedSource* m_tsFrames;
	bool m_bRunning;
	bool m_bStarting;
	int  m_iPacketsToSkip;

};
