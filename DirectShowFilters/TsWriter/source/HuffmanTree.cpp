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



#include "HuffmanTree.h"



//	External logging method
extern void LogDebug(const char *fmt, ...) ;

// Constr
CHuffmanTree::CHuffmanTree()
{
	trunkNode = NULL;
}

//	Dstr
CHuffmanTree::~CHuffmanTree()
{
	if(trunkNode != NULL)
		delete trunkNode;
}

//	Adds a leaf node decoded string with the specified path
void CHuffmanTree::AddLeaf(int path, int bitLength, char* decodedString)
{
	//	Shift the bits to the left hand side
	path = path << (32 - bitLength);
	
	if(trunkNode == NULL)
		trunkNode = new CHuffmanBranchNode();

	CHuffmanBranchNode* currentNode = (CHuffmanBranchNode*) trunkNode;

	//	Loop through all the bits to build the tree path for this leaf
	for(int i = 0; i < bitLength - 1; i++)
	{		
		CHuffmanBranchNode* connectingNode = NULL;

		//	If next bit 0, branch left
		if((path & 0x80000000) == 0x00000000)
		{		
			connectingNode = (CHuffmanBranchNode*)currentNode->GetLeft();

			//	Create if doesnt exist
			if(connectingNode == NULL)
			{
				connectingNode = new CHuffmanBranchNode();
				currentNode->SetLeft(connectingNode);
			}
		}

		//	If next bit 1, branch right
		else
		{
			connectingNode = (CHuffmanBranchNode*)currentNode->GetRight();

			//	Create if doesnt exist
			if(connectingNode == NULL)
			{
				connectingNode = new CHuffmanBranchNode();
				currentNode->SetRight(connectingNode);
			}
		}

		//	Move to next bit
		path = path << 1;

		//	Move to the connecting node
		currentNode = connectingNode;
	}

	//	Create the leaf node
	CHuffmanLeafNode* newLeafNode = new CHuffmanLeafNode(decodedString);


	//	Leaf on left
	if((path & 0x80000000) == 0x00000000)
	{
		//	Verify the leaf is empty
		if(currentNode->GetLeft() != NULL)
		{
			LogDebug("CHuffmanTree::AddLeaf() - Error building Huffman tree, leaf node not empty as expected");
			return;
		}

		currentNode->SetLeft(newLeafNode);

		//LogDebug("CHuffmanTree::AddLeaf() - Added leaf node for \"%s\" on left", decodedString);
	}

	//	Leaf on right
	else
	{
		//	Verify the leaf is empty
		if(currentNode->GetRight() != NULL)
		{
			LogDebug("CHuffmanTree::AddLeaf() - Error building Huffman tree, leaf node not empty as expected");
			return;
		}

		currentNode->SetRight(newLeafNode);
		
		//LogDebug("CHuffmanTree::AddLeaf() - Added leaf node for \"%s\" on right", decodedString);
	}
}

//	Decodes the input string using the huffman tree, returning the decoded length
int CHuffmanTree::Decode(byte* inputBuffer, int inputLength, byte startBit, byte* outputBuffer, int outputLength)
{
	byte* currentInputByte = inputBuffer;
	byte* currentInputBit = &startBit;
	byte* endInputByte = inputBuffer + inputLength;

	byte* currentOutputByte = outputBuffer;
	byte* endOutputByte = outputBuffer + outputLength;

	//	Allow for terminating byte
	outputLength--;	

	//	Safeguard to prevent a bug causing an infinite loop!
	int i = 0;
	int maxIterations = 8192;

	//LogDebug("Input length: %u", inputLength);

	try
	{
		bool hasFinished = false;

		while(!hasFinished && i < maxIterations)
		{
			bool result = trunkNode->Decode(&currentInputByte, currentInputBit, endInputByte, &currentOutputByte, endOutputByte, &hasFinished);

			if(!result)
			{
				//LogDebug("CHuffmanTree::Decode() - Error, failed to decode huffman encoded buffer");
				*outputBuffer = 0;
				return 0;
			}

			i++;
		}

		if(i >= 8192)
		{
			LogDebug("CHuffmanTree::Decode - Error parsing decoded buffer, max iterations exceeded");
			*outputBuffer = 0;
			return 0;
		}
	}
	catch(...)
	{
		LogDebug("CHuffmanTree::Decode - Exception whilst decoding input buffer");
		return 0;
	}

	//	Add a terminating character to the output buffer
	*currentOutputByte = 0;

	//LogDebug("CHuffmanTree::Decode - Item decoded successfully:");
	//LogDebug(outputBuffer);

	
	//	Return the decoded output length
	return currentOutputByte - outputBuffer;
}
