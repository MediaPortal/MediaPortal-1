///////////////////////////////////////////////////////////////////////////////
// StdUtils plug-in for NSIS
// Copyright (C) 2004-2013 LoRd_MuldeR <MuldeR2@GMX.de>
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//
// http://www.gnu.org/licenses/lgpl-2.1.txt
///////////////////////////////////////////////////////////////////////////////

#ifndef __MSVC_FUNC_H__
#define __MSVC_FUNC_H__

#ifdef __cplusplus
extern "C" {
#endif

long time(long *time);
int _snwprintf(wchar_t *buffer, size_t count, const wchar_t *format, ...);
int _snprintf(char *buffer, size_t count, const char *format, ...);
int sscanf(const char *input, const char * format, ...);
int swscanf(const wchar_t *input, const wchar_t * format, ...);
int _stricmp(const char *string1, const char *string2);
int _wcsicmp(const wchar_t *string1, const wchar_t *string2);

uintptr_t _beginthreadex( 
	void *security,
	unsigned stack_size,
	unsigned (__stdcall *start_address)(void*),
	void *arglist,
	unsigned initflag,
	unsigned *thrdaddr
);

#ifdef __cplusplus
}
#endif

#endif //__MSVC_FUNC_H__
