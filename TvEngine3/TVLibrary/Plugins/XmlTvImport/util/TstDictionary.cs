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
using System.Collections.Specialized;

namespace Tst
{
	/// <summary>
	/// Ternary Search Tree Dictionary
	/// </summary>
	/// <remarks>
	/// <para>
	/// This dictionary is an implementation of the <b>Ternary Search Tree</b>
	/// data structure proposed by J. L. Bentley and R. Sedgewick in their 
	/// paper:  Fast algorithms for sorting and searching strings
	/// in Proceedings of the Eighth Annual ACM-SIAM Symposium on Discrete Algorithms, 
	/// New Orleans Louisiana, January 5-7, 1997. 
	/// </para>
	/// <para>
	/// This dictionary acts as a symbol table: the keys must be string. It
	/// is generally faster to find symbol than the <see cref="Hashtable"/> or
	/// <see cref="SortedList"/> classes. It can also perform more complex search
	/// such as neighbor search.
	/// </para>
	/// <para>
	/// Please read the paper to get some insight on the stucture used below.
	/// </para>	
	/// </remarks>
	public class TstDictionary : 
		IEnumerable
		,ICollection
		,ICloneable
	{
		private TstDictionaryEntry root;
		private long version;

	
		///<summary>
		/// Constructor
		///</summary> 
		/// <remarks>
		/// Construct an empty ternary search tree.
		/// </remarks>
		public TstDictionary()
		{
			root=null;
			version=0;
		}

		/// <summary>
		/// Create a dictionary with a specified root.
		/// </summary>
		/// <param name="root">Root of the new dictionary</param>
		protected TstDictionary(TstDictionaryEntry root)
		{
			if (root==null)
				throw new ArgumentNullException("root is null");
			this.root = root;
			this.version = 0;
		}
	
		///<summary>
		/// Root node
		/// </summary>
		public TstDictionaryEntry Root
		{
			get
			{
				return root;
			}
		}

		/// <summary>
		/// Returns the current dictionary version.
		/// </summary>
		/// <remarks>
		/// This method is used by enumerators to track the dictionary
		/// modification.
		/// </remarks>
		public long Version
		{
			get
			{
				return version;
			}
		}
	
		///<summary>
		/// Gets the number of key-and-value pairs contained in the <see cref="TstDictionary"/>.
		/// </summary>	
		/// <value>
		/// The number of key-and-value pairs contained in the <see cref="TstDictionary"/>.
		/// </value>
		/// <remarks>
		/// Complexity: O(N)
		/// </remarks>
		public virtual int Count
		{
			get
			{
				IEnumerator en = this.GetEnumerator();
				int n=0;
				while(en.MoveNext())
					++n;
				return n;
			}
		}
	
		/// <summary>
		/// Get a value indicating whether access to the <see cref="TstDictionary"/> is synchronized (thread-safe).
		/// </summary>
		/// <value>
		/// true if access to the <see cref="TstDictionary"/> is synchronized (thread-safe); 
		/// otherwise, false. The default is false.
		/// </value>
		public virtual bool IsSynchronized
		{
			get
			{
				return false;
			}
		}
	
		/// <summary>
		/// Gets an object that can be used to synchronize access to the <see cref="TstDictionary"/>.
		/// </summary>
		/// <value>
		/// An object that can be used to synchronize access to the <see cref="TstDictionary"/>.
		/// </value>
		public virtual  Object SyncRoot
		{
			get
			{
				return this;
			}
		}
	
		/// <summary>
		/// Gets a value indicating whether the <see cref="TstDictionary"/> has a fixed size. 
		/// </summary>
		/// <value>
		/// true if the <see cref="TstDictionary"/> has a fixed size; otherwise, false. 
		/// The default is false.
		/// </value>
		public virtual  bool IsFixedSize
		{
			get
			{
				return false;
			}
		}
	
		///<summary>
		/// Gets a value indicating whether the <see cref="TstDictionary"/> is read-only.
		/// </summary>
		/// <value>
		/// true if the <see cref="TstDictionary"/> is read-only; otherwise, false. 
		/// The default is false.
		/// </value>
		public virtual bool IsReadOnly
		{
			get
			{
				return false;
			}
		}
	
		/// <summary>
		/// Gets an <see cref="StringCollection"/> containing the keys in the <see cref="TstDictionary"/>.
		/// </summary>
		/// <returns>
		/// An <see cref="StringCollection"/> containing the keys in the <see cref="TstDictionary"/>.
		/// </returns>
		public virtual ICollection Keys
		{
			get
			{
				StringCollection keys = new StringCollection();
				TstDictionaryEnumerator en = GetEnumerator();
				while(en.MoveNext())
					keys.Add(en.Key);
			
				return keys;
			}
		}
		
		/// <summary>
		/// Gets an <see cref="ICollection"/> containing the values in the <see cref="TstDictionary"/>.
		/// </summary>
		/// <returns>
		/// An <see cref="ICollection"/> containing the values in the <see cref="TstDictionary"/>.
		/// </returns>
		public virtual ICollection Values
		{
			get
			{
				ArrayList values = new ArrayList();
				foreach(DictionaryEntry de in this)
					values.Add(de.Value);
			
				return values;
			}
		}		

		/// <summary>
		/// Creates a shallow copy of the <see cref="TstDictionary"/>.
		/// </summary>
		/// <returns>A shallow copy of the <see cref="TstDictionary"/>.</returns>
		public virtual Object Clone()
		{
			return new TstDictionary(Root.Clone() as TstDictionaryEntry);
		}

		/// <summary>
		/// Returns a synchronized (thread-safe) wrapper for 
		/// the <see cref="TstDictionary"/>.
		/// </summary>
		/// <param name="table">The <see cref="TstDictionary"/> to synchronize.</param>
		/// <returns>A synchronized (thread-safe) wrapper for the 
		/// <see cref="TstDictionary"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="table"/> is a null reference.</exception>
		public static TstDictionary Synchronized(TstDictionary table)
		{
			if (table==null)
				throw new ArgumentNullException("table");
			return new TstSynchronizedDictionary(table);
		}

		/// <summary>
		/// Copies the <see cref="TstDictionary"/> elements to a one-dimensional Array instance at the specified index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the 
		/// <see cref="DictionaryEntry"/> 
		/// objects copied from <see cref="TstDictionary"/>. The <see cref="Array"/> must have zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is a null reference</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is less than zero.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="array"/> is multidimensional. 
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>. 
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The number of elements in the source <see cref="TstDictionary"/> is greater than 
		/// the available space from <paramref name="arrayIndex"/> to the end of the destination array. 
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The type of the source <see cref="TstDictionary"/> cannot be cast automatically 
		/// to the type of the destination array. 
		/// </exception>
		public virtual void CopyTo(Array array, int arrayIndex)
		{
			if (array==null)
				throw new ArgumentNullException("array");
			if (arrayIndex<0)
				throw new ArgumentOutOfRangeException("index is negative");
			if (array.Rank > 1)
				throw new ArgumentException("array is multi-dimensional");
			if (arrayIndex >= array.Length)
				throw new ArgumentException("index >= array.Length");
		
			int i=arrayIndex;
			foreach(Object de in this)
			{
				if (i>array.Length)
					throw new ArgumentException("The number of elements in the source ICollection is greater than the available space from index to the end of the destination array.");

				array.SetValue(de,i++);			
			}
		}
	
		/// <summary>
		/// Gets or sets the value associated with the specified key.
		/// </summary>
		/// <remarks>
		/// [C#] In C#, this property is the indexer for the <see cref="TstDictionary"/> class.
		/// </remarks>
		/// <param name="key">The key whose value to get or set.</param>
		/// <value>
		/// The value associated with the specified key. 
		/// If the specified key is not found, attempting to get it returns a null reference 
		/// (Nothing in Visual Basic), and attempting to set it creates a new element using the specified key.
		/// </value>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference</exception>
		/// <exception cref="ArgumentException">
		/// The property is set and <paramref name="key"/> is an empty string
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The property is set and the <see cref="TstDictionary"/> is read-only. 
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The property is set, <paramref name="key"/> does not exist in the collection,
		/// and the <see cref="TstDictionary"/> has a fixed size.
		/// </exception>
		public virtual Object this[String key]
		{
			get
			{
				if (key==null)
					throw new ArgumentNullException("key");
				TstDictionaryEntry de = Find(key);
				if (de==null)
					return null;
				else
					return de.Value;
			}
			set
			{
				if (key==null)
					throw new ArgumentNullException("key");
				if (key.Length==0)
					throw new ArgumentException("key is an empty string");
				if (IsReadOnly)
					throw new NotSupportedException("read-only dictionary");
				// updating version
				++version;
			
				TstDictionaryEntry de = Find(key);
				if (de==null)
					Add(key,value);
				else
				{
					if (IsFixedSize)
						throw new NotSupportedException("fixed-size dictionary");
					de.Value = value;			
				}
			}
		}
		
		///<summary>
		/// Adds an element with the specified key and value into the <see cref="TstDictionary"/>.
		///</summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add. The value can be a null reference (Nothing in Visual Basic).</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference (Nothing in Visual Basic).
		/// </exception>
		/// <exception cref="ArgumentException"><paramref name="key"/> is an empty string</exception>
		/// <exception cref="ArgumentException">
		/// An element with the same key already exists in the <see cref="TstDictionary"/>.
		/// </exception>
		/// <exception cref="NotSupportedException">The <see cref="TstDictionary"/> is read-only.</exception>
		/// <exception cref="NotSupportedException">The <see cref="TstDictionary"/> has a fixed size.</exception>
		public virtual void Add(String key, Object value)
		{
			if (key==null)
				throw new ArgumentNullException("key is null");
			if (key.Length==0)
				throw new ArgumentException("trying to add empty key");
			if (IsReadOnly)
				throw new NotSupportedException("dictionary is read-only");
			if (IsFixedSize)
				throw new NotSupportedException("dictionary has fixed size");
			// updating version
			++version;

			// creating root node if needed.		
			if (Root == null)
				root = new TstDictionaryEntry(null,key[0]);
		
			// adding key
			TstDictionaryEntry p = Root;
			int i=0;
			char c;
			while (i<key.Length)
			{
				c = key[i];
				if (c < p.SplitChar)
				{
					if(p.LowChild==null)
						p.LowChild = new TstDictionaryEntry(p,c);
					p = p.LowChild;
					continue;
				}
				if (c > p.SplitChar)
				{
					if(p.HighChild==null)
						p.HighChild = new TstDictionaryEntry(p,c);
					p = p.HighChild;
					continue;
				}
				else
				{
					++i;
					if (i==key.Length)
					{
						if (p.IsKey)
							throw new ArgumentException("key already in dictionary");
						break;
					}
					if (p.EqChild==null)
						p.EqChild=new TstDictionaryEntry(p,key[i]);
					p = p.EqChild;
				}
			}
			p.IsKey=true;
			p.Key=key;
			p.Value = value;
		}
	
		///<summary>
		/// Removes the element with the specified key from the <see cref="TstDictionary"/>.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference (Nothing in Visual Basic).
		/// </exception>
		/// <exception cref="ArgumentException"><paramref name="key"/> is an empty string</exception>
		/// <exception cref="NotSupportedException">The <see cref="TstDictionary"/> is read-only.</exception>
		/// <exception cref="NotSupportedException">The <see cref="TstDictionary"/> has a fixed size.</exception>
		public virtual void Remove(String key)
		{
			if (key==null)
				throw new ArgumentNullException("key is null");
			if (key.Length==0)
				throw new ArgumentException("key length cannot be 0");
			if (IsReadOnly)
				throw new NotSupportedException("dictionary is read-only");
			if (IsFixedSize)
				throw new NotSupportedException("dictionary has fixed size");
			// updating version
			++version;

			TstDictionaryEntry p = Find(key);
			if (p==null)
				return;
			p.IsKey = false;
			p.Key = null;

			while(!p.IsKey && !p.HasChildren && p.Parent!=null)
			{
				if (p.IsLowChild)
					p.Parent.LowChild=null;
				else if (p.IsHighChild)
					p.Parent.HighChild=null;
				else 
					p.Parent.EqChild=null;
				p=p.Parent;
			}
		
			if (!p.IsKey && !p.HasChildren && p==root)
				root=null;
		}
	
		/// <summary>
		/// Removes all elements from the <see cref="TstDictionary"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">The <see cref="TstDictionary"/> is read-only.</exception>
		public virtual void Clear()
		{
			if (IsReadOnly)
				throw new NotSupportedException("dictionary is read-only");
			// updating version
			++version;
			root = null;
		}
    
		/// <summary>
		/// Determines whether the <see cref="TstDictionary"/> contains a specific key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="TstDictionary"/>.</param>
		/// <returns>true if the <see cref="TstDictionary"/> contains an element with the specified key; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference (Nothing in Visual Basic).
		/// </exception>
		/// <remarks>
		/// <para>Complexity: Uses a Ternary Search Tree (tst) to find the key.</para>
		/// <para>The method behaves exactly as <see cref="TstDictionary.ContainsKey"/>.</para>
		/// </remarks>
		public bool Contains(String key)
		{
			return ContainsKey(key);
		}

		/// <summary>
		/// Determines whether the <see cref="TstDictionary"/> contains a specific key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="TstDictionary"/>.</param>
		/// <returns>true if the <see cref="TstDictionary"/> contains an element with the specified key; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference (Nothing in Visual Basic).
		/// </exception>
		/// <remarks>
		/// <para>Complexity: Uses a Ternary Search Tree (tst) to find the key.</para>
		/// <para>The method behaves exactly as <see cref="TstDictionary.Contains"/>.</para>
		/// </remarks>    
		public virtual  bool ContainsKey(String key)
		{
			if (key==null)
				throw new ArgumentNullException("key");
			TstDictionaryEntry de = Find(key);
			return de != null && de.IsKey;    
		}
    
		/// <summary>
		/// Determines whether the <see cref="TstDictionary"/> contains a specific value.
		/// </summary>
		/// <returns>
		/// true if the <see cref="TstDictionary"/> contains an element with the specified value; 
		/// otherwise, false.
		/// </returns>
		public bool ContainsValue(Object value)
		{
			foreach(DictionaryEntry de in this)
				if (de.Value==value)
					return true;
			return false;
		}
	
		/// <summary>
		/// Returns an <see cref="IDictionaryEnumerator"/> that can iterate through the <see cref="TstDictionary"/>.
		/// </summary>
		/// <returns>An <see cref="IDictionaryEnumerator"/> for the <see cref="TstDictionary"/>.</returns>
		public virtual TstDictionaryEnumerator GetEnumerator()
		{
			return new TstDictionaryEnumerator(this);
		}

		/// <summary>
		/// Returns an <see cref="IDictionaryEnumerator"/> that can iterate through the <see cref="TstDictionary"/>.
		/// </summary>
		/// <returns>An <see cref="IDictionaryEnumerator"/> for the <see cref="TstDictionary"/>.</returns>		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	
		/// <summary>
		/// Finds the tst node matching the key.
		/// </summary>
		/// <returns>the <see cref="TstDictionaryEntry"/> mathcing the key, null if not found.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
		public virtual TstDictionaryEntry Find(String key) 
		{
			if (key==null)
				throw new ArgumentNullException("key");
			int n = key.Length;
			if (n==0)
				return null;
		
			TstDictionaryEntry p = Root;	
			int index=0;
			char c;
			while(index<n && p!=null)
			{
				c = key[index];
				if (c < p.SplitChar)
					p=p.LowChild;
				else if (c > p.SplitChar)
					p=p.HighChild;
				else
				{
					if (index==n-1)
						return p;
					else
					{
						++index;
						p=p.EqChild;
					}
				}		
			}
			return p;
		} 

		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">key is a null reference</exception>
		/// <exception cref="ArgumentException">key is an empty string</exception>
		public virtual ICollection PartialMatch(string key)
		{
			return PartialMatch(key,'*');
		}

		/// <summary>
		/// Partial match search with wild char character.
		/// </summary>
		/// <param name="key">key to match</param>
		/// <param name="wildChar">"don't care" character</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">key is a null reference</exception>
		/// <exception cref="ArgumentException">key is an empty string</exception>
		/// <remarks>
		/// <para>
		/// Searching the dictionary for the pattern
		/// &quot;*o*o*o&quot; matches the single word <em>rococo</em>, 
		/// while the pattern
		/// &quot;*a*a*a&quot; matches many words, including <em>banana</em>,
		/// <em>casaba</em>, and <em>pajama</em>.
		/// </para>
		/// </remarks>
		public virtual ICollection PartialMatch(string key, char wildChar)
		{
			if (key==null)
				throw new ArgumentNullException("key");
			if (key.Length==0)
				throw new ArgumentException("key is empty");

			ArrayList matches=new ArrayList();
			PartialMatchSearch(Root,key,0,wildChar,matches);
			return matches;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="p"></param>
		/// <param name="key"></param>
		/// <param name="index"></param>
		/// <param name="wildChar"></param>
		/// <param name="matches"></param>
		internal void PartialMatchSearch(
			TstDictionaryEntry p, 
			string key, 
			int index,
			char wildChar,
			IList matches
			)
		{
			if (p==null)
				return;

			char c=key[index];
			if (c == wildChar || c < p.SplitChar)
				PartialMatchSearch(p.LowChild,key,index,wildChar,matches);
			
			if (c == wildChar || c == p.SplitChar)
			{
				if (index < key.Length-1)
					PartialMatchSearch(p.EqChild, key,index+1,wildChar,matches);
				else if(p.IsKey)
					matches.Add(new DictionaryEntry(p.Key,p.Value));
			}

			if (c == wildChar || c > p.SplitChar)
				PartialMatchSearch(p.HighChild, key,index,wildChar,matches);
		}	
	
		/// <summary>
		/// Near-neighbor search in the key string set.
		/// </summary>
		/// <param name="key">key to search for.</param>
		/// <param name="distance">Hamming distance</param>
		/// <returns>near-neighbor search within <paramref name="distance"/>
		/// Hamming distance.
		/// </returns>
		/// <exception cref="ArgumentNullException">key is a null reference</exception>
		/// <exception cref="ArgumentException">dist is negative</exception>
		/// <remarks>
		/// <para>
		/// This methods finds all words in
		/// the dictionary that are within a given Hamming distance of
		/// a query word.
		/// </para>
		/// <para>
		/// For instance, a search for all words within
		/// distance two of soda finds code, coma and 117 other
		/// words.
		/// </para>
		/// </remarks>
		public virtual ICollection NearNeighbors(string key, int distance)
		{
			if (key==null)
				throw new ArgumentNullException("key");
			if (distance<0)
				throw new ArgumentException("dist is negative");

			ArrayList matches = new ArrayList();
			NearNeighborsSearch(Root,key,0,distance,matches);
			return matches;
		}

    /// 
    /// Returns alphabetical list of all keys in the tree that begin with prefix. 
    /// 
    /// The prefix.
    /// 
    public virtual ICollection PrefixMatch(string prefix)
    {
      return PrefixMatch(prefix, -1);
    }

    /// 
    /// Returns alphabetical list of all keys in the tree that begin with prefix.
    /// 
    /// The prefix.
    /// The number of values to return.
    /// 
    public virtual ICollection PrefixMatch(string prefix, int numReturnValues)
    {
      if (prefix == null)
        throw new ArgumentNullException("prefix");
      if (prefix.Length == 0)
        throw new ArgumentException("prefix is empty");
      if (numReturnValues < -1 || numReturnValues == 0)
        throw new ArgumentException("invalid numReturnValues");

      TstDictionaryEntry startNode = Find(prefix);
      if (startNode == null)
        return null;

      int keysNumReturnValues = numReturnValues;
      StringCollection keyList = new StringCollection();

      if (startNode.IsKey)
      {
        keyList.Add(startNode.Key);
        DecreaseRecursionCount(ref keysNumReturnValues);
      }

      if (startNode.EqChild != null && (keysNumReturnValues == -1 || keysNumReturnValues > 0))
      {
        GetKeysRecursion(startNode.EqChild, ref keyList, ref keysNumReturnValues);
      }

      return keyList;
    }


    internal void GetKeysRecursion(TstDictionaryEntry currentNode, ref StringCollection keyList, ref int keysNumReturnValues)
    {
      if (currentNode == null) return;
      if (currentNode.LowChild != null && (keysNumReturnValues == -1 || keysNumReturnValues > 0))
        GetKeysRecursion(currentNode.LowChild, ref keyList, ref keysNumReturnValues);
      if (currentNode.IsKey && (keysNumReturnValues == -1 || keysNumReturnValues > 0))
      {
        keyList.Add(currentNode.Key);
        DecreaseRecursionCount(ref keysNumReturnValues);
      }
      if (currentNode.EqChild != null && (keysNumReturnValues == -1 || keysNumReturnValues > 0))
        GetKeysRecursion(currentNode.EqChild, ref keyList, ref keysNumReturnValues);
      if (currentNode.HighChild != null && (keysNumReturnValues == -1 || keysNumReturnValues > 0))
        GetKeysRecursion(currentNode.HighChild, ref keyList, ref keysNumReturnValues);
    }


    internal void DecreaseRecursionCount(ref int keysNumReturnValues)
    {
      if (keysNumReturnValues > 0)
        keysNumReturnValues--;

    }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="p"></param>
		/// <param name="key"></param>
		/// <param name="index"></param>
		/// <param name="dist"></param>
		/// <param name="matches"></param>
		internal void NearNeighborsSearch(
			TstDictionaryEntry p,
			string key,
			int index,
			int dist,
			IList matches
			)
		{
			if (p==null || dist < 0) 
				return;
				
			char c = key[index];
			// low child
			if (dist > 0 || c < p.SplitChar)
					NearNeighborsSearch(p.LowChild, key,index,dist,matches);
			
			// eq child
			if (p.IsKey) 
			{
				if (key.Length - index<= dist)
					matches.Add(new DictionaryEntry(p.Key,p.Value));
			} 
			else
			{
				int localIndex = index;
				if (localIndex!=key.Length-1)
					++localIndex;
				int localDist = dist;
				if (c!=p.SplitChar)
					--localDist;

				NearNeighborsSearch(
					p.EqChild, 
					key,
					localIndex,
					localDist,
					matches
					);
			}

			// highchild
			if (dist > 0 || c > p.SplitChar)
				NearNeighborsSearch(p.HighChild, key, index,dist,matches);
		}
	}
}
