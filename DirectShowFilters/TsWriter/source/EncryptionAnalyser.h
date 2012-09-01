/* 
 *  Copyright (C) 2006-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
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
#include "..\..\shared\TsHeader.h"
#include <map>

using namespace std;

// Enum specifying possible elementary stream encryption states.
enum EncryptionState
{
  EncryptionStateNotSet = -1,
  Clear = 0,
  Encrypted = 1
};

DEFINE_GUID(ITsEncryptionAnalyser, 0x59f8d617, 0x92fd, 0x48d5, 0x8f, 0x6d, 0xa9, 0x7b, 0xfd, 0x95, 0xc4, 0x48);

DECLARE_INTERFACE_(IEncryptionStateChangeCallBack, IUnknown)
{
  STDMETHOD(OnEncryptionStateChange)(THIS_ int pid, EncryptionState encryptionState)PURE;
};

// encryption analyser interface
DECLARE_INTERFACE_(ITsEncryptionAnalyser, IUnknown)
{
  STDMETHOD(AddPid)(THIS_ int pid)PURE;
  STDMETHOD(RemovePid)(THIS_ int pid)PURE;
  STDMETHOD(GetPidCount)(THIS_ int* pidCount)PURE;
  STDMETHOD(GetPid)(THIS_ int pidIdx, int* pid, EncryptionState* encryptionState)PURE;
  STDMETHOD(SetCallBack)(THIS_ IEncryptionStateChangeCallBack* callBack)PURE;
  STDMETHOD(Reset)(THIS_)PURE;
};

class CEncryptionAnalyser: public CUnknown, public ITsEncryptionAnalyser
{
  public:
    CEncryptionAnalyser(LPUNKNOWN pUnk, HRESULT *phr);
    ~CEncryptionAnalyser(void);

    DECLARE_IUNKNOWN
    STDMETHODIMP AddPid(int pid);
    STDMETHODIMP RemovePid(int pid);
    STDMETHODIMP GetPidCount(int* pidCount);
    STDMETHODIMP GetPid(int pidIdx, int* pid, EncryptionState* encryptionState);
    STDMETHODIMP SetCallBack(IEncryptionStateChangeCallBack* callBack);
    STDMETHODIMP Reset();

    void OnTsPacket(byte* tsPacket);
  private:
    map<int, EncryptionState> m_mPids;
    CTsHeader m_tsHeader;
    IEncryptionStateChangeCallBack* m_pCallBack;
};
