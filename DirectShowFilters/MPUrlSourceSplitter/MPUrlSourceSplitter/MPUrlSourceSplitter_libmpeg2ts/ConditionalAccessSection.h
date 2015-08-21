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

#pragma once

#ifndef __CONDITIONAL_ACCESS_SECTION_DEFINED
#define __CONDITIONAL_ACCESS_SECTION_DEFINED

#include "Section.h"
#include "DescriptorCollection.h"

#define CONDITIONAL_ACCESS_SECTION_FLAG_NONE						              SECTION_FLAG_NONE

#define CONDITIONAL_ACCESS_SECTION_FLAG_LAST                          (SECTION_FLAG_LAST + 0)

#define CONDITIONAL_ACCESS_SECTION_TABLE_ID                           0x00000001

#define CONDITIONAL_ACCESS_SECTION_VERSION_NUMBER_MASK                0x1F
#define CONDITIONAL_ACCESS_SECTION_VERSION_NUMBER_SHIFT               1

#define CONDITIONAL_ACCESS_SECTION_CURRENT_NEXT_INDICATOR_MASK        0x01
#define CONDITIONAL_ACCESS_SECTION_CURRENT_NEXT_INDICATOR_SHIFT       0

class CConditionalAccessSection : public CSection
{
public:
	CConditionalAccessSection(HRESULT *result);
	virtual ~CConditionalAccessSection(void);

	/* get methods */

  // gets version
  // @return : version
  unsigned int GetVersion(void);

  // gets section number
  // @return : section number
  unsigned int GetSectionNumber(void);

  // gets last section number
  // @return : last section number
  unsigned int GetLastSectionNumber(void);

  // gets descriptors
  // @return : decriptor collections
  CDescriptorCollection *GetDescriptors(void);

	/* set methods */

	/* other methods */

	// tests if current next indicator is set
	// @return : true if current next indicator is set, false otherwise
	bool IsCurrentNextIndicator(void);

  // parses specified section payload
  // @param sectionPayload : the section payload to parse
  // @return : S_OK if successfull, S_FALSE if more section payloads are needed to complete section, error code otherwise
  virtual HRESULT Parse(CSectionPayload *sectionPayload);

	// clears current instance to its default state
	virtual void Clear(void);

protected:
	// program association section structure (after section structure)
	// reserved                     18 bits
	// version_number               5 bits
	// current_next_indicator       1 bit
	// section_number               8 bits
	// last_section_number          8 bits
	// for (i = 0; i < N; i++)
	// {
	//    descriptor                variable bits
	// }

	// holds conditional access section header
	uint8_t reservedVersionNumberCurrentNextIndicator;
	uint8_t sectionNumber;
	uint8_t lastSectionNumber;

	// holds descriptors
  CDescriptorCollection *descriptors;

	/* methods */

	// gets new instance of section
	// @return : new section instance or NULL if error
	virtual CSection *CreateItem(void);

	// deeply clones current instance
	// @param item : the section instance to clone
	// @return : true if successful, false otherwise
	virtual bool InternalClone(CSection *item);

	// gets whole section size
	// method is called to determine section size for storing section into buffer
	// @return : size of section
	virtual unsigned int GetSectionCalculatedSize(void);

	// gets whole section into payload
	// @return : number of bytes written into buffer, zero if not successful
	virtual unsigned int GetSectionInternal(void);

	// checks table ID against actual table ID
	// @return : true if table ID is valid, false otherwise
	virtual bool CheckTableId(void);
};

#endif