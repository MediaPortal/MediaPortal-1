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

#ifndef __MEMORY_DEFINED
#define __MEMORY_DEFINED

#include <ObjBase.h>

#define ALLOC_MEM(type, length)                               (type *)CoTaskMemAlloc(length * sizeof(type))
#define REALLOC_MEM(formerVariable, type, length)             (type *)CoTaskMemRealloc(formerVariable, length * sizeof(type))
#define ALLOC_MEM_DEFINE(variable, type, length)              type *variable = ALLOC_MEM(type, length)
#define ALLOC_MEM_SET(variable, type, length, value)          ALLOC_MEM(type, length); \
                                                              if (variable != NULL) \
                                                              { \
                                                                memset(variable, value, length * sizeof(type)); \
                                                              }
#define ALLOC_MEM_DEFINE_SET(variable, type, length, value)   type *variable = ALLOC_MEM_SET(variable, type, length, value)
#define FREE_MEM(variable)                                    if (variable != NULL) \
                                                              { \
                                                                CoTaskMemFree(variable); \
                                                                variable = NULL; \
                                                              }

#define FREE_MEM_CLASS(variable)                              if (variable != NULL) \
                                                              { \
                                                                delete variable; \
                                                                variable = NULL; \
                                                              }


#define COM_SAFE_RELEASE(instance)                            if (instance != NULL) \
                                                              { \
                                                                instance->Release(); \
                                                                instance = NULL; \
                                                              }

#define CHECK_CONDITION(result, condition, case_true, case_false)                 if (result == 0) { result = (condition) ? case_true : case_false; }

#define CHECK_CONDITION_HRESULT(result, condition, case_true, case_false)         if (SUCCEEDED(result)) { result = (condition) ? case_true : case_false; }

#define CHECK_POINTER_HRESULT(result, pointer, case_true, case_false)             CHECK_CONDITION_HRESULT(result, pointer != NULL, case_true, case_false)
#define CHECK_POINTER_DEFAULT_HRESULT(result, pointer)                            CHECK_POINTER_HRESULT(result, pointer, S_OK, E_POINTER)

#define CHECK_POINTER(result, pointer, case_true, case_false)                     CHECK_CONDITION(result, pointer != NULL, case_true, case_false)

#define CHECK_CONDITION_EXECUTE(condition, command)                               if (condition) { command; }

#define CHECK_CONDITION_EXECUTE_RESULT(condition, command, result)                CHECK_CONDITION_EXECUTE(condition, result = command)

#define CHECK_CONDITION_NOT_NULL_EXECUTE(condition, command)                      CHECK_CONDITION_EXECUTE(condition != NULL, command)

#define CHECK_HRESULT_EXECUTE(result, command)                                    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), command, result)

#endif
