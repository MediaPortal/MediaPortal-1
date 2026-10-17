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

#include "stdafx.h"
#include <algorithm>
#include <Vfw.h>
#include "winddk/devioctl.h"
#include "winddk/ntddcdrm.h"
#include "DSUtil.h"
#include "Mpeg2Def.h"
#include <emmintrin.h>
#include <d3d9.h>
#include "NullRenderers.h"
#include "mvrInterfaces.h"

#include "moreuuids.h"
#include <dxva.h>
#include <dxva2api.h>
#include <locale.h>

int CountPins(IBaseFilter* pBF, int& nIn, int& nOut, int& nInC, int& nOutC)
{
    nIn = nOut = 0;
    nInC = nOutC = 0;

    BeginEnumPins(pBF, pEP, pPin) {
        PIN_DIRECTION dir;
        if (SUCCEEDED(pPin->QueryDirection(&dir))) {
            ComPtr<IPin> pPinConnectedTo;
            pPin->ConnectedTo(&pPinConnectedTo);

            if (dir == PINDIR_INPUT) {
                nIn++;
                if (pPinConnectedTo) {
                    nInC++;
                }
            } else if (dir == PINDIR_OUTPUT) {
                nOut++;
                if (pPinConnectedTo) {
                    nOutC++;
                }
            }
        }
    }
    EndEnumPins;

    return (nIn + nOut);
}

bool IsSplitter(IBaseFilter* pBF, bool fCountConnectedOnly)
{
    int nIn, nOut, nInC, nOutC;
    CountPins(pBF, nIn, nOut, nInC, nOutC);
    return (fCountConnectedOnly ? nOutC > 1 : nOut > 1);
}

bool IsMultiplexer(IBaseFilter* pBF, bool fCountConnectedOnly)
{
    int nIn, nOut, nInC, nOutC;
    CountPins(pBF, nIn, nOut, nInC, nOutC);
    return (fCountConnectedOnly ? nInC > 1 : nIn > 1);
}

bool IsStreamStart(IBaseFilter* pBF)
{
    auto pAMMF = ComQuery<IAMFilterMiscFlags>(pBF);
    if (pAMMF && pAMMF->GetMiscFlags()&AM_FILTER_MISC_FLAGS_IS_SOURCE) {
        return true;
    }

    int nIn, nOut, nInC, nOutC;
    CountPins(pBF, nIn, nOut, nInC, nOutC);
    AM_MEDIA_TYPE mt;
    ComPtr<IPin> pIn = GetFirstPin(pBF);
    return ((nOut > 1)
            || (nOut > 0 && nIn == 1 && pIn && SUCCEEDED(pIn->ConnectionMediaType(&mt)) && mt.majortype == MEDIATYPE_Stream));
}

bool IsStreamEnd(IBaseFilter* pBF)
{
    int nIn, nOut, nInC, nOutC;
    CountPins(pBF, nIn, nOut, nInC, nOutC);
    return (nOut == 0);
}

bool IsVideoRenderer(IBaseFilter* pBF)
{
    int nIn, nOut, nInC, nOutC;
    CountPins(pBF, nIn, nOut, nInC, nOutC);

    if (nInC > 0 && nOut == 0) {
        BeginEnumPins(pBF, pEP, pPin) {
            AM_MEDIA_TYPE mt;
            if (S_OK != pPin->ConnectionMediaType(&mt)) {
                continue;
            }

            FreeMediaType(mt);

            return !!(mt.majortype == MEDIATYPE_Video);
            /*&& (mt.formattype == FORMAT_VideoInfo || mt.formattype == FORMAT_VideoInfo2));*/
        }
        EndEnumPins;
    }

    CLSID clsid;
    memcpy(&clsid, &GUID_NULL, sizeof(clsid));
    pBF->GetClassID(&clsid);

    return (clsid == CLSID_VideoRenderer || clsid == CLSID_VideoRendererDefault || clsid == CLSID_VideoMixingRenderer9 || clsid == CLSID_EnhancedVideoRenderer || clsid == CLSID_madVR || clsid == CLSID_DXR || clsid == CLSID_MPCVR);
}

bool IsAudioWaveRenderer(IBaseFilter* pBF)
{
    int nIn, nOut, nInC, nOutC;
    CountPins(pBF, nIn, nOut, nInC, nOutC);

    if (nInC > 0 && nOut == 0 && ComQuery<IBasicAudio>(pBF)) {
        BeginEnumPins(pBF, pEP, pPin) {
            AM_MEDIA_TYPE mt;
            if (S_OK != pPin->ConnectionMediaType(&mt)) {
                continue;
            }

            FreeMediaType(mt);

            return !!(mt.majortype == MEDIATYPE_Audio);
            /*&& mt.formattype == FORMAT_WaveFormatEx);*/
        }
        EndEnumPins;
    }

    CLSID clsid;
    memcpy(&clsid, &GUID_NULL, sizeof(clsid));
    pBF->GetClassID(&clsid);

    return clsid == CLSID_DSoundRender ||
           clsid == CLSID_AudioRender ||
           clsid == CLSID_SANEAR_INTERNAL ||
           clsid == CLSID_SANEAR ||
           clsid == CLSID_ReClock ||
           clsid == CLSID_MPCBEAudioRenderer ||
           clsid == GUIDFromCString(L"{EC9ED6FC-7B03-4cb6-8C01-4EABE109F26B}") || // MediaPortal Audio Renderer
           clsid == GUIDFromCString(L"{50063380-2B2F-4855-9A1E-40FCA344C7AC}") || // Surodev ASIO Renderer
           clsid == GUIDFromCString(L"{8DE31E85-10FC-4088-8861-E0EC8E70744A}") || // MultiChannel ASIO Renderer
           clsid == GUIDFromCString(L"{205F9417-8EEF-40B4-91CF-C7C6A96936EF}") || // MBSE MultiChannel ASIO Renderer
           clsid == __uuidof(CNullAudioRenderer) ||
           clsid == __uuidof(CNullUAudioRenderer);
}

IBaseFilter* GetUpStreamFilter(IBaseFilter* pBF, IPin* pInputPin)
{
    return GetFilterFromPin(GetUpStreamPin(pBF, pInputPin));
}

IPin* GetUpStreamPin(IBaseFilter* pBF, IPin* pInputPin)
{
    BeginEnumPins(pBF, pEP, pPin) {
        if (pInputPin && pInputPin != pPin.Get()) {
            continue;
        }

        PIN_DIRECTION dir;
        ComPtr<IPin> pPinConnectedTo;
        if (SUCCEEDED(pPin->QueryDirection(&dir)) && dir == PINDIR_INPUT
                && SUCCEEDED(pPin->ConnectedTo(&pPinConnectedTo))) {
            IPin* pRet = pPinConnectedTo.Detach();
            pRet->Release();
            return pRet;
        }
    }
    EndEnumPins;

    return nullptr;
}

IPin* GetFirstPin(IBaseFilter* pBF, PIN_DIRECTION dir)
{
    if (pBF) {
        BeginEnumPins(pBF, pEP, pPin) {
            PIN_DIRECTION dir2;
            if (SUCCEEDED(pPin->QueryDirection(&dir2)) && dir == dir2) {
                IPin* pRet = pPin.Detach();
                pRet->Release();
                return pRet;
            }
        }
        EndEnumPins;
    }

    return nullptr;
}

IPin* GetFirstDisconnectedPin(IBaseFilter* pBF, PIN_DIRECTION dir)
{
    if (pBF) {
        BeginEnumPins(pBF, pEP, pPin) {
            PIN_DIRECTION dir2;
            ComPtr<IPin> pPinTo;
            if (SUCCEEDED(pPin->QueryDirection(&dir2)) && dir == dir2 && (S_OK != pPin->ConnectedTo(&pPinTo))) {
                IPin* pRet = pPin.Detach();
                pRet->Release();
                return pRet;
            }
        }
        EndEnumPins;
    }

    return nullptr;
}

IBaseFilter* FindFilter(LPCWSTR clsid, IFilterGraph* pFG)
{
    CLSID clsid2;
    CLSIDFromString(_bstr_t(clsid), &clsid2);
    return FindFilter(clsid2, pFG);
}

IBaseFilter* FindFilter(const CLSID& clsid, IFilterGraph* pFG)
{
    BeginEnumFilters(pFG, pEF, pBF) {
        CLSID clsid2;
        if (SUCCEEDED(pBF->GetClassID(&clsid2)) && clsid == clsid2) {
            return pBF.Get();
        }
    }
    EndEnumFilters;

    return nullptr;
}

IPin* FindPin(IBaseFilter* pBF, PIN_DIRECTION direction, const AM_MEDIA_TYPE* pRequestedMT)
{
    PIN_DIRECTION pindir;
    BeginEnumPins(pBF, pEP, pPin) {
        ComPtr<IPin> pFellow;

        if (SUCCEEDED(pPin->QueryDirection(&pindir)) &&
                pindir == direction &&
                pPin->ConnectedTo(&pFellow) == VFW_E_NOT_CONNECTED) {
            BeginEnumMediaTypes(pPin, pEM, pmt) {
                if (pmt->majortype == pRequestedMT->majortype && pmt->subtype == pRequestedMT->subtype) {
                    return (pPin.Get());
                }
            }
            EndEnumMediaTypes(pmt);
        }
    }
    EndEnumPins;
    return nullptr;
}

std::wstring GetFilterName(IBaseFilter* pBF)
{
    std::wstring name = _T("");

    if (pBF) {
        CFilterInfo fi;
        if (SUCCEEDED(pBF->QueryFilterInfo(&fi))) {
            name = fi.achName;
        }
    }

    return name;
}

std::wstring GetPinName(IPin* pPin)
{
    std::wstring name;
    CPinInfo pi;
    if (pPin && SUCCEEDED(pPin->QueryPinInfo(&pi))) {
        name = pi.achName;
    }

    return name;
}

IFilterGraph* GetGraphFromFilter(IBaseFilter* pBF)
{
    if (!pBF) {
        return nullptr;
    }
    IFilterGraph* pGraph = nullptr;
    CFilterInfo fi;
    if (pBF && SUCCEEDED(pBF->QueryFilterInfo(&fi))) {
        pGraph = fi.pGraph;
    }
    return pGraph;
}

IBaseFilter* GetFilterFromPin(IPin* pPin)
{
    if (!pPin) {
        return nullptr;
    }
    IBaseFilter* pBF = nullptr;
    CPinInfo pi;
    if (pPin && SUCCEEDED(pPin->QueryPinInfo(&pi))) {
        pBF = pi.pFilter;
    }
    return pBF;
}

IPin* AppendFilter(IPin* pPin, tstring DisplayName, IGraphBuilder* pGB)
{
    IPin* pRet = pPin;

    std::vector<ComPtr<IBaseFilter>> pFilters;
    auto isKnown = [&pFilters](IBaseFilter* pFilter) {
        for (const auto& f : pFilters) {
            if (f.Get() == pFilter) {
                return true;
            }
        }
        return false;
    };

    do {
        if (!pPin || DisplayName.empty() || !pGB) {
            break;
        }

        ComPtr<IPin> pPinTo;
        PIN_DIRECTION dir;
        if (FAILED(pPin->QueryDirection(&dir)) || dir != PINDIR_OUTPUT || SUCCEEDED(pPin->ConnectedTo(&pPinTo))) {
            break;
        }

        ComPtr<IBindCtx> pBindCtx;
        CreateBindCtx(0, &pBindCtx);

        ComPtr<IMoniker> pMoniker;
        ULONG chEaten;
        if (S_OK != MkParseDisplayName(pBindCtx.Get(), _bstr_t(DisplayName.c_str()), &chEaten, &pMoniker)) {
            break;
        }

        ComPtr<IBaseFilter> pBF;
        if (FAILED(pMoniker->BindToObject(pBindCtx.Get(), 0, IID_PPV_ARGS(&pBF))) || !pBF) {
            break;
        }

        ComPtr<IPropertyBag> pPB;
        if (FAILED(pMoniker->BindToStorage(pBindCtx.Get(), 0, IID_PPV_ARGS(&pPB)))) {
            break;
        }

        _variant_t var;
        if (FAILED(pPB->Read(_T("FriendlyName"), &var, nullptr))) {
            break;
        }

        pFilters.push_back(pBF);
        BeginEnumFilters(pGB, pEnum, pBF2);
        pFilters.push_back(pBF2);
        EndEnumFilters;

        if (FAILED(pGB->AddFilter(pBF.Get(), var.bstrVal))) {
            break;
        }

        BeginEnumFilters(pGB, pEnum, pBF2);
        if (!isKnown(pBF2.Get()) && SUCCEEDED(pGB->RemoveFilter(pBF2.Get()))) {
            pEnum->Reset();
        }
        EndEnumFilters;

        pPinTo = GetFirstPin(pBF.Get(), PINDIR_INPUT);
        if (!pPinTo) {
            pGB->RemoveFilter(pBF.Get());
            break;
        }

        if (FAILED(pGB->ConnectDirect(pPin, pPinTo.Get(), nullptr))) {
            pGB->Connect(pPin, pPinTo.Get());
            pGB->RemoveFilter(pBF.Get());
            break;
        }

        BeginEnumFilters(pGB, pEnum, pBF2);
        if (!isKnown(pBF2.Get()) && SUCCEEDED(pGB->RemoveFilter(pBF2.Get()))) {
            pEnum->Reset();
        }
        EndEnumFilters;

        pRet = GetFirstPin(pBF.Get(), PINDIR_OUTPUT);
        if (!pRet) {
            pRet = pPin;
            pGB->RemoveFilter(pBF.Get());
            break;
        }
    } while (false);

    return pRet;
}

IPin* InsertFilter(IPin* pPin, tstring DisplayName, IGraphBuilder* pGB)
{
    do {
        if (!pPin || DisplayName.empty() || !pGB) {
            break;
        }

        PIN_DIRECTION dir;
        if (FAILED(pPin->QueryDirection(&dir))) {
            break;
        }

        ComPtr<IPin> pFrom, pTo;

        if (dir == PINDIR_INPUT) {
            pPin->ConnectedTo(&pFrom);
            pTo = pPin;
        } else if (dir == PINDIR_OUTPUT) {
            pFrom = pPin;
            pPin->ConnectedTo(&pTo);
        }

        if (!pFrom || !pTo) {
            break;
        }

        ComPtr<IBindCtx> pBindCtx;
        CreateBindCtx(0, &pBindCtx);

        ComPtr<IMoniker> pMoniker;
        ULONG chEaten;
        if (S_OK != MkParseDisplayName(pBindCtx.Get(), _bstr_t(DisplayName.c_str()), &chEaten, &pMoniker)) {
            break;
        }

        ComPtr<IBaseFilter> pBF;
        if (FAILED(pMoniker->BindToObject(pBindCtx.Get(), 0, IID_PPV_ARGS(&pBF))) || !pBF) {
            break;
        }

        ComPtr<IPropertyBag> pPB;
        if (FAILED(pMoniker->BindToStorage(pBindCtx.Get(), 0, IID_PPV_ARGS(&pPB)))) {
            break;
        }

        _variant_t var;
        if (FAILED(pPB->Read(_T("FriendlyName"), &var, nullptr))) {
            break;
        }

        if (FAILED(pGB->AddFilter(pBF.Get(), var.bstrVal))) {
            break;
        }

        ComPtr<IPin> pFromTo = GetFirstPin(pBF.Get(), PINDIR_INPUT);
        if (!pFromTo) {
            pGB->RemoveFilter(pBF.Get());
            break;
        }

        if (FAILED(pGB->Disconnect(pFrom.Get())) || FAILED(pGB->Disconnect(pTo.Get()))) {
            pGB->RemoveFilter(pBF.Get());
            pGB->ConnectDirect(pFrom.Get(), pTo.Get(), nullptr);
            break;
        }

        if (FAILED(pGB->ConnectDirect(pFrom.Get(), pFromTo.Get(), nullptr))) {
            pGB->RemoveFilter(pBF.Get());
            pGB->ConnectDirect(pFrom.Get(), pTo.Get(), nullptr);
            break;
        }

        ComPtr<IPin> pToFrom = GetFirstPin(pBF.Get(), PINDIR_OUTPUT);
        if (!pToFrom) {
            pGB->RemoveFilter(pBF.Get());
            pGB->ConnectDirect(pFrom.Get(), pTo.Get(), nullptr);
            break;
        }

        if (FAILED(pGB->ConnectDirect(pToFrom.Get(), pTo.Get(), nullptr))) {
            pGB->RemoveFilter(pBF.Get());
            pGB->ConnectDirect(pFrom.Get(), pTo.Get(), nullptr);
            break;
        }

        pPin = pToFrom.Get();
    } while (false);

    return pPin;
}

void ExtractMediaTypes(IPin* pPin, std::vector<GUID>& types)
{
    types.clear();

    BeginEnumMediaTypes(pPin, pEM, pmt) {
        bool fFound = false;

        for (ptrdiff_t i = 0; !fFound && i < (int)types.size(); i += 2) {
            if (types[i] == pmt->majortype && types[i + 1] == pmt->subtype) {
                fFound = true;
            }
        }

        if (!fFound) {
            types.push_back(pmt->majortype);
            types.push_back(pmt->subtype);
        }
    }
    EndEnumMediaTypes(pmt);
}

void ExtractMediaTypes(IPin* pPin, std::list<CMediaType>& mts)
{
    mts.clear();

    BeginEnumMediaTypes(pPin, pEM, pmt) {
        bool fFound = false;

        for (const CMediaType& mt : mts) {
            if (mt.majortype == pmt->majortype && mt.subtype == pmt->subtype) {
                fFound = true;
                break;
            }
        }

        if (!fFound) {
            mts.push_back(CMediaType(*pmt));
        }
    }
    EndEnumMediaTypes(pmt);
}

int Eval_Exception(int n_except)
{
    if (n_except == STATUS_ACCESS_VIOLATION) {
        MessageBox(GetActiveWindow(), _T("The property page of this filter has just caused a\nmemory access violation. The application will gently die now :)"), nullptr, MB_OK);
    }

    return EXCEPTION_CONTINUE_SEARCH;
}

void MyOleCreatePropertyFrame(HWND hwndOwner, UINT x, UINT y, LPCOLESTR lpszCaption, ULONG cObjects, LPUNKNOWN FAR* lplpUnk, ULONG cPages, LPCLSID lpPageClsID, LCID lcid, DWORD dwReserved, LPVOID lpvReserved)
{
    __try {
        OleCreatePropertyFrame(hwndOwner, x, y, lpszCaption, cObjects, lplpUnk, cPages, lpPageClsID, lcid, dwReserved, lpvReserved);
    } __except (Eval_Exception(GetExceptionCode())) {
        // No code; this block never executed.
    }
}

void ShowPPage(tstring DisplayName, HWND hParentWnd)
{
    ComPtr<IBindCtx> pBindCtx;
    CreateBindCtx(0, &pBindCtx);

    ComPtr<IMoniker> pMoniker;
    ULONG chEaten;
    if (S_OK != MkParseDisplayName(pBindCtx.Get(), std::wstring(DisplayName).c_str(), &chEaten, &pMoniker)) {
        return;
    }

    ComPtr<IBaseFilter> pBF;
    if (FAILED(pMoniker->BindToObject(pBindCtx.Get(), 0, IID_PPV_ARGS(&pBF))) || !pBF) {
        return;
    }

    ShowPPage(pBF.Get(), hParentWnd);
}

void ShowPPage(IUnknown* pUnk, HWND hParentWnd)
{
    auto pSPP = ComQuery<ISpecifyPropertyPages>(pUnk);
    if (!pSPP) {
        return;
    }

    tstring str;

    auto pBF = ComQuery<IBaseFilter>(pSPP);
    CFilterInfo fi;
    auto pPin = ComQuery<IPin>(pSPP);
    CPinInfo pi;
    if (pBF && SUCCEEDED(pBF->QueryFilterInfo(&fi))) {
        str = fi.achName;
    } else if (pPin && SUCCEEDED(pPin->QueryPinInfo(&pi))) {
        str = pi.achName;
    }

    CAUUID caGUID;
    caGUID.pElems = nullptr;
    if (SUCCEEDED(pSPP->GetPages(&caGUID))) {
        ComPtr<IUnknown> lpUnk;
        pSPP.As(&lpUnk);
        MyOleCreatePropertyFrame(
            hParentWnd, 0, 0, StringToW(str).c_str(),
            1, lpUnk.GetAddressOf(),
            caGUID.cElems, caGUID.pElems,
            0, 0, nullptr);

        if (caGUID.pElems) {
            CoTaskMemFree(caGUID.pElems);
        }
    }
}

CLSID GetCLSID(IBaseFilter* pBF)
{
    CLSID clsid = GUID_NULL;
    if (pBF) {
        pBF->GetClassID(&clsid);
    }
    return clsid;
}

CLSID GetCLSID(IPin* pPin)
{
    return GetCLSID(GetFilterFromPin(pPin));
}

tstring CLSIDToString(CLSID& clsid)
{
    LPOLESTR pStr = nullptr;
    tstring str;
    if (S_OK == StringFromCLSID(clsid, &pStr) && pStr) {
        str = pStr;
    }
    CoTaskMemFree(pStr);
    return str;
}

static bool RegistryKeyExists(HKEY root, const tstring& subKey)
{
    HKEY key = nullptr;
    if (RegOpenKeyEx(root, subKey.c_str(), 0, KEY_READ, &key) != ERROR_SUCCESS) {
        return false;
    }
    RegCloseKey(key);
    return true;
}

// the default value of a key, empty when there is none
static tstring RegistryDefaultString(HKEY root, const tstring& subKey)
{
    TCHAR buff[MAX_PATH] = {};
    DWORD size = sizeof(buff) - sizeof(TCHAR);
    if (RegGetValue(root, subKey.c_str(), nullptr, RRF_RT_REG_SZ | RRF_RT_REG_EXPAND_SZ, nullptr, buff, &size) != ERROR_SUCCESS) {
        return tstring();
    }
    return tstring(buff);
}

bool IsCLSIDRegistered(LPCTSTR clsid)
{
    tstring rootkey1(_T("CLSID\\"));
    tstring rootkey2(_T("CLSID\\{083863F1-70DE-11d0-BD40-00A0C911CE86}\\Instance\\"));

    return RegistryKeyExists(HKEY_CLASSES_ROOT, rootkey1 + clsid)
           || RegistryKeyExists(HKEY_CLASSES_ROOT, rootkey2 + clsid);
}

bool IsCLSIDRegistered(const CLSID& clsid)
{
    bool fRet = false;

    LPOLESTR pStr = nullptr;
    if (S_OK == StringFromCLSID(clsid, &pStr) && pStr) {
        fRet = IsCLSIDRegistered(StringToT(pStr).c_str());
    }
    CoTaskMemFree(pStr);

    return fRet;
}

tstring GetFilterPath(LPCTSTR clsid)
{
    tstring rootkey1(_T("CLSID\\"));
    tstring rootkey2(_T("CLSID\\{083863F1-70DE-11d0-BD40-00A0C911CE86}\\Instance\\"));

    tstring path = RegistryDefaultString(HKEY_CLASSES_ROOT, rootkey1 + clsid + _T("\\InprocServer32"));
    if (path.empty()) {
        path = RegistryDefaultString(HKEY_CLASSES_ROOT, rootkey2 + clsid + _T("\\InprocServer32"));
    }

    return path;
}

tstring GetFilterPath(const CLSID& clsid)
{
    tstring path;

    LPOLESTR pStr = nullptr;
    if (S_OK == StringFromCLSID(clsid, &pStr) && pStr) {
        path = GetFilterPath(StringToT(pStr).c_str());
    }
    CoTaskMemFree(pStr);

    return path;
}

void CStringToBin(tstring str, std::vector<BYTE>& data)
{
    StringTrim(str);
    ASSERT((int(str.size()) & 1) == 0);
    data.resize(int(str.size()) / 2);

    BYTE b = 0;

    StringUpper(str);
    for (int i = 0, j = int(str.size()); i < j; i++) {
        TCHAR c = str[i];
        if (c >= _T('0') && c <= _T('9')) {
            if (!(i & 1)) {
                b = ((char(c - _T('0')) << 4) & 0xf0) | (b & 0x0f);
            } else {
                b = (char(c - _T('0')) & 0x0f) | (b & 0xf0);
            }
        } else if (c >= _T('A') && c <= _T('F')) {
            if (!(i & 1)) {
                b = ((char(c - _T('A') + 10) << 4) & 0xf0) | (b & 0x0f);
            } else {
                b = (char(c - _T('A') + 10) & 0x0f) | (b & 0xf0);
            }
        } else {
            break;
        }

        if (i & 1) {
            data[i >> 1] = b;
            b = 0;
        }
    }
}

tstring BinToCString(const BYTE* ptr, size_t len)
{
    tstring ret;

    while (len-- > 0) {
        TCHAR high, low;

        high = (*ptr >> 4) >= 10 ? (*ptr >> 4) - 10 + _T('A') : (*ptr >> 4) + _T('0');
        low = (*ptr & 0xf) >= 10 ? (*ptr & 0xf) - 10 + _T('A') : (*ptr & 0xf) + _T('0');

        ret += ::Format(_T("%c%c"), high, low);

        ptr++;
    }

    return ret;
}

void FindFiles(tstring fn, std::list<tstring>& files)
{
    ExtendMaxPathLengthIfNeeded(fn, MAX_PATH);
    tstring path = fn;
    StringReplace(path, '/', '\\');
    path = StringLeft(path, StringRFind(path, '\\') + 1);

    WIN32_FIND_DATA findData;
    HANDLE h = FindFirstFile(fn.c_str(), &findData);
    if (h != INVALID_HANDLE_VALUE) {
        do {
            files.push_back(path + findData.cFileName);
        } while (FindNextFile(h, &findData));

        FindClose(h);
    }
}

OpticalDiskType_t GetOpticalDiskType(TCHAR drive, std::list<tstring>& files)
{
    files.clear();

    tstring path;
    path = ::Format(_T("%c:"), drive);

    if (GetDriveType((path + _T("\\")).c_str()) != DRIVE_CDROM) {
        return OpticalDisk_NotFound;
    }

    // Check if it contains a disc
    HANDLE hDevice = CreateFile((tstring(_T("\\\\.\\")) + path).c_str(), FILE_READ_ATTRIBUTES,
        FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);
    if (hDevice == INVALID_HANDLE_VALUE) {
        return OpticalDisk_NotFound;
    }
    DWORD cbBytesReturned;
    BOOL bSuccess = DeviceIoControl(hDevice, IOCTL_STORAGE_CHECK_VERIFY2,
        NULL, 0, NULL, 0, &cbBytesReturned, (LPOVERLAPPED)NULL);
    if (!bSuccess) {
        return OpticalDisk_NotFound;
    }

    // CDROM_DVDVideo
    FindFiles(path + _T("\\VIDEO_TS\\video_ts.ifo"), files);
    if (!files.empty()) {
        return OpticalDisk_DVDVideo;
    }

    // CDROM_BD
    FindFiles(path + _T("\\BDMV\\index.bdmv"), files);
    if (!files.empty()) {
        return OpticalDisk_BD;
    }

    // CDROM_VideoCD
    FindFiles(path + _T("\\mpegav\\avseq??.dat"), files);
    FindFiles(path + _T("\\mpegav\\avseq??.mpg"), files);
    FindFiles(path + _T("\\mpeg2\\avseq??.dat"), files);
    FindFiles(path + _T("\\mpeg2\\avseq??.mpg"), files);
    FindFiles(path + _T("\\mpegav\\music??.dat"), files);
    FindFiles(path + _T("\\mpegav\\music??.mpg"), files);
    FindFiles(path + _T("\\mpeg2\\music??.dat"), files);
    FindFiles(path + _T("\\mpeg2\\music??.mpg"), files);
    if (!files.empty()) {
        return OpticalDisk_VideoCD;
    }

    // CDROM_Audio
    HANDLE hDrive = CreateFile((tstring(_T("\\\\.\\")) + path).c_str(), GENERIC_READ, FILE_SHARE_READ, nullptr,
                                OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, (HANDLE)nullptr);
    if (hDrive != INVALID_HANDLE_VALUE) {
        DWORD BytesReturned;
        CDROM_TOC TOC;
        if (DeviceIoControl(hDrive, IOCTL_CDROM_READ_TOC, nullptr, 0, &TOC, sizeof(TOC), &BytesReturned, 0)) {
            ASSERT(TOC.FirstTrack >= 1u && TOC.LastTrack <= _countof(TOC.TrackData));
            TOC.FirstTrack = std::max(TOC.FirstTrack, UCHAR(1));
            TOC.LastTrack = std::min(TOC.LastTrack, UCHAR(_countof(TOC.TrackData)));
            for (ptrdiff_t i = TOC.FirstTrack; i <= TOC.LastTrack; i++) {
                // MMC-3 Draft Revision 10g: Table 222 - Q Sub-channel control field
                auto& trackData = TOC.TrackData[i - 1];
                trackData.Control &= 5;
                if (trackData.Control == 0 || trackData.Control == 1) {
                    tstring fn;
                    fn = ::Format(_T("%s\\track%02Id.cda"), path.c_str(), i);
                    files.push_back(fn);
                }
            }
        }

        CloseHandle(hDrive);
    }
    if (!files.empty()) {
        return OpticalDisk_Audio;
    }

    // it is a cdrom but nothing special
    return OpticalDisk_Unknown;
}

tstring GetDriveLabel(TCHAR drive)
{
    tstring path;
    path = ::Format(_T("%c:\\"), drive);

    return GetDriveLabel(path);
}

tstring GetDriveLabel(tstring path)
{
    tstring label;
    PathStripToRoot(StringBuffer(path, MAX_PATH));
    StringRelease(path);

    TCHAR VolumeNameBuffer[MAX_PATH], FileSystemNameBuffer[MAX_PATH];
    DWORD VolumeSerialNumber, MaximumComponentLength, FileSystemFlags;
    if (GetVolumeInformation(path.c_str(),
                             VolumeNameBuffer, MAX_PATH, &VolumeSerialNumber, &MaximumComponentLength,
                             &FileSystemFlags, FileSystemNameBuffer, MAX_PATH)) {
        label = VolumeNameBuffer;
    }

    return label;
}

bool IsDriveVirtual(tstring drive)
{
    HKEY hkey;
    DWORD type = REG_BINARY;
    TCHAR data[1024] = { 0 };
    DWORD size = sizeof(data) - 2;

    drive=StringLeft((drive+_T(":")), 2);
    tstring subkey = _T("\\DosDevices\\") + drive;

    RegOpenKeyEx(HKEY_LOCAL_MACHINE, _T("SYSTEM\\MountedDevices"), 0, KEY_READ, &hkey);
    if (hkey == INVALID_HANDLE_VALUE) return 0;
    RegQueryValueEx(hkey, subkey.c_str(), 0, &type, (BYTE*)data, &size);

    RegCloseKey(hkey);
    tstring sig(data);
    StringUpper(sig);
    return StringFind(sig, _T("VEN_MSFT&PROD_VIRTUAL_DVD-ROM")) >= 0
        || StringFind(sig, _T("VEN_DISCSOFT&")) >= 0
        || StringFind(sig, _T("VEN_ELBY&PROD_CLONEDRIVE")) >= 0;
}

DVD_HMSF_TIMECODE RT2HMSF(REFERENCE_TIME rt, double fps /*= 0.0*/) // use to remember the current position
{
    DVD_HMSF_TIMECODE hmsf = {
        (BYTE)((rt / 10000000 / 60 / 60)),
        (BYTE)((rt / 10000000 / 60) % 60),
        (BYTE)((rt / 10000000) % 60),
        (BYTE)(1.0 * ((rt / 10000) % 1000) * fps / 1000)
    };

    return hmsf;
}

DVD_HMSF_TIMECODE RT2HMS(REFERENCE_TIME rt) // used only for information (for display on the screen)
{
    rt = rt / 10000000;
    DVD_HMSF_TIMECODE hmsf = {
        (BYTE)(rt / 3600),
        (BYTE)(rt / 60 % 60),
        (BYTE)(rt % 60),
        0
    };

    return hmsf;
}

DVD_HMSF_TIMECODE RT2HMS_r(REFERENCE_TIME rt) // used only for information (for display on the screen)
{
    // round to nearest second
    return RT2HMS(rt + 5000000);
}

REFERENCE_TIME HMSF2RT(DVD_HMSF_TIMECODE hmsf, double fps /*= -1.0*/)
{
    if (fps <= 0.0) {
        hmsf.bFrames = 0;
        fps = 1.0;
    }
    return (REFERENCE_TIME)((((REFERENCE_TIME)hmsf.bHours * 60 + hmsf.bMinutes) * 60 + hmsf.bSeconds) * 1000 + 1.0 * hmsf.bFrames * 1000 / fps) * 10000;
}

void memsetd(void* dst, unsigned int c, size_t nbytes)
{
    size_t n = nbytes / 4;
    __stosd((unsigned long*)dst, c, n);
}

void memsetw(void* dst, unsigned short c, size_t nbytes)
{
    memsetd(dst, c << 16 | c, nbytes);

    size_t n = nbytes / 2;
    size_t o = (n / 2) * 2;
    if ((n - o) == 1) {
        ((WORD*)dst)[o] = c;
    }
}

bool ExtractBIH(const AM_MEDIA_TYPE* pmt, BITMAPINFOHEADER* bih)
{
    if (pmt && bih) {
        ZeroMemory(bih, sizeof(*bih));

        if (pmt->formattype == FORMAT_VideoInfo) {
            VIDEOINFOHEADER* vih = (VIDEOINFOHEADER*)pmt->pbFormat;
            memcpy(bih, &vih->bmiHeader, sizeof(BITMAPINFOHEADER));
            return true;
        } else if (pmt->formattype == FORMAT_VideoInfo2) {
            VIDEOINFOHEADER2* vih = (VIDEOINFOHEADER2*)pmt->pbFormat;
            memcpy(bih, &vih->bmiHeader, sizeof(BITMAPINFOHEADER));
            return true;
        } else if (pmt->formattype == FORMAT_MPEGVideo) {
            VIDEOINFOHEADER* vih = &((MPEG1VIDEOINFO*)pmt->pbFormat)->hdr;
            memcpy(bih, &vih->bmiHeader, sizeof(BITMAPINFOHEADER));
            return true;
        } else if (pmt->formattype == FORMAT_MPEG2_VIDEO) {
            VIDEOINFOHEADER2* vih = &((MPEG2VIDEOINFO*)pmt->pbFormat)->hdr;
            memcpy(bih, &vih->bmiHeader, sizeof(BITMAPINFOHEADER));
            return true;
        } else if (pmt->formattype == FORMAT_DiracVideoInfo) {
            VIDEOINFOHEADER2* vih = &((DIRACINFOHEADER*)pmt->pbFormat)->hdr;
            memcpy(bih, &vih->bmiHeader, sizeof(BITMAPINFOHEADER));
            return true;
        }
    }

    return false;
}

bool ExtractAvgTimePerFrame(const AM_MEDIA_TYPE* pmt, REFERENCE_TIME& rtAvgTimePerFrame)
{
    if (pmt->formattype == FORMAT_VideoInfo) {
        rtAvgTimePerFrame = ((VIDEOINFOHEADER*)pmt->pbFormat)->AvgTimePerFrame;
    } else if (pmt->formattype == FORMAT_VideoInfo2) {
        rtAvgTimePerFrame = ((VIDEOINFOHEADER2*)pmt->pbFormat)->AvgTimePerFrame;
    } else if (pmt->formattype == FORMAT_MPEGVideo) {
        rtAvgTimePerFrame = ((MPEG1VIDEOINFO*)pmt->pbFormat)->hdr.AvgTimePerFrame;
    } else if (pmt->formattype == FORMAT_MPEG2Video) {
        rtAvgTimePerFrame = ((MPEG2VIDEOINFO*)pmt->pbFormat)->hdr.AvgTimePerFrame;
    } else {
        return false;
    }

    return true;
}

bool ExtractBIH(IMediaSample* pMS, BITMAPINFOHEADER* bih)
{
    AM_MEDIA_TYPE* pmt = nullptr;
    if (SUCCEEDED(pMS->GetMediaType(&pmt)) && pmt) {
        bool fRet = ExtractBIH(pmt, bih);
        DeleteMediaType(pmt);
        return fRet;
    }

    return false;
}

bool ExtractDim(const AM_MEDIA_TYPE* pmt, int& w, int& h, int& arx, int& ary)
{
    w = h = arx = ary = 0;

    if (pmt->formattype == FORMAT_VideoInfo || pmt->formattype == FORMAT_MPEGVideo) {
        VIDEOINFOHEADER* vih = (VIDEOINFOHEADER*)pmt->pbFormat;
        w = vih->bmiHeader.biWidth;
        h = abs(vih->bmiHeader.biHeight);
        arx = w * vih->bmiHeader.biYPelsPerMeter;
        ary = h * vih->bmiHeader.biXPelsPerMeter;
    } else if (pmt->formattype == FORMAT_VideoInfo2 || pmt->formattype == FORMAT_MPEG2_VIDEO || pmt->formattype == FORMAT_DiracVideoInfo) {
        VIDEOINFOHEADER2* vih = (VIDEOINFOHEADER2*)pmt->pbFormat;
        w = vih->bmiHeader.biWidth;
        h = abs(vih->bmiHeader.biHeight);
        arx = vih->dwPictAspectRatioX;
        ary = vih->dwPictAspectRatioY;
    } else {
        return false;
    }

    if (!arx || !ary) {
        BYTE* ptr = nullptr;
        DWORD len = 0;

        if (pmt->formattype == FORMAT_MPEGVideo) {
            ptr = ((MPEG1VIDEOINFO*)pmt->pbFormat)->bSequenceHeader;
            len = ((MPEG1VIDEOINFO*)pmt->pbFormat)->cbSequenceHeader;

            if (ptr && len >= 8 && *(DWORD*)ptr == 0xb3010000) {
                w = (ptr[4] << 4) | (ptr[5] >> 4);
                h = ((ptr[5] & 0xf) << 8) | ptr[6];
                float ar[] = {
                    1.0000f, 1.0000f, 0.6735f, 0.7031f,
                    0.7615f, 0.8055f, 0.8437f, 0.8935f,
                    0.9157f, 0.9815f, 1.0255f, 1.0695f,
                    1.0950f, 1.1575f, 1.2015f, 1.0000f,
                };
                arx = (int)((float)w / ar[ptr[7] >> 4] + 0.5);
                ary = h;
            }
        } else if (pmt->formattype == FORMAT_MPEG2_VIDEO) {
            ptr = (BYTE*)((MPEG2VIDEOINFO*)pmt->pbFormat)->dwSequenceHeader;
            len = ((MPEG2VIDEOINFO*)pmt->pbFormat)->cbSequenceHeader;

            if (ptr && len >= 8 && *(DWORD*)ptr == 0xb3010000) {
                w = (ptr[4] << 4) | (ptr[5] >> 4);
                h = ((ptr[5] & 0xf) << 8) | ptr[6];
                struct {
                    int x, y;
                } ar[] = {{w, h}, {4, 3}, {16, 9}, {221, 100}, {w, h}};
                int i = std::min(std::max(ptr[7] >> 4, 1), 5) - 1;
                arx = ar[i].x;
                ary = ar[i].y;
            }
        }
    }

    if (!arx || !ary) {
        arx = w;
        ary = h;
    }

    DWORD a = arx, b = ary;
    while (a) {
        int tmp = a;
        a = b % tmp;
        b = tmp;
    }
    if (b) {
        arx /= b, ary /= b;
    }

    return true;
}

bool CreateFilter(std::wstring DisplayName, IBaseFilter** ppBF, std::wstring& FriendlyName)
{
    if (!ppBF) {
        return false;
    }

    *ppBF = nullptr;
    FriendlyName.clear();

    ComPtr<IBindCtx> pBindCtx;
    CreateBindCtx(0, &pBindCtx);

    ComPtr<IMoniker> pMoniker;
    ULONG chEaten;
    if (S_OK != MkParseDisplayName(pBindCtx.Get(), _bstr_t(DisplayName.c_str()), &chEaten, &pMoniker)) {
        return false;
    }

    if (FAILED(pMoniker->BindToObject(pBindCtx.Get(), 0, IID_PPV_ARGS(ppBF))) || !*ppBF) {
        return false;
    }

    ComPtr<IPropertyBag> pPB;
    _variant_t var;
    if (SUCCEEDED(pMoniker->BindToStorage(pBindCtx.Get(), 0, IID_PPV_ARGS(&pPB)))
            && SUCCEEDED(pPB->Read(_T("FriendlyName"), &var, nullptr))) {
        FriendlyName = StringToT(var.bstrVal);
    }

    return true;
}

IBaseFilter* AppendFilter(IPin* pPin, IMoniker* pMoniker, IGraphBuilder* pGB)
{
    do {
        if (!pPin || !pMoniker || !pGB) {
            break;
        }

        ComPtr<IPin> pPinTo;
        PIN_DIRECTION dir;
        if (FAILED(pPin->QueryDirection(&dir)) || dir != PINDIR_OUTPUT || SUCCEEDED(pPin->ConnectedTo(&pPinTo))) {
            break;
        }

        ComPtr<IBindCtx> pBindCtx;
        CreateBindCtx(0, &pBindCtx);

        ComPtr<IPropertyBag> pPB;
        if (FAILED(pMoniker->BindToStorage(pBindCtx.Get(), 0, IID_PPV_ARGS(&pPB)))) {
            break;
        }

        _variant_t var;
        if (FAILED(pPB->Read(_T("FriendlyName"), &var, nullptr))) {
            break;
        }

        ComPtr<IBaseFilter> pBF;
        if (FAILED(pMoniker->BindToObject(pBindCtx.Get(), 0, IID_PPV_ARGS(&pBF))) || !pBF) {
            break;
        }

        if (FAILED(pGB->AddFilter(pBF.Get(), var.bstrVal))) {
            break;
        }

        BeginEnumPins(pBF, pEP, pPinTo2) {
            PIN_DIRECTION dir2;
            if (FAILED(pPinTo2->QueryDirection(&dir2)) || dir2 != PINDIR_INPUT) {
                continue;
            }

            if (SUCCEEDED(pGB->ConnectDirect(pPin, pPinTo2.Get(), nullptr))) {
                return pBF.Get();
            }
        }
        EndEnumFilters;

        pGB->RemoveFilter(pBF.Get());
    } while (false);

    return nullptr;
}

std::wstring GetFriendlyName(std::wstring displayName)
{
    std::wstring friendlyName;

    ComPtr<IBindCtx> pBindCtx;
    CreateBindCtx(0, &pBindCtx);

    ComPtr<IMoniker> pMoniker;
    ULONG chEaten;
    if (S_OK == MkParseDisplayName(pBindCtx.Get(), _bstr_t(displayName.c_str()), &chEaten, &pMoniker)) {
        ComPtr<IPropertyBag> pPB;
        _variant_t var;
        if (SUCCEEDED(pMoniker->BindToStorage(pBindCtx.Get(), 0, IID_PPV_ARGS(&pPB)))
                && SUCCEEDED(pPB->Read(_T("FriendlyName"), &var, nullptr))) {
            friendlyName = StringToT(var.bstrVal);
        }
    }

    return friendlyName;
}

typedef HRESULT(__stdcall* fDllCanUnloadNow)(void);

struct ExternalObject {
    tstring path;
    HINSTANCE hInst;
    CLSID clsid;
    fDllCanUnloadNow fpDllCanUnloadNow;
    bool bUnloadOnNextCheck;
};

static std::list<ExternalObject> s_extObjs;
static CCritSec s_csExtObjs;

HRESULT LoadExternalObject(LPCTSTR path, REFCLSID clsid, REFIID iid, void** ppv)
{
    CheckPointer(ppv, E_POINTER);

    CAutoLock lock(&s_csExtObjs);

    tstring fullpath = MakeFullPath(path);

    HINSTANCE hInst = nullptr;
    bool fFound = false;

    for (ExternalObject&eo : s_extObjs) {
        if (!StringICompare(eo.path, fullpath)) {
            hInst = eo.hInst;
            fFound = true;
            eo.bUnloadOnNextCheck = false;
            break;
        }
    }

    HRESULT hr = E_FAIL;

    if (!hInst) {
        hInst = LoadLibraryEx(fullpath.c_str(), nullptr, LOAD_WITH_ALTERED_SEARCH_PATH);
    }
    if (hInst) {
        typedef HRESULT(__stdcall * PDllGetClassObject)(REFCLSID rclsid, REFIID riid, LPVOID * ppv);
        PDllGetClassObject p = (PDllGetClassObject)GetProcAddress(hInst, "DllGetClassObject");

        if (p && FAILED(hr = p(clsid, iid, ppv))) {
            ComPtr<IClassFactory> pCF;
            if (SUCCEEDED(hr = p(clsid, IID_PPV_ARGS(&pCF)))) {
                hr = pCF->CreateInstance(nullptr, iid, ppv);
            }
        }
    }

    if (FAILED(hr) && hInst && !fFound) {
        FreeLibrary(hInst);
        return hr;
    }

    if (hInst && !fFound) {
        ExternalObject eo;
        eo.path = fullpath;
        eo.hInst = hInst;
        eo.clsid = clsid;
        eo.fpDllCanUnloadNow = (fDllCanUnloadNow)GetProcAddress(hInst, "DllCanUnloadNow");
        eo.bUnloadOnNextCheck = false;
        s_extObjs.push_back(eo);
    }

    return hr;
}

HRESULT LoadExternalFilter(LPCTSTR path, REFCLSID clsid, IBaseFilter** ppBF)
{
    return LoadExternalObject(path, clsid, IID_PPV_ARGS(ppBF));
}

HRESULT LoadExternalPropertyPage(IPersist* pP, REFCLSID clsid, IPropertyPage** ppPP)
{
    CAutoLock lock(&s_csExtObjs);

    CLSID clsid2 = GUID_NULL;
    if (FAILED(pP->GetClassID(&clsid2))) {
        return E_FAIL;
    }

    for (ExternalObject&eo : s_extObjs) {
        if (eo.clsid == clsid2) {
            return LoadExternalObject(eo.path.c_str(), clsid, IID_PPV_ARGS(ppPP));
        }
    }

    return E_FAIL;
}

bool UnloadUnusedExternalObjects()
{
    CAutoLock lock(&s_csExtObjs);

    for (auto it = s_extObjs.begin(); it != s_extObjs.end();) {
        ExternalObject& eo = *it;

        if (eo.fpDllCanUnloadNow && eo.fpDllCanUnloadNow() == S_OK) {
            // Before actually unloading it, we require that the library reports
            // that it can be unloaded safely twice in a row with a 60s delay
            // between the two checks.
            if (eo.bUnloadOnNextCheck) {
                FreeLibrary(eo.hInst);
                it = s_extObjs.erase(it);
                continue;
            }
            eo.bUnloadOnNextCheck = true;
        } else {
            eo.bUnloadOnNextCheck = false;
        }
        ++it;
    }

    return s_extObjs.empty();
}

void ExtendMaxPathLengthIfNeeded(tstring& path, int max_length /*= MAX_PATH*/, bool no_url /*= false */)
{
    if (int(path.size()) >= max_length) {
        if (no_url || StringFind(path, _T("://")) < 0) { // not URL
            if (StringLeft(path, 2) != _T("\\\\")) { // not UNC
                if (StringLeft(path, 4) != _T("\\\\?\\")) { // not already have long path prefix
                    path = _T("\\\\?\\") + path;
                }
            }
        }
    }
}

bool ContainsWildcard(tstring& path)
{
    int p = StringFind(path, '*');
    if (p >= 0) {
        return true;
    }
    p = StringFind(path, '?');
    if (p >= 0) {
        if (p == 2 && StringLeft(path, 4) == _T("\\\\?\\")) {
            tstring tmp = tstring(path);
            StringDelete(tmp, 0, 3);
            return StringFind(tmp, '?') > 0;
        }
        return true;
    }
    return false;
}

void ShortenLongPath(tstring& path)
{
    if (int(path.size()) > MAX_PATH && StringFind(path, _T("\\\\?\\")) < 0) {
        tstring longpath = _T("\\\\?\\") + path;
        TCHAR* buffer = new TCHAR[MAX_PATH];
        long length = GetShortPathName(longpath.c_str(), buffer, MAX_PATH);
        if (length > 0 && length < MAX_PATH) {
            path = buffer;
            StringReplace(path, _T("\\\\?\\"), _T(""));
            delete[] buffer;
        }
    }
}

tstring MakeFullPath(LPCTSTR path)
{
    tstring full(path);
    StringReplace(full, '/', '\\');

    if (int(full.size()) > MAX_PATH) {
        return full;
    }

    if (int(full.size()) >= 2 && full[0] == '\\') {
        if (full[1] != '\\') {
            tstring fn;
            StringRelease(fn, GetModuleFileName(ThisModule(), StringBuffer(fn, MAX_PATH), MAX_PATH));
            PathStripToRoot(StringBuffer(fn, MAX_PATH));
            StringRelease(fn);
            full = fn + StringMid(full, 1);
        }
    } else if (StringFind(full, _T(":\\")) < 0) {
        tstring fn;
        StringRelease(fn, GetModuleFileName(ThisModule(), StringBuffer(fn, MAX_PATH), MAX_PATH));
        PathRemoveFileSpec(StringBuffer(fn, MAX_PATH));
        StringRelease(fn);
        PathAddBackslash(StringBuffer(fn, MAX_PATH));
        StringRelease(fn);
        full = fn + full;
    }

    TCHAR canonical[MAX_PATH] = {};
    return PathCanonicalize(canonical, full.c_str()) ? tstring(canonical) : full;
}

inline bool _IsFourCC(const GUID& guid)
{
    // XXXXXXXX-0000-0010-8000-00AA00389B71
    return (guid.Data2 == 0x0000) && (guid.Data3 == 0x0010) &&
        (((DWORD*)guid.Data4)[0] == 0xAA000080) &&
        (((DWORD*)guid.Data4)[1] == 0x719b3800);
}

bool GetMediaTypeFourCC(const GUID& guid, tstring& fourCC)
{
    if (_IsFourCC(guid) && (guid.Data1 >= 0x10000)) {
        fourCC = ::Format(_T("%c%c%c%c"), (TCHAR)(guid.Data1 >> 0 ) & 0xFF, (TCHAR)(guid.Data1 >> 8 ) & 0xFF, (TCHAR)(guid.Data1 >> 16) & 0xFF, (TCHAR)(guid.Data1 >> 24) & 0xFF);
        StringUpper(fourCC);
        return true;
    }

    fourCC = _T("UNKN");
    return false;
}

tstring GetMediaTypeName(const GUID& guid)
{
    tstring ret = guid == GUID_NULL
                  ? tstring(_T("Any type"))
                  : StringToT(GuidNames[guid]);

    if (ret == _T("FOURCC GUID")) {
        tstring str;
        if (guid.Data1 >= 0x10000) {
            str = ::Format(_T("Video: %c%c%c%c"), (guid.Data1 >> 0) & 0xff, (guid.Data1 >> 8) & 0xff, (guid.Data1 >> 16) & 0xff, (guid.Data1 >> 24) & 0xff);
        } else {
            str = ::Format(_T("Audio: 0x%08x"), guid.Data1);
        }
        ret = str;
    } else if (ret == _T("Unknown GUID Name")) {
        WCHAR null[128] = {0}, buff[128];
        StringFromGUID2(GUID_NULL, null, _countof(null) - 1);
        ret = tstring(std::wstring(StringFromGUID2(guid, buff, _countof(buff) - 1) ? buff : null));
    }

    return ret;
}

GUID GUIDFromCString(tstring str)
{
    GUID guid = GUID_NULL;
    HRESULT hr = CLSIDFromString(_bstr_t(str.c_str()), &guid);
    ASSERT(SUCCEEDED(hr));
    UNREFERENCED_PARAMETER(hr);
    return guid;
}

HRESULT GUIDFromCString(tstring str, GUID& guid)
{
    guid = GUID_NULL;
    return CLSIDFromString(_bstr_t(str.c_str()), &guid);
}

tstring CStringFromGUID(const GUID& guid)
{
    WCHAR null[128] = {0}, buff[128];
    StringFromGUID2(GUID_NULL, null, _countof(null) - 1);
    return tstring(StringFromGUID2(guid, buff, _countof(buff) - 1) > 0 ? buff : null);
}

std::wstring UTF8To16(LPCSTR utf8)
{
    std::wstring str;
    int n = MultiByteToWideChar(CP_UTF8, 0, utf8, -1, nullptr, 0) - 1;
    if (n <= 0) {
        return str;
    }
    StringRelease(str, MultiByteToWideChar(CP_UTF8, 0, utf8, -1, StringBuffer(str, n), n + 1) - 1);
    return str;
}

std::string UTF16To8(LPCWSTR utf16)
{
    std::string str;
    int n = WideCharToMultiByte(CP_UTF8, 0, utf16, -1, nullptr, 0, nullptr, nullptr) - 1;
    if (n < 0) {
        return str;
    }
    StringRelease(str, WideCharToMultiByte(CP_UTF8, 0, utf16, -1, StringBuffer(str, n), n + 1, nullptr, nullptr) - 1);
    return str;
}

std::wstring UTF8ToStringW(const char* S)
{
    std::wstring str;
    if (S == nullptr) {
        return str;
    }

    // Don't use MultiByteToWideChar(), some characters are not well decoded
    const unsigned char* Z = (const unsigned char*)S;
    while (*Z) { //0 is end
        //1 byte
        if (*Z < 0x80) {
            str += (wchar_t)(*Z);
            Z++;
        }
        //2 bytes
        else if ((*Z & 0xE0) == 0xC0) {
            if ((*(Z + 1) & 0xC0) == 0x80) {
                str += (wchar_t)((((wchar_t)(*Z & 0x1F)) << 6) | (*(Z + 1) & 0x3F));
                Z += 2;
            } else {
                str.clear();
                return str; //Bad character
            }
        }
        //3 bytes
        else if ((*Z & 0xF0) == 0xE0) {
            if ((*(Z + 1) & 0xC0) == 0x80 && (*(Z + 2) & 0xC0) == 0x80) {
                str += (wchar_t)((((wchar_t)(*Z & 0x0F)) << 12) | ((*(Z + 1) & 0x3F) << 6) | (*(Z + 2) & 0x3F));
                Z += 3;
            } else {
                str.clear();
                return str; //Bad character
            }
        }
        //4 bytes
        else if ((*Z & 0xF8) == 0xF0) {
            if ((*(Z + 1) & 0xC0) == 0x80 && (*(Z + 2) & 0xC0) == 0x80 && (*(Z + 3) & 0xC0) == 0x80) {
                str += (wchar_t)((((wchar_t)(*Z & 0x0F)) << 18) | ((*(Z + 1) & 0x3F) << 12) || ((*(Z + 2) & 0x3F) << 6) | (*(Z + 3) & 0x3F));
                Z += 4;
            } else {
                str.clear();
                return str; //Bad character
            }
        } else {
            str.clear();
            return str; //Bad character
        }
    }
    return str;
}

std::wstring LocalToStringW(const char* S)
{
    std::wstring str;
    if (S == nullptr) {
        return str;
    }

    int Size = MultiByteToWideChar(CP_ACP, 0, S, -1, nullptr, 0);
    if (Size != 0) {
        StringRelease(str, MultiByteToWideChar(CP_ACP, 0, S, -1, StringBuffer(str, Size), Size + 1) - 1);
    }
    return str;
}

// filter registration helpers

bool DeleteRegKey(LPCTSTR pszKey, LPCTSTR pszSubkey)
{
    bool bOK = false;

    HKEY hKey;
    LONG ec = ::RegOpenKeyEx(HKEY_CLASSES_ROOT, pszKey, 0, KEY_ALL_ACCESS, &hKey);
    if (ec == ERROR_SUCCESS) {
        if (pszSubkey != 0) {
            ec = ::RegDeleteKey(hKey, pszSubkey);
        }

        bOK = (ec == ERROR_SUCCESS);

        ::RegCloseKey(hKey);
    }

    return bOK;
}

bool SetRegKeyValue(LPCTSTR pszKey, LPCTSTR pszSubkey, LPCTSTR pszValueName, LPCTSTR pszValue)
{
    bool bOK = false;

    tstring szKey(pszKey);
    if (pszSubkey != 0) {
        szKey += tstring(_T("\\")) + pszSubkey;
    }

    HKEY hKey;
    LONG ec = ::RegCreateKeyEx(HKEY_CLASSES_ROOT, szKey.c_str(), 0, 0, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, 0, &hKey, 0);
    if (ec == ERROR_SUCCESS) {
        if (pszValue != 0) {
            ec = ::RegSetValueEx(hKey, pszValueName, 0, REG_SZ,
                                 reinterpret_cast<BYTE*>(const_cast<LPTSTR>(pszValue)),
                                 (DWORD)(_tcslen(pszValue) + 1) * sizeof(TCHAR));
        }

        bOK = (ec == ERROR_SUCCESS);

        ::RegCloseKey(hKey);
    }

    return bOK;
}

bool SetRegKeyValue(LPCTSTR pszKey, LPCTSTR pszSubkey, LPCTSTR pszValue)
{
    return SetRegKeyValue(pszKey, pszSubkey, 0, pszValue);
}

void RegisterSourceFilter(const CLSID& clsid, const GUID& subtype2, LPCTSTR chkbytes, LPCTSTR ext, ...)
{
    tstring null = CStringFromGUID(GUID_NULL);
    tstring majortype = CStringFromGUID(MEDIATYPE_Stream);
    tstring subtype = CStringFromGUID(subtype2);

    SetRegKeyValue(_T("Media Type\\") + majortype, subtype, _T("0"), chkbytes);
    SetRegKeyValue(_T("Media Type\\") + majortype, subtype, _T("Source Filter"), CStringFromGUID(clsid));

    DeleteRegKey((_T("Media Type\\") + null).c_str(), subtype.c_str());

    va_list marker;
    va_start(marker, ext);
    for (; ext; ext = va_arg(marker, LPCTSTR)) {
        DeleteRegKey(_T("Media Type\\Extensions"), ext);
    }
    va_end(marker);
}

void RegisterSourceFilter(const CLSID& clsid, const GUID& subtype2, const std::list<tstring>& chkbytes, LPCTSTR ext, ...)
{
    tstring null = CStringFromGUID(GUID_NULL);
    tstring majortype = CStringFromGUID(MEDIATYPE_Stream);
    tstring subtype = CStringFromGUID(subtype2);

    ptrdiff_t i = 0;
    for (const tstring& chkbyte : chkbytes) {
        tstring idx;
        idx = ::Format(_T("%Id"), i++);
        SetRegKeyValue(_T("Media Type\\") + majortype, subtype, idx, chkbyte);
    }

    SetRegKeyValue(_T("Media Type\\") + majortype, subtype, _T("Source Filter"), CStringFromGUID(clsid));

    DeleteRegKey((_T("Media Type\\") + null).c_str(), subtype.c_str());

    va_list marker;
    va_start(marker, ext);
    for (; ext; ext = va_arg(marker, LPCTSTR)) {
        DeleteRegKey(_T("Media Type\\Extensions"), ext);
    }
    va_end(marker);
}

void UnRegisterSourceFilter(const GUID& subtype)
{
    DeleteRegKey((_T("Media Type\\") + CStringFromGUID(MEDIATYPE_Stream)).c_str(), CStringFromGUID(subtype).c_str());
}

struct DXVA2_DECODER {
    const GUID* Guid;
    LPCTSTR Description;
};

static const DXVA2_DECODER DXVA2Decoder[] = {
    {&GUID_NULL,                        _T("Unknown")},
    {&GUID_NULL,                        _T("Not using DXVA")},
    {&DXVA_Intel_H264_ClearVideo,       _T("H.264 bitstream decoder, ClearVideo(tm)")},  // Intel ClearVideo H264 bitstream decoder
    {&DXVA_Intel_VC1_ClearVideo,        _T("VC-1 bitstream decoder, ClearVideo(tm)")},   // Intel ClearVideo VC-1 bitstream decoder
    {&DXVA_Intel_VC1_ClearVideo_2,      _T("VC-1 bitstream decoder 2, ClearVideo(tm)")}, // Intel ClearVideo VC-1 bitstream decoder 2
    {&DXVA_MPEG4_ASP,                   _T("MPEG-4 ASP bitstream decoder")},             // NVIDIA MPEG-4 ASP bitstream decoder
    {&DXVA_ModeNone,                    _T("Mode none")},
    {&DXVA_ModeH261_A,                  _T("H.261 A, post processing")},
    {&DXVA_ModeH261_B,                  _T("H.261 B, deblocking")},
    {&DXVA_ModeH263_A,                  _T("H.263 A, motion compensation, no FGT")},
    {&DXVA_ModeH263_B,                  _T("H.263 B, motion compensation, FGT")},
    {&DXVA_ModeH263_C,                  _T("H.263 C, IDCT, no FGT")},
    {&DXVA_ModeH263_D,                  _T("H.263 D, IDCT, FGT")},
    {&DXVA_ModeH263_E,                  _T("H.263 E, bitstream decoder, no FGT")},
    {&DXVA_ModeH263_F,                  _T("H.263 F, bitstream decoder, FGT")},
    {&DXVA_ModeMPEG1_A,                 _T("MPEG-1 A, post processing")},
    {&DXVA_ModeMPEG2_A,                 _T("MPEG-2 A, motion compensation")},
    {&DXVA_ModeMPEG2_B,                 _T("MPEG-2 B, motion compensation + blending")},
    {&DXVA_ModeMPEG2_C,                 _T("MPEG-2 C, IDCT")},
    {&DXVA_ModeMPEG2_D,                 _T("MPEG-2 D, IDCT + blending")},
    {&DXVA_ModeH264_A,                  _T("H.264 A, motion compensation, no FGT")},
    {&DXVA_ModeH264_B,                  _T("H.264 B, motion compensation, FGT")},
    {&DXVA_ModeH264_C,                  _T("H.264 C, IDCT, no FGT")},
    {&DXVA_ModeH264_D,                  _T("H.264 D, IDCT, FGT")},
    {&DXVA_ModeH264_E,                  _T("H.264 E, bitstream decoder, no FGT")},
    {&DXVA_ModeH264_F,                  _T("H.264 F, bitstream decoder, FGT")},
    {&DXVA_ModeWMV8_A,                  _T("WMV8 A, post processing")},
    {&DXVA_ModeWMV8_B,                  _T("WMV8 B, motion compensation")},
    {&DXVA_ModeWMV9_A,                  _T("WMV9 A, post processing")},
    {&DXVA_ModeWMV9_B,                  _T("WMV9 B, motion compensation")},
    {&DXVA_ModeWMV9_C,                  _T("WMV9 C, IDCT")},
    {&DXVA_ModeVC1_A,                   _T("VC-1 A, post processing")},
    {&DXVA_ModeVC1_B,                   _T("VC-1 B, motion compensation")},
    {&DXVA_ModeVC1_C,                   _T("VC-1 C, IDCT")},
    {&DXVA_ModeVC1_D,                   _T("VC-1 D, bitstream decoder")},
    {&DXVA_NoEncrypt,                   _T("No encryption")},
    {&DXVA2_ModeMPEG2_MoComp,           _T("MPEG-2 motion compensation")},
    {&DXVA2_ModeMPEG2_IDCT,             _T("MPEG-2 IDCT")},
    {&DXVA2_ModeMPEG2_VLD,              _T("MPEG-2 variable-length decoder")},
    {&DXVA2_ModeH264_A,                 _T("H.264 A, motion compensation, no FGT")},
    {&DXVA2_ModeH264_B,                 _T("H.264 B, motion compensation, FGT")},
    {&DXVA2_ModeH264_C,                 _T("H.264 C, IDCT, no FGT")},
    {&DXVA2_ModeH264_D,                 _T("H.264 D, IDCT, FGT")},
    {&DXVA2_ModeH264_E,                 _T("H.264 E, bitstream decoder, no FGT")},
    {&DXVA2_ModeH264_F,                 _T("H.264 F, bitstream decoder, FGT")},
    {&DXVA2_ModeWMV8_A,                 _T("WMV8 A, post processing")},
    {&DXVA2_ModeWMV8_B,                 _T("WMV8 B, motion compensation")},
    {&DXVA2_ModeWMV9_A,                 _T("WMV9 A, post processing")},
    {&DXVA2_ModeWMV9_B,                 _T("WMV9 B, motion compensation")},
    {&DXVA2_ModeWMV9_C,                 _T("WMV9 C, IDCT")},
    {&DXVA2_ModeVC1_A,                  _T("VC-1 A, post processing")},
    {&DXVA2_ModeVC1_B,                  _T("VC-1 B, motion compensation")},
    {&DXVA2_ModeVC1_C,                  _T("VC-1 C, IDCT")},
    {&DXVA2_ModeVC1_D,                  _T("VC-1 D, bitstream decoder")},
    {&DXVA2_NoEncrypt,                  _T("No encryption")},
    {&DXVA2_VideoProcProgressiveDevice, _T("Progressive scan")},
    {&DXVA2_VideoProcBobDevice,         _T("Bob deinterlacing")},
    {&DXVA2_VideoProcSoftwareDevice,    _T("Software processing")}
};

LPCTSTR GetDXVAMode(const GUID* guidDecoder)
{
    int nPos = 0;

    for (int i = 1; i < _countof(DXVA2Decoder); i++) {
        if (*guidDecoder == *DXVA2Decoder[i].Guid) {
            nPos = i;
            break;
        }
    }

    return DXVA2Decoder[nPos].Description;
}

// hour, minute, second, millisec
tstring ReftimeToString(const REFERENCE_TIME& rtVal)
{
    if (rtVal == _I64_MIN) {
        return _T("INVALID TIME");
    }

    tstring strTemp;
    LONGLONG llTotalMs = ConvertToMilliseconds(rtVal);
    int lHour     = (int)(llTotalMs / (1000 * 60 * 60));
    int lMinute   = (llTotalMs / (1000 * 60)) % 60;
    int lSecond   = (llTotalMs /  1000) % 60;
    int lMillisec = llTotalMs  %  1000;

    strTemp = ::Format(_T("%02d:%02d:%02d,%03d"), lHour, lMinute, lSecond, lMillisec);
    return strTemp;
}

// hour, minute, second (round)
tstring ReftimeToString2(const REFERENCE_TIME& rtVal)
{
    tstring strTemp;
    LONGLONG seconds = (rtVal + 5000000) / 10000000;
    int lHour   = (int)(seconds / 3600);
    int lMinute = (int)(seconds / 60 % 60);
    int lSecond = (int)(seconds % 60);

    strTemp = ::Format(_T("%02d:%02d:%02d"), lHour, lMinute, lSecond);
    return strTemp;
}

tstring DVDtimeToString(const DVD_HMSF_TIMECODE& rtVal, bool bAlwaysShowHours)
{
    tstring strTemp;
    if (rtVal.bHours > 0 || bAlwaysShowHours) {
        strTemp = ::Format(_T("%02u:%02u:%02u"), rtVal.bHours, rtVal.bMinutes, rtVal.bSeconds);
    } else {
        strTemp = ::Format(_T("%02u:%02u"), rtVal.bMinutes, rtVal.bSeconds);
    }
    return strTemp;
}

REFERENCE_TIME StringToReftime(LPCTSTR strVal)
{
    REFERENCE_TIME rt = 0;
    int lHour = 0;
    int lMinute = 0;
    int lSecond = 0;
    int lMillisec = 0;

    if (_stscanf_s(strVal, _T("%02d:%02d:%02d,%03d"), &lHour, &lMinute, &lSecond, &lMillisec) == 4) {
        rt = ((((lHour * 60) + lMinute) * 60 + lSecond) * MILLISECONDS + lMillisec) * (UNITS / MILLISECONDS);
    }

    return rt;
}

const wchar_t* StreamTypeToName(PES_STREAM_TYPE _Type)
{
    switch (_Type) {
        case VIDEO_STREAM_MPEG1:
            return L"MPEG-1";
        case VIDEO_STREAM_MPEG2:
            return L"MPEG-2";
        case AUDIO_STREAM_MPEG1:
            return L"MPEG-1";
        case AUDIO_STREAM_MPEG2:
            return L"MPEG-2";
        case VIDEO_STREAM_H264:
            return L"H264";
        case VIDEO_STREAM_HEVC:
            return L"HEVC";
        case AUDIO_STREAM_LPCM:
            return L"LPCM";
        case AUDIO_STREAM_AC3:
            return L"Dolby Digital";
        case AUDIO_STREAM_DTS:
            return L"DTS";
        case AUDIO_STREAM_AC3_TRUE_HD:
            return L"Dolby TrueHD";
        case AUDIO_STREAM_AC3_PLUS:
            return L"Dolby Digital Plus";
        case AUDIO_STREAM_DTS_HD:
            return L"DTS-HD High Resolution Audio";
        case AUDIO_STREAM_DTS_HD_MASTER_AUDIO:
            return L"DTS-HD Master Audio";
        case PRESENTATION_GRAPHICS_STREAM:
            return L"Presentation Graphics Stream";
        case INTERACTIVE_GRAPHICS_STREAM:
            return L"Interactive Graphics Stream";
        case SUBTITLE_STREAM:
            return L"Subtitle";
        case SECONDARY_AUDIO_AC3_PLUS:
            return L"Secondary Dolby Digital Plus";
        case SECONDARY_AUDIO_DTS_HD:
            return L"Secondary DTS-HD High Resolution Audio";
        case VIDEO_STREAM_VC1:
            return L"VC-1";
    }
    return nullptr;
}

//
// Usage: SetThreadName (-1, "MainThread");
// Code from http://msdn.microsoft.com/en-us/library/xcb2z8hs%28v=vs.110%29.aspx
//

#pragma pack(push,8)
typedef struct tagTHREADNAME_INFO {
    DWORD  dwType;      // must be 0x1000
    LPCSTR szName;      // pointer to name (in user addr space)
    DWORD  dwThreadID;  // thread ID (-1 caller thread)
    DWORD  dwFlags;     // reserved for future use, must be zero
} THREADNAME_INFO;
#pragma pack(pop)

void SetThreadName(DWORD dwThreadID, LPCSTR szThreadName)
{
    THREADNAME_INFO info;
    info.dwType     = 0x1000;
    info.szName     = szThreadName;
    info.dwThreadID = dwThreadID;
    info.dwFlags    = 0;

    __try {
        RaiseException(0x406D1388, 0, sizeof(info) / sizeof(ULONG_PTR), (ULONG_PTR*)&info);
    } __except (EXCEPTION_EXECUTE_HANDLER) {
    }
}

tstring NormalizeUnicodeStrForSearch(tstring srcStr, LANGID langid) {
    if (srcStr.empty()) return srcStr;
    wchar_t* src;

    _locale_t locale;
    LCID lcid = MAKELCID(MAKELANGID(langid, SUBLANG_DEFAULT), SORT_DEFAULT);
    wchar_t localeName[32];
    if (0 == LCIDToLocaleName(lcid, localeName, 32, LOCALE_ALLOW_NEUTRAL_NAMES)) { //try to lowercase by locale, but if not, do a regular MakeLower()
        StringLower(srcStr);
        src = StringBuffer(srcStr, 0);
    } else {
        src = StringBuffer(srcStr, 0);
        locale = _wcreate_locale(LC_ALL, localeName);
        _wcslwr_s_l(src, wcslen(src) + 1, locale);
    }

    int dstLen = int(wcslen(src) * 4);
    wchar_t* dest = new wchar_t[dstLen];

    int cchActual = NormalizeString(NormalizationKD, src, -1, dest, dstLen);
    if (cchActual <= 0) dest[0] = 0;
    WORD* rgType = new WORD[dstLen];
    GetStringTypeW(CT_CTYPE3, dest, -1, rgType);
    PWSTR pszWrite = dest;
    for (int i = 0; dest[i]; i++) {
        if (!(rgType[i] & C3_NONSPACING)) {
            *pszWrite++ = dest[i];
        }
    }
    *pszWrite = 0;
    delete[] rgType;

    tstring ret = dest;
    delete[] dest;
    return ret;
}

inline const LONGLONG GetPerfCounter() {
    auto GetPerfFrequency = [] {
        LARGE_INTEGER freq;
        QueryPerformanceFrequency(&freq);
        return freq.QuadPart;
    };
    static const LONGLONG llPerfFrequency = GetPerfFrequency();
    if (llPerfFrequency) {
        LARGE_INTEGER llPerfCounter;
        QueryPerformanceCounter(&llPerfCounter);
        return llMulDiv(llPerfCounter.QuadPart, 10000000LL, llPerfFrequency, 0);
    } else {
        // ms to 100ns units
        return timeGetTime() * 10000;
    }
}

bool FindStringInList(const std::list<tstring>& list, tstring& value)
{
    for (const tstring& item : list) {
        if (StringICompare(item, value) == 0) {
            return true;
        }
    }
    return false;
}
