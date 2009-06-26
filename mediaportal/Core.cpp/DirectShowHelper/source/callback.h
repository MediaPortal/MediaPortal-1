#pragma once

DECLARE_INTERFACE_(IVMR9Callback, IUnknown)
{
	STDMETHOD(PresentImage)  (THIS_ WORD cx, WORD cy, WORD arx, WORD ary, DWORD pTexture, DWORD pSurface)PURE;
	STDMETHOD(SetSampleTime)(REFERENCE_TIME nsSampleTime)PURE;
};
