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

#include "stdafx.h"

#include "ConditionalAccessSectionMutiplexer.h"
#include "ConditionalAccessSection.h"

CConditionalAccessSectionMutiplexer::CConditionalAccessSectionMutiplexer(HRESULT *result, unsigned int pid, unsigned int continuityCounter)
  : CSectionMultiplexer(result, pid, continuityCounter)
{
}

CConditionalAccessSectionMutiplexer::~CConditionalAccessSectionMutiplexer()
{
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CConditionalAccessSectionMutiplexer::AddSection(CSection *section)
{
  return E_NOTIMPL;
}

HRESULT CConditionalAccessSectionMutiplexer::MultiplexSections(void)
{
  return this->FlushStreamFragmentContexts();
}

/* protected methods */

void CConditionalAccessSectionMutiplexer::IncreaseReferenceCount(CMpeg2tsStreamFragment *streamFragment)
{
  streamFragment->SetMultiplexerConditionalAccessSectionReferenceCount(streamFragment->GetMultiplexerConditionalAccessSectionReferenceCount() + 1);
}

void CConditionalAccessSectionMutiplexer::DecreaseReferenceCount(CMpeg2tsStreamFragment *streamFragment)
{
  streamFragment->SetMultiplexerConditionalAccessSectionReferenceCount(streamFragment->GetMultiplexerConditionalAccessSectionReferenceCount() - 1);
}
