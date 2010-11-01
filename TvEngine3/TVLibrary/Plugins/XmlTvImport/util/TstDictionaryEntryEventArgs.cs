// Ternary Search Tree Implementation for C# 
// 
// Copyright (c) 2004 Jonathan de Halleux
//
// This software is provided 'as-is', without any express or implied warranty. 
// 
// In no event will the authors be held liable for any damages arising from 
// the use of this software.
// Permission is granted to anyone to use this software for any purpose, 
// including commercial applications, and to alter it and redistribute it 
// freely, subject to the following restrictions:
//
//		1. The origin of this software must not be misrepresented; 
//		you must not claim that you wrote the original software. 
//		If you use this software in a product, an acknowledgment in the product 
//		documentation would be appreciated but is not required.
//
//		2. Altered source versions must be plainly marked as such, and must 
//		not be misrepresented as being the original software.
//
//		3. This notice may not be removed or altered from any source 
//		distribution.
//	
//	Ternary Search Tree Implementation for C# Library HomePage: 
//		http://www.dotnetwiki.org
//	Author: Jonathan de Halleux
//  Algorithm found in J. L. Bentley and R. Sedgewick, 
//      Fast algorithms for sorting and searching strings, 
//      in Proceedings of the Eighth Annual ACM-SIAM Symposium on Discrete Algorithms, 
//      New Orleans Louisiana, January 5-7, 
//      1997  

using System;

namespace Tst
{
	/// <summary>
	/// Defines an event argument class that holds a <see cref="TstDictionaryEntry"/>.
	/// </summary>
	public class TstDictionaryEntryEventArgs : EventArgs
	{
		private TstDictionaryEntry entry;
		
		/// <summary>Create a <see cref="TstDictionaryEntry"/> event argument.</summary>
		/// <param name="entry">A <see cref="TstDictionaryEntry"/> entry to pass as argument.</param>
		public TstDictionaryEntryEventArgs(TstDictionaryEntry entry)
		{
			this.entry = entry;
		}

		/// <summary>Gets the <see cref="TstDictionaryEntry"/> entry.</summary>
		/// <value>The <see cref="TstDictionaryEntry"/> entry.</value>
		public TstDictionaryEntry Entry
		{
			get
			{
				return entry;
			}
		}
	}

	/// <summary>
	/// A <see cref="TstDictionaryEntry"/> event handler.
	/// </summary>
	public delegate void TstDictionaryEntryEventHandler(
		Object sender,
		TstDictionaryEntryEventArgs e);
}
