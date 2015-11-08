// Copyright (C) 2005-2015 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the DSHOWHELPER_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// DSHOWHELPER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.

#ifndef TSAVRT_H
#define TSAVRT_H

#include <Avrt.h>


typedef HANDLE __stdcall TAvSetMmThreadCharacteristicsW(LPCWSTR TaskName, LPDWORD TaskIndex);
typedef BOOL __stdcall TAvRevertMmThreadCharacteristics(HANDLE AvrtHandle);
typedef BOOL __stdcall TAvSetMmThreadPriority(HANDLE AvrtHandle, AVRT_PRIORITY Priority);


class TsAVRT  
{
public:
	TsAVRT();
	virtual ~TsAVRT();

  void UnloadAVRT();
  bool LoadAVRT();

  TAvSetMmThreadCharacteristicsW*     m_pAvSetMmThreadCharacteristicsW;
  TAvSetMmThreadPriority*             m_pAvSetMmThreadPriority;
  TAvRevertMmThreadCharacteristics*   m_pAvRevertMmThreadCharacteristics;

  HANDLE SetMMCSThreadPlayback(LPDWORD pDwTaskIndex, AVRT_PRIORITY AvrtPriority);
  void RevertMMCSThread(HANDLE hAvrt);

private:
  HMODULE m_hModuleAVRT;
  
};

#endif
