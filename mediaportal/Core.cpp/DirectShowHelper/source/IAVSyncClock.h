#ifndef IAVSYNCCLOCK
#define IAVSYNCCLOCK

// {91A198BA-1C78-4c31-A50F-0F5C7578F078}
static const GUID IID_IAVSyncClock = { 0x91a198ba, 0x1c78, 0x4c31, { 0xa5, 0xf, 0xf, 0x5c, 0x75, 0x78, 0xf0, 0x78 } };
DEFINE_GUID(CLSID_IAVSyncClock, 0x91a198ba, 0x1c78, 0x4c31, 0xa5, 0xf, 0xf, 0x5c, 0x75, 0x78, 0xf0, 0x78);

MIDL_INTERFACE("91A198BA-1C78-4c31-A50F-0F5C7578F078")
IAVSyncClock: public IUnknown
{
public:
	virtual HRESULT STDMETHODCALLTYPE AdjustClock(DOUBLE adjustment) = 0;
	virtual HRESULT STDMETHODCALLTYPE SetBias(DOUBLE bias) = 0;
	virtual HRESULT STDMETHODCALLTYPE GetBias(DOUBLE *bias) = 0;
};
#endif // IAVSYNCCLOCK
