// DVD.cpp : Implementation of CDVD

#include "stdafx.h"
#include "DVD.h"
#include ".\dvd.h"
#include "resetdvd.h"
#include <comutil.h>
// CDVD


STDMETHODIMP CDVD::Reset(BSTR strPath)
{
	_bstr_t bstrPath(strPath,true);
	CResetDVD* dvd = new CResetDVD((char*)bstrPath);
	delete dvd;
	return S_OK;
}
