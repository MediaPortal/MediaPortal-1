// Simple functions to test UTF-8 characters.
// Copyright (C)2010 Francois-R.Boyer@PolyMtl.ca
// First version 2010-08
//
// Written for notepad++, and distributed under same license:
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 2 of the License, or (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

#pragma once

namespace Utf8 { // could be a static class, instead of a namespace, if it needs private members
	// basic classification of UTF-8 bytes
	inline static bool isSingleByte(unsigned char c)       { return c < 0x80; }
	inline static bool isPartOfMultibyte(unsigned char c)  { return c >= 0x80; }
	inline static bool isFirstOfMultibyte(unsigned char c) { return c >= 0xC2 && c < 0xF5; } // 0xF5 to 0xFD are defined by UTF-8, but are not currently valid Unicode
	inline static bool isContinuation(unsigned char c)     { return (c & 0xC0) == 0x80; }
	inline static bool isValid(unsigned char c)            { return c < 0xC0 || isFirstOfMultibyte(c); }	// validates a byte, out of context

	// number of continuation bytes for a given valid first character (0 for single byte characters)
	inline static int  continuationBytes(unsigned char c)  {
		static const char _len[] = { 1,1,2,3 };
		return (c < 0xC0) ? 0 : _len[(c & 0x30) >>  4];
	} 

	// validates a full character
	inline static bool isValid(const unsigned char* buf, int buflen) {
		if(isSingleByte(buf[0])) return true; // single byte is valid
		if(!isFirstOfMultibyte(buf[0])) return false; // not single byte, nor valid multi-byte first byte
		int charContinuationBytes = continuationBytes(buf[0]);
		if(buflen < charContinuationBytes+1) return false; // character does not fit in buffer
		for(int i = charContinuationBytes; i>0; --i)
			if(!isContinuation(*(++buf))) return false; // not enough continuation bytes
		return true;  // the character is valid (if there are too many continuation bytes, it is the next character that will be invalid)
	}

	// rewinds to the first byte of a multi-byte character for any valid UTF-8 (and will not rewind too much on any other input)
	inline static int characterStart(const unsigned char* buf, int startingIndex) {
		int charContinuationBytes = 0;
		while(charContinuationBytes < startingIndex	// rewind past start of buffer?
			&& charContinuationBytes < 5	// UTF-8 support up to 5 continuation bytes (but valid sequences currently do not have more than 3)
			&& isContinuation(buf[startingIndex-charContinuationBytes])
			)
			++charContinuationBytes;
		return startingIndex-charContinuationBytes;
	}

    //validates a string
    inline static bool isStringValid(const unsigned char* buf, size_t len) {
		int n;
		for (int i = 0; i < len; ++i) {
			unsigned char c = (unsigned char) buf[i];
			//if (c==0x09 || c==0x0a || c==0x0d || (0x20 <= c && c <= 0x7e) ) n = 0; // is_printable_ascii
			if (0x00 <= c && c <= 0x7f) {
				n=0; // 0bbbbbbb
			} else if ((c & 0xE0) == 0xC0) {
				n=1; // 110bbbbb
			} else if ( c==0xed && i<(len-1) && ((unsigned char)buf[i+1] & 0xa0)==0xa0) {
				return false; //U+d800 to U+dfff
			} else if ((c & 0xF0) == 0xE0) {
				n=2; // 1110bbbb
			} else if ((c & 0xF8) == 0xF0) {
				n=3; // 11110bbb
			//} else if (($c & 0xFC) == 0xF8) { n=4; // 111110bb //byte 5, unnecessary in 4 byte UTF-8
			//} else if (($c & 0xFE) == 0xFC) { n=5; // 1111110b //byte 6, unnecessary in 4 byte UTF-8
			} else {
				return false;
			}

			for (int j = 0; j < n && i < len; ++j) { // n bytes matching 10bbbbbb follow ?
				if ((++i == len) || (( (unsigned char)buf[i] & 0xC0) != 0x80)) {
					return false;
				}
			}
		}
		return true;
	}
};
