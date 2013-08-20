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

#ifndef __COMPRESS_ZLIB
#define __COMPRESS_ZLIB

#include <stdint.h>

// compress from source to destination
// @param source : the source to compress
// @param sourceLength : the length of buffer to compress
// @param destination : the destination compressed buffer
// @param destinationLength : the length of compressed buffer
// @param compressionLevel : compression level (0 - 9)
// @return : S_OK if successful, error code otherwise
HRESULT compress_zlib(const uint8_t *source, uint32_t sourceLength, uint8_t **destination, uint32_t *destinationLength, int compressionLevel);

// decompress from source to destination
// @param source : the source to decompress
// @param sourceLength : the length of buffer to decompress
// @param destination : the destination decompressed buffer
// @param destinationLength : the length of decompressed buffer
// @return : S_OK if successful, error code otherwise
HRESULT decompress_zlib(const uint8_t *source, uint32_t sourceLength, uint8_t **destination, uint32_t *destinationLength);

#endif