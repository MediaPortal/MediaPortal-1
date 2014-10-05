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

#include "ProgramAssociationParser.h"
#include "TsPacketConstants.h"
#include "ErrorCodes.h"
#include "ProgramSpecificInformationPacket.h"

CProgramAssociationParser::CProgramAssociationParser(HRESULT *result)
  : CParser(result)
{
  this->currentSection = NULL;
  this->programAssociationSectionResult = S_FALSE;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->currentSection = new CProgramAssociationSection(result);

    CHECK_POINTER_HRESULT(*result, this->currentSection, *result, E_OUTOFMEMORY);
  }
}

CProgramAssociationParser::~CProgramAssociationParser(void)
{
  FREE_MEM_CLASS(this->currentSection);
}

/* get methods */

CProgramAssociationSection *CProgramAssociationParser::GetProgramAssociationSection(void)
{
  return this->currentSection;
}

HRESULT CProgramAssociationParser::GetProgramAssociationSectionParseResult(void)
{
  return this->programAssociationSectionResult;
}

/* set methods */

/* other methods */

bool CProgramAssociationParser::IsSectionFound(void)
{
  return this->IsSetFlags(PROGRAM_ASSOCIATION_PARSER_FLAG_SECTION_FOUND);
}

HRESULT CProgramAssociationParser::Parse(CTsPacket *packet)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, packet);
  
  if (SUCCEEDED(result))
  {
    // program association section packets have always PID 0x0000

    CProgramSpecificInformationPacket *psiPacket = dynamic_cast<CProgramSpecificInformationPacket *>(packet);
    CHECK_POINTER_HRESULT(result, psiPacket, result, E_FAIL);
    CHECK_CONDITION_HRESULT(result, psiPacket->GetPID() == PROGRAM_ASSOCIATION_PARSER_PSI_PACKET_PID, result, E_FAIL);

    if (SUCCEEDED(result))
    {
      this->flags &= ~PROGRAM_ASSOCIATION_PARSER_FLAG_SECTION_FOUND;

      // found program specific information packet with PID 0x0000
      // parse it for program association section

      this->programAssociationSectionResult = this->currentSection->Parse(psiPacket, 0);
      result = this->programAssociationSectionResult;

      if (this->programAssociationSectionResult == S_FALSE)
      {
        // correct, we need to wait for more PSI packet(s)

        this->flags |= PROGRAM_ASSOCIATION_PARSER_FLAG_SECTION_FOUND;
      }
      else if (this->programAssociationSectionResult == S_OK)
      {
        // correct, whole program association section correctly received

        this->flags |= PROGRAM_ASSOCIATION_PARSER_FLAG_SECTION_FOUND;
      }
      else if (this->programAssociationSectionResult == E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION)
      {
        // current section is empty (no data received for current section), but in PSI packet is section data without new section data

        this->flags |= PROGRAM_ASSOCIATION_PARSER_FLAG_SECTION_FOUND;
      }
      else if (this->programAssociationSectionResult == E_MPEG2TS_INCOMPLETE_SECTION)
      {
        // current section is not complete, but in PSI packet is started new section without completing current section

        this->flags |= PROGRAM_ASSOCIATION_PARSER_FLAG_SECTION_FOUND;
      }
      else if (this->programAssociationSectionResult == E_MPEG2TS_SECTION_INVALID_CRC32)
      {
        // current section is corrupted

        this->flags |= PROGRAM_ASSOCIATION_PARSER_FLAG_SECTION_FOUND;
      }
    }
  }

  return result;
}

void CProgramAssociationParser::Clear(void)
{
  __super::Clear();

  this->programAssociationSectionResult = S_FALSE;
  this->currentSection->Clear();
}

/* protected methods */