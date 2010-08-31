// Copyright (C) 2005-2010 Team MediaPortal
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
#include "AC3EncoderFilter.h"


#define AC3_FRAME_LENGTH  1536


extern HRESULT CopyWaveFormatEx(WAVEFORMATEX **dst, const WAVEFORMATEX *src);
extern void Log(const char *fmt, ...);

template<class T> inline T odd2even(T x)
{
  return x&1 ? x + 1 : x;
}


CAC3EncoderFilter::CAC3EncoderFilter(void)
: m_bPassThrough(false)
, m_pInputFormat(NULL)
, m_pOutputFormat(NULL)
{
}

CAC3EncoderFilter::~CAC3EncoderFilter(void)
{
  SAFE_DELETE_WAVEFORMATEX(m_pInputFormat);
  SAFE_DELETE_WAVEFORMATEX(m_pOutputFormat);
}


HRESULT CAC3EncoderFilter::Init()
{
  return CBaseAudioSink::Init();
}

HRESULT CAC3EncoderFilter::Cleanup()
{
  return CBaseAudioSink::Cleanup();
}

// Format negotiation
HRESULT CAC3EncoderFilter::NegotiateFormat(const WAVEFORMATEX *pwfx, int nApplyChangesDepth)
{
  if (pwfx == NULL)
    return VFW_E_TYPE_NOT_ACCEPTED;

  if (FormatsEqual(pwfx, m_pInputFormat))
    return S_OK;

  if (m_pNextSink == NULL)
    return VFW_E_TYPE_NOT_ACCEPTED;

  // try passthrough
  bool bApplyChanges = (nApplyChangesDepth !=0);
  if (nApplyChangesDepth != INFINITE)
    nApplyChangesDepth--;

  HRESULT hr = m_pNextSink->NegotiateFormat(pwfx, nApplyChangesDepth);
  if (SUCCEEDED(hr))
  {
    if (bApplyChanges)
    {
      m_bPassThrough = true;
      SAFE_DELETE_WAVEFORMATEX(m_pInputFormat);
      CopyWaveFormatEx(&m_pInputFormat, pwfx);
      SAFE_DELETE_WAVEFORMATEX(m_pOutputFormat);
      CopyWaveFormatEx(&m_pOutputFormat, pwfx);
    }
    return hr;
  }
  // verify input format bit depth, channels and sample rate
  if (pwfx->nChannels > 6 || pwfx->wBitsPerSample != 16 ||
      (pwfx->nSamplesPerSec != 48000 && pwfx->nSamplesPerSec != 44100))
    return VFW_E_TYPE_NOT_ACCEPTED;

  // verify input format is PCM
  if (pwfx->wFormatTag == WAVE_FORMAT_EXTENSIBLE && 
      pwfx->cbSize >= sizeof(WAVEFORMATEXTENSIBLE)- sizeof(WAVEFORMATEX))
  {
    if (((WAVEFORMATEXTENSIBLE *)pwfx)->SubFormat != KSDATAFORMAT_SUBTYPE_PCM)
      return VFW_E_TYPE_NOT_ACCEPTED;
  }
  else if (pwfx->wFormatTag != WAVE_FORMAT_PCM)
    return VFW_E_TYPE_NOT_ACCEPTED;

  // Finally verify next sink accepts AC3 format
  WAVEFORMATEX *pAC3wfx = CreateAC3Format(pwfx->nSamplesPerSec, 0);

  hr = m_pNextSink->NegotiateFormat(pAC3wfx, nApplyChangesDepth);

  if (FAILED(hr))
  {
    SAFE_DELETE_WAVEFORMATEX(pAC3wfx);
    return hr;
  }
  if (bApplyChanges)
  {
    m_bPassThrough = false;
    SAFE_DELETE_WAVEFORMATEX(m_pInputFormat);
    CopyWaveFormatEx(&m_pInputFormat, pwfx);
    SAFE_DELETE_WAVEFORMATEX(m_pOutputFormat);
    m_pOutputFormat = pAC3wfx;
  }
  return S_OK;
}


// Processing
HRESULT CAC3EncoderFilter::PutSample(IMediaSample *pSample)
{
  return S_FALSE;
}

HRESULT CAC3EncoderFilter::EndOfStream()
{
  return S_FALSE;
}

HRESULT CAC3EncoderFilter::BeginFlush()
{
  return S_FALSE;
}

HRESULT CAC3EncoderFilter::EndFlush()
{
  return S_FALSE;
}


bool CAC3EncoderFilter::FormatsEqual(const WAVEFORMATEX *pwfx1, const WAVEFORMATEX *pwfx2)
{
  if (pwfx1 == NULL && pwfx2 != NULL)
    return true;

  if (pwfx1 != NULL && pwfx2 == NULL)
    return true;

  if (pwfx1->wFormatTag != pwfx2->wFormatTag ||
      pwfx1->nChannels != pwfx2->nChannels ||
      pwfx1->wBitsPerSample != pwfx2->wBitsPerSample) // TODO : improve the checks
    return true;

  return false;
}

WAVEFORMATEX *CAC3EncoderFilter::CreateAC3Format(int nSamplesPerSec, int nAC3BitRate)
{
  WAVEFORMATEX* pwfx = (WAVEFORMATEX*)new BYTE[sizeof(WAVEFORMATEX)];
  if (pwfx)
  {
    // SPDIF uses static 2 channels and 16 bit. 
    // AC3 header contains the real stream information
    pwfx->wFormatTag = WAVE_FORMAT_DOLBY_AC3_SPDIF;
    pwfx->wBitsPerSample = 16;
    pwfx->nBlockAlign = 4;
    pwfx->nChannels = 2;
    pwfx->nSamplesPerSec = nSamplesPerSec;
    pwfx->nAvgBytesPerSec = pwfx->nSamplesPerSec * pwfx->nBlockAlign;
    pwfx->cbSize = 0;
  }
  return pwfx;
}



long CAC3EncoderFilter::CreateAC3Bitstream(void *buf, size_t size, BYTE *pDataOut)
{
  size_t length = 0;
  size_t repetition_burst = 0x800; // 2048 = AC3 

  unsigned int size2 = AC3_FRAME_LENGTH * 4;
  length = 0;

  // Add 4 more words (8 bytes) for AC3/DTS (for backward compatibility, should be *4 for other codecs)
  // AC3/DTS streams start with 8 blank bytes (why, don't know but let's going on with)
  while (length < odd2even(size) + sizeof(WORD) * 8)
    length += repetition_burst;

  while (length < size2)
  length += repetition_burst;

  if (length == 0) length = repetition_burst;

  // IEC 61936 structure writing (HDMI bitstream, SPDIF)
  DWORD type = 0x0001;
  short subDataType = 0; 
  short errorFlag = 0;
  short datatypeInfo = 0;
  short bitstreamNumber = 0;
  
  type=1; // CODEC_ID_SPDIF_AC3

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

