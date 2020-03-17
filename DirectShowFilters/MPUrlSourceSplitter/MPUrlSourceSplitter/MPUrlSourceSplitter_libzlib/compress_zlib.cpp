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

#include "compress_zlib.h"
#include "zlib.h"

#define CHUNK_DATA_LENGTH                                                     16 * 1024

HRESULT compress_zlib(const uint8_t *source, uint32_t sourceLength, uint8_t **destination, uint32_t *destinationLength, int compressionLevel)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, source);
  CHECK_POINTER_DEFAULT_HRESULT(result, destination);
  CHECK_POINTER_DEFAULT_HRESULT(result, destinationLength);

  if (SUCCEEDED(result))
  {
    *destination = NULL;
  }

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < 2)); i++)
  {
    unsigned int encodedLength = 0;
    ALLOC_MEM_DEFINE_SET(strm, z_stream, 1, 0);
    CHECK_POINTER_HRESULT(result, strm, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      /* allocate deflate state */
      strm->zalloc = Z_NULL;
      strm->zfree = Z_NULL;
      strm->opaque = Z_NULL;

      result = (deflateInit(strm, compressionLevel) == Z_OK) ? S_OK : E_FAIL;

      if (SUCCEEDED(result))
      {
        ALLOC_MEM_DEFINE_SET(inputChunk, uint8_t, CHUNK_DATA_LENGTH, 0);
        ALLOC_MEM_DEFINE_SET(outputChunk, uint8_t, CHUNK_DATA_LENGTH, 0);
        uint32_t processed = 0;
        int flush = 0;

        // get necessary size of output buffer
        // compress until end of data
        do
        {
          strm->avail_in = min(CHUNK_DATA_LENGTH, sourceLength - processed);

          flush = ((sourceLength - processed) <= CHUNK_DATA_LENGTH) ? Z_FINISH : Z_NO_FLUSH;
          strm->next_in = inputChunk;

          if (strm->avail_in > 0)
          {
            memcpy(inputChunk, source + processed, strm->avail_in);
          }

          // run deflate() on input until output buffer not full, finish compression if all of source has been read in
          do
          {
            strm->avail_out = CHUNK_DATA_LENGTH;
            strm->next_out = outputChunk;

            result = (deflate(strm, flush) >= 0) ? S_OK : E_FAIL;

            unsigned int encoded = CHUNK_DATA_LENGTH - strm->avail_out;

            if ((encoded > 0) && ((*destination) != NULL))
            {
              // copy data only when created buffer
              memcpy((*destination) + encodedLength, outputChunk, encoded);
            }

            encodedLength += encoded;
          }
          while (strm->avail_out == 0);
          processed +=  min(CHUNK_DATA_LENGTH, sourceLength - processed);

          // done when last data in buffer processed
        }
        while (flush != Z_FINISH);

        FREE_MEM(inputChunk);
        FREE_MEM(outputChunk);
      }

      // clean up
      (void)deflateEnd(strm);
    }

    if (SUCCEEDED(result) && (i == 0) && (encodedLength > 0))
    {
      // everything is OK, first pass
      *destination = ALLOC_MEM_SET(*destination, uint8_t, encodedLength, 0);
      CHECK_POINTER_HRESULT(result, *destination, result, E_OUTOFMEMORY);
    }

    FREE_MEM(strm);

    if (SUCCEEDED(result) && (i == 1))
    {
      *destinationLength = encodedLength;
    }
  }

  return result;
}

HRESULT decompress_zlib(const uint8_t *source, uint32_t sourceLength, uint8_t **destination, uint32_t *destinationLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, source);
  CHECK_POINTER_DEFAULT_HRESULT(result, destination);
  CHECK_POINTER_DEFAULT_HRESULT(result, destinationLength);

  if (SUCCEEDED(result))
  {
    *destination = NULL;
  }

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < 2)); i++)
  {
    unsigned int encodedLength = 0;
    ALLOC_MEM_DEFINE_SET(strm, z_stream, 1, 0);
    CHECK_POINTER_HRESULT(result, strm, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      /* allocate deflate state */
      strm->zalloc = Z_NULL;
      strm->zfree = Z_NULL;
      strm->opaque = Z_NULL;

      result = (inflateInit(strm) == Z_OK) ? S_OK : E_FAIL;

      if (SUCCEEDED(result))
      {
        ALLOC_MEM_DEFINE_SET(inputChunk, uint8_t, CHUNK_DATA_LENGTH, 0);
        ALLOC_MEM_DEFINE_SET(outputChunk, uint8_t, CHUNK_DATA_LENGTH, 0);
        uint32_t processed = 0;
        int flush = 0;

        // get necessary size of output buffer
        // compress until end of data
        do
        {
          strm->avail_in = min(CHUNK_DATA_LENGTH, sourceLength - processed);

          flush = ((sourceLength - processed) <= CHUNK_DATA_LENGTH) ? Z_FINISH : Z_NO_FLUSH;
          strm->next_in = inputChunk;

          if (strm->avail_in > 0)
          {
            memcpy(inputChunk, source + processed, strm->avail_in);
          }

          // run deflate() on input until output buffer not full, finish compression if all of source has been read in
          do
          {
            strm->avail_out = CHUNK_DATA_LENGTH;
            strm->next_out = outputChunk;

            result = (inflate(strm, flush) >= 0) ? S_OK : E_FAIL;

            unsigned int encoded = CHUNK_DATA_LENGTH - strm->avail_out;

            if ((encoded > 0) && ((*destination) != NULL))
            {
              // copy data only when created buffer
              memcpy((*destination) + encodedLength, outputChunk, encoded);
            }

            encodedLength += encoded;
          }
          while (strm->avail_out == 0);
          processed +=  min(CHUNK_DATA_LENGTH, sourceLength - processed);

          // done when last data in buffer processed
        }
        while (flush != Z_FINISH);

        FREE_MEM(inputChunk);
        FREE_MEM(outputChunk);
      }

      // clean up
      (void)deflateEnd(strm);
    }

    if (SUCCEEDED(result) && (i == 0) && (encodedLength > 0))
    {
      // everything is OK, first pass
      *destination = ALLOC_MEM_SET(*destination, uint8_t, encodedLength, 0);
      CHECK_POINTER_HRESULT(result, *destination, result, E_OUTOFMEMORY);
    }

    FREE_MEM(strm);

    if (SUCCEEDED(result) && (i == 1))
    {
      *destinationLength = encodedLength;
    }
  }

  return result;
}