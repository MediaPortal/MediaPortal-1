// VMR9Helper.cpp : Implementation of CVMR9Helper

#include "stdafx.h"
#include "VMR9Helper.h"
#include ".\vmr9helper.h"
#include <dvdmedia.h>


// CVMR9Helper
#define MY_USER_ID 0x6ABE51
#define IsInterlaced(x) ((x) & AMINTERLACE_IsInterlaced)
#define IsSingleField(x) ((x) & AMINTERLACE_1FieldPerSample)
#define IsField1First(x) ((x) & AMINTERLACE_Field1First)

VMR9_SampleFormat ConvertInterlaceFlags(DWORD dwInterlaceFlags)
{
    if (IsInterlaced(dwInterlaceFlags)) {
        if (IsSingleField(dwInterlaceFlags)) {
            if (IsField1First(dwInterlaceFlags)) {
                return VMR9_SampleFieldSingleEven;
            }
            else {
                return VMR9_SampleFieldSingleOdd;
            }
        }
        else {
            if (IsField1First(dwInterlaceFlags)) {
                return VMR9_SampleFieldInterleavedEvenFirst;
             }
            else {
                return VMR9_SampleFieldInterleavedOddFirst;
            }
        }
    }
    else {
        return VMR9_SampleProgressiveFrame;  // Not interlaced.
    }
}

STDMETHODIMP CVMR9Helper::Init(IVMR9Callback* callback, DWORD dwD3DDevice, IBaseFilter* vmr9Filter,DWORD monitor)
{
	HRESULT hr;
	m_pDevice = (LPDIRECT3DDEVICE9)(dwD3DDevice);
	m_pVMR9Filter.Attach(vmr9Filter);
	
	CComQIPtr<IVMRFilterConfig9> pConfig = m_pVMR9Filter;
	if(!pConfig)
		return E_FAIL;

	if(FAILED(hr = pConfig->SetRenderingMode(VMR9Mode_Renderless)))
		return E_FAIL;
	
	if(FAILED(hr = pConfig->SetNumberOfStreams(1)))
		return E_FAIL;
	

	CComQIPtr<IVMRSurfaceAllocatorNotify9> pSAN = m_pVMR9Filter;
	if(!pSAN)
		return E_FAIL;

    g_allocator.Attach(new CVMR9AllocatorPresenter( m_pDevice, callback,(HMONITOR)monitor));

	if(FAILED(hr = pSAN->AdviseSurfaceAllocator(MY_USER_ID, g_allocator)))
		return E_FAIL;
	
	if (FAILED(hr = g_allocator->AdviseNotify(pSAN)))
		return E_FAIL;

	return S_OK;
}
	
STDMETHODIMP CVMR9Helper::Deinit(void)
{
    //g_allocator    = NULL;        
	//m_pVMR9Filter=NULL;
	m_pDevice=NULL;
	return S_OK;
}
STDMETHODIMP CVMR9Helper::SetDeinterlace(DWORD dwMethod)
{
	int hr;
	CComQIPtr<IVMRDeinterlaceControl9> pDeint=m_pVMR9Filter;
	switch (dwMethod)
	{
		case 1:
			hr=pDeint->SetDeinterlacePrefs(DeinterlacePref9_BOB);
		break;
		case 2:
			hr=pDeint->SetDeinterlacePrefs(DeinterlacePref9_Weave);
		break;
		case 3:
			hr=pDeint->SetDeinterlacePrefs(DeinterlacePref9_NextBest );
		break;
	}

	VMR9VideoDesc VideoDesc; 
	DWORD dwNumModes = 0;

	AM_MEDIA_TYPE pmt;
	ULONG fetched;
	IPin* pins[10];
	CComPtr<IEnumPins> pinEnum;
	hr=m_pVMR9Filter->EnumPins(&pinEnum);
	pinEnum->Reset();
	pinEnum->Next(1,&pins[0],&fetched);
	hr=pins[0]->ConnectionMediaType(&pmt);
	pins[0]->Release();

	VIDEOINFOHEADER2* vidInfo2 =(VIDEOINFOHEADER2*)pmt.pbFormat;
	VideoDesc.dwFourCC=vidInfo2->bmiHeader.biCompression;
	VideoDesc.dwSampleWidth=vidInfo2->bmiHeader.biWidth;
	VideoDesc.dwSampleHeight=vidInfo2->bmiHeader.biHeight;
	VideoDesc.SampleFormat=ConvertInterlaceFlags(vidInfo2->dwInterlaceFlags);
	VideoDesc.InputSampleFreq.dwDenominator=(DWORD)vidInfo2->AvgTimePerFrame;
	VideoDesc.InputSampleFreq.dwNumerator=10000000;
	VideoDesc.OutputFrameFreq.dwDenominator=(DWORD)vidInfo2->AvgTimePerFrame;
	VideoDesc.OutputFrameFreq.dwNumerator=VideoDesc.InputSampleFreq.dwNumerator;
	if (VideoDesc.SampleFormat != VMR9_SampleProgressiveFrame)
	{
		VideoDesc.OutputFrameFreq.dwNumerator=2*VideoDesc.InputSampleFreq.dwNumerator;
	}

	// Fill in the VideoDesc structure (not shown).
	hr = pDeint->GetNumberOfDeinterlaceModes(&VideoDesc, &dwNumModes, NULL);
	if (SUCCEEDED(hr) && (dwNumModes != 0))
	{
		// Allocate an array for the GUIDs that identify the modes.
		GUID *pModes = new GUID[dwNumModes];
		if (pModes)
		{
			// Fill the array.
			hr = pDeint->GetNumberOfDeinterlaceModes(&VideoDesc, &dwNumModes, pModes);
			if (SUCCEEDED(hr))
			{
				// Loop through each item and get the capabilities.
				for (int i = 0; i < dwNumModes; i++)
				{
					VMR9DeinterlaceCaps Caps;
					hr = pDeint->GetDeinterlaceModeCaps(pModes + i, &VideoDesc, &Caps);
					if (SUCCEEDED(hr))
					{
						// Examine the Caps structure.
						pDeint->SetDeinterlaceMode(0,&pModes[0]);
						break;
					}
				}
			}
			delete [] pModes;
		}
	}


	return S_OK;
}