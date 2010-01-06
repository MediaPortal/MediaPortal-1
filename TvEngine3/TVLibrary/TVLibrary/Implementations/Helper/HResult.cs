#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System;
using System.Runtime.InteropServices;

namespace TvLibrary
{
  /// <summary>
  ///  This class handles HResult codes returned by com class
  /// </summary>
  /// <remarks>
  /// A HResult code is a 32 bit value layed out as follows:
  ///
  ///   3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
  ///   1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
  ///  +---+-+-+-----------------------+-------------------------------+
  ///  |Sev|C|R|     Facility          |               Code            |
  ///  +---+-+-+-----------------------+-------------------------------+
  ///
  ///  where
  ///
  ///      Sev - is the severity code
  ///
  ///          00 - Success
  ///          01 - Informational
  ///          10 - Warning
  ///          11 - Error
  ///
  ///      C - is the Customer code flag
  ///
  ///      R - is a reserved bit
  ///
  ///      Facility - is the facility code
  ///
  ///      Code - is the facility's status code
  /// </remarks>
  public class HResult
  {
    #region Enums

    /// <summary>
    /// HResult serverity enum
    /// </summary>
    public enum Serverity
    {
      /// <summary>
      /// Unknown serverity
      /// </summary>
      Unknown = -1,
      /// <summary>
      /// Success serverity
      /// </summary>
      Success = 0,
      /// <summary>
      /// Info serverity
      /// </summary>
      Info = 1,
      /// <summary>
      /// Warning serverity
      /// </summary>
      Warning = 2,
      /// <summary>
      /// Errror serverity
      /// </summary>
      Error = 3
    }

    /// <summary>
    /// Facility code enum
    /// </summary>
    public enum Facility
    {
      /// <summary>
      /// Unknown
      /// </summary>
      Unknown = -1,
      /// <summary>
      /// Null
      /// </summary>
      Null = 0,
      /// <summary>
      /// RPC
      /// </summary>
      RPC = 1,
      /// <summary>
      /// Dispatch
      /// </summary>
      Dispatch = 2,
      /// <summary>
      /// Storage
      /// </summary>
      Storage = 3,
      /// <summary>
      /// ITF
      /// </summary>
      ITF = 4,
      /// <summary>
      /// Win32
      /// </summary>
      Win32 = 7,
      /// <summary>
      /// Windows
      /// </summary>
      Windows = 8,
      /// <summary>
      /// Security
      /// </summary>
      Security = 9,
      /// <summary>
      /// Control
      /// </summary>
      Control = 10,
      /// <summary>
      /// Cert
      /// </summary>
      Cert = 11,
      /// <summary>
      /// Internet
      /// </summary>
      Internet = 12,
      /// <summary>
      /// MediaServer
      /// </summary>
      MediaServer = 13,
      /// <summary>
      /// MSMQ
      /// </summary>
      MSMQ = 14,
      /// <summary>
      /// SetupAPI
      /// </summary>
      SetupAPI = 15,
      /// <summary>
      /// SCard
      /// </summary>
      SCard = 16,
      /// <summary>
      /// ComPlus
      /// </summary>
      ComPlus = 17,
      /// <summary>
      /// AAF
      /// </summary>
      AAF = 18,
      /// <summary>
      /// ACS
      /// </summary>
      ACS = 20,
      /// <summary>
      /// DPlay
      /// </summary>
      DPlay = 21,
      /// <summary>
      /// UMI
      /// </summary>
      UMI = 22,
      /// <summary>
      /// SXS
      /// </summary>
      SXS = 23,
      /// <summary>
      /// Windows CE
      /// </summary>
      WindowsCE = 24,
      /// <summary>
      /// HTTP
      /// </summary>
      HTTP = 25,
      /// <summary>
      /// BackgroundCopy
      /// </summary>
      BackgroundCopy = 32,
      /// <summary>
      /// Configuration
      /// </summary>
      Configuration = 33,
      /// <summary>
      /// StateManagement
      /// </summary>
      StateManagement = 34,
      /// <summary>
      /// MetaDirectory
      /// </summary>
      MetaDirectory = 35,
      /// <summary>
      /// D3DX
      /// </summary>
      D3DX = 0x877
    }

    #endregion

    #region Variables

    private uint _hresult;
    private int _facilityCode;
    private Facility _facility = Facility.Unknown;
    private int _serverityCode;
    private Serverity _serverity = Serverity.Unknown;
    private int _code;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="HResult"/> class.
    /// </summary>
    /// <param name="hresult">The hresult code.</param>
    public HResult(int hresult)
    {
      Set(hresult);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the facility type.
    /// </summary>
    /// <value>The facility type.</value>
    public Facility FacilityType
    {
      get { return _facility; }
    }

    /// <summary>
    /// Gets the facility code.
    /// </summary>
    /// <value>The facility code.</value>
    public int FacilityCode
    {
      get { return _facilityCode; }
    }

    /// <summary>
    /// Gets the serverity level.
    /// </summary>
    /// <value>The serverity level.</value>
    public Serverity ServerityLevel
    {
      get { return _serverity; }
    }

    /// <summary>
    /// Gets the code.
    /// </summary>
    /// <value>The code.</value>
    public int Code
    {
      get { return _code; }
    }

    /// <summary>
    /// Gets the DX error string.
    /// </summary>
    /// <value>The DX error string.</value>
    private string DXErrorString
    {
      get { return GetDXErrorString((int)_hresult); }
    }

    /// <summary>
    /// Gets the DX error description.
    /// </summary>
    /// <value>The DX error description.</value>
    private string DXErrorDescription
    {
      get { return GetDXErrorDescription((int)_hresult); }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the specified hresult.
    /// </summary>
    /// <param name="hresult">The hresult.</param>
    public void Set(int hresult)
    {
      _hresult = (uint)hresult;

      _serverityCode = (int)(_hresult >> 30);
      _serverity = (Serverity)_serverityCode;

      _facilityCode = (int)(_hresult >> 16);
      _facilityCode = _facilityCode & 0x0FFF;

      if (Enum.IsDefined(typeof (Facility), _facilityCode))
        _facility = (Facility)_facilityCode;
      else
        _facility = Facility.Unknown;

      _code = (int)_hresult & 0x0000FFFF;
    }

    /// <summary>
    /// Static method which gets the DX error string.
    /// </summary>
    /// <param name="hresult">The hresult.</param>
    /// <returns>the DX error string</returns>
    [DllImport("Dxerr9.dll", EntryPoint = "GetDXErrorString", ExactSpelling = false,
      CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern string GetDXErrorString(int hresult);

    /// <summary>
    /// Static method which gets the DX error description.
    /// </summary>
    /// <param name="hresult">The hresult.</param>
    /// <returns>the DX error description</returns>
    [DllImport("Dxerr9.dll", EntryPoint = "GetDXErrorDescription", ExactSpelling = false,
      CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern string GetDXErrorDescription(int hresult);

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return _facility == Facility.Unknown
               ? String.Format("0x{0} - {1}:Unknown(0x{2}):0x{3}", _hresult.ToString("X").PadLeft(8, '0'), _serverity,
                               _facilityCode.ToString("X").PadLeft(3, '0'), _code.ToString("X").PadLeft(4, '0'))
               : String.Format("0x{0} - {1}:{2}:0x{3}", _hresult.ToString("X").PadLeft(8, '0'), _serverity, _facility,
                               _code.ToString("X").PadLeft(4, '0'));
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public string ToDXString()
    {
      return _hresult == 0
               ? String.Format("No DX Error")
               : String.Format("DX Error: {0} - Error: {1}, Description:{2}", ToString(), DXErrorString,
                               DXErrorDescription);
    }

    /// <summary>
    /// Operator ==s the specified a.
    /// </summary>
    /// <param name="a">A hresult</param>
    /// <param name="b">an int</param>
    /// <returns>bool</returns>
    public static bool operator ==(HResult a, int b)
    {
      return a._hresult == (uint)b;
    }

    /// <summary>
    /// Overloaded lt operator
    /// </summary>
    /// <param name="a">A</param>
    /// <param name="b">B</param>
    /// <returns>true if a._hresult lt b</returns>
    public static bool operator <(HResult a, int b)
    {
      return a._hresult < b;
    }

    /// <summary>
    /// Overloaded gt operator
    /// </summary>
    /// <param name="a">A</param>
    /// <param name="b">B</param>
    /// <returns>true if a._hresult gt b</returns>
    public static bool operator >(HResult a, int b)
    {
      return a._hresult > b;
    }

    /// <summary>
    /// Overloaded lte operator
    /// </summary>
    /// <param name="a">A</param>
    /// <param name="b">B</param>
    /// <returns>true if a._hresult lte b</returns>
    public static bool operator <=(HResult a, int b)
    {
      return a._hresult <= b;
    }

    /// <summary>
    /// Overloaded gte operator
    /// </summary>
    /// <param name="a">A</param>
    /// <param name="b">B</param>
    /// <returns>true if a._hresult gte b</returns>
    public static bool operator >=(HResult a, int b)
    {
      return a._hresult >= b;
    }


    /// <summary>
    /// Operator !=s the specified a.
    /// </summary>
    /// <param name="a">A hresult</param>
    /// <param name="b">an int</param>
    /// <returns>bool</returns>
    public static bool operator !=(HResult a, int b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
    /// <returns>
    /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
    /// </returns>
    public override bool Equals(Object obj)
    {
      // Check for null values and compare run-time types.
      if (obj == null || GetType() != obj.GetType())
        return false;
      return _hresult == ((HResult)obj)._hresult;
    }

    /// <summary>
    /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override int GetHashCode()
    {
      return _hresult.GetHashCode();
    }

    #endregion
  }
}