/*
 *  $Id: VobSubFileRipper.h 2786 2010-12-17 16:42:55Z XhmikosR $
 *
 *  (C) 2003-2006 Gabest
 *  (C) 2006-2010 see AUTHORS
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#pragma once

#include <atlcoll.h>
#include "../DeCSS/VobFile.h"
#include "VobSubFile.h"

#pragma pack(push)
#pragma pack(1)

typedef struct {
	WORD perm_displ       : 2;
	WORD ratio            : 2;
	WORD system           : 2;
	WORD compression      : 2;
	WORD mode             : 1;
	WORD letterboxed      : 1;
	WORD source_res       : 2;
	WORD cbrvbr           : 2;
	WORD line21_2         : 1;
	WORD line21_1         : 1;
} vidinfo;

typedef struct {
	BYTE vob, cell;
	DWORD tTime, tOffset, tTotal;
	DWORD start, end;
	int iAngle;
	bool fDiscontinuity;
} vc_t;

typedef struct {
	int nAngles;
	CAtlArray<vc_t> angles[10];
	int iSelAngle;
	RGBQUAD pal[16];
	WORD ids[32];
} PGC;

typedef struct VSFRipperData_t {
	CSize vidsize;
	vidinfo vidinfo;
	CAtlArray<PGC> pgcs;
	int iSelPGC;
	bool fResetTime, fClosedCaption, fForcedOnly;

	bool fClose, fBeep, fAuto; // only used by the UI externally, but may be set through the parameter file
	bool fCloseIgnoreError;

	CAtlArray<UINT> selvcs;
	CAtlMap<BYTE, bool> selids;

	void Reset();
	void Copy(struct VSFRipperData_t& rd);

} VSFRipperData;

typedef struct {
	__int64 start, end;
	DWORD vc;
} vcchunk;

#pragma pack(pop)

// note: these interfaces only meant to be used internally with static linking

//
// IVSFRipperCallback
//

interface __declspec(uuid("9E2EBB5C-AD7C-452f-A48B-38685716AC46"))
IVSFRipperCallback :
public IUnknown {
	STDMETHOD (OnMessage) (LPCTSTR msg) PURE;
	STDMETHOD (OnProgress) (double progress /*0->1*/) PURE;
	STDMETHOD (OnFinished) (bool fSucceeded) PURE;
};

// IVSFRipperCallbackImpl

#ifndef QI
#define QI(i) (riid == __uuidof(i)) ? GetInterface((i*)this, ppv) :
#endif

class IVSFRipperCallbackImpl : public CUnknown, public IVSFRipperCallback
{
protected:
	DECLARE_IUNKNOWN
	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv) {
		return
			QI(IVSFRipperCallback)
			__super::NonDelegatingQueryInterface(riid, ppv);
	}

	// IVSFRipperCallback
	STDMETHODIMP OnMessage(LPCTSTR msg) {
		return S_FALSE;
	}
	STDMETHODIMP OnProgress(double progress /*0->1*/) {
		return S_FALSE;
	}
	STDMETHODIMP OnFinished(bool fSucceeded) {
		return S_FALSE;
	}

public:
	IVSFRipperCallbackImpl() : CUnknown(NAME("IVSFRipperCallbackImpl"), NULL) {}
};

//
// IVSFRipper
//

interface __declspec(uuid("69F935BB-B8D0-43f5-AA2E-BBD0851CC9A6"))
IVSFRipper :
public IUnknown {
	STDMETHOD (SetCallBack) (IVSFRipperCallback* pCallback) PURE;
	STDMETHOD (LoadParamFile) (CString fn) PURE;
	STDMETHOD (SetInput) (CString infn) PURE;
	STDMETHOD (SetOutput) (CString outfn) PURE;
	STDMETHOD (GetRipperData) (VSFRipperData& rd) PURE;
	STDMETHOD (UpdateRipperData) (VSFRipperData& rd) PURE;
	STDMETHOD (Index) () PURE;
	STDMETHOD (IsIndexing) () PURE;
	STDMETHOD (Abort) (bool fSavePartial) PURE;
};

class CVobSubFileRipper : public CVobSubFile, protected CAMThread, public IVSFRipper
{
private:
	bool m_fThreadActive, m_fBreakThread, m_fIndexing;
	enum {CMD_EXIT, CMD_INDEX};
	DWORD ThreadProc();
	bool Create();

	//

	typedef enum {LOG_INFO, LOG_WARNING, LOG_ERROR} log_t;
	void Log(log_t type, LPCTSTR lpszFormat, ...);
	void Progress(double progress);
	void Finished(bool fSucceeded);

	//

	CCritSec m_csAccessLock;
	CString m_infn, m_outfn;
	CVobFile m_vob;
	VSFRipperData m_rd;

	bool LoadIfo(CString fn);
	bool LoadVob(CString fn);
	bool LoadChunks(CAtlArray<vcchunk>& chunks);
	bool SaveChunks(CAtlArray<vcchunk>& chunks);

	//

	CCritSec m_csCallback;
	CComPtr<IVSFRipperCallback> m_pCallback;

public:
	CVobSubFileRipper();
	virtual ~CVobSubFileRipper();

	DECLARE_IUNKNOWN
	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);

	// IVSFRipper
	STDMETHODIMP SetCallBack(IVSFRipperCallback* pCallback);
	STDMETHODIMP LoadParamFile(CString fn);
	STDMETHODIMP SetInput(CString infn);
	STDMETHODIMP SetOutput(CString outfn);
	STDMETHODIMP GetRipperData(VSFRipperData& rd);
	STDMETHODIMP UpdateRipperData(VSFRipperData& rd);
	STDMETHODIMP Index();
	STDMETHODIMP IsIndexing();
	STDMETHODIMP Abort(bool fSavePartial);
};
