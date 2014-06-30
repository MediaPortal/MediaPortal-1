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

#ifndef __DUMP_FILE_DEFINED
#define __DUMP_FILE_DEFINED

#include "BoxCollection.h"
#include "DumpBox.h"

#define DUMP_FILE_MINIMUM_DUMP_SIZE                                   (10 * 1024 * 1024)  // 10 MB of minimum dump data

class CDumpFile
{
public:
  CDumpFile(HRESULT *result);
  ~CDumpFile(void);

  /* get methods */

  // gets dump file name
  // @return : dump file name or NULL if error or not set
  const wchar_t *GetDumpFile(void);

  /* set methods */

  // sets dump file name
  // @param dumpFile : the dump file name
  // @return : true if successful, false otherwise
  bool SetDumpFile(const wchar_t *dumpFile);

  /* other methods */

  // adds dump box
  // @param dumpBox : the dump box to add
  // @return : true if successful, false otherwise
  bool AddDumpBox(CDumpBox *dumpBox);

  // dump packages to dump file
  void DumpBoxes(void);

protected:
  // holds dump file
  wchar_t *dumpFile;
  // holds dump boxes to dump to file
  CBoxCollection *dumpBoxes;
  // holds size of not dumped packages
  unsigned int size;

  /* methods */
};

#endif