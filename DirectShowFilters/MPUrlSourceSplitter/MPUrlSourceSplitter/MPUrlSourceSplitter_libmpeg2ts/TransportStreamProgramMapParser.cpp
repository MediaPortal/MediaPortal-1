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

#include "TransportStreamProgramMapParser.h"
#include "TsPacketConstants.h"
#include "ErrorCodes.h"
#include "ProgramSpecificInformationPacket.h"

CTransportStreamProgramMapParser::CTransportStreamProgramMapParser(HRESULT *result, uint16_t pid)
  : CParser(result)
{
  this->transportStreamProgramMapSectionPID = TRANSPORT_STREAM_PROGRAM_MAP_PARSER_PID_NOT_DEFINED;
  this->currentSection = NULL;
  this->transportStreamProgramMapSectionResult = S_FALSE;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_CONDITION_HRESULT(*result, pid < TS_PACKET_PID_COUNT, *result, E_INVALIDARG);

    if (SUCCEEDED(*result))
    {
      this->currentSection = new CTransportStreamProgramMapSection(result);
      this->transportStreamProgramMapSectionPID = pid;

      CHECK_POINTER_HRESULT(*result, this->currentSection, *result, E_OUTOFMEMORY);
    }
  }
}

CTransportStreamProgramMapParser::~CTransportStreamProgramMapParser(void)
{
  FREE_MEM_CLASS(this->currentSection);
}

/* get methods */

CTransportStreamProgramMapSection *CTransportStreamProgramMapParser::GetTransportStreamProgramMapSection(void)
{
  return this->currentSection;
}

HRESULT CTransportStreamProgramMapParser::GetTransportStreamProgramMapSectionParseResult(void)
{
  return this->transportStreamProgramMapSectionResult;
}

uint16_t CTransportStreamProgramMapParser::GetTransportStreamProgramMapSectionPID(void)
{
  return this->transportStreamProgramMapSectionPID;
}

/* set methods */

/* other methods */

bool CTransportStreamProgramMapParser::IsSectionFound(void)
{
  return this->IsSetFlags(TRANSPORT_STREAM_PROGRAM_MAP_PARSER_FLAG_SECTION_FOUND);
}

HRESULT CTransportStreamProgramMapParser::Parse(CTsPacket *packet)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, packet);
  
  if (SUCCEEDED(result))
  {
    // transport stream program map section packets must have specified PID

    CProgramSpecificInformationPacket *psiPacket = dynamic_cast<CProgramSpecificInformationPacket *>(packet);
    CHECK_POINTER_HRESULT(result, psiPacket, result, E_FAIL);
    CHECK_CONDITION_HRESULT(result, psiPacket->GetPID() == this->GetTransportStreamProgramMapSectionPID(), result, E_FAIL);

    if (SUCCEEDED(result))
    {
      this->flags &= ~TRANSPORT_STREAM_PROGRAM_MAP_PARSER_FLAG_SECTION_FOUND;

      // found program specific information packet with specified PID
      // parse it for transport stream program map section

      this->transportStreamProgramMapSectionResult = this->currentSection->Parse(psiPacket, 0);
      result = this->transportStreamProgramMapSectionResult;

      if (this->transportStreamProgramMapSectionResult == S_FALSE)
      {
        // correct, we need to wait for more PSI packet(s)

        this->flags |= TRANSPORT_STREAM_PROGRAM_MAP_PARSER_FLAG_SECTION_FOUND;
      }
      else if (this->transportStreamProgramMapSectionResult == S_OK)
      {
        // correct, whole transport stream program map section correctly received

        this->flags |= TRANSPORT_STREAM_PROGRAM_MAP_PARSER_FLAG_SECTION_FOUND;
      }
      else if (this->transportStreamProgramMapSectionResult == E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION)
      {
        // current section is empty (no data received for current section), but in PSI packet is section data without new section data

        this->flags |= TRANSPORT_STREAM_PROGRAM_MAP_PARSER_FLAG_SECTION_FOUND;
      }
      else if (this->transportStreamProgramMapSectionResult == E_MPEG2TS_INCOMPLETE_SECTION)
      {
        // current section is not complete, but in PSI packet is started new section without completing current section

        this->flags |= TRANSPORT_STREAM_PROGRAM_MAP_PARSER_FLAG_SECTION_FOUND;
      }
      else if (this->transportStreamProgramMapSectionResult == E_MPEG2TS_SECTION_INVALID_CRC32)
      {
        // current section is corrupted

        this->flags |= TRANSPORT_STREAM_PROGRAM_MAP_PARSER_FLAG_SECTION_FOUND;
      }
    }
  }

  return result;
}

void CTransportStreamProgramMapParser::Clear(void)
{
  __super::Clear();

  this->transportStreamProgramMapSectionResult = S_FALSE;
  this->currentSection->Clear();
}

/* protected methods */