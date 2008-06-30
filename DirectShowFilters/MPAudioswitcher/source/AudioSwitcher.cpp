/* 
 *	Copyright (C) 2003-2006 Gabest
 *	http://www.gabest.org
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

#include "StdAfx.h"
#include <windows.h>
#include <shlobj.h>
#include "Shlwapi.h"
#include <math.h>
#include <atlpath.h>
#include <mmreg.h>
#include <ks.h>
#include <ksmedia.h>
#include "AudioSwitcher.h"
#include "Audio.h"
#include "DSUtil.h"

#include <initguid.h>
#include "moreuuids.h"


#ifdef REGISTER_FILTER

const AMOVIESETUP_MEDIATYPE sudPinTypesIn[] =
{
	{&MEDIATYPE_Audio, &MEDIASUBTYPE_NULL}
};

const AMOVIESETUP_MEDIATYPE sudPinTypesOut[] =
{
	{&MEDIATYPE_Audio, &MEDIASUBTYPE_NULL}
};

const AMOVIESETUP_PIN sudpPins[] =
{
    {L"Input", FALSE, FALSE, FALSE, FALSE, &CLSID_NULL, NULL, countof(sudPinTypesIn), sudPinTypesIn},
    {L"Output", FALSE, TRUE, FALSE, FALSE, &CLSID_NULL, NULL, countof(sudPinTypesOut), sudPinTypesOut}
};

const AMOVIESETUP_FILTER sudFilter[] =
{
	{&__uuidof(CAudioSwitcherFilter), L"MediaPortal AudioSwitcher", MERIT_DO_NOT_USE, countof(sudpPins), sudpPins}
};

CFactoryTemplate g_Templates[] =
{
    {sudFilter[0].strName, sudFilter[0].clsID, CreateInstance<CAudioSwitcherFilter>, NULL, &sudFilter[0]}
};

int g_cTemplates = countof(g_Templates);

STDAPI DllRegisterServer()
{
	return AMovieDllRegisterServer2(TRUE);
}

STDAPI DllUnregisterServer()
{
	return AMovieDllRegisterServer2(FALSE);
}

#include "FilterApp.h"

CFilterApp theApp;

#endif

void GetLogFile(TCHAR *pLog)
{
	TCHAR folder[MAX_PATH];
  ::SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
  wsprintf(pLog,L"%s\\Team MediaPortal\\MediaPortal\\Log\\MPAudioSwitcher.log",folder);
}

void LogDebug(const char *fmt, ...) 
{
	va_list ap;
	va_start(ap,fmt);

	char buffer[1000]; 
	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(buffer, fmt, ap);
	va_end(ap); 
	SYSTEMTIME systemTime;
	GetLocalTime(&systemTime);

//#ifdef DONTLOG
  TCHAR filename[1024];
  GetLogFile(filename);
  FILE* fp = _wfopen(filename,L"a+");

	if (fp!=NULL)
	{
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%x]%s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			systemTime.wMilliseconds,
			GetCurrentThreadId(),
			buffer);
		fclose(fp);
  }
//#endif
	TCHAR buf[1000];
	wsprintf(buf,L"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
		systemTime.wDay, systemTime.wMonth, systemTime.wYear,
		systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
		buffer);
	::OutputDebugString(buf);
};

//
// CAudioSwitcherFilter
//

CAudioSwitcherFilter::CAudioSwitcherFilter(LPUNKNOWN lpunk, HRESULT* phr)
	: CStreamSwitcherFilter(lpunk, phr, __uuidof(this))
	, m_fCustomChannelMapping(false)
	, m_fDownSampleTo441(false)
	, m_rtAudioTimeShift(0)
	, m_rtNextStart(0)
	, m_rtNextStop(1)
	, m_fNormalize(false)
	, m_fNormalizeRecover(false)
	, m_boost(1)
	, m_sample_max(0.1f)
{
	memset(m_pSpeakerToChannelMap, 0, sizeof(m_pSpeakerToChannelMap));
  TCHAR filename[1024];
  GetLogFile(filename);
  ::DeleteFile(filename);
  LogDebug("-------------- v1.0.0 ----------------");
	if(phr)
	{
		if(FAILED(*phr)) return;
		else *phr = S_OK;
	}
}

STDMETHODIMP CAudioSwitcherFilter::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
	if (riid == __uuidof(IAudioSwitcherFilter)) 
		return GetInterface((IAudioSwitcherFilter*)this, ppv);
	if (riid == __uuidof(IMPAudioSwitcherFilter))
		return GetInterface((IMPAudioSwitcherFilter*)this, ppv);
	return __super::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT CAudioSwitcherFilter::CheckMediaType(const CMediaType* pmt)
{
	if(pmt->formattype == FORMAT_WaveFormatEx
	&& ((WAVEFORMATEX*)pmt->pbFormat)->nChannels > 2
	&& ((WAVEFORMATEX*)pmt->pbFormat)->wFormatTag != WAVE_FORMAT_EXTENSIBLE)
		return VFW_E_INVALIDMEDIATYPE; // stupid iviaudio tries to fool us

	return (pmt->majortype == MEDIATYPE_Audio
			&& pmt->formattype == FORMAT_WaveFormatEx
			&& (((WAVEFORMATEX*)pmt->pbFormat)->wBitsPerSample == 8
				|| ((WAVEFORMATEX*)pmt->pbFormat)->wBitsPerSample == 16
				|| ((WAVEFORMATEX*)pmt->pbFormat)->wBitsPerSample == 24
				|| ((WAVEFORMATEX*)pmt->pbFormat)->wBitsPerSample == 32)
			&& (((WAVEFORMATEX*)pmt->pbFormat)->wFormatTag == WAVE_FORMAT_PCM
				|| ((WAVEFORMATEX*)pmt->pbFormat)->wFormatTag == WAVE_FORMAT_IEEE_FLOAT
				|| ((WAVEFORMATEX*)pmt->pbFormat)->wFormatTag == WAVE_FORMAT_DOLBY_AC3_SPDIF
				|| ((WAVEFORMATEX*)pmt->pbFormat)->wFormatTag == WAVE_FORMAT_EXTENSIBLE))
		? S_OK
		: VFW_E_TYPE_NOT_ACCEPTED;
}

template<class T, class U, int Umin, int Umax> 
void mix(DWORD mask, int ch, int bps, BYTE* src, BYTE* dst)
{
	U sum = 0;

	for(int i = 0, j = min(18, ch); i < j; i++)
	{
		if(mask & (1<<i))
		{
			sum += *(T*)&src[bps*i];
		}
	}

	if(sum < Umin) sum = Umin;
	if(sum > Umax) sum = Umax;
	
	*(T*)dst = (T)sum;
}

template<> 
void mix<int, INT64, (-1<<24), (+1<<24)-1>(DWORD mask, int ch, int bps, BYTE* src, BYTE* dst)
{
	INT64 sum = 0;

	for(int i = 0, j = min(18, ch); i < j; i++)
	{
		if(mask & (1<<i))
		{
			int tmp;
			memcpy((BYTE*)&tmp+1, &src[bps*i], 3);
			sum += tmp >> 8;
		}
	}

	sum = min(max(sum, (-1<<24)), (+1<<24)-1);

	memcpy(dst, (BYTE*)&sum, 3);
}

template<class T>
T clamp(double s, T smin, T smax)
{
	if(s < -1) s = -1;
	else if(s > 1) s = 1;
	T t = (T)(s * smax);
	if(t < smin) t = smin;
	else if(t > smax) t = smax;
	return t;
}

HRESULT CAudioSwitcherFilter::Transform(IMediaSample* pIn, IMediaSample* pOut)
{
	CStreamSwitcherInputPin* pInPin = GetInputPin();
	CStreamSwitcherOutputPin* pOutPin = GetOutputPin();
	if(!pInPin || !pOutPin) 
		return __super::Transform(pIn, pOut);

	WAVEFORMATEX* wfe = (WAVEFORMATEX*)pInPin->CurrentMediaType().pbFormat;
	WAVEFORMATEX* wfeout = (WAVEFORMATEX*)pOutPin->CurrentMediaType().pbFormat;
	WAVEFORMATEXTENSIBLE* wfex = (WAVEFORMATEXTENSIBLE*)wfe;
	WAVEFORMATEXTENSIBLE* wfexout = (WAVEFORMATEXTENSIBLE*)wfeout;

	int bps = wfe->wBitsPerSample>>3;

	int len = pIn->GetActualDataLength() / (bps*wfe->nChannels);
	int lenout = len * wfeout->nSamplesPerSec / wfe->nSamplesPerSec;

	REFERENCE_TIME rtStart, rtStop;
	if(SUCCEEDED(pIn->GetTime(&rtStart, &rtStop)))
	{
		rtStart += m_rtAudioTimeShift;
		rtStop += m_rtAudioTimeShift;
		pOut->SetTime(&rtStart, &rtStop);

		m_rtNextStart = rtStart;
		m_rtNextStop = rtStop;
	}
	else
	{
		pOut->SetTime(&m_rtNextStart, &m_rtNextStop);
	}

	REFERENCE_TIME rtDur = 10000000i64*len/wfe->nSamplesPerSec;

	m_rtNextStart += rtDur;
	m_rtNextStop += rtDur;

	if(pIn->IsDiscontinuity() == S_OK)
	{
		m_sample_max = 0.1f;
	}

	WORD tag = wfe->wFormatTag;
	bool fPCM = tag == WAVE_FORMAT_PCM || tag == WAVE_FORMAT_EXTENSIBLE && wfex->SubFormat == KSDATAFORMAT_SUBTYPE_PCM;
	bool fFloat = tag == WAVE_FORMAT_IEEE_FLOAT || tag == WAVE_FORMAT_EXTENSIBLE && wfex->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT;
	if(!fPCM && !fFloat) return __super::Transform(pIn, pOut);

	BYTE* pDataIn = NULL;
	BYTE* pDataOut = NULL;

	HRESULT hr;
	if(FAILED(hr = pIn->GetPointer(&pDataIn))) return hr;
	if(FAILED(hr = pOut->GetPointer(&pDataOut))) return hr;

	if(!pDataIn || !pDataOut || len <= 0 || lenout <= 0) return S_FALSE;

	memset(pDataOut, 0, pOut->GetSize());

	if(m_fCustomChannelMapping)
	{
		if(m_chs[wfe->nChannels-1].GetCount() > 0)
		{
			for(int i = 0; i < wfeout->nChannels; i++)
			{
				DWORD mask = m_chs[wfe->nChannels-1][i].Channel;

				BYTE* src = pDataIn;
				BYTE* dst = &pDataOut[bps*i];

				int srcstep = bps*wfe->nChannels;
				int dststep = bps*wfeout->nChannels;

				if(fPCM && wfe->wBitsPerSample == 8)
				{
					for(int k = 0; k < len; k++, src += srcstep, dst += dststep)
					{
						mix<unsigned char, INT64, 0, UCHAR_MAX>(mask, wfe->nChannels, bps, src, dst);
					}
				}
				else if(fPCM && wfe->wBitsPerSample == 16)
				{
					for(int k = 0; k < len; k++, src += srcstep, dst += dststep)
					{
						mix<short, INT64, SHRT_MIN, SHRT_MAX>(mask, wfe->nChannels, bps, src, dst);
					}
				}
				else if(fPCM && wfe->wBitsPerSample == 24)
				{
					for(int k = 0; k < len; k++, src += srcstep, dst += dststep)
					{
						mix<int, INT64, (-1<<24), (+1<<24)-1>(mask, wfe->nChannels, bps, src, dst);
					}
				}
				else if(fPCM && wfe->wBitsPerSample == 32)
				{
					for(int k = 0; k < len; k++, src += srcstep, dst += dststep)
					{
						mix<int, __int64, INT_MIN, INT_MAX>(mask, wfe->nChannels, bps, src, dst);
					}
				}
				else if(fFloat && wfe->wBitsPerSample == 32)
				{
					for(int k = 0; k < len; k++, src += srcstep, dst += dststep)
					{
						mix<float, double, -1, 1>(mask, wfe->nChannels, bps, src, dst);
					}
				}
				else if(fFloat && wfe->wBitsPerSample == 64)
				{
					for(int k = 0; k < len; k++, src += srcstep, dst += dststep)
					{
						mix<double, double, -1, 1>(mask, wfe->nChannels, bps, src, dst);
					}
				}
			}
		}
		else
		{
			BYTE* pDataOut = NULL;
			HRESULT hr;
			if(FAILED(hr = pOut->GetPointer(&pDataOut)) || !pDataOut) return hr;
			memset(pDataOut, 0, pOut->GetSize());
		}
	}
	else
	{
		HRESULT hr;
		if(S_OK != (hr = __super::Transform(pIn, pOut)))
			return hr;
	}

	if(m_fDownSampleTo441
	&& wfe->nSamplesPerSec > 44100 && wfeout->nSamplesPerSec == 44100 
	&& wfe->wBitsPerSample <= 16 && fPCM)
	{
		if(BYTE* buff = new BYTE[len*bps])
		{
			for(int ch = 0; ch < wfeout->nChannels; ch++)
			{
				memset(buff, 0, len*bps);

				for(int i = 0; i < len; i++)
					memcpy(buff + i*bps, (char*)pDataOut + (ch + i*wfeout->nChannels)*bps, bps);

				m_pResamplers[ch]->Downsample(buff, len, buff, lenout);

				for(int i = 0; i < lenout; i++)
					memcpy((char*)pDataOut + (ch + i*wfeout->nChannels)*bps, buff + i*bps, bps);
			}

			delete [] buff;
		}
	}

	if(m_fNormalize || m_boost > 1)
	{
		int samples = lenout*wfeout->nChannels;

		if(double* buff = new double[samples])
		{
			for(int i = 0; i < samples; i++)
			{
				if(fPCM && wfe->wBitsPerSample == 8) buff[i] = (double)((BYTE*)pDataOut)[i] / UCHAR_MAX;
				else if(fPCM && wfe->wBitsPerSample == 16) buff[i] = (double)((short*)pDataOut)[i] / SHRT_MAX;
				else if(fPCM && wfe->wBitsPerSample == 24) {int tmp; memcpy(((BYTE*)&tmp)+1, &pDataOut[i*3], 3); buff[i] = (float)(tmp >> 8) / ((1<<23)-1);}
				else if(fPCM && wfe->wBitsPerSample == 32) buff[i] = (double)((int*)pDataOut)[i] / INT_MAX;
				else if(fFloat && wfe->wBitsPerSample == 32) buff[i] = (double)((float*)pDataOut)[i];
				else if(fFloat && wfe->wBitsPerSample == 64) buff[i] = ((double*)pDataOut)[i];
			}

			double sample_mul = 1;

			if(m_fNormalize)
			{
				for(int i = 0; i < samples; i++)
				{
					double s = buff[i];
					if(s < 0) s = -s;
					if(s > 1) s = 1;
					if(m_sample_max < s) m_sample_max = s;
				}

				sample_mul = 1.0f / m_sample_max;

				if(m_fNormalizeRecover) m_sample_max -= 1.0*rtDur/200000000; // -5%/sec
				if(m_sample_max < 0.1) m_sample_max = 0.1;
			}

			if(m_boost > 1)
			{
				sample_mul *= (1+log10(m_boost));
			}

			for(int i = 0; i < samples; i++)
			{
				double s = buff[i] * sample_mul;

				if(fPCM && wfe->wBitsPerSample == 8) ((BYTE*)pDataOut)[i] = clamp<BYTE>(s, 0, UCHAR_MAX);
				else if(fPCM && wfe->wBitsPerSample == 16) ((short*)pDataOut)[i] = clamp<short>(s, SHRT_MIN, SHRT_MAX);
				else if(fPCM && wfe->wBitsPerSample == 24)  {int tmp = clamp<int>(s, -1<<23, (1<<23)-1); memcpy(&pDataOut[i*3], &tmp, 3);}
				else if(fPCM && wfe->wBitsPerSample == 32) ((int*)pDataOut)[i] = clamp<int>(s, INT_MIN, INT_MAX);
				else if(fFloat && wfe->wBitsPerSample == 32) ((float*)pDataOut)[i] = clamp<float>(s, -1, +1);
				else if(fFloat && wfe->wBitsPerSample == 64) ((double*)pDataOut)[i] = clamp<double>(s, -1, +1);
			}

			delete buff;
		}
	}

	pOut->SetActualDataLength(lenout*bps*wfeout->nChannels);

	return S_OK;
}

CMediaType CAudioSwitcherFilter::CreateNewOutputMediaType(CMediaType mt, long& cbBuffer)
{
	CStreamSwitcherInputPin* pInPin = GetInputPin();
	CStreamSwitcherOutputPin* pOutPin = GetOutputPin();
	if(!pInPin || !pOutPin || ((WAVEFORMATEX*)mt.pbFormat)->wFormatTag == WAVE_FORMAT_DOLBY_AC3_SPDIF) 
		return __super::CreateNewOutputMediaType(mt, cbBuffer);

	WAVEFORMATEX* wfe = (WAVEFORMATEX*)pInPin->CurrentMediaType().pbFormat;

	if(m_fCustomChannelMapping)
	{
		m_chs[wfe->nChannels-1].RemoveAll();

		DWORD mask = DWORD((__int64(1)<<wfe->nChannels)-1);
		for(int i = 0; i < 18; i++)
		{
			if(m_pSpeakerToChannelMap[wfe->nChannels-1][i]&mask)
			{
				ChMap cm = {1<<i, m_pSpeakerToChannelMap[wfe->nChannels-1][i]};
				m_chs[wfe->nChannels-1].Add(cm);
			}
		}

		if(m_chs[wfe->nChannels-1].GetCount() > 0)
		{
			mt.ReallocFormatBuffer(sizeof(WAVEFORMATEXTENSIBLE));
			WAVEFORMATEXTENSIBLE* wfex = (WAVEFORMATEXTENSIBLE*)mt.pbFormat;
			wfex->Format.cbSize = sizeof(WAVEFORMATEXTENSIBLE)-sizeof(WAVEFORMATEX);
			wfex->Format.wFormatTag = WAVE_FORMAT_EXTENSIBLE;
			wfex->Samples.wValidBitsPerSample = wfe->wBitsPerSample;
			wfex->SubFormat = 
				wfe->wFormatTag == WAVE_FORMAT_PCM ? KSDATAFORMAT_SUBTYPE_PCM :
				wfe->wFormatTag == WAVE_FORMAT_IEEE_FLOAT ? KSDATAFORMAT_SUBTYPE_IEEE_FLOAT :
				wfe->wFormatTag == WAVE_FORMAT_EXTENSIBLE ? ((WAVEFORMATEXTENSIBLE*)wfe)->SubFormat :
				KSDATAFORMAT_SUBTYPE_PCM; // can't happen

			wfex->dwChannelMask = 0;
			for(int i = 0; i < m_chs[wfe->nChannels-1].GetCount(); i++)
				wfex->dwChannelMask |= m_chs[wfe->nChannels-1][i].Speaker;

			wfex->Format.nChannels = (WORD)m_chs[wfe->nChannels-1].GetCount();
			wfex->Format.nBlockAlign = wfex->Format.nChannels*wfex->Format.wBitsPerSample>>3;
			wfex->Format.nAvgBytesPerSec = wfex->Format.nBlockAlign*wfex->Format.nSamplesPerSec;
		}
	}

	WAVEFORMATEX* wfeout = (WAVEFORMATEX*)mt.pbFormat;

	if(m_fDownSampleTo441)
	{
		if(wfeout->nSamplesPerSec > 44100 && wfeout->wBitsPerSample <= 16)
		{
			wfeout->nSamplesPerSec = 44100;
			wfeout->nAvgBytesPerSec = wfeout->nBlockAlign*wfeout->nSamplesPerSec;
		}
	}

	int bps = wfe->wBitsPerSample>>3;
	int len = cbBuffer / (bps*wfe->nChannels);
	int lenout = len * wfeout->nSamplesPerSec / wfe->nSamplesPerSec;
	cbBuffer = lenout*bps*wfeout->nChannels;

//	mt.lSampleSize = (ULONG)max(mt.lSampleSize, wfe->nAvgBytesPerSec * rtLen / 10000000i64);
//	mt.lSampleSize = (mt.lSampleSize + (wfe->nBlockAlign-1)) & ~(wfe->nBlockAlign-1);

	return mt;
}

void CAudioSwitcherFilter::OnNewOutputMediaType(const CMediaType& mtIn, const CMediaType& mtOut)
{
	const WAVEFORMATEX* wfe = (WAVEFORMATEX*)mtIn.pbFormat;
	const WAVEFORMATEX* wfeout = (WAVEFORMATEX*)mtOut.pbFormat;

	m_pResamplers.RemoveAll();
	for(int i = 0; i < wfeout->nChannels; i++)
	{
		CAutoPtr<AudioStreamResampler> pResampler;
		pResampler.Attach(new AudioStreamResampler(wfeout->wBitsPerSample>>3, wfe->nSamplesPerSec, wfeout->nSamplesPerSec, true));
		m_pResamplers.Add(pResampler);
	}

	TRACE(_T("CAudioSwitcherFilter::OnNewOutputMediaType\n"));
	m_sample_max = 0.1f;
}

HRESULT CAudioSwitcherFilter::DeliverEndFlush()
{
	TRACE(_T("CAudioSwitcherFilter::DeliverEndFlush\n"));
	m_sample_max = 0.1f;
	return __super::DeliverEndFlush();
}

HRESULT CAudioSwitcherFilter::DeliverNewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
	TRACE(_T("CAudioSwitcherFilter::DeliverNewSegment\n"));
	m_sample_max = 0.1f;
	return __super::DeliverNewSegment(tStart, tStop, dRate);
}

// IMPAudioSwitcherFilter

STDMETHODIMP CAudioSwitcherFilter::GetAudioDualMonoMode(ULONG* mode)
{
	LogDebug("GetAudioDualMonoMode...");
	LogDebug("CustomChannelMappingEnabled=%d",m_fCustomChannelMapping);
	*mode=eAudioDualMonoMode_STEREO;
	if (!m_fCustomChannelMapping)
		*mode=eAudioDualMonoMode_STEREO;
	else if (m_pSpeakerToChannelMap[0][0]==1)
			*mode=eAudioDualMonoMode_LEFT_MONO;
	else if (m_pSpeakerToChannelMap[0][0]==2)
			*mode=eAudioDualMonoMode_RIGHT_MONO;
	else if (m_pSpeakerToChannelMap[0][0]==3)
			*mode=eAudioDualMonoMode_MIXED;
	LogDebug("Result=%d",*mode);
	LogDebug("=======================");
	return S_OK;
}

void SetChannelMapping(DWORD mapping[18][18],DWORD value)
{
	for (int y=0;y<18;y++)
	{
		for (int x=0;x<18;x++)
			mapping[y][x]=value;
	}
}

STDMETHODIMP CAudioSwitcherFilter::SetAudioDualMonoMode(ULONG mode)
{
	LogDebug("SetAudioDualMonoMode...");
	LogDebug("Mode is %d",mode);
	if (mode<0 || mode >5)
		return E_INVALIDARG;

	DWORD newMapping[18][18];
	memset(newMapping, 0, sizeof(newMapping));
	bool bEnabled=true;
	switch (mode)
	{
		case eAudioDualMonoMode_STEREO:
			memset(newMapping, 0, sizeof(newMapping));
			bEnabled=false;
			LogDebug("STEREO");
			break;
		case eAudioDualMonoMode_LEFT_MONO:
			SetChannelMapping(newMapping,1);
			LogDebug("LEFT_MONO");
			break;
		case eAudioDualMonoMode_RIGHT_MONO:
			SetChannelMapping(newMapping,2);
			LogDebug("RIGHT MONO");
			break;
		case eAudioDualMonoMode_MIXED:
			SetChannelMapping(newMapping,3);
			LogDebug("MIXED");
			break;
	}
	int hr=SetSpeakerConfig(bEnabled,newMapping);
	LogDebug("=======================");
	return hr;
}

// IAudioSwitcherFilter

STDMETHODIMP CAudioSwitcherFilter::GetInputSpeakerConfig(DWORD* pdwChannelMask)
{
	if(!pdwChannelMask) 
		return E_POINTER;

	*pdwChannelMask = 0;

	CStreamSwitcherInputPin* pInPin = GetInputPin();
	if(!pInPin || !pInPin->IsConnected())
		return E_UNEXPECTED;

	WAVEFORMATEX* wfe = (WAVEFORMATEX*)pInPin->CurrentMediaType().pbFormat;

	if(wfe->wFormatTag == WAVE_FORMAT_EXTENSIBLE)
	{
		WAVEFORMATEXTENSIBLE* wfex = (WAVEFORMATEXTENSIBLE*)wfe;
		*pdwChannelMask = wfex->dwChannelMask;
	}
	else
	{
		*pdwChannelMask = 0/*wfe->nChannels == 1 ? 4 : wfe->nChannels == 2 ? 3 : 0*/;
	}

	return S_OK;
}

STDMETHODIMP CAudioSwitcherFilter::GetSpeakerConfig(bool* pfCustomChannelMapping, DWORD pSpeakerToChannelMap[18][18])
{
	if(pfCustomChannelMapping) *pfCustomChannelMapping = m_fCustomChannelMapping;
	memcpy(pSpeakerToChannelMap, m_pSpeakerToChannelMap, sizeof(m_pSpeakerToChannelMap));

	return S_OK;
}

STDMETHODIMP CAudioSwitcherFilter::SetSpeakerConfig(bool fCustomChannelMapping, DWORD pSpeakerToChannelMap[18][18])
{
	if(m_State == State_Stopped || m_fCustomChannelMapping != fCustomChannelMapping
	|| memcmp(m_pSpeakerToChannelMap, pSpeakerToChannelMap, sizeof(m_pSpeakerToChannelMap)))
	{
		PauseGraph;
		
		CStreamSwitcherInputPin* pInput = GetInputPin();

		SelectInput(NULL);

		m_fCustomChannelMapping = fCustomChannelMapping;
		memcpy(m_pSpeakerToChannelMap, pSpeakerToChannelMap, sizeof(m_pSpeakerToChannelMap));

		SelectInput(pInput);

		ResumeGraph;
	}

	return S_OK;
}

STDMETHODIMP_(int) CAudioSwitcherFilter::GetNumberOfInputChannels()
{
	CStreamSwitcherInputPin* pInPin = GetInputPin();
	return pInPin ? ((WAVEFORMATEX*)pInPin->CurrentMediaType().pbFormat)->nChannels : 0;
}

STDMETHODIMP_(bool) CAudioSwitcherFilter::IsDownSamplingTo441Enabled()
{
	return m_fDownSampleTo441;
}

STDMETHODIMP CAudioSwitcherFilter::EnableDownSamplingTo441(bool fEnable)
{
	if(m_fDownSampleTo441 != fEnable)
	{
		PauseGraph;
		m_fDownSampleTo441 = fEnable;
		ResumeGraph;
	}

	return S_OK;
}

STDMETHODIMP_(REFERENCE_TIME) CAudioSwitcherFilter::GetAudioTimeShift()
{
	return m_rtAudioTimeShift;
}

STDMETHODIMP CAudioSwitcherFilter::SetAudioTimeShift(REFERENCE_TIME rtAudioTimeShift)
{
	m_rtAudioTimeShift = rtAudioTimeShift;
	return S_OK;
}

STDMETHODIMP CAudioSwitcherFilter::GetNormalizeBoost(bool& fNormalize, bool& fNormalizeRecover, float& boost)
{
	fNormalize = m_fNormalize;
	fNormalizeRecover = m_fNormalizeRecover;
	boost = m_boost;
	return S_OK;
}

STDMETHODIMP CAudioSwitcherFilter::SetNormalizeBoost(bool fNormalize, bool fNormalizeRecover, float boost)
{
	if(m_fNormalize != fNormalize) m_sample_max = 0.1f;
	m_fNormalize = fNormalize;
	m_fNormalizeRecover = fNormalizeRecover;
	m_boost = boost;
	return S_OK;
}

// IAMStreamSelect

STDMETHODIMP CAudioSwitcherFilter::Enable(long lIndex, DWORD dwFlags)
{
	HRESULT hr = __super::Enable(lIndex, dwFlags);
	if(S_OK == hr) m_sample_max = 0.1f;
	return hr;
}