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

#define MAX_SECTION_LENGTH 4096
#define MAX_TABLE_VERSION_NUMBER 31


class CSection
{
  public:
    CSection();
    virtual ~CSection();

    void Reset();
    unsigned short AppendData(const unsigned char* data, unsigned long dataLength);
    bool IsComplete() const;
    bool IsValid() const;

    CSection& operator = (const CSection& section);
    void Copy(const CSection &section);

    unsigned char TableId;
    bool SectionSyntaxIndicator;
    bool PrivateIndicator;
    unsigned short SectionLength;
    unsigned short TableIdExtension;
    unsigned char VersionNumber;
    bool CurrentNextIndicator;
    unsigned char SectionNumber;
    unsigned char LastSectionNumber;

    unsigned short BufferPos;
    unsigned char Data[MAX_SECTION_LENGTH];

    // TODO Kept only for compatibility with TsReader PAT and PMT parsers. Remove if/when possible.
    int table_id;
    int section_length;
    int table_id_extension;
    int version_number;
};