/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2014, 2017 see Authors.txt
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
#include "DSUtil.h"
#include "DSMPropertyBag.h"

//
// IDSMPropertyBagImpl
//

IDSMPropertyBagImpl::IDSMPropertyBagImpl()
{
}

IDSMPropertyBagImpl::~IDSMPropertyBagImpl()
{
}

// IPropertyBag

STDMETHODIMP IDSMPropertyBagImpl::Read(LPCOLESTR pszPropName, VARIANT* pVar, IErrorLog* pErrorLog)
{
    CheckPointer(pVar, E_POINTER);
    if (pVar->vt != VT_EMPTY) {
        return E_INVALIDARG;
    }
    std::wstring value = Lookup(pszPropName);
    if (value.empty()) {
        return E_FAIL;
    }
    VariantClear(pVar);
    *pVar = _variant_t(value.c_str()).Detach();
    return S_OK;
}

STDMETHODIMP IDSMPropertyBagImpl::Write(LPCOLESTR pszPropName, VARIANT* pVar)
{
    return SetProperty(pszPropName, pVar);
}

// IPropertyBag2

STDMETHODIMP IDSMPropertyBagImpl::Read(ULONG cProperties, PROPBAG2* pPropBag, IErrorLog* pErrLog, VARIANT* pvarValue, HRESULT* phrError)
{
    CheckPointer(pPropBag, E_POINTER);
    CheckPointer(pvarValue, E_POINTER);
    CheckPointer(phrError, E_POINTER);
    for (ULONG i = 0; i < cProperties; phrError[i] = S_OK, i++) {
        VariantClear(pvarValue);
        *pvarValue = _variant_t(Lookup(pPropBag[i].pstrName).c_str()).Detach();
    }
    return S_OK;
}

STDMETHODIMP IDSMPropertyBagImpl::Write(ULONG cProperties, PROPBAG2* pPropBag, VARIANT* pvarValue)
{
    CheckPointer(pPropBag, E_POINTER);
    CheckPointer(pvarValue, E_POINTER);
    for (ULONG i = 0; i < cProperties; i++) {
        SetProperty(pPropBag[i].pstrName, &pvarValue[i]);
    }
    return S_OK;
}

STDMETHODIMP IDSMPropertyBagImpl::CountProperties(ULONG* pcProperties)
{
    CheckPointer(pcProperties, E_POINTER);
    *pcProperties = GetSize();
    return S_OK;
}

STDMETHODIMP IDSMPropertyBagImpl::GetPropertyInfo(ULONG iProperty, ULONG cProperties, PROPBAG2* pPropBag, ULONG* pcProperties)
{
    CheckPointer(pPropBag, E_POINTER);
    CheckPointer(pcProperties, E_POINTER);
    for (ULONG i = 0; i < cProperties; i++, iProperty++, (*pcProperties)++) {
        std::wstring key = GetKeyAt(iProperty);
        pPropBag[i].pstrName = (BSTR)CoTaskMemAlloc((int(key.size()) + 1) * sizeof(WCHAR));
        if (!pPropBag[i].pstrName) {
            return E_FAIL;
        }
        wcscpy_s(pPropBag[i].pstrName, int(key.size()) + 1, key.c_str());
    }
    return S_OK;
}

STDMETHODIMP IDSMPropertyBagImpl::LoadObject(LPCOLESTR pstrName, DWORD dwHint, IUnknown* pUnkObject, IErrorLog* pErrLog)
{
    return E_NOTIMPL;
}

// IDSMProperyBag

HRESULT IDSMPropertyBagImpl::SetProperty(LPCWSTR key, LPCWSTR value)
{
    CheckPointer(key, E_POINTER);
    CheckPointer(value, E_POINTER);
    if (!Lookup(key).empty()) {
        SetAt(key, value);
    } else {
        Add(key, value);
    }
    return S_OK;
}

HRESULT IDSMPropertyBagImpl::SetProperty(LPCWSTR key, VARIANT* var)
{
    CheckPointer(key, E_POINTER);
    CheckPointer(var, E_POINTER);
    if ((var->vt & (VT_BSTR | VT_BYREF)) != VT_BSTR) {
        return E_INVALIDARG;
    }
    return SetProperty(key, var->bstrVal);
}

HRESULT IDSMPropertyBagImpl::GetProperty(LPCWSTR key, BSTR* value)
{
    CheckPointer(key, E_POINTER);
    CheckPointer(value, E_POINTER);
    int i = FindKey(key);
    if (i < 0) {
        return E_FAIL;
    }
    *value = StringAllocSys(GetValueAt(i));
    return S_OK;
}

HRESULT IDSMPropertyBagImpl::DelAllProperties()
{
    RemoveAll();
    return S_OK;
}

HRESULT IDSMPropertyBagImpl::DelProperty(LPCWSTR key)
{
    return Remove(key) ? S_OK : S_FALSE;
}

//
// CDSMResource
//

CCritSec CDSMResource::m_csResources;
std::map<uintptr_t, CDSMResource*> CDSMResource::m_resources;

CDSMResource::CDSMResource()
    : tag(0)
    , mime(_T("application/octet-stream"))
{
    CAutoLock cAutoLock(&m_csResources);
    m_resources[reinterpret_cast<uintptr_t>(this)] = this;
}

CDSMResource::CDSMResource(const CDSMResource& r)
{
    *this = r;

    CAutoLock cAutoLock(&m_csResources);
    m_resources[reinterpret_cast<uintptr_t>(this)] = this;
}

CDSMResource::CDSMResource(LPCWSTR name, LPCWSTR desc, LPCWSTR mime, BYTE* pData, int len, DWORD_PTR tag)
{
    this->name = name;
    this->desc = desc;
    this->mime = mime;
    data.resize(len);
    memcpy(data.data(), pData, data.size());
    this->tag = tag;

    CAutoLock cAutoLock(&m_csResources);
    m_resources[reinterpret_cast<uintptr_t>(this)] = this;
}

CDSMResource::~CDSMResource()
{
    CAutoLock cAutoLock(&m_csResources);
    m_resources.erase(reinterpret_cast<uintptr_t>(this));
}

CDSMResource& CDSMResource::operator = (const CDSMResource& r)
{
    if (this != &r) {
        tag = r.tag;
        name = r.name;
        desc = r.desc;
        mime = r.mime;
        data = r.data;
    }
    return *this;
}

//
// IDSMResourceBagImpl
//

IDSMResourceBagImpl::IDSMResourceBagImpl()
{
}

// IDSMResourceBag

STDMETHODIMP_(DWORD) IDSMResourceBagImpl::ResGetCount()
{
    return (DWORD)m_resources.size();
}

STDMETHODIMP IDSMResourceBagImpl::ResGet(DWORD iIndex, BSTR* ppName, BSTR* ppDesc, BSTR* ppMime, BYTE** ppData, DWORD* pDataLen, DWORD_PTR* pTag)
{
    if (ppData) {
        CheckPointer(pDataLen, E_POINTER);
    }

    if (iIndex >= m_resources.size()) {
        return E_INVALIDARG;
    }

    CDSMResource& r = m_resources[iIndex];

    if (ppName) {
        *ppName = StringAllocSys(r.name);
    }
    if (ppDesc) {
        *ppDesc = StringAllocSys(r.desc);
    }
    if (ppMime) {
        *ppMime = StringAllocSys(r.mime);
    }
    if (ppData) {
        *pDataLen = (DWORD)r.data.size();
        memcpy(*ppData = (BYTE*)CoTaskMemAlloc(*pDataLen), r.data.data(), *pDataLen);
    }
    if (pTag) {
        *pTag = r.tag;
    }

    return S_OK;
}

STDMETHODIMP IDSMResourceBagImpl::ResSet(DWORD iIndex, LPCWSTR pName, LPCWSTR pDesc, LPCWSTR pMime, const BYTE* pData, DWORD len, DWORD_PTR tag)
{
    if (iIndex >= m_resources.size()) {
        return E_INVALIDARG;
    }

    CDSMResource& r = m_resources[iIndex];

    if (pName) {
        r.name = pName;
    }
    if (pDesc) {
        r.desc = pDesc;
    }
    if (pMime) {
        r.mime = pMime;
    }
    if (pData || len == 0) {
        r.data.resize(len);
        if (pData) {
            memcpy(r.data.data(), pData, r.data.size());
        }
    }
    r.tag = tag;

    return S_OK;
}

STDMETHODIMP IDSMResourceBagImpl::ResAppend(LPCWSTR pName, LPCWSTR pDesc, LPCWSTR pMime, BYTE* pData, DWORD len, DWORD_PTR tag)
{
    m_resources.emplace_back();
    return ResSet((DWORD)(m_resources.size() - 1), pName, pDesc, pMime, pData, len, tag);
}

STDMETHODIMP IDSMResourceBagImpl::ResRemoveAt(DWORD iIndex)
{
    if (iIndex >= m_resources.size()) {
        return E_INVALIDARG;
    }

    m_resources.erase(m_resources.begin() + iIndex);

    return S_OK;
}

STDMETHODIMP IDSMResourceBagImpl::ResRemoveAll(DWORD_PTR tag)
{
    if (tag) {
        for (ptrdiff_t i = m_resources.size() - 1; i >= 0; i--) {
            if (m_resources[i].tag == tag) {
                m_resources.erase(m_resources.begin() + i);
            }
        }
    } else {
        m_resources.clear();
    }

    return S_OK;
}

//
// CDSMChapter
//

CDSMChapter::CDSMChapter()
{
    order = counter++;
    rt = 0;
}

CDSMChapter::CDSMChapter(REFERENCE_TIME rt, LPCWSTR name)
{
    order = counter++;
    this->rt = rt;
    this->name = name;
}

CDSMChapter& CDSMChapter::operator = (const CDSMChapter& c)
{
    if (this != &c) {
        order = c.counter;
        rt = c.rt;
        name = c.name;
    }
    return *this;
}

int CDSMChapter::counter = 0;

//
// IDSMChapterBagImpl
//

IDSMChapterBagImpl::IDSMChapterBagImpl()
{
    m_fSorted = false;
}

// IDSMRChapterBag

STDMETHODIMP_(DWORD) IDSMChapterBagImpl::ChapGetCount()
{
    return (DWORD)m_chapters.size();
}

STDMETHODIMP IDSMChapterBagImpl::ChapGet(DWORD iIndex, REFERENCE_TIME* prt, BSTR* ppName)
{
    if (iIndex >= m_chapters.size()) {
        return E_INVALIDARG;
    }

    CDSMChapter& c = m_chapters[iIndex];

    if (prt) {
        *prt = c.rt;
    }
    if (ppName) {
        *ppName = StringAllocSys(c.name);
    }

    return S_OK;
}

STDMETHODIMP IDSMChapterBagImpl::ChapSet(DWORD iIndex, REFERENCE_TIME rt, LPCWSTR pName)
{
    if (iIndex >= m_chapters.size()) {
        return E_INVALIDARG;
    }

    CDSMChapter& c = m_chapters[iIndex];

    c.rt = rt;
    if (pName) {
        c.name = pName;
    }

    m_fSorted = false;

    return S_OK;
}

STDMETHODIMP IDSMChapterBagImpl::ChapAppend(REFERENCE_TIME rt, LPCWSTR pName)
{
    m_chapters.emplace_back();
    return ChapSet((DWORD)(m_chapters.size() - 1), rt, pName);
}

STDMETHODIMP IDSMChapterBagImpl::ChapRemoveAt(DWORD iIndex)
{
    if (iIndex >= m_chapters.size()) {
        return E_INVALIDARG;
    }

    m_chapters.erase(m_chapters.begin() + iIndex);

    return S_OK;
}

STDMETHODIMP IDSMChapterBagImpl::ChapRemoveAll()
{
    m_chapters.clear();

    m_fSorted = false;

    return S_OK;
}

STDMETHODIMP_(long) IDSMChapterBagImpl::ChapLookup(REFERENCE_TIME* prt, BSTR* ppName)
{
    CheckPointer(prt, -1);
    if (m_chapters.empty()) {
        return -1;
    }

    size_t result = 0;

    if (m_fSorted) {
        result = range_bsearch(m_chapters, *prt);
    }
    else {
        // assume first entry is best, find better match
        for (size_t i = 1; i < m_chapters.size(); ++i) {
            if (*prt >= m_chapters[i].rt && m_chapters[i].rt >= m_chapters[result].rt) {
                result = i;
            }
        }
        // validate first if it was best
        if (result == 0 && *prt < m_chapters[result].rt) {
            return -1;
        }
    }

    if (result != MAXSIZE_T) {
        *prt = m_chapters[result].rt;
        if (ppName) {
            *ppName = StringAllocSys(m_chapters[result].name);
        }
    }

    return (long)result;
}

STDMETHODIMP_(long) IDSMChapterBagImpl::ChapLookupPrevious(REFERENCE_TIME* prt, BSTR* ppName)
{
    CheckPointer(prt, -1);

    size_t chapcount = m_chapters.size();
    if (chapcount < 1 || *prt < 0) {
        return -1;
    }

    size_t result = 0;
    if (*prt < m_chapters[0].rt) {
        return -1;
    } else {
        for (size_t i = 1; i < chapcount; ++i) {
            if (*prt > m_chapters[i].rt) {
                result = i;
            } else {
                break;
            }
        }
    }

    *prt = m_chapters[result].rt;
    if (ppName) {
        *ppName = StringAllocSys(m_chapters[result].name);
    }

    return (long)result;
}

STDMETHODIMP_(long) IDSMChapterBagImpl::ChapLookupNext(REFERENCE_TIME* prt, BSTR* ppName)
{
    CheckPointer(prt, -1);

    size_t chapcount = m_chapters.size();
    if (chapcount < 1) {
        return -1;
    }

    size_t result = 0;
    if (*prt >= m_chapters[chapcount-1].rt) {
        return -1;
    } else {
        for (size_t i = 0; i < chapcount; ++i) {
            if (*prt < m_chapters[i].rt) {
                result = i;
                break;
            }
        }
    }

    *prt = m_chapters[result].rt;
    if (ppName) {
        *ppName = StringAllocSys(m_chapters[result].name);
    }

    return (long)result;
}

STDMETHODIMP IDSMChapterBagImpl::ChapSort()
{
    if (m_fSorted) {
        return S_FALSE;
    }
    std::sort(m_chapters.data(), m_chapters.data() + m_chapters.size());
    m_fSorted = true;
    return S_OK;
}

//
// CDSMChapterBag
//

CDSMChapterBag::CDSMChapterBag(LPUNKNOWN pUnk, HRESULT* phr)
    : CUnknown(_T("CDSMChapterBag"), nullptr)
{
}

STDMETHODIMP CDSMChapterBag::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
    CheckPointer(ppv, E_POINTER);

    return
        QI(IDSMChapterBag)
        __super::NonDelegatingQueryInterface(riid, ppv);
}
