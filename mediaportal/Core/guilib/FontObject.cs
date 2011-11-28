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
using System.Linq;
using System.Text;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// This is used for caching system fonts (non-latin char support)
  /// </summary>
  internal class FontObject : IDisposable
  {
    protected bool disposed = false;

    #region Properties

    public string name { get; set; }
    public int size { get; set; }
    public System.Drawing.FontStyle style { get; set; }

    public System.Drawing.Font font { get; set; }

    #endregion //properties

    #region IDisposable Members

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
        }
        if (this.font != null)
        {
          this.font.Dispose();
          this.font = null;
        }
      }
      disposed = true;
    }

    #endregion //IDisposable
  }
}