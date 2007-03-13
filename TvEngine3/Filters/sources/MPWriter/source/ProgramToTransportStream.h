#pragma once

#include "livemedia.hh"
#include "BasicUsageEnvironment.hh"
#include "MemoryBuffer.h"
#include "TSThread.h"
#include "MultiWriterFileSink.h"
#include "MemoryStreamSource.h" 
class CProgramToTransportStream: public TSThread
{
public:
  CProgramToTransportStream(void);
  virtual ~CProgramToTransportStream(void);
  void Initialize(char* fileNameOut);
  void SetTimeShiftParams( int minFiles, int maxFiles, ULONG maxFileSize);
  void ClearStreams();
  void Close();
  void Write(byte* data, int len);

	void StartBufferThread();
	void StopBufferThread();
  void Flush();
	virtual void ThreadProc();
  bool m_BufferThreadActive;
  
private:
  UsageEnvironment* m_env;
  CMemoryBuffer m_buffer;
  CMultiWriterFileSink* m_outputSink;
  CMemoryStreamSource* m_inputSource;
  FramedSource* m_tsFrames;
  bool m_bRunning;
  bool m_bStarting;
  int  m_iPacketsToSkip;
  
  int m_minFiles;
  int m_maxFiles;
  ULONG m_maxFileSize;
};
