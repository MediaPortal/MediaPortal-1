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

#include "ConditionalAccessParser.h"
#include "TsPacketConstants.h"
#include "ErrorCodes.h"
#include "ProgramSpecificInformationPacket.h"

CConditionalAccessParser::CConditionalAccessParser(HRESULT *result)
  : CSectionPayloadParser(result)
{
  this->currentSection = NULL;
  this->conditionalAccessSectionResult = S_FALSE;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->currentSection = new CConditionalAccessSection(result);

    CHECK_POINTER_HRESULT(*result, this->currentSection, *result, E_OUTOFMEMORY);
  }
}

CConditionalAccessParser::~CConditionalAccessParser(void)
{
  FREE_MEM_CLASS(this->currentSection);
}

/* get methods */

CConditionalAccessSection *CConditionalAccessParser::GetConditionalAccessSection(void)
{
  return this->currentSection;
}

HRESULT CConditionalAccessParser::GetConditionalAccessSectionParseResult(void)
{
  return this->conditionalAccessSectionResult;
}

/* set methods */

/* other methods */

bool CConditionalAccessParser::IsSectionFound(void)
{
  return this->IsSetFlags(CONDITIONAL_ACCESS_PARSER_FLAG_SECTION_FOUND);
}

HRESULT CConditionalAccessParser::Parse(CSectionPayload *sectionPayload)
{
  HRESULT result = __super::Parse(sectionPayload);

  if (SUCCEEDED(result))
  {
    this->flags &= ~CONDITIONAL_ACCESS_PARSER_FLAG_SECTION_FOUND;

    // found program specific information packet with PID 0x0001
    // parse it for program association section

    this->conditionalAccessSectionResult = this->currentSection->Parse(sectionPayload);
    result = this->conditionalAccessSectionResult;

    if (this->conditionalAccessSectionResult == S_FALSE)
    {
      // correct, we need to wait for more PSI packet(s)

      this->flags |= CONDITIONAL_ACCESS_PARSER_FLAG_SECTION_FOUND;
    }
    else if (this->conditionalAccessSectionResult == S_OK)
    {
      // correct, whole conditional access section correctly received

      this->flags |= CONDITIONAL_ACCESS_PARSER_FLAG_SECTION_FOUND;
    }
    else if (this->conditionalAccessSectionResult == E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION)
    {
      // current section is empty (no data received for current section), but in PSI packet is section data without new section data

      this->flags |= CONDITIONAL_ACCESS_PARSER_FLAG_SECTION_FOUND;
    }
    else if (this->conditionalAccessSectionResult == E_MPEG2TS_INCOMPLETE_SECTION)
    {
      // current section is not complete, but in PSI packet is started new section without completing current section

      this->flags |= CONDITIONAL_ACCESS_PARSER_FLAG_SECTION_FOUND;
    }
    else if (this->conditionalAccessSectionResult == E_MPEG2TS_SECTION_INVALID_CRC32)
    {
      // current section is corrupted

      this->flags |= CONDITIONAL_ACCESS_PARSER_FLAG_SECTION_FOUND;
    }
  }

  return result;
}

void CConditionalAccessParser::Clear(void)
{
  __super::Clear();

  this->conditionalAccessSectionResult = S_FALSE;
  this->currentSection->Clear();
}

/* protected methods */