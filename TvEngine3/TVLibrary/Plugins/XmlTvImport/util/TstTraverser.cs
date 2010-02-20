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
	/// Defines a <see cref="TstDictionary"/> tree traverser object.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class can be used to traverse the ternary tree. It defines two events:
	/// TreeEntry and DiscoverEntry.
	/// </para>
	/// </remarks>
	public class TstTraverser
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public TstTraverser()
		{}

		/// <summary>
		/// Tarverses the ternary search tree nodes of <paramref name="dic"/>.
		/// </summary>
		/// <param name="dic">Tree to explore</param>
		/// <exception cref="ArgumentNullException"><paramref name="dic"/> is null.</exception>
		public void Traverse(TstDictionary dic)
		{
			if (dic==null)
				throw new ArgumentNullException("dic");
			Traverse(dic.Root);
		}

		///<summary>Event called when a node is integrated into the traversed tree.</summary>
		public event TstDictionaryEntryEventHandler TreeEntry;
		
		/// <summary>
		/// Raises the <see cref="TstTraverser.TreeEntry"/> event.
		/// </summary>
		/// <param name="p">tst node added to the tree</param>
		protected virtual void OnTreeEntry(TstDictionaryEntry p)
		{
			if (TreeEntry!=null)
				TreeEntry(this,
					new TstDictionaryEntryEventArgs(p)
					);
		}

		///<summary>Event called when a node is discovered but not yet in the explore tree.</summary>
		public event TstDictionaryEntryEventHandler LowChild;
		
		/// <summary>
		/// Raises the <see cref="TstTraverser.HighChild"/> event.
		/// </summary>
		/// <param name="p">high child about to be added to the tree</param>	
		protected virtual void OnLowChild(TstDictionaryEntry p)
		{
			if (LowChild!=null)
				LowChild(this,
					new TstDictionaryEntryEventArgs(p)
					);
		}
		
		///<summary>Event called when a node is discovered but not yet in the explore tree.</summary>
		public event TstDictionaryEntryEventHandler EqChild;
		
		/// <summary>
		/// Raises the <see cref="TstTraverser.HighChild"/> event.
		/// </summary>
		/// <param name="p">high child about to be added to the tree</param>	
		protected virtual void OnEqChild(TstDictionaryEntry p)
		{
			if (EqChild!=null)
				EqChild(this,
					new TstDictionaryEntryEventArgs(p)
					);
		}

		///<summary>Event called when a node is discovered but not yet in the explore tree.</summary>
		public event TstDictionaryEntryEventHandler HighChild;
		
		/// <summary>
		/// Raises the <see cref="TstTraverser.HighChild"/> event.
		/// </summary>
		/// <param name="p">high child about to be added to the tree</param>	
		protected virtual void OnHighChild(TstDictionaryEntry p)
		{
			if (HighChild!=null)
				HighChild(this,
					new TstDictionaryEntryEventArgs(p)
					);
		}

		/// <summary>
		/// Traverses the <paramref name="p"/> sub-tree.
		/// </summary>
		/// <param name="p">node to traverse.</param>
		protected void Traverse(TstDictionaryEntry p)
		{
			if (p==null)
				return;

			OnTreeEntry(p);

			OnLowChild(p.LowChild);
			Traverse(p.LowChild);
			OnEqChild(p.EqChild);
			Traverse(p.EqChild);
			OnHighChild(p.HighChild);
			Traverse(p.HighChild);
		}
		
 	}
}
