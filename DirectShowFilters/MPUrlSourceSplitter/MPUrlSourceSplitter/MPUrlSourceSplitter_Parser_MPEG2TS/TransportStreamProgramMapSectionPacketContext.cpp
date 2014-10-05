/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#include "StdAfx.h"

#include "TransportStreamProgramMapSectionPacketContext.h"

CTransportStreamProgramMapSectionPacketContext::CTransportStreamProgramMapSectionPacketContext(HRESULT *result)
  : CTsPacketContext(result)
{
}

CTransportStreamProgramMapSectionPacketContext::~CTransportStreamProgramMapSectionPacketContext(void)
{
}

/* get methods */

CTransportStreamProgramMapSectionContext *CTransportStreamProgramMapSectionPacketContext::GetSectionContext(void)
{
  return (CTransportStreamProgramMapSectionContext *)__super::GetSectionContext();
}

/* set methods */

bool CTransportStreamProgramMapSectionPacketContext::SetSectionContext(CSectionContext *sectionContext)
{
  CTransportStreamProgramMapSectionContext *transportStreamProgramMapSectionContext = dynamic_cast<CTransportStreamProgramMapSectionContext *>(sectionContext);

  return (transportStreamProgramMapSectionContext == NULL) ? false : __super::SetSectionContext(transportStreamProgramMapSectionContext);
}

/* other methods */

/* protected methods */