// Copyright (C) 2005-2012 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#include "stdafx.h"
#include "Globals.h"
#include "AC3EncoderFilter.h"

#include "alloctracing.h"

#define AC3_OUT_BUFFER_SIZE   ((AC3_MAX_COMP_FRAME_SIZE + AC3_BITSTREAM_OVERHEAD) * 2)
#define AC3_OUT_BUFFER_COUNT  20

template<class T> inline T odd2even(T x)
{
  return x&1 ? x + 1 : x;
}

CAC3EncoderFilter::CAC3EncoderFilter(AudioRendererSettings* pSettings) : 
  CBaseAudioSink(true),
  m_pSettings(pSettings),
  m_bPassThrough(false),
  m_cbRemainingInput(0),
  m_pRemainingInput(NULL),
  m_nFrameSize(AC3_FRAME_LENGTH * AC3_MAX_CHANNELS * 2),
  m_pEncoder(NULL),
  m_nBitRate(448000),
  m_rtInSampleTime(0),
  m_rtNextIncomingSampleTime(0),
  m_nMaxCompressedAC3FrameSize(AC3_MAX_COMP_FRAME_SIZE)
{
}

CAC3EncoderFilter::~CAC3EncoderFilter()
{
  CloseAC3Encoder(); // just in case
  SAFE_DELETE_ARRAY(m_pRemainingInput);
}

HRESULT CAC3EncoderFilter::Init()
{
  HRESULT hr = InitAllocator();
  if (FAILED(hr))
    return hr;

  return CBaseAudioSink::Init();
}

HRESULT CAC3EncoderFilter::Cleanup()
{
  return CBaseAudioSink::Cleanup();
}

// Format negotiation
HRESULT CAC3EncoderFilter::NegotiateFormat(const WAVEFORMATEXTENSIBLE* pwfx, int nApplyChangesDepth, ChannelOrder* pChOrder)
{
  if (!pwfx)
    return VFW_E_TYPE_NOT_ACCEPTED;

  if (FormatsEqual(pwfx, m_pInputFormat))
  {
    *pChOrder = m_chOrder;
    return S_OK;
  }

  if (!m_pNextSink)
    return VFW_E_TYPE_NOT_ACCEPTED;

  // Try passthrough
  bool bApplyChanges = (nApplyChangesDepth !=0 );
  if (nApplyChangesDepth != INFINITE && nApplyChangesDepth > 0)
    nApplyChangesDepth--;

  HRESULT hr = VFW_E_TYPE_NOT_ACCEPTED;

  // If AC3 encoding is not forced check first if the audio format can be played without 
  // encoding (for example 2 channel on SPDIF or channel mixer downmixing the channels)
  if (m_pSettings->m_lAC3Encoding == DISABLED || m_pSettings->m_lAC3Encoding == AUTO)
  {
    hr = m_pNextSink->NegotiateFormat(pwfx, nApplyChangesDepth, pChOrder);
    if (SUCCEEDED(hr))
    {
      if (bApplyChanges)
      {
        m_bPassThrough = true;
        SAFE_DELETE_ARRAY(m_pRemainingInput);
        SetInputFormat(pwfx);
        SetOutputFormat(pwfx);
        CloseAC3Encoder();
      }

      m_chOrder = *pChOrder;
      return hr;
    }
  }

  if (m_pSettings->m_lAC3Encoding == DISABLED)
    return VFW_E_TYPE_NOT_ACCEPTED;

  // Verify input format bit depth, channels and sample rate
  if (pwfx->Format.nChannels > 6 || pwfx->Format.wBitsPerSample != 16 ||
      (pwfx->Format.nSamplesPerSec != 48000 && pwfx->Format.nSamplesPerSec != 44100))
    return VFW_E_TYPE_NOT_ACCEPTED;

  // Verify input format is PCM
  if (pwfx->Format.wFormatTag == WAVE_FORMAT_EXTENSIBLE && 
      pwfx->Format.cbSize >= sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX))
  {
    if (((WAVEFORMATEXTENSIBLE *)pwfx)->SubFormat != KSDATAFORMAT_SUBTYPE_PCM)
      return VFW_E_TYPE_NOT_ACCEPTED;
  }
  else if (pwfx->Format.wFormatTag != WAVE_FORMAT_PCM)
    return VFW_E_TYPE_NOT_ACCEPTED;

  // Finally verify next sink accepts AC3 format
  WAVEFORMATEXTENSIBLE* pAC3wfx = CreateAC3Format(pwfx->Format.nSamplesPerSec, 0);

  hr = m_pNextSink->NegotiateFormat(pAC3wfx, nApplyChangesDepth, pChOrder);

  if (FAILED(hr))
  {
    SAFE_DELETE_WAVEFORMATEX(pAC3wfx);
    return hr;
  }

  if (bApplyChanges)
  {
    LogWaveFormat(pwfx, "AC3  - applying ");
    
    m_bPassThrough = false;
    SAFE_DELETE_ARRAY(m_pRemainingInput);
    SetInputFormat(pwfx);
    SetOutputFormat(pAC3wfx, true);
    m_nFrameSize = m_pInputFormat->Format.nChannels * AC3_FRAME_LENGTH * 2;
    m_pRemainingInput = new BYTE[m_nFrameSize];
    m_nMaxCompressedAC3FrameSize = (m_nBitRate/8 * AC3_FRAME_LENGTH + m_pInputFormat->Format.nSamplesPerSec-1) / m_pInputFormat->Format.nSamplesPerSec;
    
    if (m_nMaxCompressedAC3FrameSize & 1)
      m_nMaxCompressedAC3FrameSize++;

    OpenAC3Encoder(m_nBitRate, m_pInputFormat->Format.nChannels, m_pInputFormat->Format.nSamplesPerSec);
  }
  else
    LogWaveFormat(pwfx, "AC3  -          ");

  *pChOrder = AC3_ORDER;

  return S_OK;
}

// Processing
HRESULT CAC3EncoderFilter::PutSample(IMediaSample *pSample)
{
  if (!pSample)
    return S_OK;

  AM_MEDIA_TYPE* pmt = NULL;
  bool bFormatChanged = false;
  
  HRESULT hr = S_OK;

  CAutoLock lock (&m_csOutputSample);
  if (m_bFlushing)
    return S_OK;

  if (SUCCEEDED(pSample->GetMediaType(&pmt)) && pmt)
    bFormatChanged = !FormatsEqual((WAVEFORMATEXTENSIBLE*)pmt->pbFormat, m_pInputFormat);

  if (bFormatChanged)
  {
    // Process any remaining input
    if (!m_bPassThrough)
    {
      hr = ProcessAC3Data(NULL, 0, NULL);
      if (FAILED(hr))
        Log("AC3Encoder - Failed to process remaining data on format change: 0x%08x");
    }

    // Apply format change locally, 
    // next filter will evaluate the format change when it receives the sample
    ChannelOrder chOrder;
    hr = NegotiateFormat((WAVEFORMATEXTENSIBLE*)pmt->pbFormat, 1, &chOrder);
    if (FAILED(hr))
    {
      DeleteMediaType(pmt);
      Log("AC3Encoder: PutSample failed to change format: 0x%08x", hr);
      return hr;
    }
	m_chOrder = chOrder;
  }

  if (pmt)
    DeleteMediaType(pmt);

  if (m_bPassThrough)
  {
    if (m_pNextSink)
      return m_pNextSink->PutSample(pSample);
    return S_OK; // perhaps we should return S_FALSE to indicate sample was dropped
  }

  long nOffset = 0;
  long cbSampleData = pSample->GetActualDataLength();
  BYTE *pData = NULL;
  REFERENCE_TIME rtStop = 0;
  REFERENCE_TIME rtStart = 0;
  pSample->GetTime(&rtStart, &rtStop);

  // Detect discontinuity in stream timeline
  if ((abs(m_rtNextIncomingSampleTime - rtStart) > MAX_SAMPLE_TIME_ERROR) && m_nSampleNum != 0)
  {
    Log("AC3Encoder - stream discontinuity: %6.3f", (rtStart - m_rtNextIncomingSampleTime) / 10000000.0);

    m_rtInSampleTime = rtStart;

    if (m_nSampleNum > 0)
    {
      Log("AC3Encoder - using buffered sample data");
      ProcessAC3Data(NULL, 0, NULL);
    }
    else
      Log("AC3Encoder - discarding buffered sample data");
  }

  if (m_nSampleNum == 0)
    m_rtInSampleTime = rtStart;

  UINT nFrames = cbSampleData / m_pInputFormat->Format.nBlockAlign;
  REFERENCE_TIME duration = nFrames * UNITS / m_pOutputFormat->Format.nSamplesPerSec;

  m_rtNextIncomingSampleTime = rtStart + duration;
  m_nSampleNum++;

  hr = pSample->GetPointer(&pData);
  ASSERT(pData);

  while (nOffset < cbSampleData && SUCCEEDED(hr))
  {
    long cbProcessed = 0;
    hr = ProcessAC3Data(pData+nOffset, cbSampleData - nOffset, &cbProcessed);
    nOffset += cbProcessed;
  }
  return hr;
}

HRESULT CAC3EncoderFilter::EndOfStream()
{
  if (!m_bPassThrough)
    ProcessAC3Data(NULL, 0, NULL);

  return CBaseAudioSink::EndOfStream();
}

HRESULT CAC3EncoderFilter::BeginFlush()
{ 
  // locking?
  m_cbRemainingInput = 0;

  return CBaseAudioSink::BeginFlush();
}

HRESULT CAC3EncoderFilter::EndFlush()
{
  return CBaseAudioSink::EndFlush();
}

HRESULT CAC3EncoderFilter::OnInitAllocatorProperties(ALLOCATOR_PROPERTIES *properties)
{
  properties->cBuffers = AC3_OUT_BUFFER_COUNT;
  properties->cbBuffer = AC3_OUT_BUFFER_SIZE;
  properties->cbPrefix = 0;
  properties->cbAlign = 8;

  return S_OK;
}

WAVEFORMATEXTENSIBLE* CAC3EncoderFilter::CreateAC3Format(int nSamplesPerSec, int nAC3BitRate)
{
  WAVEFORMATEXTENSIBLE* pwfx = (WAVEFORMATEXTENSIBLE*)new BYTE[sizeof(WAVEFORMATEXTENSIBLE)];
  if (pwfx)
  {
    // SPDIF uses static 2 channels and 16 bit. 
    // AC3 header contains the real stream information
    pwfx->Format.wFormatTag = WAVE_FORMAT_DOLBY_AC3_SPDIF;
    pwfx->Format.wBitsPerSample = 16;
    pwfx->Format.nBlockAlign = 4;
    pwfx->Format.nChannels = 2;
    pwfx->Format.nSamplesPerSec = nSamplesPerSec;
    pwfx->Format.nAvgBytesPerSec = pwfx->Format.nSamplesPerSec * pwfx->Format.nBlockAlign;
    pwfx->Format.cbSize = 22;
    pwfx->SubFormat = KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL;
    pwfx->dwChannelMask = SPEAKER_FRONT_LEFT | SPEAKER_FRONT_RIGHT;
  }
  return pwfx;
}

long CAC3EncoderFilter::CreateAC3Bitstream(void* buf, size_t size, BYTE* pDataOut)
{
  size_t length = AC3_DATA_BURST_LENGTH;

  // IEC 61936 structure writing (HDMI bitstream, SPDIF)
  DWORD type = 0x0001; // CODEC_ID_SPDIF_AC3
  short subDataType = 0; 
  short errorFlag = 0;
  short datatypeInfo = 0;
  short bitstreamNumber = 0;
  
  DWORD Pc=type | (subDataType << 5) | (errorFlag << 7) | (datatypeInfo << 8) | (bitstreamNumber << 13);

  WORD *pDataOutW=(WORD*)pDataOut; // Header is filled with words instead of bytes

  // Preamble : 16 bytes for AC3/DTS, 8 bytes for other formats
  int index = 0;
  pDataOutW[0] = pDataOutW[1] = pDataOutW[2] = pDataOutW[3] = 0; // Stuffing at the beginning, not sure if this is useful
  index = 4; // First additional four words filled with 0 only for backward compatibility for AC3/DTS

  // Fill after the input buffer with zeros if any extra bytes
  if (length > 8 + index * 2 + size)
  {
    // Fill the output buffer with zeros 
    memset(pDataOut + 8 + index * 2 + size, 0, length - 8 - index * 2 - size); 
  }

  // Fill the 8 bytes (4 words) of IEC header
  pDataOutW[index++] = 0xf872;
  pDataOutW[index++] = 0x4e1f;
  pDataOutW[index++] = (WORD)Pc;
  pDataOutW[index++] = WORD(size * 8); // size in bits for AC3/DTS

  // Data : swap bytes from first byte of data on size length (input buffer lentgh)
  _swab((char*)buf,(char*)&pDataOutW[index],(int)(size & ~1));
  if (size & 1) // _swab doesn't like odd number.
  {
    pDataOut[index * 2 + size] = ((BYTE*)buf)[size - 1];
    pDataOut[index * 2 - 1 + size] = 0;
  }

  return length;
}

HRESULT CAC3EncoderFilter::OpenAC3Encoder(unsigned int bitrate, unsigned int channels, unsigned int sampleRate)
{
  CloseAC3Encoder();
  Log("OpenEncoder - Creating AC3 encoder - bitrate: %d sampleRate: %d channels: %d", bitrate, sampleRate, channels);

  m_pEncoder = ac3_encoder_open();
  if (!m_pEncoder) return S_FALSE;

  m_pEncoder->bit_rate = bitrate;
  m_pEncoder->sample_rate = sampleRate;
  m_pEncoder->channels = channels;

  if (ac3_encoder_init(m_pEncoder) < 0) 
  {
    ac3_encoder_close(m_pEncoder);
    m_pEncoder = NULL;
  }
	
  return S_OK;
}

HRESULT CAC3EncoderFilter::CloseAC3Encoder()
{
  if (!m_pEncoder) return S_FALSE;

  Log("CloseEncoder - Closing AC3 encoder");

  ac3_encoder_close(m_pEncoder);
  m_pEncoder = NULL;

  return S_OK;
}

HRESULT CAC3EncoderFilter::ProcessAC3Data(const BYTE *pData, long cbData, long *pcbDataProcessed)
{
  HRESULT hr = S_OK;

  if (!pData) // need to flush any existing data
  {
    if (m_pNextOutSample)
    {
      long nOffset = m_pNextOutSample->GetActualDataLength();
      long nSize = m_pNextOutSample->GetSize();
      if (nOffset + AC3_DATA_BURST_LENGTH > nSize)
        hr = OutputNextSample();
    }

    if (m_cbRemainingInput)
    {
      REFERENCE_TIME rtStart = m_rtInSampleTime - (m_cbRemainingInput * UNITS / m_pOutputFormat->Format.nAvgBytesPerSec);
      if(!m_pNextOutSample && FAILED(hr = RequestNextOutBuffer(rtStart)))
      {
        m_cbRemainingInput = 0;
        if (pcbDataProcessed)
          *pcbDataProcessed = 0;
        return hr;
      }
      
      // TODO: Handle channel reorder
      ASSERT(m_pRemainingInput != NULL);
      memset(m_pRemainingInput + m_cbRemainingInput, 0, m_nFrameSize - m_cbRemainingInput);

      hr = ProcessAC3Frame(m_pRemainingInput);
      m_cbRemainingInput = 0;
      // Flush any output samples too
      if (SUCCEEDED(hr) && m_pNextOutSample)
      {
        hr = OutputNextSample();
        if (FAILED(hr))
          Log("AC3Encoder::ProcessAC3Data OutputNextSample failed with: 0x%08x", hr);
      }
    }
    if (pcbDataProcessed)
      *pcbDataProcessed = 0;
    return hr;
  }

  long bytesOutput = 0;

  while (cbData)
  {
    // do we have enough data for a frame?
    if (cbData + m_cbRemainingInput < m_nFrameSize)
    {
      // TODO: Handle channel reorder
      // no, just keep remaining data in a buffer for next time
      memcpy(m_pRemainingInput + m_cbRemainingInput, pData, cbData);
      m_cbRemainingInput += cbData;
      if (pcbDataProcessed)
        *pcbDataProcessed = bytesOutput + cbData;
      return hr;
    }

    if (m_pNextOutSample)
    {
      // if there is not enough space in output sample, flush it
      long nOffset = m_pNextOutSample->GetActualDataLength();
      long nSize = m_pNextOutSample->GetSize();
      if (nOffset + AC3_DATA_BURST_LENGTH > nSize)
      {
        hr = OutputNextSample();

        UINT nFrames = nOffset / m_pOutputFormat->Format.nBlockAlign;
        m_rtInSampleTime += nFrames * UNITS / m_pOutputFormat->Format.nSamplesPerSec;

        if (FAILED(hr))
        {
          Log("AC3Encoder::ProcessAC3Data OutputNextSample failed with: 0x%08x", hr);
          return hr;
        }
      }
    }

    // try to get an output buffer if none available
    if (!m_pNextOutSample && FAILED(hr = RequestNextOutBuffer(m_rtInSampleTime)))
    {
      if (pcbDataProcessed)
        *pcbDataProcessed = bytesOutput + cbData;
      return hr;
    }

    if (m_cbRemainingInput)
    {
      // TODO: Handle channel reorder
      // we have left-overs from previous calls
      int bytesToCopy = m_nFrameSize - m_cbRemainingInput;
      memcpy(m_pRemainingInput + m_cbRemainingInput, pData, bytesToCopy);
      hr = ProcessAC3Frame(m_pRemainingInput);
      pData += bytesToCopy;
      cbData -= bytesToCopy; 
      bytesOutput += bytesToCopy;
      m_cbRemainingInput = 0;
    }
    else
    {
      // TODO: Handle channel reorder
      hr = ProcessAC3Frame(pData);
      pData += m_nFrameSize;
      cbData -= m_nFrameSize; 
      bytesOutput += m_nFrameSize;
    }
  }
  
  if (pcbDataProcessed)
    *pcbDataProcessed = bytesOutput;
  return hr;
}

HRESULT CAC3EncoderFilter::ProcessAC3Frame(const BYTE* pData)
{
  HRESULT hr;
  long nOffset = m_pNextOutSample->GetActualDataLength();
  long nSize = m_pNextOutSample->GetSize();
  BYTE* pOutData = NULL;

  if (FAILED(hr = m_pNextOutSample->GetPointer(&pOutData)))
  {
    Log("AC3Encoder: Failed to get output buffer pointer: 0x%08x", hr);
    return hr;
  }

  ASSERT(pOutData);
  BYTE* buf = (BYTE*)malloc(m_nMaxCompressedAC3FrameSize); // temporary buffer

  int AC3length = ac3_encoder_frame(m_pEncoder, (short*)pData, buf, m_nMaxCompressedAC3FrameSize);
  nOffset += CreateAC3Bitstream(buf, AC3length, pOutData + nOffset);
  m_pNextOutSample->SetActualDataLength(nOffset);
  if (nOffset >= nSize)
    OutputNextSample();

  return S_OK;
}

