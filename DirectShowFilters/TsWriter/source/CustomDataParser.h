/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#include "..\..\shared\sectiondecoder.h"
#include "..\..\shared\isectioncallback.h"
#include "CriticalSection.h"
#include "entercriticalsection.h"
#include "filewriter.h"
#include "epgDecoder.h"
#include <vector>

using namespace std;

class CCustomDataParser :  public CUnknown, public ISectionCallback
{
public:
	
  CCustomDataParser(LPUNKNOWN pUnk, HRESULT *phr);
  ~CCustomDataParser(void);
  
  void GrabCustomData();
  bool isGrabbing();
  void	AbortGrabbing();
  void  SetCustomFileName();
  void OnTsPacket(byte* tsPacket);
  void Reset();
  void  OnNewSection(int pid, int tableId, CSection& section); 
  void SetFileName(wchar_t* pwszFileName);
  void AddSectionDecoder(int pid);
  void OpenFile();
  void Stop();

private:

	bool	IsPidWanted(int pid);
    vector<CSectionDecoder*> m_vecDecoders;
	vector<int> m_WantedPids;
	bool				m_bGrabbing;
	CCriticalSection m_section;
	FileWriter*     m_CustomPacketWriter;

};
