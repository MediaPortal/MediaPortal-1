#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

  }
}
