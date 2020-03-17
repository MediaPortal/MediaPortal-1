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

#ifndef __HEX_DEFINED
#define __HEX_DEFINED

// encodes binary input to null-terminated HEX encoded string
// caller is responsible of freeing allocated memory by FREE_MEM() method
// @param input : binary input to encode
// @param lenght : the length of binary input
// @param output : reference to output HEX encoded string
// @return : S_OK if successfull, E_POINTER if input or output is NULL, E_OUTOFMEMORY if cannot be allocated required memory for output
HRESULT hex_encode(const unsigned char *input, unsigned int length, char **output);

// encodes binary input to null-terminated HEX encoded string
// caller is responsible of freeing allocated memory by FREE_MEM() method
// @param input : binary input to encode
// @param lenght : the length of binary input
// @param output : reference to output HEX encoded string
// @param outputLength : reference to output length (can be NULL)
// @return : S_OK if successfull, E_POINTER if input or output is NULL, E_OUTOFMEMORY if cannot be allocated required memory for output
HRESULT hex_encode(const unsigned char *input, unsigned int length, char **output, unsigned int *outputLength);

// encodes binary input to null-terminated unicode HEX encoded string
// caller is responsible of freeing allocated memory by FREE_MEM() method
// @param input : binary input to encode
// @param lenght : the length of binary input
// @param output : reference to output unicode HEX encoded string
// @return : S_OK if successfull, E_POINTER if input or output is NULL, E_OUTOFMEMORY if cannot be allocated required memory for output
HRESULT hex_encode(const unsigned char *input, unsigned int length, wchar_t **output);

// encodes binary input to null-terminated unicode HEX encoded string
// caller is responsible of freeing allocated memory by FREE_MEM() method
// @param input : binary input to encode
// @param lenght : the length of binary input
// @param output : reference to output unicode HEX encoded string
// @param outputLength : reference to output length (can be NULL)
// @return : S_OK if successfull, E_POINTER if input or output is NULL, E_OUTOFMEMORY if cannot be allocated required memory for output
HRESULT hex_encode(const unsigned char *input, unsigned int length, wchar_t **output, unsigned int *outputLength);

// decodes HEX encoded string to binary output
// caller is responsible of freeing allocated memory by FREE_MEM() method
// @param input : HEX encoded string to decode
// @param output : reference to binary output
// @param outputLength : reference to the length of binary output
// @return : S_OK if successfull, E_POINTER if input, output or length are NULL, E_INVALIDARG if input contains not valid HEX character, E_OUTOFMEMORY if cannot be allocated required memory for output
HRESULT hex_decode(const char *input, unsigned char **output, unsigned int *outputLength);

// decodes HEX encoded string to binary output
// caller is responsible of freeing allocated memory by FREE_MEM() method
// @param input : HEX encoded string to decode
// @param inputLength : the length of HEX encoded string
// @param output : reference to binary output
// @param outputLength : reference to the length of binary output
// @return : S_OK if successfull, E_POINTER if input, output or length are NULL, E_INVALIDARG if input contains not valid HEX character, E_OUTOFMEMORY if cannot be allocated required memory for output
HRESULT hex_decode(const char *input, unsigned int inputLength, unsigned char **output, unsigned int *outputLength);

// decodes unicode HEX encoded string to binary output
// caller is responsible of freeing allocated memory by FREE_MEM() method
// @param input : unicode HEX encoded string to decode
// @param output : reference to binary output
// @param outputLength : reference to the length of binary output
// @return : S_OK if successfull, E_POINTER if input, output or length are NULL, E_INVALIDARG if input contains not valid HEX character, E_OUTOFMEMORY if cannot be allocated required memory for output
HRESULT hex_decode(const wchar_t *input, unsigned char **output, unsigned int *outputLength);

// decodes unicode HEX encoded string to binary output
// caller is responsible of freeing allocated memory by FREE_MEM() method
// @param input : unicode HEX encoded string to decode
// @param inputLength : the length of HEX encoded string
// @param output : reference to binary output
// @param outputLength : reference to the length of binary output
// @return : S_OK if successfull, E_POINTER if input, output or length are NULL, E_INVALIDARG if input contains not valid HEX character, E_OUTOFMEMORY if cannot be allocated required memory for output
HRESULT hex_decode(const wchar_t *input, unsigned int inputLength, unsigned char **output, unsigned int *outputLength);

// get HEX character for binary number
// @param c : binary number to get HEX character
// @return : HEX character or (char)0 if error
char get_charA(unsigned char c);

// get HEX character for binary number
// @param c : binary number to get HEX character
// @return : HEX character or (wchar_t)0 if error
wchar_t get_charW(unsigned char c);

#endif