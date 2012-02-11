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


#include "HuffmanNode.h";
#include "HuffmanBranchNode.h"


//	External logging method
extern void LogDebug(const char *fmt, ...) ;

//	Cstr
CHuffmanBranchNode::CHuffmanBranchNode()
{
	left = NULL;
	right = NULL;
}

//	Flzr
CHuffmanBranchNode::~CHuffmanBranchNode()
{
	if(left != NULL)
		delete left;

	if(right != NULL)
		delete right;

	LogDebug("CHuffmanBranchNode::flzr()");
}

//	Gets the type of this node (branch node)
int CHuffmanBranchNode::GetType()
{
	return HUFFMAN_BRANCH_NODE;
}

//	Gets the left node
CHuffmanNode* CHuffmanBranchNode::GetLeft()
{
	return left;
}

//	Sets the right node
void CHuffmanBranchNode::SetLeft(CHuffmanNode* newLeft)
{
	left = newLeft;
}

//	Gets the right node
CHuffmanNode* CHuffmanBranchNode::GetRight()
{
	return right;
}

//	Sets the right node
void CHuffmanBranchNode::SetRight(CHuffmanNode* newRight)
{
	right = newRight;
}

//	Traverses the tree to decode the huffman data
bool CHuffmanBranchNode::Decode(byte** currentInputByte, byte* currentInputBit, byte* endInputByte, byte** currentOutputByte, byte* endOutputByte, bool* hasFinished)
{
	byte testByte = **currentInputByte;

	byte bitTest = 0x80 >> *currentInputBit;
	
	CHuffmanNode* nextNode;

	//	If right (0)
	if((testByte & bitTest) == bitTest)
	{
		nextNode = GetRight();
	}

	//	Else left (1)
	else
	{
		nextNode = GetLeft();
	}

	if(nextNode == NULL)
	{
		LogDebug("CHuffmanBranchNode::Decode() - No branch to follow.  Huffman tree hasnt been initialized properly, or is incomplete");
		*hasFinished = true;
		return false;
	}

	//	Once we go past the end of the input buffer, we have finished
	if(*currentInputByte >= endInputByte)
	{
		*hasFinished = true;
		return true;
	}

	//	Move to the next bit
	(*currentInputBit)++;
	
	if(*currentInputBit == 8)
	{
		*currentInputBit = 0;
		(*currentInputByte)++;
		
	}

	/*
	//	Once we go past the end of the input buffer, we have finished
	if(*currentInputByte > endInputByte)
	{
		*hasFinished = true;
		return true;
	}
*/
	//	Pass to the next node in the tree to decode
	return nextNode->Decode(currentInputByte, currentInputBit, endInputByte, currentOutputByte, endOutputByte, hasFinished);
}
