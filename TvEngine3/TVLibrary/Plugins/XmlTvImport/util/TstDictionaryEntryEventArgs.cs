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
      get { return entry; }
    }
  }

  /// <summary>
  /// A <see cref="TstDictionaryEntry"/> event handler.
  /// </summary>
  public delegate void TstDictionaryEntryEventHandler(
    Object sender,
    TstDictionaryEntryEventArgs e);
}