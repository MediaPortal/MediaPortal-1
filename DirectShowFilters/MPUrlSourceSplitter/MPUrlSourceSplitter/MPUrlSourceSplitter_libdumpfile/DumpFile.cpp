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

#include "DumpFile.h"

CDumpFile::CDumpFile(HRESULT *result)
{
  this->dumpFile = NULL;
  this->dumpBoxes = NULL;
  this->size = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->dumpBoxes = new CBoxCollection(result);
    CHECK_POINTER_HRESULT(*result, this->dumpBoxes, *result, E_OUTOFMEMORY);
  }
}

CDumpFile::~CDumpFile(void)
{
  this->DumpBoxes();

  FREE_MEM_CLASS(this->dumpBoxes);
  FREE_MEM(this->dumpFile);
}

/* get methods */

const wchar_t *CDumpFile::GetDumpFile(void)
{
  return this->dumpFile;
}

/* set methods */

bool CDumpFile::SetDumpFile(const wchar_t *dumpFile)
{
  SET_STRING_RETURN_WITH_NULL(this->dumpFile, dumpFile);
}

/* other methods */

bool CDumpFile::AddDumpBox(CDumpBox *dumpBox)
{
  bool result = this->dumpBoxes->Add(dumpBox);

  if (result)
  {
    this->size += (uint32_t)dumpBox->GetSize();
    if (this->size > DUMP_FILE_MINIMUM_DUMP_SIZE)
    {
      this->DumpBoxes();
      this->size = 0;
    }
  }

  return result;
}

void CDumpFile::DumpBoxes(void)
{
  if ((this->dumpBoxes != NULL) && (this->dumpFile != NULL) && (this->size != 0))
  {
    HANDLE dumpFile = CreateFile(this->dumpFile, GENERIC_WRITE, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

    if (dumpFile != INVALID_HANDLE_VALUE)
    {
      // move to end of dump file
      LARGE_INTEGER distanceToMove;
      distanceToMove.QuadPart = 0;

      SetFilePointerEx(dumpFile, distanceToMove, NULL, FILE_END);

      ALLOC_MEM_DEFINE_SET(buffer, uint8_t, this->size, 0);
      if (buffer != NULL)
      {
        unsigned int position = 0;
        for (unsigned int i = 0; i < this->dumpBoxes->Count(); i++)
        {
          CDumpBox *dumpBox = (CDumpBox *)this->dumpBoxes->GetItem(i);

          // there is no reason why GetBox() method can fail
          dumpBox->GetBox(buffer + position, this->size - position);
          position += (uint32_t)dumpBox->GetSize();
        }

        if (position > 0)
        {
          // write data to file
          DWORD written = 0;
          WriteFile(dumpFile, buffer, position, &written, NULL);
        }
      }
      FREE_MEM(buffer);

      CloseHandle(dumpFile);
      this->size = 0;
      this->dumpBoxes->Clear();
    }
  }
}

/* protected methods */