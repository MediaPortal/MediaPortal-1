// VMR9Helper.cpp : Implementation of CVMR9Helper

#include "stdafx.h"
#include "VMR9Helper.h"
#include ".\vmr9helper.h"


// CVMR9Helper
#define MY_USER_ID 0x6ABE51

STDMETHODIMP CVMR9Helper::Init(IVMR9Callback* callback, DWORD dwD3DDevice, IBaseFilter* vmr9Filter,DWORD monitor)
{
	HRESULT hr;
	m_pDevice = (LPDIRECT3DDEVICE9)(dwD3DDevice);
	m_pVMR9Filter=vmr9Filter;
	UINT mem=m_pDevice->GetAvailableTextureMem();

	CComQIPtr<IVMRFilterConfig9> pConfig = m_pVMR9Filter;
	if(!pConfig)
		return E_FAIL;

	if(FAILED(hr = pConfig->SetRenderingMode(VMR9Mode_Renderless)))
		return E_FAIL;

	CComQIPtr<IVMRSurfaceAllocatorNotify9> pSAN = m_pVMR9Filter;
	if(!pSAN)
		return E_FAIL;
	m_allocator = new CVMR9AllocatorPresenter(m_pDevice, callback,(HMONITOR)monitor);

	if(FAILED(hr = pSAN->AdviseSurfaceAllocator(MY_USER_ID, static_cast<IVMRSurfaceAllocator9*>(m_allocator))))
		return E_FAIL;
	
	if (FAILED(hr = m_allocator->AdviseNotify(pSAN)))
		return E_FAIL;

	return S_OK;
}
	
STDMETHODIMP CVMR9Helper::Deinit(void)
{
	if (m_allocator!=NULL)
	{
		m_allocator->Deinit();
		m_allocator->Release();
		delete m_allocator;
		m_allocator=NULL;
		m_pVMR9Filter->Release();
		m_pVMR9Filter=NULL;
		m_pDevice=NULL;
	}
	return S_OK;
}
STDMETHODIMP CVMR9Helper::Version(void)
{


	return S_OK;
}

STDMETHODIMP CVMR9Helper::GetVideoSize(ULONG* Width, ULONG* Height)
{
	if (m_allocator==NULL) return E_FAIL;
	CSize size=m_allocator->GetVideoSize(false);
	*Width = (ULONG)size.cx;
	*Height = (ULONG)size.cy;
	return S_OK;
}
