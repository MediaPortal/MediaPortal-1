/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2017 see Authors.txt
 *
 * This file is part of MPC-HC.
 *
 * MPC-HC is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * MPC-HC is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

#pragma once

#include "HdmvClipInfo.h"
#include "MediaTypeEx.h"
#include "text.h"
#include "vd.h"
#include "streams.h"
#include "Utils.h"

#define LCID_NOSUBTITLES -1

#define IsWaveFormatExtensible(wfe) (wfe->wFormatTag == WAVE_FORMAT_EXTENSIBLE && wfe->cbSize == 22)

extern int  CountPins(IBaseFilter* pBF, int& nIn, int& nOut, int& nInC, int& nOutC);
extern bool IsSplitter(IBaseFilter* pBF, bool fCountConnectedOnly = false);
extern bool IsMultiplexer(IBaseFilter* pBF, bool fCountConnectedOnly = false);
extern bool IsStreamStart(IBaseFilter* pBF);
extern bool IsStreamEnd(IBaseFilter* pBF);
extern bool IsVideoRenderer(IBaseFilter* pBF);
extern bool IsAudioWaveRenderer(IBaseFilter* pBF);
extern IBaseFilter* GetUpStreamFilter(IBaseFilter* pBF, IPin* pInputPin = nullptr);
extern IPin* GetUpStreamPin(IBaseFilter* pBF, IPin* pInputPin = nullptr);
extern IPin* GetFirstPin(IBaseFilter* pBF, PIN_DIRECTION dir = PINDIR_INPUT);
extern IPin* GetFirstDisconnectedPin(IBaseFilter* pBF, PIN_DIRECTION dir);
extern void  NukeDownstream(IBaseFilter* pBF, IFilterGraph* pFG);
extern void  NukeDownstream(IPin* pPin, IFilterGraph* pFG);
extern IBaseFilter* FindFilter(LPCWSTR clsid, IFilterGraph* pFG);
extern IBaseFilter* FindFilter(const CLSID& clsid, IFilterGraph* pFG);
extern IPin* FindPin(IBaseFilter* pBF, PIN_DIRECTION direction, const AM_MEDIA_TYPE* pRequestedMT);
extern std::wstring GetFilterName(IBaseFilter* pBF);
extern std::wstring GetPinName(IPin* pPin);
extern IFilterGraph* GetGraphFromFilter(IBaseFilter* pBF);
extern IBaseFilter*  GetFilterFromPin(IPin* pPin);
extern IPin* AppendFilter(IPin* pPin, tstring DisplayName, IGraphBuilder* pGB);
extern IPin* InsertFilter(IPin* pPin, tstring DisplayName, IGraphBuilder* pGB);
extern void  ExtractMediaTypes(IPin* pPin, std::vector<GUID>& types);
extern void  ExtractMediaTypes(IPin* pPin, std::list<CMediaType>& mts);
extern void  ShowPPage(tstring DisplayName, HWND hParentWnd);
extern void  ShowPPage(IUnknown* pUnknown, HWND hParentWnd);
extern CLSID GetCLSID(IBaseFilter* pBF);
extern CLSID GetCLSID(IPin* pPin);
extern tstring CLSIDToString(CLSID& clsid);
extern bool  IsCLSIDRegistered(LPCTSTR clsid);
extern bool  IsCLSIDRegistered(const CLSID& clsid);
extern tstring GetFilterPath(LPCTSTR clsid);
extern tstring GetFilterPath(const CLSID& clsid);
extern void  CStringToBin(tstring str, std::vector<BYTE>& data);
extern tstring BinToCString(const BYTE* ptr, size_t len);
extern void FindFiles(tstring fn, std::list<tstring>& files);
enum OpticalDiskType_t {
    OpticalDisk_NotFound,
    OpticalDisk_Audio,
    OpticalDisk_VideoCD,
    OpticalDisk_DVDVideo,
    OpticalDisk_BD,
    OpticalDisk_Unknown
};
extern OpticalDiskType_t GetOpticalDiskType(TCHAR drive, std::list<tstring>& files);
extern tstring GetDriveLabel(TCHAR drive);
extern tstring GetDriveLabel(tstring path);
bool IsDriveVirtual(tstring drive);
extern DVD_HMSF_TIMECODE RT2HMSF(REFERENCE_TIME rt, double fps = 0.0); // used to remember the current position
extern DVD_HMSF_TIMECODE RT2HMS(REFERENCE_TIME rt);
extern DVD_HMSF_TIMECODE RT2HMS_r(REFERENCE_TIME rt);                  // used only to display information with rounding to nearest second
extern REFERENCE_TIME HMSF2RT(DVD_HMSF_TIMECODE hmsf, double fps = -1.0);
extern void memsetd(void* dst, unsigned int c, size_t nbytes);
extern void memsetw(void* dst, unsigned short c, size_t nbytes);
extern bool ExtractBIH(const AM_MEDIA_TYPE* pmt, BITMAPINFOHEADER* bih);
extern bool ExtractBIH(IMediaSample* pMS, BITMAPINFOHEADER* bih);
extern bool ExtractAvgTimePerFrame(const AM_MEDIA_TYPE* pmt, REFERENCE_TIME& rtAvgTimePerFrame);
extern bool ExtractDim(const AM_MEDIA_TYPE* pmt, int& w, int& h, int& arx, int& ary);
extern bool CreateFilter(std::wstring DisplayName, IBaseFilter** ppBF, std::wstring& FriendlyName);
extern IBaseFilter* AppendFilter(IPin* pPin, IMoniker* pMoniker, IGraphBuilder* pGB);
extern std::wstring GetFriendlyName(std::wstring DisplayName);
extern HRESULT LoadExternalObject(LPCTSTR path, REFCLSID clsid, REFIID iid, void** ppv);
extern HRESULT LoadExternalFilter(LPCTSTR path, REFCLSID clsid, IBaseFilter** ppBF);
extern HRESULT LoadExternalPropertyPage(IPersist* pP, REFCLSID clsid, IPropertyPage** ppPP);
extern bool UnloadUnusedExternalObjects();
extern void ExtendMaxPathLengthIfNeeded(tstring& path, int max_length = MAX_PATH, bool no_url = false);
extern bool ContainsWildcard(tstring& path);
extern void ShortenLongPath(tstring& path);
extern tstring MakeFullPath(LPCTSTR path);
extern bool GetMediaTypeFourCC(const GUID& guid, tstring& fourCC);
extern tstring GetMediaTypeName(const GUID& guid);
extern GUID GUIDFromCString(tstring str);
extern HRESULT GUIDFromCString(tstring str, GUID& guid);
extern tstring CStringFromGUID(const GUID& guid);
extern std::wstring UTF8To16(LPCSTR utf8);
extern std::string UTF16To8(LPCWSTR utf16);
extern std::wstring UTF8ToStringW(const char* S);
extern std::wstring LocalToStringW(const char* S);
extern bool DeleteRegKey(LPCTSTR pszKey, LPCTSTR pszSubkey);
extern bool SetRegKeyValue(LPCTSTR pszKey, LPCTSTR pszSubkey, LPCTSTR pszValueName, LPCTSTR pszValue);
extern bool SetRegKeyValue(LPCTSTR pszKey, LPCTSTR pszSubkey, LPCTSTR pszValue);
inline bool SetRegKeyValue(const tstring& key, const tstring& subkey, const tstring& valueName, const tstring& value)
{
    return SetRegKeyValue(key.c_str(), subkey.c_str(), valueName.c_str(), value.c_str());
}
inline bool SetRegKeyValue(const tstring& key, const tstring& subkey, LPCTSTR valueName, const tstring& value)
{
    return SetRegKeyValue(key.c_str(), subkey.c_str(), valueName, value.c_str());
}
inline bool SetRegKeyValue(const tstring& key, const tstring& subkey, LPCTSTR valueName, LPCTSTR value)
{
    return SetRegKeyValue(key.c_str(), subkey.c_str(), valueName, value);
}
extern void RegisterSourceFilter(const CLSID& clsid, const GUID& subtype2, LPCTSTR chkbytes, LPCTSTR ext = nullptr, ...);
extern void RegisterSourceFilter(const CLSID& clsid, const GUID& subtype2, const std::list<tstring>& chkbytes, LPCTSTR ext = nullptr, ...);
extern void UnRegisterSourceFilter(const GUID& subtype);
extern LPCTSTR GetDXVAMode(const GUID* guidDecoder);
extern tstring ReftimeToString(const REFERENCE_TIME& rtVal);
extern tstring ReftimeToString2(const REFERENCE_TIME& rtVal);
extern tstring DVDtimeToString(const DVD_HMSF_TIMECODE& rtVal, bool bAlwaysShowHours = false);
extern REFERENCE_TIME StringToReftime(LPCTSTR strVal);
extern void SetThreadName(DWORD dwThreadID, LPCSTR szThreadName);
extern tstring FindCoverArt(const tstring& path, const tstring& author);
extern tstring NormalizeUnicodeStrForSearch(tstring srcStr, LANGID langid);
extern bool FindStringInList(const std::list<tstring>& list, tstring& value);

extern inline const LONGLONG GetPerfCounter();

enum FF_FIELD_TYPE {
    PICT_NONE,
    PICT_TOP_FIELD,
    PICT_BOTTOM_FIELD,
    PICT_FRAME
};

class CPinInfo : public PIN_INFO
{
public:
    CPinInfo() {
        pFilter = nullptr;
    }
    ~CPinInfo() {
        if (pFilter) {
            pFilter->Release();
        }
    }
};

class CFilterInfo : public FILTER_INFO
{
public:
    CFilterInfo() {
        pGraph = nullptr;
    }
    ~CFilterInfo() {
        if (pGraph) {
            pGraph->Release();
        }
    }
};

#define BeginEnumFilters(pFilterGraph, pEnumFilters, pBaseFilter)                                                      \
{                                                                                                                      \
    ComPtr<IEnumFilters> pEnumFilters;                                                                                \
    if (pFilterGraph && SUCCEEDED(pFilterGraph->EnumFilters(&pEnumFilters))) {                                         \
        for (ComPtr<IBaseFilter> pBaseFilter; S_OK == pEnumFilters->Next(1, &pBaseFilter, 0); pBaseFilter = nullptr) {

#define EndEnumFilters }}}

#define BeginEnumCachedFilters(pGraphConfig, pEnumFilters, pBaseFilter)                                                \
{                                                                                                                      \
    ComPtr<IEnumFilters> pEnumFilters;                                                                                \
    if (pGraphConfig && SUCCEEDED(pGraphConfig->EnumCacheFilter(&pEnumFilters))) {                                     \
        for (ComPtr<IBaseFilter> pBaseFilter; S_OK == pEnumFilters->Next(1, &pBaseFilter, 0); pBaseFilter = nullptr) {

#define EndEnumCachedFilters }}}

#define BeginEnumPins(pBaseFilter, pEnumPins, pPin)                                     \
{                                                                                       \
    ComPtr<IEnumPins> pEnumPins;                                                       \
    if (pBaseFilter && SUCCEEDED(pBaseFilter->EnumPins(&pEnumPins))) {                  \
        for (ComPtr<IPin> pPin; S_OK == pEnumPins->Next(1, &pPin, 0); pPin = nullptr) {

#define EndEnumPins }}}

#define BeginEnumMediaTypes(pPin, pEnumMediaTypes, pMediaType)                                                             \
{                                                                                                                          \
    ComPtr<IEnumMediaTypes> pEnumMediaTypes;                                                                              \
    if (pPin && SUCCEEDED(pPin->EnumMediaTypes(&pEnumMediaTypes))) {                                                       \
        AM_MEDIA_TYPE* pMediaType = nullptr;                                                                               \
        for (; S_OK == pEnumMediaTypes->Next(1, &pMediaType, nullptr); DeleteMediaType(pMediaType), pMediaType = nullptr) {

#define EndEnumMediaTypes(pMediaType)                                                                               \
        }                                                                                                           \
        if (pMediaType) {                                                                                           \
            DeleteMediaType(pMediaType);                                                                            \
        }                                                                                                           \
    }                                                                                                               \
}

#define BeginEnumSysDev(clsid, pMoniker)                                                                            \
{                                                                                                                   \
    ComPtr<ICreateDevEnum> pDevEnum4$##clsid;                                                                      \
    if (SUCCEEDED(pDevEnum4$##CoCreateInstance(CLSID_SystemDeviceEnum, nullptr, CLSCTX_ALL, IID_PPV_ARGS(&clsid)))) {                                    \
        ComPtr<IEnumMoniker> pClassEnum4$##clsid;                                                                  \
        if (SUCCEEDED(pDevEnum4$##clsid->CreateClassEnumerator(clsid, &pClassEnum4$##clsid, 0))                     \
            && pClassEnum4$##clsid) {                                                                               \
            for (ComPtr<IMoniker> pMoniker; pClassEnum4$##clsid->Next(1, &pMoniker, 0) == S_OK; pMoniker = nullptr) {

#define EndEnumSysDev }}}}

#define PauseGraph                                                                                              \
    auto _pMC = ComQuery<IMediaControl>(m_pGraph);                                                                    \
    OAFilterState _fs = -1;                                                                                     \
    if (_pMC)                                                                                                   \
        _pMC->GetState(1000, &_fs);                                                                             \
    if (_fs == State_Running)                                                                                   \
        _pMC->Pause();                                                                                          \
                                                                                                                \
    HRESULT _hr = E_FAIL;                                                                                       \
    auto _pMS = ComQuery<IMediaSeeking>((IUnknown*)(INonDelegatingUnknown*)m_pGraph);                                 \
    REFERENCE_TIME _rtNow = 0;                                                                                  \
    if (_pMS)                                                                                                   \
        _hr = _pMS->GetCurrentPosition(&_rtNow);

#define ResumeGraph                                                                                             \
    if (SUCCEEDED(_hr) && _pMS && _fs != State_Stopped)                                                         \
        _hr = _pMS->SetPositions(&_rtNow, AM_SEEKING_AbsolutePositioning, nullptr, AM_SEEKING_NoPositioning);   \
                                                                                                                \
    if (_fs == State_Running && _pMS)                                                                           \
        _pMC->Run();

#define CallQueue(call)         \
    if (!m_pOutputQueue)        \
            return NOERROR;     \
    m_pOutputQueue->##call;     \
    return NOERROR;

#define QI(i)  (riid == __uuidof(i)) ? GetInterface((i*)this, ppv) :
#define QI2(i) (riid == IID_##i) ? GetInterface((i*)this, ppv) :

#define SAFE_DELETE(p)       { if (p) { delete (p);     (p) = nullptr; } }
#define SAFE_DELETE_ARRAY(p) { if (p) { delete [] (p);  (p) = nullptr; } }
#define SAFE_RELEASE(p)      { if (p) { (p)->Release(); (p) = nullptr; } }
#define SAFE_CLOSE_HANDLE(p) { if (p) { if ((p) != INVALID_HANDLE_VALUE) VERIFY(CloseHandle(p)); (p) = nullptr; } }
#define EXIT_ON_ERROR(hres)  { if (FAILED(hres)) return hres; }

#define StrRes(id)  MAKEINTRESOURCE((id))
#define ResStr(id)  tstring(StrRes((id)))

#define UNREACHABLE_CODE()  \
    do {                    \
        ASSERT(false);      \
        __assume(false);    \
    } while (false)

template <class T>
static CUnknown* WINAPI CreateInstance(LPUNKNOWN lpunk, HRESULT* phr)
{
    *phr = S_OK;
    CUnknown* punk = new T(lpunk, phr);
    if (punk == nullptr) {
        *phr = E_OUTOFMEMORY;
    }
    return punk;
}

template <class T>
typename std::enable_if<std::is_unsigned<T>::value, T>::type GCD(T a, T b)
{
    static_assert(std::is_integral<T>::value, "GCD supports integral types only");
    if (a == 0 || b == 0) {
        return std::max(std::max(a, b), T(1));
    }
    while (a != b) {
        if (a < b) {
            b -= a;
        } else if (a > b) {
            a -= b;
        }
    }
    return a;
}

template <class T>
typename std::enable_if<std::is_signed<T>::value, T>::type GCD(T a, T b)
{
    using uT = typename std::make_unsigned<T>::type;

    return T(GCD(uT(std::abs(a)), uT(std::abs(b))));
}

template<class T>
constexpr typename std::enable_if<std::is_integral<T>::value, bool>::type IsEqual(T a, T b)
{
    return a == b;
}

template<class T>
constexpr typename std::enable_if<std::is_floating_point<T>::value, bool>::type IsEqual(T a, T b)
{
    return std::abs(a - b) < std::numeric_limits<T>::epsilon();
}

template<class T>
constexpr typename std::enable_if<std::is_floating_point<T>::value, bool>::type IsNearlyEqual(T a, T b, T epsilon)
{
    return std::abs(a - b) < epsilon;
}

template <class T>
constexpr typename std::enable_if < std::is_integral<T>::value&& std::is_unsigned<T>::value, int >::type SGN(T n)
{
    return T(0) < n;
}

template <typename T>
constexpr typename std::enable_if < std::is_integral<T>::value&& std::is_signed<T>::value, int >::type SGN(T n)
{
    return (T(0) < n) - (n < T(0));
}

template <typename T>
constexpr typename std::enable_if <std::is_floating_point<T>::value, int>::type SGN(T n)
{
    return IsEqual(n, T(0)) ? 0 : (n > 0 ? 1 : -1);
}

namespace CStringUtils
{
    struct IgnoreCaseLess {
        bool operator()(const tstring& str1, const tstring& str2) const {
            return StringICompare(str1, str2) < 0;
        }
    };
    struct LogicalLess {
        bool operator()(const tstring& str1, const tstring& str2) const {
            return StrCmpLogicalW(str1.c_str(), str2.c_str()) < 0;
        }
    };
}
