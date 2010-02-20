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
	/// Defines a Ternary Search Tree node pair that can be set or retrieved.
	/// </summary>
	public class TstDictionaryEntry : ICloneable
	{
		private char splitChar;
		private bool isKey;
		private string key;
		private Object value;
		private TstDictionaryEntry parent;
		private TstDictionaryEntry lowChild;
		private TstDictionaryEntry eqChild;
		private TstDictionaryEntry highChild;

		/// <summary>
		/// Construct a tst node.
		/// </summary>
		/// <param name="parent">parent node</param>
		/// <param name="splitChar">split character</param>
		public TstDictionaryEntry(
			TstDictionaryEntry parent, 
			char splitChar
			)
		{
			this.isKey = false;
			this.key=null;
			this.value = null;
			this.parent = parent;
			this.splitChar = splitChar;
			this.lowChild = null;
			this.eqChild = null;
			this.highChild = null;
		}
	
		/// <summary>
		/// Gets the parent of the node.
		/// </summary>	
		/// <value>
		/// The parent node.
		/// </value>	
		public TstDictionaryEntry Parent
		{
			get
			{
				return parent;
			}
		}
	
		/// <summary>
		/// Gets the split character.
		/// </summary>	
		/// <value>
		/// The split character.
		/// </value>	
		public char SplitChar
		{
			get
			{
				return splitChar;
			}
		}
	
		/// <summary>
		/// Gets a value indicating wheter the node is a key.
		/// </summary>	
		/// <value>
		/// true is the node is a key, false otherwize.
		/// </value>	
		public bool IsKey
		{
			get
			{
				return isKey;
			}
			set
			{
				isKey = value;
			}
		}
	
		/// <summary>
		/// Gets the node key, if it is a key.
		/// </summary>
		/// <value>
		/// The node key.
		/// </value>
		/// <exception cref="InvalidOperationException">The node does not hold a key-value pair.</exception>
		public String Key
		{
			get
			{
				if (!IsKey)
					throw new InvalidOperationException("node is not a key");
				return key;
			}
			set
			{
				key=value;
			}
		}
	
		/// <summary>
		/// Gets the node value.
		/// </summary>	
		/// <value>
		/// The node value.
		/// </value>
		/// <exception cref="InvalidOperationException">The node does not hold a key-value pair.</exception>
		public Object Value
		{
			get
			{
				if (!IsKey)
					throw new InvalidOperationException("node is not a key");
				return value;
			}
			set
			{
				this.value = value;
			}
		}	
	
		/// <summary>
		/// Gets the node low child.
		/// </summary>	
		/// <value>
		/// The low child.
		/// </value>	
		public TstDictionaryEntry LowChild
		{
			get
			{
				return lowChild;
			}
			set
			{
				lowChild=value;
			}
		}
	
		/// <summary>
		/// Gets the node ep child.
		/// </summary>	
		/// <value>
		/// The eq child.
		/// </value>	
		public TstDictionaryEntry EqChild
		{
			get
			{
				return eqChild;
			}
			set
			{
				eqChild = value;
			}
		}
	
		/// <summary>
		/// Gets the node high child.
		/// </summary>	
		/// <value>
		/// The high child.
		/// </value>	
		public TstDictionaryEntry HighChild
		{
			get
			{
				return highChild;
			}
			set
			{
				highChild = value;
			}
		}
	
		/// <summary>
		/// Gets a value indicating wheter the node has children.
		/// </summary>	
		/// <value>
		/// true if the node has children, false otherwize.
		/// </value>	
		public bool HasChildren
		{
			get
			{
				return LowChild!=null || EqChild!=null || HighChild!=null;
			}
		}
	
		/// <summary>
		/// Gets a value indicating wheter the node is a low child.
		/// </summary>	
		/// <value>
		/// true if the node is a low child, false otherwize.
		/// </value>
		public bool IsLowChild
		{
			get
			{
				return Parent!=null && Parent.LowChild==this;
			}
		}
	
		/// <summary>
		/// Gets a value indicating wheter the node is a high child.
		/// </summary>	
		/// <value>
		/// true if the node is a high child, false otherwize.
		/// </value>
		public bool IsHighChild
		{
			get
			{
				return Parent!=null && Parent.HighChild==this;
			}
		}
	
		/// <summary>
		/// Gets a value indicating wheter the node is a eq child.
		/// </summary>	
		/// <value>
		/// true if the node is a eq child, false otherwize.
		/// </value>
		public bool IsEqChild
		{
			get
			{
				return Parent!=null && Parent.EqChild==this;
			}
		}
		#region ICloneable Members

		/// <summary>
		/// Creates a shallow copy of the entry
		/// </summary>
		/// <returns>entry shallow copy</returns>
		public Object Clone()
		{
			TstDictionaryEntry entry = new TstDictionaryEntry(Parent,SplitChar);
			if (LowChild!=null)
				entry.LowChild = LowChild.Clone() as TstDictionaryEntry;
			if (EqChild!=null)
				entry.EqChild = EqChild.Clone() as TstDictionaryEntry;
			if (HighChild!=null)
				entry.HighChild = HighChild.Clone() as TstDictionaryEntry;
			return entry;
		}
		
		public override string ToString()
		{
			char c;
			if (this.IsEqChild)
				c='E';
			else if (this.IsLowChild)
				c='L';
			else if (this.IsHighChild)
				c='H';
			else
				c='R';
			
			if (this.IsKey)
				return String.Format("{0} {1} {2}",c,this.SplitChar,this.Key);
			else 
				return String.Format("{0} {1}",c,this.SplitChar);
		}

		#endregion
	}
}
