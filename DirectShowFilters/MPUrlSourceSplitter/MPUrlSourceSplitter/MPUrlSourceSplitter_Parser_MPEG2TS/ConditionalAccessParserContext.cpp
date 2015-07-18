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

#include "ConditionalAccessParserContext.h"

CConditionalAccessParserContext::CConditionalAccessParserContext(HRESULT *result)
  : CParserContext(result)
{
  this->lastSectionCrc32 = SECTION_CRC32_UNDEFINED;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->parser = new CConditionalAccessParser(result);

    CHECK_POINTER_HRESULT(*result, this->parser, *result, E_OUTOFMEMORY);
  }
}

CConditionalAccessParserContext::~CConditionalAccessParserContext(void)
{
}

/* get methods */

CConditionalAccessParser *CConditionalAccessParserContext::GetParser(void)
{
  return (CConditionalAccessParser *)__super::GetParser();
}

/* set methods */

HRESULT CConditionalAccessParserContext::SetKnownSection(CSection *section)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, section);

  if (SUCCEEDED(result))
  {
    CConditionalAccessSection *conditionalAccessSection = dynamic_cast<CConditionalAccessSection *>(section);
    CHECK_POINTER_HRESULT(result, conditionalAccessSection, result, E_INVALIDARG);

    if (SUCCEEDED(result))
    {
      this->lastSectionCrc32 = conditionalAccessSection->GetCrc32();
    }
  }

  return result;
}

/* other methods */

void CConditionalAccessParserContext::Clear(void)
{
  __super::Clear();
  this->lastSectionCrc32 = SECTION_CRC32_UNDEFINED;
}

bool CConditionalAccessParserContext::IsKnownSection(CSection *section)
{
  CConditionalAccessSection *conditionalAccessSection = dynamic_cast<CConditionalAccessSection *>(section);
  bool result = (conditionalAccessSection != NULL);

  if (result)
  {
    result &= (conditionalAccessSection->GetCrc32() == this->lastSectionCrc32);
  }

  return result;
}

/* protected methods */
