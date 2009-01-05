
#include <stdafx.h>

#include <streams.h>
#include <initguid.h>
#include <bdaiface.h>
#include "TSDataFilter.h"
#include "CTTPremiumSource.h"
#include "CTTPremiumOutputPin.h"
#include "CTTEnumPIDMap.h"
#include <tchar.h>
#include <stdio.h>

// Using this pointer in constructor
#pragma warning(disable:4355 4127)

// Setup data

const AMOVIESETUP_MEDIATYPE sudPinTypes =
{
	&MEDIATYPE_NULL, &MEDIASUBTYPE_NULL
};

const AMOVIESETUP_PIN psudPins[] =
{
    { L"Output",            // Pin's string name
      FALSE,                // Is it rendered
      TRUE,                 // Is it an output
      FALSE,                // Allowed none
      FALSE,                // Allowed many
      &CLSID_NULL,          // Connects to filter
      L"Input",             // Connects to pin
      1,                    // Number of types
      &sudPinTypes }        // Pin information
};

const AMOVIESETUP_FILTER sudTTPremium =
{
    &CLSID_TTPremiumSource,           // CLSID of filter
    L"TTPremiumSource",    // Filter's name
    MERIT_DO_NOT_USE,       // Filter merit
    1,                      // Number of pins
    psudPins                // Pin information
};


CFactoryTemplate g_Templates [1] = {
    { L"TTPremiumSource"
    , &CLSID_TTPremiumSource
    , CTTPremiumSource::CreateInstance
    , NULL
    , &sudTTPremium }
};
int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);



/*HRESULT SetupDemuxerPin(IPin *pVideo,int videoPID, int elementary_stream, bool unmapOtherPids)
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;
	int				maxCounter;
	HRESULT hr=0;

	// video
	if (pVideo!=NULL)
	{
		hr=pVideo->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
		if(FAILED(hr) || pMap==NULL)
			return 1;
		// 
		if (unmapOtherPids)
		{
			hr=pMap->EnumPIDMap(&pPidEnum);
			if(FAILED(hr) || pPidEnum==NULL)
				return 5;
			// enum and unmap the pids
			maxCounter=20;
			while(pPidEnum->Next(1,&pm,&count)== S_OK)
			{
				maxCounter--;
				if (maxCounter<0) break;
				if (count !=1) break;
				umPid=pm.ulPID;
				hr=pMap->UnmapPID(1,&umPid);
				if(FAILED(hr))
					return 6;
			}
			pPidEnum->Release();
		}
		if (videoPID>=0)
		{
			// map new pid
			pid = (ULONG)videoPID;
				hr=pMap->MapPID(1,&pid,(MEDIA_SAMPLE_CONTENT)elementary_stream);
			if(FAILED(hr))
				return 2;
		}
		pMap->Release();
	}
	return S_OK;
}*/

#define DISEQC_HIGH_NIBLE	0xF0
#define DISEQC_LOW_BAND	0x00
#define DISEQC_HIGH_BAND	0x01
#define DISEQC_VERTICAL		0x00
#define DISEQC_HORIZONTAL	0x02
#define DISEQC_POSITION_A	0x00
#define DISEQC_POSITION_B	0x04
#define DISEQC_OPTION_A		0x00
#define DISEQC_OPTION_B		0x08

//
// CreateInstance
//
// Creator function for the class ID
//
CUnknown * WINAPI CTTPremiumSource::CreateInstance(LPUNKNOWN pUnk, HRESULT *phr)
{
    return new CTTPremiumSource(NAME("TTPremium Source Filter"), pUnk, phr);
}

//
// Constructor
//
CTTPremiumSource::CTTPremiumSource(TCHAR *pName, LPUNKNOWN pUnk, HRESULT *phr) :
	CSource(NAME("TTPremium Source Filter"), pUnk, CLSID_TTPremiumSource, phr)
{
	m_boardControl = new CDVBBoardControl();
	m_frontend = new CDVBFrontend();
	m_AVControl = new CDVBAVControl();

	/*AM_MEDIA_TYPE mtEPG;
	memset(&mtEPG, 0, sizeof(AM_MEDIA_TYPE));
	mtEPG.majortype = MEDIATYPE_MPEG2_SECTIONS;
	mtEPG.subtype = MEDIASUBTYPE_NULL;
	mtEPG.formattype = FORMAT_None;

	AM_MEDIA_TYPE mtSections;
	memset(&mtSections, 0, sizeof(AM_MEDIA_TYPE));
	mtSections.majortype = MEDIATYPE_MPEG2_SECTIONS;
	mtSections.subtype = MEDIASUBTYPE_NULL;
	mtSections.formattype = FORMAT_None;

	IPin* pinEPGout = NULL;
	IPin* pinMHW1Out = NULL;
	IPin* pinMHW2Out = NULL;
	IPin* pinSectionsOut = NULL;

	CreateOutputPin(&mtSections, L"sections",  &pinSectionsOut);
	CreateOutputPin(&mtEPG, L"MHW1",  &pinMHW1Out);
	CreateOutputPin(&mtEPG, L"MHW2",  &pinMHW2Out);
	CreateOutputPin(&mtEPG, L"EPG",  &pinEPGout);

	Init();

	//this->Tune(13216000,20000,1,44,11250,true,1,QAM_16,BW_NONE,true);

	SetupDemuxerPin(pinSectionsOut, 0x0, (int)MEDIA_MPEG2_PSI, true);
    SetupDemuxerPin(pinSectionsOut, 0x10, (int)MEDIA_MPEG2_PSI, false);
    SetupDemuxerPin(pinSectionsOut, 0x11, (int)MEDIA_MPEG2_PSI, false);
    //SetupDemuxerPin(pinSectionsOut, 0x12, (int)MEDIA_MPEG2_PSI, false);

	this->Tune(12224000,20000,1,44,11250,true,1,QAM_16,BW_NONE,true);*/
}

//
// Destructor
//
CTTPremiumSource::~CTTPremiumSource()
{
	std::vector<CTTPremiumOutputPin*> remove;
	for (int i = 0; i < GetPinCount(); i++)
	{
		CTTPremiumOutputPin *pin = (CTTPremiumOutputPin *)GetPin(i);
		remove.push_back(pin);
	}

	for (std::vector<CTTPremiumOutputPin*>::iterator it = remove.begin(); it != remove.end(); it++)
	{
		CSource::RemovePin((*it));
	}
	remove.clear();

	//delete m_boardControl;
	//delete m_frontend;
	//delete m_AVControl;
}

//
// Initialized the filter
//
HRESULT CTTPremiumSource::Init()
{
	if (!CDVBCommon::OpenDevice(0, "TTPremiumSource", FALSE))
  {
		if (!CDVBCommon::OpenDevice(0, "TTPremiumSource", TRUE))
    {
			//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumSource::Init() - failed to OpenDevice")));
			return S_FALSE;
    }
	}

	// Boot the device
	//CString path = "C:\\src\\Media_Portal_w_src\\mediaportal_src\\mediaportal_work\\Filters\\TTPremiumSource\\Debug\\Boot\\24\\";
	CString path = ".\\TTPremiumBoot\\24\\";
	DVB_ERROR error = m_boardControl->BootARM(&path);
	if (error != DVB_ERR_NONE)
	{
		//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumSource::Init() - failed to BootARM in path: %s"), path));
		return S_FALSE;
	}

	CDVBBoardControl::BE_VERSION version;
	error = m_boardControl->GetBEVersion(version);
	if (error != DVB_ERR_NONE)
	{
		//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumSource::Init() - failed to GetBEVersion")));
		return S_FALSE;
	}
	else
	{
		// 2.1er TI
		if ((version.Firmware >> 16) == 0xF021)
		{
		  path = ".\\TTPremiumBoot\\21\\";
		  error = m_boardControl->BootARM(&path);
		  if (error != DVB_ERR_NONE)
		  {
			  //DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumSource::Init() - failed to BootARM - 21 in path: %s"), path));
			  return S_FALSE;
		  }
		}
	}

	// Init all the interfaces
	error = m_frontend->Init();
	if (error != DVB_ERR_NONE)
	{
		//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumSource::Init() - failed to frontend->Init")));
		return S_FALSE;
	}
	
	error = m_boardControl->EnableDataDMA(TRUE);
	if (error != DVB_ERR_NONE)
	{
		//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumSource::Init() - failed to EnableDMA")));
		return S_FALSE;
	}

  error = m_AVControl->Init();
  if (error != DVB_ERR_NONE)
  {
	  //DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumSource::Init() - failed to AVControl->Init")));
	  return S_FALSE;
  }

	return S_OK;
}

//
// De-initialize the instance
//
HRESULT CTTPremiumSource::Close()
{
	m_boardControl->EnableDataDMA(FALSE);
	CDVBCommon::CloseDevice();
	return S_OK;
}

//
// Get the network type
//
HRESULT CTTPremiumSource::GetNetworkType(NetworkType &network)
{
	CDVBFrontend::FRONTEND_TYPE ft  = m_frontend->GetType();
	network = (NetworkType)((int)ft);
	return S_OK;
}

//
// Tune to a channel
//
HRESULT CTTPremiumSource::Tune(ULONG frequency, ULONG symbolRate, ULONG polarity, ULONG LNBKhz,
													  ULONG LNBFreq, BOOL LNBPower, ULONG diseq, ModType modulation, 
													  BandwidthType bwType, BOOL specInv)
{
	NetworkType nt;
	if (GetNetworkType(nt) != S_OK)
	{
		//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumSource::Tune() - failed to GetNetworkType")));
		return S_FALSE;
	}

	DVB_ERROR error = DVB_ERR_NONE;
	CDVBFrontend::CHANNEL_TYPE ct;
	switch (nt)
	{
	case NT_DVB_S:
		{
			BYTE par[4];
			switch (diseq)
			{
				// None
				case 0:
					par[0] = 0xE0; par[1] = 0x00; par[2] = 0x00;
					error = m_frontend->SendDiSEqCMsg(par, 3, 0xff);
					if (error != DVB_ERR_NONE)
					{
						//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumSource::Tune() - failed to SendDiSEqCMsg - case 0")));
						return S_FALSE;
					}
					Sleep(120);
					break;

				// Simple
				case 1:
				case 2:
					error = m_frontend->SendDiSEqCMsg(par, 0, ((diseq == 1) ? 0 : 1));
					if (error != DVB_ERR_NONE)
					{
						//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumSource::Tune() - failed to SendDiSEqCMsg - case 1 & 2")));
						return S_FALSE;
					}
					Sleep(120);
					break;

				// Multi
				case 3:
				case 4:
				case 5:
				case 6:
					par[0] = 0xE0; par[1] = 0x10; par[2] = 0x38;
					par[3] = DISEQC_HIGH_NIBLE;
					par[3] |= polarity ? DISEQC_VERTICAL : DISEQC_HORIZONTAL;
					par[3] |= (LNBKhz == 1) ? DISEQC_HIGH_BAND : DISEQC_LOW_BAND;
					par[3] |= (diseq % 2 == 0) ? DISEQC_POSITION_B : DISEQC_POSITION_A;
					par[3] |= (diseq >= 5) ? DISEQC_OPTION_B : DISEQC_OPTION_A;
					for (int i = 0; i < 3; i++)
					{
						error = m_frontend->SendDiSEqCMsg(par, 4, 0xff);
						if (error != DVB_ERR_NONE)
						{
							//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumSource::Tune() - failed to SendDiSEqCMsg - case 3, 4, 5, 6")));
							return S_FALSE;
						}
						par[0] = 0xE1;
						Sleep(120);
					}
					break;
			}

			ct.dvb_s.dwSymbRate = symbolRate * 1000;
			ct.dvb_s.Viterbi = CDVBFrontend::VR_AUTO;
			ct.dvb_s.LNB_Power = (!LNBPower) ? CDVBFrontend::POWER_OFF : (!polarity) ? CDVBFrontend::POL_HORZ : CDVBFrontend::POL_VERT;
			ct.dvb_s.dwFreq = frequency;
			ct.dvb_s.bF22kHz = LNBKhz;
			ct.dvb_s.dwLOF = LNBFreq * 1000;
			if (m_frontend->GetCapabilities() & HAS_SI_AUTO)
			{
				ct.dvb_s.Inversion = CDVBFrontend::SI_AUTO;
			}
			else
			{
				ct.dvb_s.Inversion = (specInv) ? CDVBFrontend::SI_ON : CDVBFrontend::SI_OFF;
			}
		}
		break;

	case NT_DVB_C:
		{
			ct.dvb_c.dwFreq = frequency;
			ct.dvb_c.dwSymbRate = symbolRate;
			if(m_frontend->GetCapabilities() & HAS_SI_AUTO)
			{
				ct.dvb_c.Inversion = CDVBFrontend::SI_AUTO;
			}
			else
			{
				ct.dvb_c.Inversion = (specInv) ? CDVBFrontend::SI_ON : CDVBFrontend::SI_OFF;
			}
			ct.dvb_c.Qam = (CDVBFrontend::QAM_TYPE)modulation;
		
			if(m_frontend->GetCapabilities() & HAS_BW_AUTO)
			{
				ct.dvb_c.BandWidth = CDVBFrontend::BW_AUTO;
			}
			else
			{
				CDVBFrontend::BANDWITH_TYPE bwMin, bwMax;
				m_frontend->GetBandwidthRange(bwMin, bwMax);
				if (bwMin == bwMax)
				{
					ct.dvb_c.BandWidth = bwMin;
				}
				else
				{
					ct.dvb_c.BandWidth = (bwType == 0) ? CDVBFrontend::BW_7MHz : CDVBFrontend::BW_8MHz;
				}
			}
		}
		break;

	case NT_DVB_T:
		{
			ct.dvb_t.dwFreq = frequency;
			if (m_frontend->GetCapabilities() & HAS_SI_AUTO)
			{
				ct.dvb_t.Inversion = CDVBFrontend::SI_AUTO;
			}
			else
			{
				ct.dvb_t.Inversion = (specInv) ? CDVBFrontend::SI_ON : CDVBFrontend::SI_OFF;
			}

			if (m_frontend->GetCapabilities() & HAS_BW_AUTO)
			{
				ct.dvb_t.BandWidth = CDVBFrontend::BW_AUTO;
			}
			else
			{
				CDVBFrontend::BANDWITH_TYPE bwMin, bwMax;
				m_frontend->GetBandwidthRange(bwMin, bwMax);
				if (bwMin == bwMax)
				{
					ct.dvb_t.BandWidth = bwMin;
				}
				else
				{
					ct.dvb_t.BandWidth = (bwType == 0) ? CDVBFrontend::BW_7MHz : CDVBFrontend::BW_8MHz;
				}
			}
			ct.dvb_t.bScan = FALSE;
		}
		break;
	}

	error = m_frontend->SetChannel(ct);
	if (error != DVB_ERR_NONE)
	{
		//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumSource::Tune() - failed to SetChannel - error: %d"), error));
		return S_FALSE;
	}

	return S_OK;
}

//
//
//
HRESULT CTTPremiumSource::GetSignalState(BOOL &locked, ULONG &quality, ULONG &level)
{
	CDVBFrontend::SIGNAL_TYPE st;
	CDVBFrontend::LOCK_STATE ls;
	DVB_ERROR error = m_frontend->GetState(locked, &st, &ls);
	if (error != DVB_ERR_NONE)
	{
		return S_FALSE;
	}

	if (st.BER < 10e-4)
    {
      quality = 75; // 75%
    }
    else if (st.BER < 10e-3)
    {
      quality = 50; // 50%
    }
    else if (st.BER < 10e-2)
    {
      quality = 25; // 25%
    }
    else
    {
      quality = 0; // 0%
    }

    if (locked)
    {
      quality += 25; 
    }
    else
    {
      quality = 0;
    }

    level = st.SNR100;

	return S_OK;
}

//
// Create a new output pin, no duplicate names allowed
//
HRESULT CTTPremiumSource::CreateOutputPin(AM_MEDIA_TYPE* pMediaType, LPWSTR pszPinName, IPin** ppIPin)
{
	CAutoLock lock(m_pLock);

	HRESULT hr = NOERROR;
	CTTPremiumOutputPin *pPin = new CTTPremiumOutputPin(NAME("TTPremium Source Output"), this, &hr, pszPinName, pMediaType);
  if (FAILED(hr) || pPin == NULL) 
  {
	  return S_FALSE;
  }

	CMediaType mt(*pMediaType, &hr);
	if (SUCCEEDED(hr))
	{
		pPin->SetMediaType(&mt);
		*ppIPin = pPin;
		pPin->AddRef();
	}
	else
	{
		CSource::RemovePin(pPin);
	}

	return hr;
}

//
// Set the output type of a particular pin
//
HRESULT CTTPremiumSource::SetOutputPinMediaType(LPWSTR pszPinName, AM_MEDIA_TYPE* pMediaType)
{
	CAutoLock lock(m_pLock);

	IPin *pPin = NULL;
	HRESULT hr = CSource::FindPin(pszPinName, &pPin);
	if (SUCCEEDED(hr))
	{
		CTTPremiumOutputPin *pOut = (CTTPremiumOutputPin *)pPin;
		CMediaType mt(*pMediaType, &hr);
		if (SUCCEEDED(hr))
		{
			pOut->SetMediaType(&mt);
		}
	}
	return hr;
}

//
// Delete an output pin
//
HRESULT CTTPremiumSource::DeleteOutputPin( LPWSTR pszPinName)
{
	CAutoLock lock(m_pLock);

	IPin *pPin = NULL;
	HRESULT hr = CSource::FindPin(pszPinName, &pPin);
	if (SUCCEEDED(hr))
	{
		CTTPremiumOutputPin *pOut = (CTTPremiumOutputPin *)pPin;
		pOut->Release();
		CSource::RemovePin(pOut);
		return NOERROR;
	}
	return hr;
}

//
// Return the interface(s) that we support
//
STDMETHODIMP CTTPremiumSource::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
    CheckPointer(ppv, E_POINTER);
    CAutoLock lock(pStateLock());

	// Do we have this interface
	if (riid == IID_IMpeg2Demultiplexer)
	{
		return GetInterface((IMpeg2Demultiplexer*)this, ppv);
	}
	else if (riid == IID_TTPremiumSource)
	{
		return GetInterface((ITTPremiumSource*)this, ppv);
	}
    return CSource::NonDelegatingQueryInterface(riid, ppv);
}

////////////////////////////////////////////////////////////////////////
//
// Exported entry points for registration and unregistration 
// (in this case they only call through to default implementations).
//
////////////////////////////////////////////////////////////////////////

//
// DllRegisterServer
//
STDAPI DllRegisterServer()
{
    return AMovieDllRegisterServer2(TRUE);
}


//
// DllUnregisterServer
//
STDAPI
DllUnregisterServer()
{
    return AMovieDllRegisterServer2(FALSE);
}


//
// DllEntryPoint
//
extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HANDLE hModule, 
                      DWORD  dwReason, 
                      LPVOID lpReserved)
{
	return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}

