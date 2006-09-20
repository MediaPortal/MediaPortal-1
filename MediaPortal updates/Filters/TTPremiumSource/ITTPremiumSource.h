#pragma once

#include "TTPremiumTypes.h"

// Interface GUID
// {6aa08757-7fa2-48a2-a9e5-58910e3fe8a7}
DEFINE_GUID(IID_TTPremiumSource,
0x6aa08757, 0x7fa2, 0x48a2, 0xa9, 0xe5, 0x58, 0x91, 0x0e, 0x3f, 0xe8, 0xa7);

DECLARE_INTERFACE_(ITTPremiumSource, IUnknown)
{
	STDMETHOD(Init) () PURE;
	STDMETHOD(Close)() PURE;
	STDMETHOD(GetNetworkType) (THIS_ NetworkType &network) PURE;
	STDMETHOD(Tune)(THIS_ ULONG frequency, ULONG symbolRate, ULONG polarity, ULONG LNBKhz, 
								  ULONG LNBFreq, BOOL LNBPower, ULONG diseq, ModType modulation, 
								  BandwidthType bwType, BOOL specInv) PURE;
	STDMETHOD(GetSignalState)(THIS_ BOOL &locked, ULONG &quality, ULONG &level) PURE;
};
