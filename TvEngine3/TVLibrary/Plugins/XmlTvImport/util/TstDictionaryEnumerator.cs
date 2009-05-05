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
using System.Collections;

namespace Tst
{
	
	/// <summary>
	/// Enumerates the elements of a <see cref="TstDictionary"/>.
	/// </summary>
	public sealed class TstDictionaryEnumerator : IDictionaryEnumerator
	{
		private long version;
		private Stack stack;
		private TstDictionaryEntry currentNode;
		private TstDictionary dictionary;
	
		/// <summary>Constructs an enumerator over <paramref name="tst"/></summary>
		/// <param name="tst">dictionary to enumerate.</param>
		/// <exception cref="ArgumentNullException">tst is null</exception>
		public TstDictionaryEnumerator(TstDictionary tst)
		{
			if (tst==null)
				throw new ArgumentNullException("tst");
			this.version = tst.Version;
			this.dictionary = tst;
			this.currentNode = null;
			this.stack = null;
		}

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		public void Reset()
		{
			this.ThrowIfChanged();
			this.stack.Clear();
			stack=null;
		}
	     
	
		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		/// <value>The current element in the collection.</value>
		public DictionaryEntry Current
		{
			get
			{
				this.ThrowIfChanged();
				return this.Entry;
			}
		}
	
		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		/// <value>The current element in the collection.</value>	
		Object IEnumerator.Current
		{
			get
			{
				return this.Current;
			}
		}
	
		/// <summary>
		/// Gets both the key and the value of the current dictionary entry.
		/// </summary>
		/// <value>
		/// A <see cref="DictionaryEntry"/> containing both the key and the value of the current 
		/// dictionary entry.
		/// </value>
		/// <exception cref="InvalidOperationException">
		/// The enumerator is positioned before the first entry of the dictionary or after the last entry. 
		/// </exception>
		public DictionaryEntry Entry
		{
			get
			{
				this.ThrowIfChanged();
				if (currentNode==null)
					throw new InvalidOperationException();
				return new DictionaryEntry(currentNode.Key,currentNode.Value);			
			}
		}
	
		/// <summary>
		/// Gets the key of the current dictionary entry.
		/// </summary>
		/// <value>
		/// The key of the current element of the enumeration.
		/// </value>
		/// <exception cref="InvalidOperationException">
		/// The enumerator is positioned before the first entry of the dictionary or after the last entry. 
		/// </exception>	
		public String Key
		{
			get
			{
				this.ThrowIfChanged();
				if (currentNode==null)
					throw new InvalidOperationException();
				return currentNode.Key;
			}
		}
	
		/// <summary>
		/// Gets the key of the current dictionary entry.
		/// </summary>
		/// <value>
		/// The key of the current element of the enumeration.
		/// </value>
		/// <exception cref="InvalidOperationException">
		/// The enumerator is positioned before the first entry of the dictionary or after the last entry. 
		/// </exception>		
		Object IDictionaryEnumerator.Key
		{
			get
			{
				return this.Key;			
			}
		}
	
		/// <summary>
		/// Gets the value of the current dictionary entry.
		/// </summary>
		/// <value>
		/// The value of the current element of the enumeration.
		/// </value>
		/// <exception cref="InvalidOperationException">
		/// The enumerator is positioned before the first entry of the dictionary or after the last entry. 
		/// </exception>		
		public Object Value
		{
			get
			{
				this.ThrowIfChanged();
				if (currentNode==null)
					throw new InvalidOperationException();
				return currentNode.Value;
			}
		}
		
			/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// true if the enumerator was successfully advanced to the next element; 
		/// false if the enumerator has passed the end of the collection.
		/// </returns>
		public bool MoveNext()
		{
			this.ThrowIfChanged();

			// we are at the beginning
			if (stack==null)
			{
				stack=new Stack();
				currentNode=null;
				if (dictionary.Root!=null) 	
					stack.Push(dictionary.Root);
			}
			// we are at the end node, finished
			else if (currentNode==null)
				throw new InvalidOperationException("out of range");
			
			if (stack.Count==0)
				currentNode=null;
			
			while (stack.Count > 0)
			{				
				currentNode = (TstDictionaryEntry)stack.Pop();				
				if (currentNode.HighChild!=null)
					stack.Push(currentNode.HighChild);
				if (currentNode.EqChild!=null)
					stack.Push(currentNode.EqChild);
				if (currentNode.LowChild!=null)
					stack.Push(currentNode.LowChild);			
				
				if (currentNode.IsKey)
					break;
			}
					
			return currentNode != null;
		}                               	
		
		internal void ThrowIfChanged()
		{
			if (version != dictionary.Version)
				throw new InvalidOperationException("Collection changed");
		}
	
	}
}
