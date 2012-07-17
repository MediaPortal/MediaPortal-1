#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;

namespace MediaPortal.ExtensionMethods
{
  public static class DictionaryExtMethods
  {
    public static void Dispose<TKey, TValue>(this IDictionary<TKey, TValue> dictionaryInterface)
    {
      if (dictionaryInterface != null)
      {
        ICollection<TValue> values = dictionaryInterface.Values;

        foreach (object o in values)
        {
          DisposeHelper.DisposeItem(o);
        }
      }
    }

    public static void DisposeAndClear<TKey, TValue>(this IDictionary<TKey, TValue> dictionaryInterface)
    {
      if (dictionaryInterface != null)
      {
        ICollection<TValue> values = dictionaryInterface.Values;

        /*Dictionary<int, ConditionalAccessContext>.Enumerator en = _mapSubChannels.GetEnumerator();
        while (en.MoveNext())
        {

        }*/

        foreach (object o in values)
        {
          IDisposable disposable = o as IDisposable;
          DisposeHelper.DisposeItem(o);
        }
        dictionaryInterface.Clear();
      }
    }

    /// <summary>
    /// Merge two dictionaries.  Collisions are replaced; the value in 'this' dictionary is overwritten.
    /// Example: 
    ///   result = dictA.Merge(dictB)
    /// </summary>
    public static IDictionary<TKey, TVal> Merge<TKey, TVal>(this IDictionary<TKey, TVal> dictA, IDictionary<TKey, TVal> dictB)
    {
      return dictA.Merge(dictB, true);
    }

    /// <summary>
    /// Merge two dictionaries.  If replace is true then collisions are replaced; the value in 'this' dictionary is overwritten
    /// by the value in dictB.  If replace is false then collisions are ignored; the value in 'this' dictionary is retained.
    /// Example:
    ///   result = dictA.Merge(dictB, true)
    /// </summary>
    public static IDictionary<TKey, TVal> Merge<TKey, TVal>(this IDictionary<TKey, TVal> dictA, IDictionary<TKey, TVal> dictB, bool replace)
    {
      foreach (KeyValuePair<TKey, TVal> pair in dictB)
      {
        // Check for collisions
        if (!dictA.ContainsKey(pair.Key))
        {
          dictA.Add(pair.Key, pair.Value);
        }
        else if (replace)
        {
          // On collision, replace the existing value with the new value if requested.
          dictA.Remove(pair.Key);
          dictA.Add(pair.Key, pair.Value);
        }
      }
      return dictA;
    }
  }
}