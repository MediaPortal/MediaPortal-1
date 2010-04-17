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

#region Usings

using System;

#endregion

namespace MediaPortal.Threading
{
  public class WorkEventArgs
  {
    public readonly IWork Work;
    private Type resultType;
    private object result;

    public WorkEventArgs(IWork work)
    {
      Work = work;
    }

    public WorkState State
    {
      get { return Work.State; }
    }

    public Exception Exception
    {
      get { return Work.Exception; }
    }

    public void SetResult<T>(T result)
    {
      resultType = typeof (T);
      this.result = result;
    }

    public T GetResult<T>()
    {
      Type t = typeof (T);
      if (t == resultType)
      {
        return (T)result;
      }
      return default(T);
    }
  }
}