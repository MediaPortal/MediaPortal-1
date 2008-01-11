#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System.Runtime.InteropServices;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// This is a wrapper class for direct communication with IO ports.
  /// It wraps the methods in the interop assembly and exposes them in a more 
  /// developer friendly way.
  /// </summary>
  /// <remarks>
  /// This class implements the Visitor pattern.
  /// Use the static Port property to get access to the single instance
  /// </remarks>
  /// <author>JoeDalton</author>
  public class IO
  {
    private static readonly IOPort m_Port = new IOPort();

    /// <summary>
    /// Provides access to the single instance
    /// </summary>
    /// <value>
    /// Gets the single instance of</value>
    public static IOPort Port
    {
      get { return m_Port; }
    }

    public class IOPort
    {
      internal IOPort()
      {
      }

      // For sending to a port
      [DllImport("dlportio.dll", EntryPoint = "DlPortWritePortUchar")]
      private static extern void Output(int adress, byte value);

      //For receiving from a port
      [DllImport("dlportio.dll", EntryPoint = "DlPortReadPortUchar")]
      private static extern int Input(int adress);

      /// <summary>
      /// The indexer for this class
      /// </summary>
      /// <value>
      /// Reads or writes to the specified port address
      /// </value>
      public int this[int _addres]
      {
        get { return Input(_addres); }
        set { Output(_addres, (byte)value); }
      }
    }
  }
}