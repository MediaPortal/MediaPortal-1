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

#include "..\..\shared\sectiondecoder.h"
#include <map>
#include <vector>
using namespace std;

class IPatCallBack
{
  public:
    virtual void OnPatReceived(int programNumber, int pmtPid) = 0;
    virtual void OnPatChanged(int programNumber, int oldPmtPid, int newPmtPid) = 0;
    virtual void OnPatRemoved(int programNumber, int pmtPid) = 0;
};

typedef struct ProgramDetail
{
  int ProgramNumber;
  int Pid;
  bool IsCurrent;
}ProgramDetail;

#define PID_PAT 0x0

class CPatParser : public CSectionDecoder
{
  public:
    CPatParser(void);
    virtual ~CPatParser(void);
    void Reset();
    void SetCallBack(IPatCallBack* callBack);
    void OnNewSection(CSection& sections);
    bool IsReady();
    int GetTransportStreamId();
    int GetProgramCount();
    int GetProgram(int idx, int* programNumber, int* pmtPid);
    int GetPmtPid(int programNumber, int* pmtPid);

  private:
    void CleanUp();

    IPatCallBack* m_callBack;
    int m_currentVersionNumber;
    vector<int> m_unseenSections;
    bool m_isReady;
    bool m_wasReset;
    int m_transportStreamId;
    map<int, ProgramDetail*> m_programs;
};