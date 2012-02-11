/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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


#include "HuffmanNode.h"
#include "HuffmanLeafNode.h"



//	External logging method
extern void LogDebug(const char *fmt, ...) ;


//	Cstr
CHuffmanLeafNode::CHuffmanLeafNode(char* leafDecodedString)
{
	decodedString = leafDecodedString;
}

//	Gets the type of this node (branch node)
int CHuffmanLeafNode::GetType()
{
	return HUFFMAN_LEAF_NODE;
}

//	Traverses the tree to decode the huffman data
bool CHuffmanLeafNode::Decode(byte** currentInputByte, byte* currentInputBit, byte* endInputByte, byte** currentOutputByte, byte* endOutputByte, bool* hasFinished)
{
	//	Check we have enough room left in the output buffer
	int decodedStringLength = strlen(decodedString);
	int outputBufferBytesLeft = endOutputByte - *currentOutputByte;

	if(decodedStringLength > outputBufferBytesLeft)
	{
		LogDebug("CHuffmanLeafNode::Decode() - not enough bytes left in output buffer to hold decoded string");

		//	Failed
		return false;
	}
	/*
	//	If we are now past the first bit in the last byte of the buffer, we have finished
	if(*currentInputByte >= endInputByte)// || (*currentInputByte == endInputByte && *currentInputBit >= 1))
	{
		*hasFinished = true;
		return true;
	}
*/
	//LogDebug("Leaf: %s - next bit: %u %u", decodedString, *currentInputByte, *currentInputBit);

	//	Copy the decoded string to the output buffer
	memcpy(*currentOutputByte, decodedString, decodedStringLength);

	//	Move the output buffer to the new end position
	*currentOutputByte = *currentOutputByte + decodedStringLength;
	
	

	return true;
}
