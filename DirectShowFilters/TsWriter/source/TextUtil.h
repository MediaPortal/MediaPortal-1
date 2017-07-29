/*
 *  Copyright (C) 2006-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
// The Dish network code is based on code taken from a thread on DVBN http://dvbn.happysat.org/viewtopic.php?t=28761&highlight=dish+epg+myth
// which seems to be the basis for MythTV code.
#pragma once
#include <map>
#include <vector>
#include "MhwProvider.h"

using namespace std;


class CTextUtil
{
  public:
    static bool AtscScteMultipleStringStructureToStrings(unsigned char* data,
                                                          unsigned short dataLength,
                                                          map<unsigned long, char*>& strings);
    static bool AtscScteMultilingualTextToString(unsigned char* data,
                                                  unsigned short dataLength,
                                                  char** text);
    static bool DishTextToString(unsigned char* data,
                                  unsigned char dataLength,
                                  unsigned char tableId,
                                  char** text);
    static bool DvbTextToString(unsigned char* data, unsigned short dataLength, char** text);
    static bool IsoIec10646ToString(unsigned char* data, unsigned short dataLength, char** text);
    static bool MhwTextToString(unsigned char* data,
                                unsigned short dataLength,
                                MhwProvider provider,
                                char** text);
    static bool OpenTvTextToString(unsigned char* data,
                                    unsigned char dataLength,
                                    bool isItalian,
                                    char** text);

  private:
    struct HuffmanSequence
    {
      unsigned char EncodedSequenceBitCount;
      unsigned long EncodedSequence;
      unsigned char DecodedSequenceByteCount;
      char* DecodedSequence;
    };

    struct CodePageEntry
    {
      unsigned char Utf8EncodedByteCount;
      char* Utf8Encoding;
    };

    static bool AtscScteTextToString(unsigned char* data,
                                      unsigned short dataLength,
                                      unsigned char compressionType,
                                      unsigned char mode,
                                      char** text);
    static bool AtscScteCombineSegments(vector<char*>& segments, char** text);
    static bool BbcHuffmanToString(unsigned char* data,
                                    unsigned short dataLength,
                                    unsigned char tableId,
                                    char** text);
    static unsigned char GetBit(unsigned long bitIndex, const unsigned char* data);
    static bool MultiRootHuffmanToString(unsigned char* data,
                                          unsigned short dataLength,
                                          unsigned char* huffmanTable,
                                          bool isUtf8UncompressedContext,
                                          char** text);
    static bool SingleRootHuffmanToString(unsigned char* data,
                                          unsigned short dataLength,
                                          unsigned char dataBitOffset,
                                          HuffmanSequence* huffmanTable,
                                          unsigned short tableSize,
                                          unsigned char encodedSequenceBitCountMinimum,
                                          unsigned char encodedSequenceBitCountMaximum,
                                          unsigned char decodedSequenceByteCountMaximum,
                                          char** text);
    static void MinimiseMemoryUsage(char* input, unsigned long actualInputLength, char** output);

    // multi-root tables
    static const unsigned char HUFFMAN_TABLE_BBC_1[];
    static const unsigned char HUFFMAN_TABLE_BBC_2[];
    static const unsigned char HUFFMAN_TABLE_GENERAL_INSTRUMENTS_1[];
    static const unsigned char HUFFMAN_TABLE_GENERAL_INSTRUMENTS_2[];

    // single root tables
    static const HuffmanSequence HUFFMAN_TABLE_DISH_NETWORK_128[];
    static const HuffmanSequence HUFFMAN_TABLE_DISH_NETWORK_255[];
    static const HuffmanSequence HUFFMAN_TABLE_OPENTV_DEFAULT[];
    static const HuffmanSequence HUFFMAN_TABLE_OPENTV_SKY_ITALY[];

    static const CodePageEntry WIN1250_TO_UTF8[];
};