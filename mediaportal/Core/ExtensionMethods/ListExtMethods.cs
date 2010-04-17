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

using System.Collections;
using System.Collections.Generic;

namespace MediaPortal.ExtensionMethods
{
  public static class ListExtMethods
  {
    public static void Dispose(this IList listInterface)
    {
      if (listInterface != null)
      {
        //for (int i = listInterface.Count - 1; i >= 0; i--)        
        //foreach (object o in listInterface)
        for (int i = 0; i < listInterface.Count; i++)
        {
          object o = listInterface[i];
          DisposeHelper.DisposeItem(o);          
        }        
      }
    }   

    public static void DisposeAndClear(this IList listInterface)
    {
      if (listInterface != null)
      {        
        //for (int i = listInterface.Count - 1; i >= 0; i--)
        for (int i = 0; i < listInterface.Count; i++)        
        {
          object o = listInterface[i];
          DisposeHelper.DisposeItem(o);          
        }

        listInterface.Clear();
      }
    }   

    
    public static void DisposeAndClearCollection<T>(this ICollection<T> listInterface)
    {
      if (listInterface != null)
      {       
        foreach (object o in listInterface)
        {
          DisposeHelper.DisposeItem(o);          
        }
                
        listInterface.Clear(); 
      }      
    }

    public static void DisposeAndClearList(this IList listInterface)
    {
      if (listInterface != null)
      {        
        //for (int i = listInterface.Count - 1; i >= 0; i--)
        //foreach (object o in listInterface)
        for (int i = 0; i < listInterface.Count; i++)
        {
          object o = listInterface[i];
          DisposeHelper.DisposeItem(o);          
        }

        listInterface.Clear();
      }
    }   
    
  }
}
