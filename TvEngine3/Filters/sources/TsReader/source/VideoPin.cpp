/* 
 *	Copyright (C) 2005 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#include <streams.h>
#include "tsreader.h"
#include "AudioPin.h"
#include "Videopin.h"

BYTE g_Mpeg2ProgramVideo[]= {
      0x00, 0x00, 0x00, 0x00,							//  .hdr.rcSource.left
      0x00, 0x00, 0x00, 0x00,							//  .hdr.rcSource.top
      0xd0, 0x02, 0x00, 0x00,							//  .hdr.rcSource.right
      0x40, 0x02, 0x00, 0x00,							//  .hdr.rcSource.bottom
      0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.left
      0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.top
      0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.right
      0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.bottom
      0xc0, 0xe1, 0xe4, 0x00,							//  .hdr.dwBitRate
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwBitErrorRate
      0x80, 0x1a, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //  .hdr.AvgTimePerFrame
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwInterlaceFlags
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwCopyProtectFlags
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwPictAspectRatioX
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwPictAspectRatioY
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwReserved1
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwReserved2
      0x28, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biSize
      0xd0, 0x02, 0x00, 0x00,							//  .hdr.bmiHeader.biWidth
      0x40, 0x02, 0x00, 0x00,							//  .hdr.bmiHeader.biHeight
      0x00, 0x00,										//  .hdr.bmiHeader.biPlanes
      0x00, 0x00,										//  .hdr.bmiHeader.biBitCount
      0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biCompression
      0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biSizeImage
      0xd0, 0x07, 0x00, 0x00,							//  .hdr.bmiHeader.biXPelsPerMeter
      0x42, 0xd8, 0x00, 0x00,							//  .hdr.bmiHeader.biYPelsPerMeter
      0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biClrUsed
      0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biClrImportant
      0x00, 0x00, 0x00, 0x00,							//  .dwStartTimeCode
      0x4c, 0x00, 0x00, 0x00,							//  .cbSequenceHeader
      0x00, 0x00, 0x00, 0x00,							//  .dwProfile
      0x00, 0x00, 0x00, 0x00,							//  .dwLevel
      0x00, 0x00, 0x00, 0x00,							//  .Flags
			                        //  .dwSequenceHeader [1]
      0x00, 0x00, 0x01, 0xb3, 0x2d, 0x02, 0x40, 0x33, 
      0x24, 0x9f, 0x23, 0x81, 0x10, 0x11, 0x11, 0x12, 
      0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14, 
      0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15, 
      0x15, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 
      0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 
      0x18, 0x18, 0x18, 0x19, 0x18, 0x18, 0x18, 0x19, 
      0x1a, 0x1a, 0x1a, 0x1a, 0x19, 0x1b, 0x1b, 0x1b, 
      0x1b, 0x1b, 0x1c, 0x1c, 0x1c, 0x1c, 0x1e, 0x1e, 
      0x1e, 0x1f, 0x1f, 0x21, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
};

extern void LogDebug(const char *fmt, ...) ;

CVideoPin::CVideoPin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section) :
	CSourceStream(NAME("pinVideo"), phr, pFilter, L"Video"),
	m_pTsReaderFilter(pFilter),
	m_section(section)
{
	m_rtStart=0;
}

CVideoPin::~CVideoPin()
{
	LogDebug("pin:dtor()");
}
STDMETHODIMP CVideoPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
	if (riid == IID_IAsyncReader)
  {
		int x=1;
	}
  if (riid == IID_IMediaSeeking)
  {
		return GetInterface((IMediaSeeking*)this, ppv);
  }
  return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT CVideoPin::GetMediaType(CMediaType *pmt)
{

	pmt->InitMediaType();
	pmt->SetType      (& MEDIATYPE_Video);
	pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_VIDEO);
	pmt->SetFormatType(&FORMAT_MPEG2Video);
	pmt->SetSampleSize(1);
	pmt->SetTemporalCompression(FALSE);
	pmt->SetVariableSize();
	pmt->SetFormat(g_Mpeg2ProgramVideo,sizeof(g_Mpeg2ProgramVideo));

	return S_OK;
}

HRESULT CVideoPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
	HRESULT hr;


	CheckPointer(pAlloc, E_POINTER);
	CheckPointer(pRequest, E_POINTER);

	if (pRequest->cBuffers == 0)
	{
			pRequest->cBuffers = 2;
	}

	pRequest->cbBuffer = 0x10000;


	ALLOCATOR_PROPERTIES Actual;
	hr = pAlloc->SetProperties(pRequest, &Actual);
	if (FAILED(hr))
	{
			return hr;
	}

	if (Actual.cbBuffer < pRequest->cbBuffer)
	{
			return E_FAIL;
	}

	return S_OK;
}

HRESULT CVideoPin::CompleteConnect(IPin *pReceivePin)
{
	LogDebug("pin:CompleteConnect()");
	HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
	if (SUCCEEDED(hr))
	{
		LogDebug("pin:CompleteConnect() done");
	}
	else
	{
		LogDebug("pin:CompleteConnect() failed:%x",hr);
	}
	return hr;
}

HRESULT CVideoPin::FillBuffer(IMediaSample *pSample)
{
//	::OutputDebugStringA("CVideoPin::FillBuffer()\n");
	CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
	CBuffer* buffer=demux.GetVideo();
	if (buffer==NULL)
	{
		pSample->SetActualDataLength(0);
		::OutputDebugStringA("CVideoPin::FillBuffer() no video\n");
		return NOERROR;
	}
	byte* pSampleBuffer;
	HRESULT hr = pSample->GetPointer(&pSampleBuffer);
	if (FAILED(hr))
	{
		::OutputDebugStringA("CVideoPin::FillBuffer() invalid ptr\n");
		return hr;
	}

	pSample->SetActualDataLength(buffer->Length());
	memcpy(pSampleBuffer, buffer->Data(), buffer->Length());

	pSample->SetDiscontinuity(m_bDiscontinuity);

	CRefTime streamTime;
	m_pTsReaderFilter->StreamTime(streamTime);
	double dStreamTime=streamTime.Millisecs();
	double dStartTime= m_rtStart.Millisecs();

	if (buffer->Pts()>0)
	{
		long presentationTime=(long)(buffer->Pts() - m_pTsReaderFilter->GetStartTime());
		CRefTime referenceTime(presentationTime);
		CRefTime timeStamp=referenceTime - m_rtStart; 
		
		REFERENCE_TIME refTimeStamp=(REFERENCE_TIME)timeStamp;
		pSample->SetTime(&refTimeStamp,NULL);
		char buf[100];
		sprintf(buf,"  V: %05.2f %05.2f %05.2f %05.2f %d %d\n",
					(dStartTime/1000.0),
					(dStreamTime/1000.0), 
					(referenceTime.Millisecs()/1000.0),
					(timeStamp.Millisecs()/1000.0),
					demux.VideoPacketCount(), demux.AudioPacketCount());
		::OutputDebugString(buf);
	}
	delete buffer;

	m_bDiscontinuity=FALSE;
//	::OutputDebugStringA("CVideoPin::FillBuffer() done\n");
  return NOERROR;
}



HRESULT CVideoPin::OnThreadStartPlay()
{    
	::OutputDebugString("CVideoPin::OnThreadStartPlay()\n");
	REFERENCE_TIME pStop;
	double dRate;
	m_bDiscontinuity = TRUE;
	m_pTsReaderFilter->GetAudioPin()->GetStopPosition(&pStop);
	m_pTsReaderFilter->GetAudioPin()->GetRate(&dRate);
  return DeliverNewSegment(m_rtStart, pStop, dRate);
}
void CVideoPin::FlushOutput()
{
		::OutputDebugString("CAudioPin::FlushOutput()\n");
	if (ThreadExists()) 
  {
    DeliverBeginFlush();
    Stop();
    DeliverEndFlush();
    Run();
  }
}
void CVideoPin::SetStart(CRefTime rtStartTime)
{
	m_rtStart=rtStartTime;
	double startTime=m_rtStart/UNITS;
	char buf[100];
	sprintf(buf,"CVideoPin::SetStart %x %05.2f\n",(DWORD)m_rtStart,startTime);
	::OutputDebugString(buf);
	FlushOutput();
}

HRESULT CVideoPin::GetCapabilities(DWORD *pCapabilities)
{
	return m_pTsReaderFilter->GetAudioPin()->GetCapabilities(pCapabilities);
}
HRESULT CVideoPin::CheckCapabilities(DWORD *pCapabilities)
{
	return m_pTsReaderFilter->GetAudioPin()->CheckCapabilities(pCapabilities);
}
HRESULT CVideoPin::IsFormatSupported(const GUID *pFormat)
{
	return m_pTsReaderFilter->GetAudioPin()->IsFormatSupported(pFormat);
}
HRESULT CVideoPin::QueryPreferredFormat(GUID *pFormat)
{
	return m_pTsReaderFilter->GetAudioPin()->QueryPreferredFormat(pFormat);
}
HRESULT CVideoPin::GetTimeFormat(GUID *pFormat)
{
	return m_pTsReaderFilter->GetAudioPin()->GetTimeFormat(pFormat);
}
HRESULT CVideoPin::IsUsingTimeFormat(const GUID *pFormat)
{
	return m_pTsReaderFilter->GetAudioPin()->IsUsingTimeFormat(pFormat);
}
HRESULT CVideoPin::SetTimeFormat(const GUID *pFormat)
{
	return m_pTsReaderFilter->GetAudioPin()->SetTimeFormat(pFormat);
}
HRESULT CVideoPin::GetDuration(LONGLONG *pDuration)
{
	return m_pTsReaderFilter->GetAudioPin()->GetDuration(pDuration);
}
HRESULT CVideoPin::GetStopPosition(LONGLONG *pStop)
{
	return m_pTsReaderFilter->GetAudioPin()->GetStopPosition(pStop);
}
HRESULT CVideoPin::GetCurrentPosition(LONGLONG *pCurrent)
{
	return m_pTsReaderFilter->GetAudioPin()->GetCurrentPosition(pCurrent);
}
HRESULT CVideoPin::ConvertTimeFormat(LONGLONG *pTarget,const GUID *pTargetFormat,LONGLONG Source,const GUID *pSourceFormat)
{
	return m_pTsReaderFilter->GetAudioPin()->ConvertTimeFormat(pTarget,pTargetFormat,Source,pSourceFormat);
}
HRESULT CVideoPin::SetPositions( /* [out][in] */ LONGLONG *pCurrent,DWORD dwCurrentFlags,/* [out][in] */ LONGLONG *pStop,DWORD dwStopFlags)
{
	return S_OK;
}
HRESULT CVideoPin::GetPositions(LONGLONG *pCurrent,LONGLONG *pStop)
{
	return m_pTsReaderFilter->GetAudioPin()->GetPositions(pCurrent,pStop);
}
HRESULT CVideoPin::GetAvailable(LONGLONG *pEarliest,LONGLONG *pLatest)
{
	return m_pTsReaderFilter->GetAudioPin()->GetAvailable(pEarliest,pLatest);
}
HRESULT CVideoPin::SetRate( double dRate)
{
	return S_OK;
}
HRESULT CVideoPin::GetRate(double *pdRate)
{
	return m_pTsReaderFilter->GetAudioPin()->GetRate(pdRate);
}
HRESULT CVideoPin::GetPreroll(LONGLONG *pllPreroll)
{
	return m_pTsReaderFilter->GetAudioPin()->GetPreroll(pllPreroll);
}