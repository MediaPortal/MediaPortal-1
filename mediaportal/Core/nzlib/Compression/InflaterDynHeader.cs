// InflaterDynHeader.cs
// Copyright (C) 2001 Mike Krueger
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// As a special exception, if you link this library with other files to
// produce an executable, this library does not by itself cause the
// resulting executable to be covered by the GNU General Public License.
// This exception does not however invalidate any other reasons why the
// executable file might be covered by the GNU General Public License.

using System;

using NZlib.Streams;

namespace NZlib.Compression {
	
	public class InflaterDynHeader
	{
		private const int LNUM   = 0;
		private const int DNUM   = 1;
		private const int BLNUM  = 2;
		private const int BLLENS = 3;
		private const int LLENS  = 4;
		private const int DLENS  = 5;
		private const int LREPS  = 6;
		private const int DREPS  = 7;
		private const int FINISH = 8;
		
		private byte[] blLens;
		private byte[] litlenLens;
		private byte[] distLens;
		
		private InflaterHuffmanTree blTree;
		
		private int mode;
		private int lnum, dnum, blnum;
		private int repBits;
		private byte repeatedLen;
		private int ptr;
		
		private static int[] BL_ORDER = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };
		
		public InflaterDynHeader()
		{
		}
		
		public bool Decode(StreamManipulator input)
		{
			decode_loop:
			for (;;) {
				switch (mode) {
					case LNUM:
						lnum = input.PeekBits(5);
						if (lnum < 0) {
							return false;
						}
						lnum += 257;
						input.DropBits(5);
						litlenLens = new byte[lnum];
						//  	    System.err.println("LNUM: "+lnum);
						mode = DNUM;
						goto case DNUM;/* fall through */
					case DNUM:
						dnum = input.PeekBits(5);
						if (dnum < 0) {
							return false;
						}
						dnum++;
						input.DropBits(5);
						distLens = new byte[dnum];
						//  	    System.err.println("DNUM: "+dnum);
						mode = BLNUM;
						goto case BLNUM;/* fall through */
					case BLNUM:
						blnum = input.PeekBits(4);
						if (blnum < 0) {
							return false;
						}
						blnum += 4;
						input.DropBits(4);
						blLens = new byte[19];
						ptr = 0;
						//  	    System.err.println("BLNUM: "+blnum);
						mode = BLLENS;
						goto case BLLENS;/* fall through */
					case BLLENS:
						while (ptr < blnum) {
							int len = input.PeekBits(3);
							if (len < 0) {
								return false;
							}
							input.DropBits(3);
							//  		System.err.println("blLens["+BL_ORDER[ptr]+"]: "+len);
							blLens[BL_ORDER[ptr]] = (byte) len;
							ptr++;
						}
						blTree = new InflaterHuffmanTree(blLens);
						blLens = null;
						ptr = 0;
						mode = LLENS;
						goto case LLENS;/* fall through */
					case LLENS:
						while (ptr < lnum) {
							int symbol = blTree.GetSymbol(input);
							if (symbol < 0) {
								return false;
							}
							switch (symbol) {
								default:
									//  		    System.err.println("litlenLens["+ptr+"]: "+symbol);
									litlenLens[ptr++] = (byte) symbol;
									break;
								case 16: /* repeat last len 3-6 times */
									if (ptr == 0) {
										throw new Exception("Repeating, but no prev len");
									}
									
									//  		    System.err.println("litlenLens["+ptr+"]: repeat");
									repeatedLen = litlenLens[ptr-1];
									repBits = 2;
									for (int i = 3; i-- > 0; ) {
										if (ptr >= lnum) {
											throw new Exception();
										}
										litlenLens[ptr++] = repeatedLen;
									}
									mode = LREPS;
									goto decode_loop;
								case 17: /* repeat zero 3-10 times */
									//  		    System.err.println("litlenLens["+ptr+"]: zero repeat");
									repeatedLen = 0;
									repBits = 3;
									for (int i = 3; i-- > 0; ) {
										if (ptr >= lnum) {
											throw new Exception();
										}
										litlenLens[ptr++] = repeatedLen;
									}
									mode = LREPS;
									goto decode_loop;
								case 18: /* repeat zero 11-138 times */
									//  		    System.err.println("litlenLens["+ptr+"]: zero repeat");
									repeatedLen = 0;
									repBits = 7;
									for (int i = 11; i-- > 0; ) {
										if (ptr >= lnum) {
											throw new Exception();
										}
										litlenLens[ptr++] = repeatedLen;
									}
									mode = LREPS;
									goto decode_loop;
							}
						}
						ptr = 0;
						mode = DLENS;
						goto case DLENS;/* fall through */
						case DLENS:
							while (ptr < dnum) {
								int symbol = blTree.GetSymbol(input);
								if (symbol < 0) {
									return false;
								}
								switch (symbol) {
									default:
										distLens[ptr++] = (byte) symbol;
										//  		    System.err.println("distLens["+ptr+"]: "+symbol);
										break;
									case 16: /* repeat last len 3-6 times */
										if (ptr == 0) {
											throw new Exception("Repeating, but no prev len");
										}
										//  		    System.err.println("distLens["+ptr+"]: repeat");
										repeatedLen = distLens[ptr-1];
										repBits = 2;
										for (int i = 3; i-- > 0; ) {
											if (ptr >= dnum) {
												throw new Exception();
											}
											distLens[ptr++] = repeatedLen;
										}
										mode = DREPS;
										goto decode_loop;
									case 17: /* repeat zero 3-10 times */
										//  		    System.err.println("distLens["+ptr+"]: repeat zero");
										repeatedLen = 0;
										repBits = 3;
										for (int i = 3; i-- > 0; ) {
											if (ptr >= dnum) {
												throw new Exception();
											}
											distLens[ptr++] = repeatedLen;
										}
										mode = DREPS;
										goto decode_loop;
									case 18: /* repeat zero 11-138 times */
										//  		    System.err.println("distLens["+ptr+"]: repeat zero");
										repeatedLen = 0;
										repBits = 7;
										for (int i = 11; i-- > 0; ) {
											if (ptr >= dnum) {
												throw new Exception();
											}
											distLens[ptr++] = repeatedLen;
										}
										mode = DREPS;
										goto decode_loop;
								}
							}
							mode = FINISH;
						return true;
					case LREPS:
						{
							int count = input.PeekBits(repBits);
							if (count < 0) {
								return false;
							}
							input.DropBits(repBits);
							//  	      System.err.println("litlenLens repeat: "+repBits);
							while (count-- > 0) {
								if (ptr >= lnum) {
									throw new Exception();
								}
								litlenLens[ptr++] = repeatedLen;
							}
						}
						mode = LLENS;
						goto decode_loop;
					case DREPS:
						{
							int count = input.PeekBits(repBits);
							if (count < 0) {
								return false;
							}
							input.DropBits(repBits);
							while (count-- > 0) {
								if (ptr >= dnum) {
									throw new Exception();
								}
								distLens[ptr++] = repeatedLen;
							}
						}
						mode = DLENS;
						goto decode_loop;
				}
			}
		}
		
		public InflaterHuffmanTree BuildLitLenTree()
		{
			return new InflaterHuffmanTree(litlenLens);
		}
		
		public InflaterHuffmanTree BuildDistTree()
		{
			return new InflaterHuffmanTree(distLens);
		}
	}
}
