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
#include "..\..\shared\CriticalSection.h"
#include "..\..\shared\ISectionDispatcher.h"
#include "..\..\shared\Section.h"
#include "..\..\shared\SectionDecoder.h"
#include "ICallBackCat.h"

using namespace MediaPortal;


#define PID_CAT 1
#define TABLE_ID_CAT 1


class CGrabberCat : public CSectionDecoder
{
  public:
    CGrabberCat(ISectionDispatcher* sectionDispatcher);
    ~CGrabberCat();

    void Reset();
    void SetCallBack(ICallBackCat* callBack);
    void OnNewSection(const CSection& section);
    bool IsReady() const;

    bool GetTable(unsigned char* table, unsigned short& tableBufferSize) const;

  private:
    CCriticalSection m_section;
    bool m_isReady;
    unsigned char m_version;
    ICallBackCat* m_callBack;
    CSection m_catSection;
};