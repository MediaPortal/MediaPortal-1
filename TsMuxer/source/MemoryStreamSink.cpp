#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "MemoryStreamSink.h"
#include "GroupsockHelper.hh"

////////// CMemoryStreamSink //////////
extern void LogDebug(const char *fmt, ...) ;

CMemoryStreamSink::CMemoryStreamSink(UsageEnvironment& env, CTsMuxerTsOutputPin* outputPin,unsigned bufferSize)
: MediaSink(env),
m_pTsOutputPin(outputPin),
fBufferSize(bufferSize)
{
	LogDebug("CMemoryStreamSink::ctor");
	fBuffer = new unsigned char[bufferSize];
	m_bReEntrant=false;
	m_pWriteBuffer = new byte[SUBMIT_BUFFER_SIZE];
	m_iWriteBufferPos = 0;
}

CMemoryStreamSink::~CMemoryStreamSink() 
{
	LogDebug("CMemoryStreamSink::dtor");
	delete[] fBuffer;
	delete[] m_pWriteBuffer;
}

CMemoryStreamSink* CMemoryStreamSink::createNew(UsageEnvironment& env, CTsMuxerTsOutputPin* outputPin,unsigned bufferSize) 
{
	return new CMemoryStreamSink(env, outputPin,bufferSize);
}

Boolean CMemoryStreamSink::continuePlaying() 
{
	if (fSource == NULL) return False;

	fSource->getNextFrame(fBuffer, fBufferSize,afterGettingFrame, this,onSourceClosure, this);

	return True;
}

void CMemoryStreamSink::afterGettingFrame(void* clientData, unsigned frameSize,
										  unsigned /*numTruncatedBytes*/,
struct timeval presentationTime,
	unsigned /*durationInMicroseconds*/) {
		CMemoryStreamSink* sink = (CMemoryStreamSink*)clientData;
		sink->afterGettingFrame1(frameSize, presentationTime);
} 

static int testsize=0;
void CMemoryStreamSink::OnTsPacket(byte* tsPacket)
{

		int hr;
	if(m_pSample==NULL){
		hr = m_pTsOutputPin->GetDeliveryBuffer(&m_pSample, NULL, NULL, 0);
		if(hr != S_OK) return;
		m_pSample->SetDiscontinuity(FALSE);
		m_pSample->SetPreroll(FALSE);
		m_pSample->SetSyncPoint(TRUE);
		hr = m_pSample->GetPointer(&m_pWriteBuffer);
		if(hr != S_OK) return;
		m_iWriteBufferPos = 0;
	}
	if(m_iWriteBufferPos +TS_PACKET_LEN >= m_pSample->GetSize()){
		m_pSample->SetActualDataLength(m_iWriteBufferPos);
		m_pTsOutputPin->Deliver(m_pSample);
		hr = m_pTsOutputPin->GetDeliveryBuffer(&m_pSample, NULL, NULL, 0);
		if(hr != S_OK) return;
		m_pSample->SetDiscontinuity(FALSE);
		m_pSample->SetPreroll(FALSE);
		m_pSample->SetSyncPoint(TRUE);
		hr = m_pSample->GetPointer(&m_pWriteBuffer);
		if(hr != S_OK) return;
		m_iWriteBufferPos = 0;
	}
	memcpy(&m_pWriteBuffer[m_iWriteBufferPos],tsPacket,TS_PACKET_LEN);
	m_iWriteBufferPos += TS_PACKET_LEN;
/*
		byte* pSampleBuffer;
		IMediaSample* pSample;
		hr = m_pTsOutputPin->GetDeliveryBuffer(&pSample, NULL, NULL, 0);
		if(hr != S_OK) return;
		pSample->SetActualDataLength(TS_PACKET_LEN);
		pSample->SetDiscontinuity(FALSE);
		pSample->SetPreroll(FALSE);
		pSample->SetSyncPoint(TRUE);
		hr = pSample->GetPointer(&pSampleBuffer);
		if(hr != S_OK) return;
		
		memcpy(pSampleBuffer,tsPacket,TS_PACKET_LEN);
		pSample->AddRef();
		//m_pTsOutputPin->Deliver(pSample);
*/
}


void CMemoryStreamSink::afterGettingFrame1(unsigned frameSize,struct timeval presentationTime) 
{
	CAutoLock BufferLock(&m_BufferLock);

	OnRawData(fBuffer, frameSize);
	//addData(fBuffer, frameSize, presentationTime);
	// Then try getting the next frame:
	continuePlaying();
}
