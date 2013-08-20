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

#ifndef __BUFFER_HELPER_DEFINED
#define __BUFFER_HELPER_DEFINED

/* read from buffer helpers */

#ifndef RBE8
#define RBE8(buffer, position)                              ((uint8_t)((const uint8_t*)(buffer + position))[0])
#endif

#ifndef RBE8INC
#define RBE8INC(buffer, position, result)                   result = RBE8(buffer, position); \
                                                            position++;
#endif

#ifndef RBE8INC_DEFINE
#define RBE8INC_DEFINE(buffer, position, result, type)      type result = 0; \
                                                            RBE8INC(buffer, position, result);
#endif

#ifndef RBE16
#define RBE16(buffer, position)                             (((uint16_t)((const uint8_t*)(buffer + position))[0] << 8) | \
                                                                       (((const uint8_t*)(buffer + position))[1]))
#endif

#ifndef RBE16INC
#define RBE16INC(buffer, position, result)                  result = RBE16(buffer, position); \
                                                            position += 2;
#endif

#ifndef RBE16INC_DEFINE
#define RBE16INC_DEFINE(buffer, position, result, type)     type result = 0; \
                                                            RBE16INC(buffer, position, result);
#endif


#ifndef RBE24
#define RBE24(buffer, position)                             (((uint32_t)((const uint8_t*)(buffer + position))[0] << 16) | \
                                                                       (((const uint8_t*)(buffer + position))[1] << 8)  | \
                                                                       (((const uint8_t*)(buffer + position))[2]))
#endif

#ifndef RBE24INC
#define RBE24INC(buffer, position, result)                  result = RBE24(buffer, position); \
                                                            position += 3;
#endif

#ifndef RBE24INC_DEFINE
#define RBE24INC_DEFINE(buffer, position, result, type)     type result = 0; \
                                                            RBE24INC(buffer, position, result);
#endif

#ifndef RBE32
#define RBE32(buffer, position)                            (((uint32_t)((const uint8_t*)(buffer + position))[0] << 24) | \
                                                                       (((const uint8_t*)(buffer + position))[1] << 16) | \
                                                                       (((const uint8_t*)(buffer + position))[2] <<  8) | \
                                                                       (((const uint8_t*)(buffer + position))[3]))
#endif

#ifndef RBE32INC
#define RBE32INC(buffer, position, result)                  result = RBE32(buffer, position); \
                                                            position += 4;
#endif

#ifndef RBE32INC_DEFINE
#define RBE32INC_DEFINE(buffer, position, result, type)     type result = 0; \
                                                            RBE32INC(buffer, position, result);
#endif


#ifndef RBE64
#define RBE64(buffer, position)                             (((uint64_t)((const uint8_t*)(buffer + position))[0] << 56) | \
                                                             ((uint64_t)((const uint8_t*)(buffer + position))[1] << 48) | \
                                                             ((uint64_t)((const uint8_t*)(buffer + position))[2] << 40) | \
                                                             ((uint64_t)((const uint8_t*)(buffer + position))[3] << 32) | \
                                                             ((uint64_t)((const uint8_t*)(buffer + position))[4] << 24) | \
                                                             ((uint64_t)((const uint8_t*)(buffer + position))[5] << 16) | \
                                                             ((uint64_t)((const uint8_t*)(buffer + position))[6] <<  8) | \
                                                             ((uint64_t)((const uint8_t*)(buffer + position))[7]))
#endif

#ifndef RBE64INC
#define RBE64INC(buffer, position, result)                  result = RBE64(buffer, position); \
                                                            position += 8;
#endif

#ifndef RBE64INC_DEFINE
#define RBE64INC_DEFINE(buffer, position, result, type)     type result = 0; \
                                                            RBE64INC(buffer, position, result);
#endif

/* write to buffer helpers */

#ifndef WBE8
#define WBE8(buffer, position, value)                       (*(buffer + position)) = (uint8_t)value;
#endif

#ifndef WBE8INC
#define WBE8INC(buffer, position, value)                    WBE8(buffer, position, value); \
                                                            position++;
#endif

#ifndef WBE16
#define WBE16(buffer, position, value)                      WBE8(buffer, position, (value >> 8)); \
                                                            WBE8(buffer, (position + 1), value);
#endif

#ifndef WBE16INC
#define WBE16INC(buffer, position, value)                   WBE16(buffer, position, value); \
                                                            position += 2;
#endif

#ifndef WBE24
#define WBE24(buffer, position, value)                      WBE8(buffer, position, (value >> 16)); \
                                                            WBE8(buffer, (position + 1), (value >> 8)); \
                                                            WBE8(buffer, (position + 2), value);
#endif

#ifndef WBE24INC
#define WBE24INC(buffer, position, value)                   WBE24(buffer, position, value); \
                                                            position += 3;
#endif

#ifndef WBE32
#define WBE32(buffer, position, value)                      WBE16(buffer, position, (value >> 16)); \
                                                            WBE16(buffer, (position + 2), value);
#endif

#ifndef WBE32INC
#define WBE32INC(buffer, position, value)                   WBE32(buffer, position, value); \
                                                            position += 4;
#endif

#ifndef WBE64
#define WBE64(buffer, position, value)                      WBE32(buffer, position, (value >> 32)); \
                                                            WBE32(buffer, (position + 4), value);
#endif

#ifndef WBE64INC
#define WBE64INC(buffer, position, value)                   WBE64(buffer, position, value); \
                                                            position += 8;
#endif

#endif