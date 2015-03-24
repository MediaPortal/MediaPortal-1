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

#ifndef __SAMPLE_TO_CHUNK_DEFINED
#define __SAMPLE_TO_CHUNK_DEFINED

#include <stdint.h>

class CSampleToChunk
{
public:
  // initializes a new instance of CSampleToChunk class
  CSampleToChunk(HRESULT *result);

  // destructor
  ~CSampleToChunk(void);

  /* get methods */

  // gets the index of the first chunk in this run 
  // @return : the index of the first chunk in this run 
  virtual uint32_t GetFirstChunk(void);

  // gets integer that gives the number of samples in each of these chunks
  // @return : integer that gives the number of samples in each of these chunks
  virtual uint32_t GetSamplesPerChunk(void);

  // gets the index of the sample entry that describes the samples in this chunk
  // the index ranges from 1 to the number of sample entries in the Sample Description Box
  // @return : the index of the sample entry that describes the samples in this chunk
  virtual uint32_t GetSampleDescriptionIndex(void);

  /* set methods */

  // sets the index of the first chunk in this run 
  // @param firstChunk : the index of the first chunk in this run to set
  virtual void SetFirstChunk(uint32_t firstChunk);

  // gets integer that gives the number of samples in each of these chunks
  // @param samplesPerChunk : the number of samples in each of these chunks to set
  virtual void SetSamplesPerChunk(uint32_t samplesPerChunk);

  // gets the index of the sample entry that describes the samples in this chunk
  // the index ranges from 1 to the number of sample entries in the Sample Description Box
  // @param sampleDescriptionIndex : the index of the sample entry that describes the samples in this chunk to set
  virtual void SetSampleDescriptionIndex(uint32_t sampleDescriptionIndex);

  /* other methods */

protected:

  // stores the index of the first chunk in this run of chunks that share the same samples-per-chunk
  // and sample-description-index; the index of the first chunk in a track has the value 1 (the first_chunk field
  // in the first record of this box has the value 1, identifying that the first sample maps to the first chunk)
  uint32_t firstChunk;

  // stores integer that gives the number of samples in each of these chunks
  uint32_t samplesPerChunk;

  // integer that gives the index of the sample entry that describes the samples in this chunk
  // the index ranges from 1 to the number of sample entries in the Sample Description Box
  uint32_t sampleDescriptionIndex;
};

#endif