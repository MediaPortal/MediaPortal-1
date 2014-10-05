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

#include "SectionContext.h"

CSectionContext::CSectionContext(HRESULT *result)
  : CFlags()
{
  this->flags = SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_EMPTY;
  this->originalSection = NULL;
  this->updatedSection = NULL;
  this->packetCount = 0;
  this->continuityCounter = 0;
  this->packets = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->packets = new CTsPacketCollection(result);

    CHECK_POINTER_HRESULT(*result, this->packets, *result, E_OUTOFMEMORY);
  }
}

CSectionContext::~CSectionContext(void)
{
  FREE_MEM_CLASS(this->originalSection);
  FREE_MEM_CLASS(this->updatedSection);
  FREE_MEM_CLASS(this->packets);
}

/* get methods */

CSection *CSectionContext::GetOriginalSection(void)
{
  return this->originalSection;
}

CSection *CSectionContext::GetUpdatedSection(void)
{
  return this->updatedSection;
}

unsigned int CSectionContext::GetPacketCount(void)
{
  return this->packetCount;
}

unsigned int CSectionContext::GetContinuityCounter(void)
{
  return this->continuityCounter;
}

CTsPacketCollection *CSectionContext::GetPackets(void)
{
  return this->packets;
}

/* set methods */

bool CSectionContext::SetOriginalSection(CSection *section)
{
  FREE_MEM_CLASS(this->originalSection);
  this->originalSection = section;

  return true;
}

void CSectionContext::SetOriginalSectionEmpty(bool sectionEmpty)
{
  this->flags &= ~SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_EMPTY;
  this->flags |= sectionEmpty ? SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_EMPTY : SECTION_CONTEXT_FLAG_NONE;
}

void CSectionContext::SetOriginalSectionIncomplete(bool sectionIncomplete)
{
  this->flags &= ~SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_INCOMPLETE;
  this->flags |= sectionIncomplete ? SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_INCOMPLETE : SECTION_CONTEXT_FLAG_NONE;
}

void CSectionContext::SetOriginalSectionComplete(bool sectionComplete)
{
  this->flags &= ~SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_COMPLETE;
  this->flags |= sectionComplete ? SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_COMPLETE : SECTION_CONTEXT_FLAG_NONE;
}

void CSectionContext::SetOriginalSectionError(bool sectionError)
{
  this->flags &= ~SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_ERROR;
  this->flags |= sectionError ? SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_ERROR : SECTION_CONTEXT_FLAG_NONE;
}

void CSectionContext::SetPacketCount(unsigned int packetCount)
{
  this->packetCount = packetCount;
}

void CSectionContext::SetContinuityCounter(unsigned int continuityCounter)
{
  this->continuityCounter = continuityCounter;
}

/* other methods */

bool CSectionContext::IsOriginalSectionEmpty(void)
{
  return this->IsSetFlags(SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_EMPTY);
}

bool CSectionContext::IsOriginalSectionIncomplete(void)
{
  return this->IsSetFlags(SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_INCOMPLETE);
}

bool CSectionContext::IsOriginalSectionComplete(void)
{
  return this->IsSetFlags(SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_COMPLETE);
}

bool CSectionContext::IsOriginalSectionError(void)
{
  return this->IsSetFlags(SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_ERROR);
}

/* protected methods */